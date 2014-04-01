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

namespace SearchDirLists
{
    delegate void SaveDirListingsStatusDelegate(int nIndex, String strText);
    delegate void SaveDirListingsDoneDelegate(int nFilesWritten);

    class Win32
    {
        internal enum FileAttributes
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
            internal FileAttributes dwFileAttributes;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            internal int nFileSizeHigh;
            internal int nFileSizeLow;
            internal int dwReserved0;
            internal int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string cFileName;
            // not using this
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal string cAlternate;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        //Closes a file search handle opened by the FindFirstFile, FindFirstFileEx, FindFirstFileNameW, FindFirstFileNameTransactedW, FindFirstFileTransacted, FindFirstStreamTransactedW, or FindFirstStreamW functions.
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FindClose(IntPtr hFindFile);

        public class FileInfo
        {
            bool m_bValid = false;
            public bool IsValid { get { return m_bValid; } }

            String m_strName = "";
            public String Name { get { return m_strName; } }

            DateTime m_dtCreationTime = DateTime.MinValue;
            public DateTime CreationTime { get { return m_dtCreationTime; } }

            DateTime m_dtLastWriteTime = DateTime.MinValue;
            public DateTime LastWriteTime { get { return m_dtCreationTime; } }

            System.IO.FileAttributes m_fileAttributes;
            public System.IO.FileAttributes Attributes { get { return m_fileAttributes; } }

            long m_nLength = 0;
            public long Length { get { return m_nLength; } }

            DateTime FromFileTime(System.Runtime.InteropServices.ComTypes.FILETIME time)
            {
                return DateTime.FromFileTimeUtc((time.dwHighDateTime << 32) + (long)(uint)time.dwLowDateTime);
            }

            static object FileInfo_lock = new object();

            public void FromFindData(WIN32_FIND_DATAW ffd)
            {
                m_strName = ffd.cFileName;
                m_dtCreationTime = FromFileTime(ffd.ftCreationTime);
                m_dtLastWriteTime = FromFileTime(ffd.ftLastWriteTime);
                m_fileAttributes = (System.IO.FileAttributes)ffd.dwFileAttributes;
                m_nLength = (ffd.nFileSizeHigh << 32) + ffd.nFileSizeLow;
                m_bValid = true;
            }

            public FileInfo(WIN32_FIND_DATAW ffd)
            {
                FromFindData(ffd);
            }

            public FileInfo(String strFile)
            {
                lock (FileInfo_lock)
                {
                    WIN32_FIND_DATAW ffd = new WIN32_FIND_DATAW();
                    IntPtr handle = FindFirstFileW(@"\\?\" + strFile, out ffd);

                    if (handle != (IntPtr) (-1))
                    {
                        FromFindData(ffd);
                    }
                }
            }
        }

        static object GetSubItems_lock = new object();

        public static void GetDirectory(String strDir, out Win32.FileInfo di, out List<String> listDirs, out List<String> listFiles)
        {
            lock (GetSubItems_lock)
            {
                WIN32_FIND_DATAW ffd = new WIN32_FIND_DATAW();
                IntPtr handle = FindFirstFileW(@"\\?\" + strDir + @"\*", out ffd);
                di = null;
                listDirs = null;
                listFiles = null;

                if (handle == (IntPtr) (-1))
                {
                    return;
                }

                di = new Win32.FileInfo(ffd);
                listDirs = new List<string>();
                listFiles = new List<string>();

                do
                {
                    if ("..".Contains(ffd.cFileName))
                    {
                        continue;
                    }

                    if ((ffd.dwFileAttributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) != 0)
                    {
                        listDirs.Add(Path.Combine(strDir, ffd.cFileName));
                    }
                    else
                    {
                        listFiles.Add(Path.Combine(strDir, ffd.cFileName));
                    }
                }
                while (FindNextFileW(handle, out ffd));

                FindClose(handle);
                listDirs.Sort();
                listFiles.Sort();
            }
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
        public const String m_str_USING_FILE = "Using file.";
        public const String m_str_SAVED = "Saved.";
        public const int nColLENGTH = 7;
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


        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
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

        public static string Unzip(byte[] bytes)
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

            if ((strDirName == null) || Directory.Exists(strDirName))
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
                    Debug.Assert(strDirName != null);
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
            if (strPath.Length > 0)
            {
                strPath += Path.DirectorySeparatorChar;

                if (FormatPath(ref strPath, bFailOnDirectory) == false)
                {
                    MessageBox.Show("Error in Source path.                   ", "Save Directory Listing");
                    return false;
                }
            }

            if (strSaveAs.Length > 0)
            {
                strSaveAs = Path.GetFullPath(strSaveAs.Trim());

                if (FormatPath(ref strSaveAs, bFailOnDirectory) == false)
                {
                    MessageBox.Show("Error in Save filename.                  ", "Save Directory Listing");
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

        public static String FormatString(String strDir = "", String strFile = "", DateTime? dtCreated = null, DateTime? dtModified = null, String strAttributes = null, long nLength = 0, String strError1 = null, String strError2 = null, int? nHeader = null)
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

            if ((strDir + strFile + strCreated + strModified + strAttributes + strLength + strError1 + strError2).Length <= 0)
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

            if ((strDir.TrimEnd() != strDir) || (strFile.TrimEnd() != strFile))
            {
                strError1 += " Trailing whitespace";
                strError1.Trim();
                Debug.Assert(strDir.Length > 0 || strFile.Length > 0);
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
            String strLine_out = strLineType  + "\t" + nLineNo;

            if (strLine_in != null)
            {
                strLine_out += '\t' + strLine_in;
            }

            return strLine_out;
        }

        public static void ConvertFile(String strFile)
        {
            String strFile_01 = Path.Combine(Path.GetDirectoryName(strFile),
                Path.GetFileNameWithoutExtension(strFile) + "_01" + Path.GetExtension(strFile));

            if (File.Exists(strFile_01) == false)
            {
                File.Move(strFile, strFile_01);
            }

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
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Length, nLineNo, m_str_TOTAL_LENGTH_LOC));
                            continue;
                        }

                        String[] strArray = strLine.Split('\t');
                        String strDir = strArray[0];

                        if (strDir.Length <= 0)
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
                        file_out.WriteLine(FormatLine(bAtErrors ? m_strLINETYPE_Error_Dir : m_strLINETYPE_Directory, nLineNo, strLine));
                    }
                }
            }
        }
    }

    class LVvolStrings
    {
        String m_strVolumeName = null;
        String m_strPath = null;
        String m_strSaveAs = null;
        String m_strStatus = null;
        String m_strInclude = null;
        String m_strVolumeGroup = null;
        public String VolumeName { get { return m_strVolumeName; } }
        public String Path { get { return m_strPath; } }
        public String SaveAs { get { return m_strSaveAs; } }
        public String Status { get { return m_strStatus; } }
        public String Include { get { return m_strInclude; } }
        public String VolumeGroup { get { return m_strVolumeGroup; } }

        public LVvolStrings(ListViewItem lvItem)
        {
            m_strVolumeName = lvItem.SubItems[0].Text;
            m_strPath = lvItem.SubItems[1].Text;
            m_strSaveAs = lvItem.SubItems[2].Text;
            m_strStatus = lvItem.SubItems[3].Text;
            m_strInclude = lvItem.SubItems[4].Text;

            if (lvItem.SubItems.Count > 5)
            {
                m_strVolumeGroup = lvItem.SubItems[5].Text;
            }
        }
    }

    class SaveDirListings : Utilities
    {
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();
        long m_nTotalLength = 0;
        List<String> m_list_Errors = new List<string>();
        SaveDirListingsStatusDelegate m_callbackStatus = null;
        SaveDirListingsDoneDelegate m_callbackDone = null;
        System.Threading.Timer m_timerStatus = null;
        int m_nVolIx = -1;
        int m_nStatusCount = 0;

        void SaveDirListings_TimerCallback(object state)
        {
            m_callbackStatus(m_nVolIx, "Saving " + m_nStatusCount.ToString("###,###,###"));
        }

        public SaveDirListings(ListView.ListViewItemCollection lvItems,
            SaveDirListingsStatusDelegate callbackStatus, SaveDirListingsDoneDelegate callbackDone)
        {
            foreach (ListViewItem lvItem in lvItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_callbackStatus = callbackStatus;
            m_callbackDone = callbackDone;
            m_timerStatus = new System.Threading.Timer(new TimerCallback(SaveDirListings_TimerCallback), null, 1000, 1000);
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
            String strFilenameCheck = strFile.Replace(":" + Path.DirectorySeparatorChar, "");
            Char chrErrorMessage = ' ';

            if ((strFilenameCheck.Contains(chrErrorMessage = ':'))
                || (strFilenameCheck.Contains(chrErrorMessage = '?'))
                || (strFilenameCheck.Contains(chrErrorMessage = '"')))
            {
                String strErrorFile = null;
                String strErrorDir = null;

                if (bFile)
                {
                    strErrorFile = strFile;
                }
                else
                {
                    strErrorDir = strFile;
                }

                m_list_Errors.Add(FormatString(strFile: strErrorFile, strDir: strErrorDir, strError1: "NTFS Char", strError2: chrErrorMessage.ToString()));
                return false;
            }
            else
            {
                return true;
            }
        }

        public void TraverseTree(TextWriter fs, string root)
        {
            Stack<string> stackDirs = new Stack<string>(64);

            stackDirs.Push(root);

            while (stackDirs.Count > 0)
            {
                ++m_nStatusCount;

                string currentDir = stackDirs.Pop();

                if (CheckNTFS_chars(currentDir, false) == false)
                {
                    continue;
                }

                Win32.FileInfo di = null;
                List<String> listSubDirs = null;
                List<String> listFiles = null;

                try
                {
                    Win32.GetDirectory(currentDir, out di, out listSubDirs, out listFiles);
                }
                catch (PathTooLongException)
                {
                    m_list_Errors.Add(FormatString(strDir: currentDir, strError1: "PathTooLongException"));
                    continue;
                }
                catch (ArgumentException e)
                {
                    m_list_Errors.Add(FormatString(strDir: currentDir, strError1: "ArgumentException", strError2: e.Message));
                    continue;
                }
                catch (UnauthorizedAccessException e)
                {
                    m_list_Errors.Add(FormatString(strDir: currentDir, strError1: "UnauthorizedAccessException", strError2: e.Message));
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    m_list_Errors.Add(FormatString(strDir: currentDir, strError1: "DirectoryNotFoundException", strError2: e.Message));
                    continue;
                }
                catch (Exception e)
                {
                    m_list_Errors.Add(FormatString(strDir: currentDir, strError1: "Exception", strError2: e.Message));
                    continue;
                }

                long nDirLength = 0;

                // Perform the required action on each file here. 
                // Modify this block to perform your required task. 
                foreach (string strFile in listFiles)
                {
                    String strOut = null;

                    if (CheckNTFS_chars(strFile) == false)
                    {
                        Debug.Assert(false);
                        continue;
                    }

                    try
                    {
                        Win32.FileInfo fi = new Win32.FileInfo(strFile);

                        Debug.Assert(fi.IsValid);

                        String strError1 = null;
                        String strError2 = null;

                        if (strFile.Length > 260)
                        {
                            strError1 = "Path Length";
                            strError2 = strFile.Length.ToString();
                        }

                        strOut = FormatString(strFile: fi.Name, dtCreated: fi.CreationTime, strAttributes: fi.Attributes.ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Length, strError1: strError1, strError2: strError2);
                        m_nTotalLength += fi.Length;
                        nDirLength += fi.Length;
                    }
                    catch (PathTooLongException)
                    {
                        strOut = FormatString(strFile: strFile, strError1: "PathTooLongException");
                        m_list_Errors.Add(strOut);
                        continue;
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        // If file was deleted by a separate application 
                        //  or thread since the call to TraverseTree() 
                        // then just continue.
                        strOut = FormatString(strFile: strFile, strError1: "FileNotFoundException");
                        m_list_Errors.Add(strOut);
                        continue;
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
                    fs.WriteLine(FormatString(strDir: currentDir, dtCreated: di.CreationTime, strAttributes: di.Attributes.ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: strError2));
                    Console.WriteLine();
                }

                // Push the subdirectories onto the stack for traversal. 
                // This could also be done before handing the files. 
                foreach (string str in listSubDirs)
                {
                    stackDirs.Push(str);
                }
            }
        }

        private bool SaveDirectoryListing(LVvolStrings lvStrings)
        {
            String strVolumeName = lvStrings.VolumeName;
            String strPath = lvStrings.Path;
            String strSaveAs = lvStrings.SaveAs;

            if (FormatPath(ref strPath, ref strSaveAs) == false)
            {
                return false;
            }

            if (Directory.Exists(strPath) == false)
            {
                MessageBox.Show("Source Path does not exist.                  ", "Save Directory Listing");
                return false;
            }

            if (strSaveAs.Length <= 0)
            {
                MessageBox.Show("Must specify save filename.                  ", "Save Directory Listing");
                return false;
            }

            String strPathOrig = Directory.GetCurrentDirectory();

            m_list_Errors.Clear();
            m_nTotalLength = 0;

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
                fs.WriteLine(FormatString(strDir: m_str_TOTAL_LENGTH_LOC_01, nLength: m_nTotalLength));
            }

            Directory.SetCurrentDirectory(strPathOrig);
            return true;
        }

        public void Go()
        {
            int nFilesWritten = 0;

            foreach (LVvolStrings lvStrings in m_list_lvVolStrings)
            {
                ++m_nVolIx;

                if ((m_str_USING_FILE + m_str_SAVED).Contains(lvStrings.Status))
                {
                    continue;
                }

                m_callbackStatus(m_nVolIx, "Saving...");

                if (SaveDirectoryListing(lvStrings))
                {
                    m_callbackStatus(m_nVolIx, m_str_SAVED);
                    ++nFilesWritten;
                }
                else
                {
                    m_callbackStatus(m_nVolIx, "Not saved.");
                }
            }

            m_timerStatus.Dispose();
            m_callbackDone(nFilesWritten);
        }
    }

    public partial class Form1 : Form
    {
        private bool m_bThreadingSaveDirListings = false;

        void SaveDirListingsStatusCallback(int nIndex, String strText)
        {
            if (InvokeRequired) { Invoke(new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback), new object[] { nIndex, strText }); return; }

            form_lvVolumesMain.Items[nIndex].SubItems[3].Text = strText;
        }

        void SaveDirListingsDoneCallback(int nFilesWritten)
        {
            if (InvokeRequired) { Invoke(new SaveDirListingsDoneDelegate(SaveDirListingsDoneCallback), new object[] { nFilesWritten }); return; }

            if (nFilesWritten > 0)
            {
                m_bBrowseLoaded = false;
                DoTree(true);
            }

            MessageBox.Show("Completed. " + nFilesWritten + " files written.               ", "Save Directory Listings");
            m_bThreadingSaveDirListings = false;
        }

        private void DoSaveDirListings()
        {
            if (m_bThreadingSaveDirListings)
            {
                MessageBox.Show("Already in progress.                       ", "Save Directory Listings");
                return;
            }

            SaveDirListings saveDirListings = new SaveDirListings(form_lvVolumesMain.Items,
                new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback),
                new SaveDirListingsDoneDelegate(SaveDirListingsDoneCallback));

            m_bThreadingSaveDirListings = true;

            new Thread(new ThreadStart(saveDirListings.Go)).Start();
        }
    }
}
