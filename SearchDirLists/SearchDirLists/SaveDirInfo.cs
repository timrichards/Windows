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

namespace SearchDirLists
{
    delegate void SaveDirListingsStatusDelegate(int nIndex, String strText = null, bool bSuccess = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0);
    delegate void SaveDirListingsDoneDelegate();

    class Win32
    {
        internal enum W32FileAttributes
        {
            FILE_ATTRIBUTE_READONLY = 1,
            FILE_ATTRIBUTE_HIDDEN = 2,
            FILE_ATTRIBUTE_SYSTEM = 4,
            FILE_ATTRIBUTE_DIRECTORY = 16,
            FILE_ATTRIBUTE_ARCHIVE = 32,
            FILE_ATTRIBUTE_DEVICE = 64,
            FILE_ATTRIBUTE_NORMAL = 128,
            FILE_ATTRIBUTE_TEMPORARY = 256,
            FILE_ATTRIBUTE_SPARSE_FILE = 512,
            FILE_ATTRIBUTE_REPARSE_POINT = 1024,
            FILE_ATTRIBUTE_COMPRESSED = 2048,
            FILE_ATTRIBUTE_OFFLINE = 4096,
            FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 8192,
            FILE_ATTRIBUTE_ENCRYPTED = 16384,
            FILE_ATTRIBUTE_VIRTUAL = 65536
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WIN32_FIND_DATAW
        {
            internal W32FileAttributes dwFileAttributes;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            internal int nFileSizeHigh;
            internal int nFileSizeLow;
            internal int dwReserved0;
            internal int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal String cFileName;
            // not using this
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal String cAlternate;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr FindFirstFileW(String lpFileName, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FindClose(IntPtr hFindFile);

        public class FileInfo
        {
            bool m_bValid = false;
            public bool IsValid { get { return m_bValid; } }

            String m_strName = null;
            public String Name { get { return m_strName; } }

            DateTime? m_dtCreationTime = DateTime.MinValue;
            public DateTime? CreationTime { get { return m_dtCreationTime; } }

            DateTime? m_dtLastWriteTime = DateTime.MinValue;
            public DateTime? LastWriteTime { get { return m_dtCreationTime; } }

            FileAttributes? m_fileAttributes;
            public FileAttributes? Attributes { get { return m_fileAttributes; } }

            long m_nLength = 0;
            public long Length { get { return m_nLength; } }

            DateTime GetFileTime(System.Runtime.InteropServices.ComTypes.FILETIME time)
            {
                long highBits = time.dwHighDateTime;

                highBits = highBits << 32;
                return DateTime.FromFileTimeUtc(highBits + (long)(uint)time.dwLowDateTime);
            }

            public void FromFindData(WIN32_FIND_DATAW ffd)
            {
                String P = Path.DirectorySeparatorChar.ToString();
                String PP = P + P;

                m_bValid = false;
                m_strName = String.Empty;
                m_dtCreationTime = null;
                m_dtLastWriteTime = null;
                m_fileAttributes = null;
                m_nLength = 0;
                m_strName = ffd.cFileName.Replace(PP, P).TrimEnd(Path.DirectorySeparatorChar);
                m_dtCreationTime = GetFileTime(ffd.ftCreationTime);
                m_dtLastWriteTime = GetFileTime(ffd.ftLastWriteTime);
                m_fileAttributes = (FileAttributes)ffd.dwFileAttributes;

                long highBits = ffd.nFileSizeHigh;

                highBits = highBits << 32;
                m_nLength = highBits + (long)(uint)ffd.nFileSizeLow;
                m_bValid = true;
            }

            public void FromFile(String strFile)
            {
                WIN32_FIND_DATAW ffd = new WIN32_FIND_DATAW();
                IntPtr handle = FindFirstFileW(@"\\?\" + strFile, out ffd);

                if (handle != (IntPtr)(-1))
                {
                    FromFindData(ffd);
                }
            }

            public FileInfo(WIN32_FIND_DATAW ffd)
            {
                FromFindData(ffd);
            }

            public FileInfo()
            {
                m_bValid = false;
            }
        }

        public static bool GetDirectory(String strDir, ref Win32.FileInfo di, ref List<String> listDirs, ref List<String> listFiles)
        {
            String P = Path.DirectorySeparatorChar.ToString();
            String PP = P + P;
            WIN32_FIND_DATAW ffd;
            IntPtr handle = FindFirstFileW(PP + '?' + P + strDir + P + '*', out ffd);

            if (handle == (IntPtr)(-1))
            {
                return false;
            }

            di.FromFindData(ffd);
            Debug.Assert(di.IsValid);
            listDirs.Clear();
            listFiles.Clear();

            do
            {
                if ("..".Contains(ffd.cFileName))
                {
                    continue;
                }

                if ((ffd.dwFileAttributes & W32FileAttributes.FILE_ATTRIBUTE_DIRECTORY) != 0)
                {
                    if ((ffd.dwFileAttributes & W32FileAttributes.FILE_ATTRIBUTE_REPARSE_POINT) != 0)
                    {
                        const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
                        const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;

                        // stay on source drive. Treat mount points and symlinks as files.
                        if (((ffd.dwReserved0 & IO_REPARSE_TAG_MOUNT_POINT) != 0)
                            || ((ffd.dwReserved0 & IO_REPARSE_TAG_SYMLINK) != 0))
                        {
                            listFiles.Add((strDir + P + ffd.cFileName).Replace(PP, P));        //
                            continue;
                        }
                    }

                    listDirs.Add((strDir + P + ffd.cFileName).Replace(PP, P));                 //
                }
                else
                {
                    listFiles.Add((strDir + P + ffd.cFileName).Replace(PP, P));                // Path.Combine() whines about bad chars in path
                }
            }
            while (FindNextFileW(handle, out ffd));

            FindClose(handle);
            listDirs.Sort();
            listFiles.Sort();
            return true;
        }
    }

    class Utilities
    {
        public const String m_str_HEADER_01 = "SearchDirLists 0.1";
        public const String m_str_START_01 = m_str_HEADER_01 + " START";
        public const String m_str_END_01 = m_str_HEADER_01 + " END";
        public const String m_str_ERRORS_LOC_01 = m_str_HEADER_01 + " ERRORS";
        public const String m_str_TOTAL_LENGTH_LOC_01 = m_str_HEADER_01 + " LENGTH";
        public const String m_str_DRIVE_01 = m_str_HEADER_01 + " DRIVE";
        public const String m_str_VOLUME_LIST_HEADER_01 = m_str_HEADER_01 + " VOLUME LIST";
        public const String m_str_HEADER = "SearchDirLists 0.2";
        public const String m_str_START = m_str_HEADER + " START";
        public const String m_str_END = m_str_HEADER + " END";
        public const String m_str_ERRORS_LOC = m_str_HEADER + " ERRORS";
        public const String m_str_TOTAL_LENGTH_LOC = m_str_HEADER + " LENGTH";
        public const String m_str_DRIVE = m_str_HEADER + " DRIVE";
        public const String m_str_VOLUME_LIST_HEADER = m_str_HEADER + " VOLUME LIST";
        public const String m_str_COPYDIRS_LIST_HEADER = m_str_HEADER + " COPYDIRS LIST";
        public const String m_str_USING_FILE = "Using file.";
        public const String m_str_SAVED = "Saved.";
        public const int nColLENGTH = 7;
        public const int nColLENGTH_01 = 5;
        public const int nColLENGTH_LV = 4;
        public const String m_strLINETYPE_Version = "V";
        public const String m_strLINETYPE_Nickname = "N";
        public const String m_strLINETYPE_Path = "P";
        public const String m_strLINETYPE_DriveInfo = "I";
        public const String m_strLINETYPE_Comment = "C";
        public const String m_strLINETYPE_Start = "S";
        public const String m_strLINETYPE_Directory = "D";
        public const String m_strLINETYPE_File = "F";
        public const String m_strLINETYPE_End = "E";
        public const String m_strLINETYPE_Blank = "B";
        public const String m_strLINETYPE_Error_Dir = "R";
        public const String m_strLINETYPE_Error_File = "r";
        public const String m_strLINETYPE_Length = "L";

        static MessageBoxDelegate m_MessageboxCallback_ = null;
        protected MessageBoxDelegate m_MessageboxCallback = null;

        protected Utilities()
        {
            Debug.Assert(m_MessageboxCallback_ != null);

            m_MessageboxCallback = m_MessageboxCallback_;
        }

        public static void SetMessageBoxDelegate(MessageBoxDelegate messageBoxCallback)
        {
            m_MessageboxCallback_ = messageBoxCallback;
        }

        public static String NotNull(String str)
        {
            return str ?? "";
        }

        public static bool StrValid(String str)
        {
            return ((str != null) && (str.Length > 0));
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(String str)
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

        public static String Unzip(byte[] bytes)
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

        public static bool FormatPath(ref String strPath, bool bFailOnDirectory = true)
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

        public static bool FormatPath(ref String strPath, ref String strSaveAs, bool bFailOnDirectory = true)
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

        public static bool LV_VolumesItemInclude(LVvolStrings lvStrings)
        {
            return (lvStrings.Include == "Yes");
        }

        public static bool LV_VolumesItemCanLoad(LVvolStrings lvStrings)
        {
            return (LV_VolumesItemInclude(lvStrings) &&
                ((Utilities.m_str_USING_FILE + Utilities.m_str_SAVED).Contains(lvStrings.Status)));
        }

        public static String FormatSize(String in_str, bool bBytes = false)
        {
            return FormatSize(long.Parse(in_str), bBytes);
        }

        public static String FormatSize(long nLength, bool bBytes = false, bool bNoDecimal = false)
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

        public static String FormatString(String strDir = null, String strFile = null, DateTime? dtCreated = null, DateTime? dtModified = null, String strAttributes = null, long nLength = 0, String strError1 = null, String strError2 = null, int? nHeader = null)
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

        public static void ConvertFile(String strFile)
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
        public int Index { get { return m_nIndex; } }
        public String VolumeName { get { return m_strVolumeName; } }
        public String StrPath { get { return m_strPath; } }
        public String SaveAs { get { return m_strSaveAs; } }
        public String Status { get { return m_strStatus; } }
        public String Include { get { return m_strInclude; } }
        public String VolumeGroup { get { return m_strVolumeGroup; } }

        public LVvolStrings(ListViewItem lvItem)
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

        public void SetStatus_BadFile(ListView lv)
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

            return nStdDev *= Math.Sign(values[values.Count] - nAvg);
        }
        
        void SaveDirListing_TimerCallback(object state)
        {
            m_listFileDiffs.Add(m_nFilesDiff);

            double nFilesDiff = m_nFilesDiff/StdDevSign(m_listFileDiffs);

            m_nFilesDiff = 0;
            m_statusCallback(m_volStrings.Index, nFilesTotal: m_nFilesTotal, nLengthTotal: m_nLengthTotal, nFilesDiff: nFilesDiff);
        }

        public SaveDirListing(LVvolStrings volStrings,
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

        bool CheckNTFS_chars(String strFile, bool bFile = true)
        {
            char[] arrChar = bFile ? Path.GetInvalidFileNameChars() : Path.GetInvalidPathChars();
            int nIx = -1;
            String strFileCheck = bFile ? strFile.Substring(strFile.LastIndexOf(Path.DirectorySeparatorChar) + 1) : strFile;

            if ((nIx = strFileCheck.IndexOfAny(arrChar)) > -1)
            {
                String strErrorFile = null;
                String strErrorDir = null;

                if (bFile)
                {
                    strErrorFile = strFileCheck;
                    strErrorDir = strFile.Substring(0, strFile.LastIndexOf(Path.DirectorySeparatorChar));
                }
                else
                {
                    strErrorDir = strFile;
                }

                m_list_Errors.Add(FormatString(strFile: strErrorFile, strDir: strErrorDir, strError1: "NTFS Char", strError2: "ASCII " + ((int)strFileCheck[nIx]).ToString()));
                return false;
            }
            else
            {
                return true;
            }
        }

        void TraverseTree(TextWriter fs, String root)
        {
            Stack<String> stackDirs = new Stack<String>(64);
            Win32.FileInfo di = new Win32.FileInfo();
            Win32.FileInfo fi = new Win32.FileInfo();
            List<String> listSubDirs = new List<String>();
            List<String> listFiles = new List<String>();

            stackDirs.Push(root);

            while (stackDirs.Count > 0)
            {
                String currentDir = stackDirs.Pop();

                if (CheckNTFS_chars(currentDir, bFile: false) == false)
                {
                    continue;
                }

                if (Win32.GetDirectory(currentDir, ref di, ref listSubDirs, ref listFiles) == false)
                {
                    m_list_Errors.Add(FormatString(strDir: currentDir,
                        strError1: new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message));
                    continue;
                }

                long nDirLength = 0;
                bool bHasLength = false;

                foreach (String strFile in listFiles)
                {
                    if (CheckNTFS_chars(strFile) == false)
                    {
                        continue;
                    }

                    fi.FromFile(strFile);
                    Debug.Assert(fi.IsValid);

                    m_nLengthTotal += fi.Length;
                    ++m_nFilesTotal;
                    ++m_nFilesDiff;

                    nDirLength += fi.Length;

                    String strError1 = null;
                    String strError2 = null;

                    if (strFile.Length > 260)
                    {
                        strError1 = "Path Length";
                        strError2 = strFile.Length.ToString();
                    }

                    String strOut = FormatString(strFile: fi.Name, dtCreated: fi.CreationTime, strAttributes: fi.Attributes.Value.ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Length, strError1: strError1, strError2: strError2);

                    Debug.Assert(fi.Length >= 0);

                    if (fi.Length > 0)
                    {
                        bHasLength = true;
                    }

                    fs.WriteLine(strOut);
                }

                {
                    String strError1 = null;
                    String strError2 = null;

                    if (currentDir.Length > 240)
                    {
                        strError1 = "Path Length";
                        strError2 = currentDir.Length.ToString();
                    }

                    Debug.Assert(di.IsValid);

                    if (bHasLength)
                    {
                        Debug.Assert(nDirLength > 0);
                    }

                    fs.WriteLine(FormatString(strDir: currentDir, dtCreated: di.CreationTime, strAttributes: di.Attributes.Value.ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: strError2));
                }

                // Push the subdirectories onto the stack for traversal. 
                // This could also be done before handing the files. 
                foreach (String str in listSubDirs)
                {
                    stackDirs.Push(str);
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

        public Thread DoThreadFactory()
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
        public int FilesWritten { get { return m_nFilesWritten; } set { m_nFilesWritten = value; } }

        public SaveDirListings(ListView.ListViewItemCollection lvVolItems,
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

        public void EndThread()
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

        public void DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }
    }

    public partial class Form1 : Form
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
