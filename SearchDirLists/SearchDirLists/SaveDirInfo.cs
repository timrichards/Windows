using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.IO.Compression;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Drawing;
using System.Security;

namespace SearchDirLists
{
    delegate void SaveDirListingsStatusDelegate(int nIndex, String strText = null, bool bSuccess = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0);
    delegate void SaveDirListingsDoneDelegate();

    class Win32
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WIN32_FIND_DATA
        {
            internal FileAttributes fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal int dwReserved0;
            internal int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string strFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal string strAltFileName;
        }

        internal enum IndexInfoLevels
        {
            FindExInfoStandard = 0,
            FindExInfoBasic,
            FindExInfoMaxInfoLevel
        };

        private enum IndexSearchOps
        {
            FindExSearchNameMatch = 0,
            FindExSearchLimitToDirectories,
            FindExSearchLimitToDevices
        };

        private const int FIND_FIRST_EX_LARGE_FETCH = 0x02;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr FindFirstFileExW(string lpFileName, IndexInfoLevels infoLevel, out WIN32_FIND_DATA lpFindFileData, IndexSearchOps fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlag);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindNextFileW(IntPtr handle, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindClose(IntPtr hFindFile);

        private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

        [Serializable]
        internal class FileData
        {
            FileAttributes m_Attributes;
            DateTime m_CreationTimeUtc;
            DateTime m_LastAccessTimeUtc;
            DateTime m_LastWriteTimeUtc;
            string m_Name;
            long m_Size;
            bool m_bValid = false;

            internal FileAttributes Attributes { get { return m_Attributes; } }
            internal DateTime CreationTimeUtc { get { return m_CreationTimeUtc; } }
            internal DateTime LastAccessTimeUtc { get { return m_LastAccessTimeUtc; } }
            internal DateTime LastWriteTimeUtc { get { return m_LastWriteTimeUtc; } }
            internal string Name { get { return m_Name; } }
            internal long Size { get { return m_Size; } }
            internal bool IsValid { get { return m_bValid; } }

            internal static bool WinFile(String strFile, out WIN32_FIND_DATA winFindData)
            {
                String P = Path.DirectorySeparatorChar.ToString();
                String PP = P + P;
                IntPtr handle = FindFirstFileExW(PP + '?' + P + strFile, IndexInfoLevels.FindExInfoBasic, out winFindData, IndexSearchOps.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);

                winFindData.strAltFileName = strFile.Replace(PP, P);                        // 8.3 not used
                return (handle != InvalidHandleValue);
            }

            internal FileData(WIN32_FIND_DATA findData)
            {
                m_Attributes = findData.fileAttributes;
                m_CreationTimeUtc = ConvertDateTime(findData.ftCreationTimeHigh, findData.ftCreationTimeLow);
                m_LastAccessTimeUtc = ConvertDateTime(findData.ftLastAccessTimeHigh, findData.ftLastAccessTimeLow);
                m_LastWriteTimeUtc = ConvertDateTime(findData.ftLastWriteTimeHigh, findData.ftLastWriteTimeLow);
                m_Name = findData.strFileName;
                m_Size = CombineHighLowInts(findData.nFileSizeHigh, findData.nFileSizeLow);
                m_bValid = (findData.ftCreationTimeHigh | findData.ftCreationTimeLow) != 0;
            }

            internal DateTime CreationTime
            {
                get { return CreationTimeUtc.ToLocalTime(); }
            }

            internal DateTime LastAccessTime
            {
                get { return LastAccessTimeUtc.ToLocalTime(); }
            }

            internal DateTime LastWriteTime
            {
                get { return LastWriteTimeUtc.ToLocalTime(); }
            }

            private static long CombineHighLowInts(uint high, uint low)
            {
                return (((long)high) << 0x20) | low;
            }

            private static DateTime ConvertDateTime(uint high, uint low)
            {
                long fileTime = CombineHighLowInts(high, low);
                return DateTime.FromFileTimeUtc(fileTime);
            }
        }

        internal static bool GetDirectory(String strDir, ref List<WIN32_FIND_DATA> listDirs, ref List<WIN32_FIND_DATA> listFiles)
        {
            String P = Path.DirectorySeparatorChar.ToString();
            String PP = P + P;
            WIN32_FIND_DATA winFindData;
            IntPtr handle = FindFirstFileExW(PP + '?' + P + strDir + P + '*', IndexInfoLevels.FindExInfoBasic, out winFindData, IndexSearchOps.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);

            if (handle == InvalidHandleValue)
            {
                return false;
            }

            listDirs.Clear();
            listFiles.Clear();

            do
            {
                if ("..".Contains(winFindData.strFileName))
                {
                    continue;
                }

                winFindData.strAltFileName = (strDir + P + winFindData.strFileName).Replace(PP, P);     // 8.3 not used

                if ((winFindData.fileAttributes & FileAttributes.Directory) != 0)
                {
                    if ((winFindData.fileAttributes & FileAttributes.ReparsePoint) != 0)
                    {
                        const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
                        const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;

                        // stay on source drive. Treat mount points and symlinks as files.
                        if (((winFindData.dwReserved0 & IO_REPARSE_TAG_MOUNT_POINT) != 0)
                            || ((winFindData.dwReserved0 & IO_REPARSE_TAG_SYMLINK) != 0))
                        {
                            listFiles.Add(winFindData);
                            continue;
                        }
                    }

                    listDirs.Add(winFindData);
                }
                else
                {
                    listFiles.Add(winFindData);
                }
            }
            while (FindNextFileW(handle, out winFindData));

            FindClose(handle);
            listDirs.Sort((x, y) => x.strFileName.CompareTo(y.strFileName));
            listFiles.Sort((x, y) => x.strFileName.CompareTo(y.strFileName));
            return true;
        }
    }

    class Utilities
    {
        internal const String m_str_HEADER_01 = "SearchDirLists 0.1";
        internal const String m_str_START_01 = m_str_HEADER_01 + " START";
        internal const String m_str_END_01 = m_str_HEADER_01 + " END";
        internal const String m_str_ERRORS_LOC_01 = m_str_HEADER_01 + " ERRORS";
        internal const String m_str_TOTAL_LENGTH_LOC_01 = m_str_HEADER_01 + " LENGTH";
        internal const String m_str_DRIVE_01 = m_str_HEADER_01 + " DRIVE";
        internal const String m_str_VOLUME_LIST_HEADER_01 = m_str_HEADER_01 + " VOLUME LIST";
        internal const String m_str_HEADER = "SearchDirLists 0.2";
        internal const String m_str_START = m_str_HEADER + " START";
        internal const String m_str_END = m_str_HEADER + " END";
        internal const String m_str_ERRORS_LOC = m_str_HEADER + " ERRORS";
        internal const String m_str_TOTAL_LENGTH_LOC = m_str_HEADER + " LENGTH";
        internal const String m_str_DRIVE = m_str_HEADER + " DRIVE";
        internal const String m_str_VOLUME_LIST_HEADER = m_str_HEADER + " VOLUME LIST";
        internal const String m_str_COPYDIRS_LIST_HEADER = m_str_HEADER + " COPYDIRS LIST";
        internal const String m_str_USING_FILE = "Using file.";
        internal const String m_str_SAVED = "Saved.";
        internal const int nColLENGTH = 7;
        internal const int nColLENGTH_01 = 5;
        internal const int nColLENGTH_LV = 4;
        internal const String m_strLINETYPE_Version = "V";
        internal const String m_strLINETYPE_Nickname = "N";
        internal const String m_strLINETYPE_Path = "P";
        internal const String m_strLINETYPE_DriveInfo = "I";
        internal const String m_strLINETYPE_Comment = "C";
        internal const String m_strLINETYPE_Start = "S";
        internal const String m_strLINETYPE_Directory = "D";
        internal const String m_strLINETYPE_File = "F";
        internal const String m_strLINETYPE_End = "E";
        internal const String m_strLINETYPE_Blank = "B";
        internal const String m_strLINETYPE_Error_Dir = "R";
        internal const String m_strLINETYPE_Error_File = "r";
        internal const String m_strLINETYPE_Length = "L";

        static MessageBoxDelegate m_MessageboxCallback_ = null;
        protected MessageBoxDelegate m_MessageboxCallback = null;

        protected Utilities()
        {
            Debug.Assert(m_MessageboxCallback_ != null);

            m_MessageboxCallback = m_MessageboxCallback_;
        }

        internal static void SetMessageBoxDelegate(MessageBoxDelegate messageBoxCallback)
        {
            m_MessageboxCallback_ = messageBoxCallback;
        }

        internal static String NotNull(String str)
        {
            return str ?? "";
        }

        internal static bool StrValid(String str)
        {
            return ((str != null) && (str.Length > 0));
        }

        internal static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        internal static byte[] Zip(String str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        //msi.CopyTo(gs);
                        CopyTo(msi, gs);
                    }

                    return mso.ToArray();
                }
            }
        }

        internal static String Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        //gs.CopyTo(mso);
                        CopyTo(gs, mso);
                    }

                    return Encoding.UTF8.GetString(mso.ToArray());
                }
            }
        }

        internal static bool FormatPath(ref String strPath, bool bFailOnDirectory = true)
        {
            while (strPath.Contains(@"\\"))
            {
                strPath = strPath.Replace(@"\\", @"\");
            }

            String strDirName = Path.GetDirectoryName(strPath);

            if ((StrValid(strDirName) == false) || Directory.Exists(strDirName))
            {
                String strCapDrive = strPath.Substring(0, strPath.IndexOf(":" + Path.DirectorySeparatorChar) + 2);

                strPath = Path.GetFullPath(strPath).Replace(strCapDrive, strCapDrive.ToUpper());

                if (strPath == strCapDrive.ToUpper())
                {
                    Debug.Assert(strDirName == null);
                }
                else
                {
                    strPath = strPath.TrimEnd(Path.DirectorySeparatorChar);
                    Debug.Assert(StrValid(strDirName));
                }
            }
            else if (bFailOnDirectory)
            {
                return false;
            }

            return true;
        }

        internal static bool FormatPath(ref String strPath, ref String strSaveAs, bool bFailOnDirectory = true)
        {
            if (StrValid(strPath))
            {
                strPath += Path.DirectorySeparatorChar;

                if (FormatPath(ref strPath, bFailOnDirectory) == false)
                {
                    m_MessageboxCallback_("Error in Source path.                   ", "Save Directory Listing");
                    return false;
                }
            }

            if (StrValid(strSaveAs))
            {
                strSaveAs = Path.GetFullPath(strSaveAs.Trim());

                if (FormatPath(ref strSaveAs, bFailOnDirectory) == false)
                {
                    m_MessageboxCallback_("Error in Save filename.                  ", "Save Directory Listing");
                    return false;
                }
            }

            return true;
        }

        internal static bool LV_VolumesItemInclude(LVvolStrings lvStrings)
        {
            return (lvStrings.Include == "Yes");
        }

        internal static bool LV_VolumesItemCanLoad(LVvolStrings lvStrings)
        {
            return (LV_VolumesItemInclude(lvStrings) &&
                ((Utilities.m_str_USING_FILE + Utilities.m_str_SAVED).Contains(lvStrings.Status)));
        }

        internal static String FormatSize(String in_str, bool bBytes = false)
        {
            return FormatSize(long.Parse(in_str), bBytes);
        }

        internal static String FormatSize(long nLength, bool bBytes = false, bool bNoDecimal = false)
        {
            double nT = nLength / 1024.0 / 1024.0 / 1024 / 1024 - .05;
            double nG = nLength / 1024.0 / 1024 / 1024 - .05;
            double nM = nLength / 1024.0 / 1024 - .05;
            double nK = nLength / 1024.0 - .05;     // Windows Explorer seems to not round
            String strFmt_big = "###,##0.0";
            String strFormat = bNoDecimal ? "###,###" : strFmt_big;
            String strSz = null;

            if (((int)nT) > 0) strSz = nT.ToString(strFmt_big) + " TB";
            else if (((int)nG) > 0) strSz = nG.ToString(strFmt_big) + " GB";
            else if (((int)nM) > 0) strSz = nM.ToString(strFormat) + " MB";
            else if (((int)nK) > 0) strSz = nK.ToString(strFormat) + " KB";
            else strSz = "1 KB";                    // Windows Explorer mins at 1K

            if (nLength > 0)
            {
                return strSz + (bBytes ? (" (" + nLength.ToString("###,###,###,###,###") + " bytes)") : "");
            }
            else
            {
                return "0 bytes";
            }
        }

        internal static String FormatString(String strDir = null, String strFile = null, DateTime? dtCreated = null, DateTime? dtModified = null, String strAttributes = null, long nLength = 0, String strError1 = null, String strError2 = null, int? nHeader = null)
        {
            String strLength = null;
            String strCreated = null;
            String strModified = null;

            if (nLength > 0)
            {
                strLength = nLength.ToString();
            }

            if (dtCreated != null)
            {
                strCreated = dtCreated.ToString();
            }

            if (dtModified != null)
            {
                strModified = dtModified.ToString();
            }

            if (StrValid(strDir + strFile + strCreated + strModified + strAttributes + strLength + strError1 + strError2) == false)
            {
                Debug.Assert(nHeader is int);

                if (nHeader == 0)
                {
                    return "2" + '\t' + "3" + '\t' + "4" + '\t' + "5" + '\t' + "6" + '\t' + "7" + '\t' + "8" + '\t' + "9";
                }
                else if (nHeader == 1)
                {
                    return "Dir" + '\t' + "File" + '\t' + "Created" + '\t' + "Modded" + '\t' + "Attrib" + '\t' + "Length" + '\t' + "Error1" + '\t' + "Error2";
                }
            }

            bool bDbgCheck = false;

            if ((NotNull(strDir).TrimEnd() != NotNull(strDir)) || (NotNull(strFile).TrimEnd() != NotNull(strFile)))
            {
                strError1 += " Trailing whitespace";
                strError1.Trim();
                Debug.Assert(StrValid(strDir) || StrValid(strFile));
                bDbgCheck = true;
            }

            String strRet = (strDir + '\t' + strFile + '\t' + strCreated + '\t' + strModified + '\t' + strAttributes + '\t' + strLength + '\t' + strError1 + '\t' + strError2).TrimEnd();

            if (bDbgCheck)
            {
                String[] strArray = strRet.Split('\t');
                DateTime dtParse;

                if ((strArray.Length > 5) && strArray[5].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                {
                    Debug.Assert(false);
                }
            }

            return strRet;
        }

        static String FormatLine(String strLineType, long nLineNo, String strLine_in = null)
        {
            String strLine_out = strLineType + "\t" + nLineNo;

            if (StrValid(strLine_in))
            {
                strLine_out += '\t' + strLine_in;
            }

            return strLine_out;
        }

        protected static String StrFile_01(String strFile)
        {
            return Path.Combine(Path.GetDirectoryName(strFile),
                Path.GetFileNameWithoutExtension(strFile) + "_01" + Path.GetExtension(strFile));
        }

        internal static bool ValidateFile(String strSaveAs)
        {
            do
            {
                String[] arrLine = File.ReadLines(strSaveAs).Take(1).ToArray();

                if (arrLine.Length <= 0) break;

                if (arrLine[0] == m_str_HEADER_01)
                {
                    Console.WriteLine("Converting " + strSaveAs);
                    ConvertFile(strSaveAs);
                    Console.WriteLine("File converted to " + m_str_HEADER);
                }

                String[] arrToken = File.ReadLines(strSaveAs).Take(1).ToArray()[0].Split('\t');

                if (arrToken.Length < 3) break;
                if (arrToken[2] != m_str_HEADER) break;

                String[] arrLine_A = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Length)).ToArray();

                if (arrLine_A.Length == 0) break;

                String[] arrToken_A = arrLine_A[0].Split('\t');

                if (arrToken_A.Length < 3) break;
                if (arrToken_A[2] != m_str_TOTAL_LENGTH_LOC) break;

                return true;
            }
            while (false);

            return false;
        }

        internal static void ConvertFile(String strFile)
        {
            String strFile_01 = StrFile_01(strFile);

            if (File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            File.Move(strFile, strFile_01);

            using (StreamWriter file_out = new StreamWriter(strFile))
            {
                using (StreamReader file_in = new StreamReader(strFile_01))
                {
                    String strLine = null;
                    long nLineNo = 0;       // lines number from one
                    bool bAtErrors = false;

                    while ((strLine = file_in.ReadLine()) != null)
                    {
                        ++nLineNo;

                        if (strLine == m_str_HEADER_01)
                        {
                            Debug.Assert(nLineNo == 1);
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Version, nLineNo, m_str_HEADER));
                            continue;
                        }
                        else if (nLineNo == 2)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Nickname, nLineNo, strLine));
                            continue;
                        }
                        else if (nLineNo == 3)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Path, nLineNo, strLine));
                            continue;
                        }
                        else if (strLine == m_str_DRIVE_01)
                        {
                            Debug.Assert(nLineNo == 4);
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Comment, nLineNo, m_str_DRIVE));

                            for (int i = 0; i < 8; ++i)
                            {
                                strLine = file_in.ReadLine();
                                file_out.WriteLine(FormatLine(m_strLINETYPE_DriveInfo, nLineNo, strLine));
                                ++nLineNo;
                            }

                            continue;
                        }
                        else if (strLine.Length == 0)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Blank, nLineNo));
                            continue;
                        }
                        else if (nLineNo == 14)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Comment, nLineNo, FormatString(nHeader: 0)));
                            continue;
                        }
                        else if (nLineNo == 15)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Comment, nLineNo, FormatString(nHeader: 1)));
                            continue;
                        }
                        else if (strLine.StartsWith(m_str_START_01))
                        {
                            Debug.Assert(nLineNo == 16);
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Start, nLineNo, m_str_START));
                            continue;
                        }
                        else if (strLine.StartsWith(m_str_END_01))
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_End, nLineNo, m_str_END));
                            continue;
                        }
                        else if (strLine == m_str_ERRORS_LOC_01)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Comment, nLineNo, m_str_ERRORS_LOC));
                            bAtErrors = true;
                            continue;
                        }
                        else if (strLine.StartsWith(m_str_TOTAL_LENGTH_LOC_01))
                        {
                            String[] arrLine = strLine.Split('\t');

                            file_out.WriteLine(FormatLine(m_strLINETYPE_Length, nLineNo, FormatString(strDir: m_str_TOTAL_LENGTH_LOC, nLength: long.Parse(arrLine[nColLENGTH_01]))));
                            continue;
                        }

                        String[] strArray = strLine.Split('\t');
                        String strDir = strArray[0];

                        if (StrValid(strDir) == false)
                        {
                            DateTime dtParse;
                            String strTab = null;

                            if ((strArray.Length > 5) && strArray[5].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                            {
                                strTab = "\t";
                            }

                            file_out.WriteLine(FormatLine(bAtErrors ? m_strLINETYPE_Error_File : m_strLINETYPE_File, nLineNo, strTab + strLine));
                            continue;
                        }
                        else if (strDir.Contains(":" + Path.DirectorySeparatorChar) == false)
                        {
                            Debug.Assert(false);        // all that's left is directories
                            continue;
                        }

                        // directory
                        String P = Path.DirectorySeparatorChar.ToString();
                        String PP = P + P;
                        String str = strLine.Replace(PP, P);

                        file_out.WriteLine(FormatLine(bAtErrors ? m_strLINETYPE_Error_Dir : m_strLINETYPE_Directory, nLineNo, str));
                    }
                }
            }
        }
    }

    class LVvolStrings : Utilities
    {
        int m_nIndex = -1;
        String m_strVolumeName = null;
        String m_strPath = null;
        String m_strSaveAs = null;
        String m_strStatus = null;
        String m_strInclude = null;
        String m_strVolumeGroup = null;
        internal int Index { get { return m_nIndex; } }
        internal String VolumeName { get { return m_strVolumeName; } }
        internal String StrPath { get { return m_strPath; } }
        internal String SaveAs { get { return m_strSaveAs; } }
        internal String Status { get { return m_strStatus; } }
        internal String Include { get { return m_strInclude; } }
        internal String VolumeGroup { get { return m_strVolumeGroup; } }

        internal LVvolStrings(ListViewItem lvItem)
        {
            m_nIndex = lvItem.Index;
            m_strVolumeName = lvItem.SubItems[0].Text;
            m_strPath = lvItem.SubItems[1].Text;
            m_strSaveAs = lvItem.SubItems[2].Text;
            m_strStatus = lvItem.SubItems[3].Text;
            m_strInclude = lvItem.SubItems[4].Text;

            if ((lvItem.SubItems.Count > 5) && StrValid(lvItem.SubItems[5].Text))
            {
                m_strVolumeGroup = lvItem.SubItems[5].Text;
            }
        }

        internal void SetStatus_BadFile(ListView lv)
        {
            lv.Items[Index].SubItems[3].Text =
                m_strStatus = "Bad file. Will overwrite.";
        }
    }

    class SaveDirListing : Utilities
    {
        SaveDirListingsStatusDelegate m_statusCallback = null;
        Thread m_thread = null;
        LVvolStrings m_volStrings = null;
        long m_nLengthTotal = 0;
        long m_nFilesTotal = 0;
        long m_nFilesDiff = 0;
        List<double> m_listFileDiffs = new List<double>();
        List<String> m_list_Errors = new List<String>();
        System.Threading.Timer m_timerStatus = null;

        private double StdDevSign(List<double> values)
        {
            double nAvg = values.Average();
            double nStdDev = Math.Sqrt(
                values.Sum(d => Math.Pow(d - nAvg, 2)) /
                (values.Count() - 1));

            return nStdDev *= Math.Sign(values[values.Count - 1] - nAvg);
        }
        
        void SaveDirListing_TimerCallback(object state)
        {
            m_listFileDiffs.Add(m_nFilesDiff);

            if (m_listFileDiffs.Count < 2)
            {
                return;
            }

            double nFilesDiff = m_nFilesDiff/StdDevSign(m_listFileDiffs);

            m_nFilesDiff = 0;
            m_statusCallback(m_volStrings.Index, nFilesTotal: m_nFilesTotal, nLengthTotal: m_nLengthTotal, nFilesDiff: nFilesDiff);
        }

        internal SaveDirListing(LVvolStrings volStrings,
            SaveDirListingsStatusDelegate statusCallback)
        {
            m_volStrings = volStrings;
            m_statusCallback = statusCallback;
            m_timerStatus = new System.Threading.Timer(new TimerCallback(SaveDirListing_TimerCallback), null, 1000, 1000);
        }

        private void WriteHeader(TextWriter fs, String strVolumeName, String strPath)
        {
            fs.WriteLine(m_str_HEADER_01);
            // assume SaveFields() by caller because SaveFields() has already prompted user
            fs.WriteLine(strVolumeName);
            fs.WriteLine(strPath);

            DriveInfo driveInfo = new DriveInfo(strPath.Substring(0, strPath.IndexOf(Path.DirectorySeparatorChar)));

            fs.WriteLine(m_str_DRIVE_01);
            fs.WriteLine(driveInfo.AvailableFreeSpace);
            fs.WriteLine(driveInfo.DriveFormat);
            fs.WriteLine(driveInfo.DriveType);
            fs.WriteLine(driveInfo.Name);
            fs.WriteLine(driveInfo.RootDirectory);
            fs.WriteLine(driveInfo.TotalFreeSpace);
            fs.WriteLine(driveInfo.TotalSize);
            fs.WriteLine(driveInfo.VolumeLabel);
        }

        String CheckNTFS_chars(String strFile, bool bFile = false)
        {
            char[] arrChar = bFile ? Path.GetInvalidFileNameChars() : Path.GetInvalidPathChars();
            int nIx = -1;

            if ((nIx = strFile.IndexOfAny(arrChar)) > -1)
            {
                return "NTFS ASCII " + ((int)strFile[nIx]).ToString();
            }
            else
            {
                return null;
            }
        }

        void TraverseTree(TextWriter fs, String root)
        {
            Stack<Win32.WIN32_FIND_DATA> stackDirs = new Stack<Win32.WIN32_FIND_DATA>(64);
            List<Win32.WIN32_FIND_DATA> listSubDirs = new List<Win32.WIN32_FIND_DATA>();
            List<Win32.WIN32_FIND_DATA> listFiles = new List<Win32.WIN32_FIND_DATA>();
            Win32.WIN32_FIND_DATA winRoot;

            Win32.FileData.WinFile(root, out winRoot);
            stackDirs.Push(winRoot);

            while (stackDirs.Count > 0)
            {
                Win32.WIN32_FIND_DATA winDir = stackDirs.Pop();
                long nDirLength = 0;
                bool bHasLength = false;
                String strError2_Dir = CheckNTFS_chars(winDir.strAltFileName);

                if (Win32.GetDirectory(winDir.strAltFileName, ref listSubDirs, ref listFiles) == false)
                {
                    m_list_Errors.Add(FormatString(strDir: winDir.strAltFileName,
                        strError1: new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message, strError2: strError2_Dir));
                    continue;
                }

                foreach (Win32.WIN32_FIND_DATA winData in listFiles)
                {
                    Win32.FileData fi = new Win32.FileData(winData);
                    String strError2_File = CheckNTFS_chars(winData.strFileName, bFile: true);

                    if (fi.IsValid == false)
                    {
                        String strErrorFile = winData.strFileName;
                        String strErrorDir = winData.strAltFileName.Substring(0, winData.strAltFileName.LastIndexOf(Path.DirectorySeparatorChar));

                        m_list_Errors.Add(FormatString(strFile: strErrorFile, strDir: strErrorDir, strError2: strError2_File));
                        continue;
                    }

                    m_nLengthTotal += fi.Size;
                    nDirLength += fi.Size;
                    ++m_nFilesTotal;
                    ++m_nFilesDiff;

                    String strError1 = null;

                    if (winData.strAltFileName.Length > 260)
                    {
                        strError1 = "Path Length: " + winData.strAltFileName.Length.ToString();
                    }

                    String strOut = FormatString(strFile: fi.Name, dtCreated: fi.CreationTime, strAttributes: fi.Attributes.ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Size, strError1: strError1, strError2: strError2_File);

                    Debug.Assert(fi.Size >= 0);

                    if (fi.Size > 0)
                    {
                        bHasLength = true;
                    }

                    fs.WriteLine(strOut);
                }

                {
                    String strError1 = null;

                    if (winDir.strAltFileName.Length > 240)
                    {
                        strError1 = "Path Length: " + winDir.strAltFileName.Length.ToString();
                    }

                    Debug.Assert(bHasLength == (nDirLength > 0));
                    Win32.FileData di = new Win32.FileData(winDir);

                    if (winDir.strAltFileName.EndsWith(@":\"))
                    {
                        Debug.Assert(di.IsValid == false);          // yes, yes...
                        Debug.Assert(winDir.strAltFileName.Length == 3);
                        fs.WriteLine(FormatString(strDir: winDir.strAltFileName, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
                    }
                    else
                    {
                        Debug.Assert(di.IsValid);
                        fs.WriteLine(FormatString(strDir: winDir.strAltFileName, dtCreated: di.CreationTime, strAttributes: di.Attributes.ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
                    }
                }

                foreach (Win32.WIN32_FIND_DATA winData in listSubDirs)
                {
                    stackDirs.Push(winData);
                }
            }
        }

        void Go()
        {
            String strVolumeName = m_volStrings.VolumeName;
            String strPath = m_volStrings.StrPath;
            String strSaveAs = m_volStrings.SaveAs;

            if (FormatPath(ref strPath, ref strSaveAs) == false)
            {
                // FormatPath() has its own message box
                m_statusCallback(m_volStrings.Index, "Not saved.");
                return;
            }

            if (Directory.Exists(strPath) == false)
            {
                m_statusCallback(m_volStrings.Index, "Not saved.");
                m_MessageboxCallback("Source Path does not exist.                  ", "Save Directory Listing");
                return;
            }

            if (StrValid(strSaveAs) == false)
            {
                m_statusCallback(m_volStrings.Index, "Not saved.");
                m_MessageboxCallback("Must specify save filename.                  ", "Save Directory Listing");
                return;
            }

            String strPathOrig = Directory.GetCurrentDirectory();

            using (TextWriter fs = File.CreateText(strSaveAs))
            {
                WriteHeader(fs, strVolumeName, strPath);
                fs.WriteLine();
                fs.WriteLine(FormatString(nHeader: 0));
                fs.WriteLine(FormatString(nHeader: 1));
                fs.WriteLine(m_str_START_01 + " " + DateTime.Now.ToString());
                TraverseTree(fs, strPath);
                fs.WriteLine(m_str_END_01 + " " + DateTime.Now.ToString());
                fs.WriteLine();
                fs.WriteLine(m_str_ERRORS_LOC_01);

                foreach (String strError in m_list_Errors)
                {
                    fs.WriteLine(strError);
                }

                fs.WriteLine();
                fs.WriteLine(FormatString(strDir: m_str_TOTAL_LENGTH_LOC_01, nLength: m_nLengthTotal));
            }

            Directory.SetCurrentDirectory(strPathOrig);
            m_statusCallback(m_volStrings.Index, strText: m_str_SAVED, bSuccess: true);
            m_timerStatus.Dispose();
        }

        internal Thread DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
            return m_thread;
        }
    }

    class SaveDirListings : Utilities
    {
        SaveDirListingsStatusDelegate m_statusCallback = null;
        SaveDirListingsDoneDelegate m_doneCallback = null;
        Thread m_thread = null;
        List<Thread> m_listThreads = new List<Thread>();
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();

        int m_nFilesWritten = 0;
        internal int FilesWritten { get { return m_nFilesWritten; } set { m_nFilesWritten = value; } }

        internal SaveDirListings(ListView.ListViewItemCollection lvVolItems,
                SaveDirListingsStatusDelegate statusCallback,
                SaveDirListingsDoneDelegate doneCallback)
        {
            foreach (ListViewItem lvItem in lvVolItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        void Go()
        {
            Console.WriteLine();
            Console.WriteLine("Saving directory listings.");

            DateTime dtStart = DateTime.Now;
            int nVolIx = -1;

            foreach (LVvolStrings volStrings in m_list_lvVolStrings)
            {
                ++nVolIx;

                if ((m_str_USING_FILE + m_str_SAVED).Contains(volStrings.Status))
                {
                    continue;
                }

                m_statusCallback(nVolIx, "Saving...");
                m_listThreads.Add(new SaveDirListing(volStrings, m_statusCallback).DoThreadFactory());
            }

            foreach (Thread thread in m_listThreads)
            {
                thread.Join();
            }

            Console.WriteLine(String.Format("Finished saving directory listings in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));
            m_doneCallback();
        }

        internal void EndThread()
        {
            foreach (Thread thread in m_listThreads)
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }

            m_listThreads = new List<Thread>();

            if ((m_thread != null) && m_thread.IsAlive)
            {
                m_thread.Abort();
            }

            m_thread = null;
        }

        internal void DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }
    }

    partial class Form1 : Form
    {
        SaveDirListings m_saveDirListings = null;

        void SaveDirListingsStatusCallback(int nIndex, String strText = null, bool bSuccess = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0)
        {
            if (InvokeRequired) { Invoke(new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback), new object[] { nIndex, strText, bSuccess, nFilesTotal, nLengthTotal, nFilesDiff }); return; }

            if (nLengthTotal > 0)
            {
                Debug.Assert(strText == null);
                Debug.Assert(bSuccess == false);

                strText = nFilesTotal.ToString("###,###,###,###") + " (" + Utilities.FormatSize(nLengthTotal) + ") SD " + nFilesDiff.ToString("#.0");
            }

            if (bSuccess)
            {
                lock (m_saveDirListings)
                {
                    ++m_saveDirListings.FilesWritten;
                }
            }

            lock (form_lvVolumesMain)
            {
                form_lvVolumesMain.Items[nIndex].SubItems[3].Text = strText;
            }
        }

        void SaveDirListingsDoneCallback()
        {
            if (InvokeRequired) { Invoke(new SaveDirListingsDoneDelegate(SaveDirListingsDoneCallback)); return; }

            if (m_saveDirListings.FilesWritten > 0)
            {
                m_bBrowseLoaded = false;
                RestartTreeTimer();
            }

            MessageBox.Show("Completed. " + m_saveDirListings.FilesWritten + " files written.               ", "Save Directory Listings");
            m_saveDirListings = null;
        }

        private void DoSaveDirListings()
        {
            if (m_saveDirListings != null)
            {
                MessageBox.Show("Already in progress.                       ", "Save Directory Listings");
                return;
            }

            m_saveDirListings = new SaveDirListings(form_lvVolumesMain.Items,
                new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback),
                new SaveDirListingsDoneDelegate(SaveDirListingsDoneCallback));
            m_saveDirListings.DoThreadFactory();
        }
    }
}
