using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Drawing;

namespace SearchDirLists
{
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
            long m_Size;
            bool m_bValid = false;

            internal FileAttributes Attributes { get { return m_Attributes; } }
            internal DateTime CreationTimeUtc { get { return m_CreationTimeUtc; } }
            internal DateTime LastAccessTimeUtc { get { return m_LastAccessTimeUtc; } }
            internal DateTime LastWriteTimeUtc { get { return m_LastWriteTimeUtc; } }
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

    static class ExtensionMethods
    {
        internal static bool IsChildOf(this TreeNode child, TreeNode treeNode)
        {
            if (child.Level <= treeNode.Level)
            {
                return false;
            }

            TreeNode parentNode = child.Parent;

            while (parentNode != null)
            {
                if (parentNode == treeNode)
                {
                    return true;
                }

                parentNode = parentNode.Parent;
            }

            return false;
        }

        internal static TreeNode Root(this TreeNode treeNode)
        {
            TreeNode nodeParent = treeNode;

            while (nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            return nodeParent;
        }

        internal static Rectangle Scale(this Rectangle rc_in, SizeF scale)
        {
            RectangleF rc = rc_in;

            rc.X *= scale.Width;
            rc.Y *= scale.Height;
            rc.Width *= scale.Width;
            rc.Height *= scale.Height;
            return Rectangle.Ceiling(rc);
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
        internal const String m_str_IGNORE_LIST_HEADER = m_str_HEADER + " IGNORE LIST";
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
        static double m_nLastAssertLoc = -1;
        static DateTime m_dtLastAssert = DateTime.MinValue;

        protected Utilities()
        {
            Utilities.Assert(1303.43101, m_MessageboxCallback_ != null);

            m_MessageboxCallback = m_MessageboxCallback_;
        }

        internal static bool Assert(double nLocation, bool bCondition, String strError_in = null)
        {
            if (bCondition) return true;

            bool bShowCustomDialog = true;
            String strError = strError_in ?? "Assertion failed at location " + nLocation + ".";

            if ((m_nLastAssertLoc == nLocation) && ((DateTime.Now - m_dtLastAssert).Seconds < 1))
            {
                return false;
            }

#if (DEBUG)
            Console.WriteLine(strError);

            bShowCustomDialog = (Trace.Listeners.Cast<TraceListener>().Any(i => i is DefaultTraceListener) == false);
            
            if (bShowCustomDialog == false)
            {
                Debug.Assert(false, strError);
            }
#endif
            if (bShowCustomDialog)
            {
                m_MessageboxCallback_(strError + " Please discuss this bug at http://sourceforge.net/projects/searchdirlists/.", "SearchDirLists Assertion Failure");
            }

            m_nLastAssertLoc = nLocation;
            m_dtLastAssert = DateTime.Now;
            return false;
        }

        internal static object CheckAndInvoke(Control control, Delegate action, object[] args = null)
        {
            if (control.InvokeRequired)
            {
                return control.Invoke(action);
            }
            else
            {
                if (action is Action)
                {
                    ((Action)action)();
                }
                else if (action is BoolAction)
                {
                    return ((BoolAction)action)();
                }
                {
                    return action.DynamicInvoke(args);     // late-bound and slow
                }
            }
        }

        internal static String CheckNTFS_chars(ref String strFile, bool bFile = false)
        {
            char[] arrChar = bFile ? Path.GetInvalidFileNameChars() : Path.GetInvalidPathChars();
            int nIx = -1;

            if ((nIx = strFile.IndexOfAny(arrChar)) > -1)
            {
                String strRet = "NTFS ASCII " + ((int)strFile[nIx]).ToString();

                strFile = strFile.Replace("\n", "").Replace("\r", "").Replace("\t", "");    // program-incompatible
                return strRet;
            }
            else
            {
                return null;
            }
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
                            Utilities.Assert(1303.4306, nLineNo == 1);
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
                            Utilities.Assert(1303.4307, nLineNo == 4);
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
                            Utilities.Assert(1303.4308, nLineNo == 16);
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
                            Utilities.Assert(1303.4309, false);        // all that's left is directories
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
                    Utilities.Assert(1303.4301, strDirName == null);
                }
                else
                {
                    strPath = strPath.TrimEnd(Path.DirectorySeparatorChar);
                    Utilities.Assert(1303.4302, StrValid(strDirName));
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
                    m_MessageboxCallback_("Error in Source path.".PadRight(100), "Save Directory Listing");
                    return false;
                }
            }

            if (StrValid(strSaveAs))
            {
                strSaveAs = Path.GetFullPath(strSaveAs.Trim());

                if (FormatPath(ref strSaveAs, bFailOnDirectory) == false)
                {
                    m_MessageboxCallback_("Error in Save filename.".PadRight(100), "Save Directory Listing");
                    return false;
                }
            }

            return true;
        }

        internal static bool LV_VolumesItemCanLoad(LVvolStrings lvStrings)
        {
            return (LV_VolumesItemInclude(lvStrings) &&
                ((Utilities.m_str_USING_FILE + Utilities.m_str_SAVED).Contains(lvStrings.Status)));
        }

        internal static bool LV_VolumesItemInclude(LVvolStrings lvStrings)
        {
            return (lvStrings.Include == "Yes");
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

        internal static String FormatSize(String in_str, bool bBytes = false)
        {
            return FormatSize(ulong.Parse(in_str), bBytes);
        }

        internal static String FormatSize(long nLength, bool bBytes = false, bool bNoDecimal = false)
        {
            return FormatSize((ulong)nLength, bBytes, bNoDecimal);
        }

        internal static String FormatSize(ulong nLength, bool bBytes = false, bool bNoDecimal = false)
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
                return strSz + (bBytes ? (" (" + nLength.ToString("###,###,###,###,###") + " bytes)") : String.Empty);
            }
            else
            {
                return "0 bytes";
            }
        }

        internal static String FormatString(String strDir = null, String strFile = null, DateTime? dtCreated = null, DateTime? dtModified = null, String strAttributes = null, long nLength = -1, String strError1 = null, String strError2 = null, int? nHeader = null)
        {
            String strLength = null;
            String strCreated = null;
            String strModified = null;

            if (nLength > -1)
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
                Utilities.Assert(1303.4303, nHeader is int);

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
                Utilities.Assert(1303.4304, StrValid(strDir) || StrValid(strFile));
                bDbgCheck = true;
            }

            String strRet = (strDir + '\t' + strFile + '\t' + strCreated + '\t' + strModified + '\t' + strAttributes + '\t' + strLength + '\t' + strError1 + '\t' + strError2).TrimEnd();

            if (bDbgCheck)
            {
#if (DEBUG)
                String[] strArray = strRet.Split('\t');
                DateTime dtParse = DateTime.MinValue;

                if (strArray[nColLENGTH_01].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                {
                    Utilities.Assert(1303.4306, false);
                }
#endif
            }

            return strRet;
        }

        internal static String NotNull(String str)
        {
            return str ?? String.Empty;
        }

        internal static void SetMessageBoxDelegate(MessageBoxDelegate messageBoxCallback)
        {
            m_MessageboxCallback_ = messageBoxCallback;
        }

        protected static String StrFile_01(String strFile)
        {
            return Path.Combine(Path.GetDirectoryName(strFile),
                Path.GetFileNameWithoutExtension(strFile) + "_01" + Path.GetExtension(strFile));
        }

        internal static bool StrValid(String str)
        {
            return ((str != null) && (str.Length > 0));
        }

        internal static bool ValidateFile(String strSaveAs)
        {
            if (File.Exists(strSaveAs) == false) return false;

            String[] arrLine = File.ReadLines(strSaveAs).Take(1).ToArray();

            if (arrLine.Length <= 0) return false;

            bool bConvertFile = false;

            if (arrLine[0] == m_str_HEADER_01)
            {
                Console.WriteLine("Converting " + strSaveAs);
                ConvertFile(strSaveAs);
                Console.WriteLine("File converted to " + m_str_HEADER);
                bConvertFile = true;
            }

            String[] arrToken = File.ReadLines(strSaveAs).Take(1).ToArray()[0].Split('\t');

            if (arrToken.Length < 3) return false;
            if (arrToken[2] != m_str_HEADER) return false;

            String[] arrLine_A = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Length)).ToArray();

            if (arrLine_A.Length == 0) return false;

            String[] arrToken_A = arrLine_A[0].Split('\t');

            if (arrToken_A.Length < 3) return false;
            if (arrToken_A[2] != m_str_TOTAL_LENGTH_LOC) return false;

            String strFile_01 = StrFile_01(strSaveAs);

            if (bConvertFile && File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            return true;
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
}
