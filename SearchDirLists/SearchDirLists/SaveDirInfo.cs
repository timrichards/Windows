using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using del = Delimon.Win32.IO;
using System.Diagnostics;
using System.Threading;
using System.IO.Compression;
using System.Text;

namespace SearchDirLists
{
    delegate void SavePathInfoStatusDelegate(int nIndex, String strText);
    delegate void SavePathInfoDoneDelegate();

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

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
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

        public static String FormatSize(String in_str, bool bBytes = false)
        {
            long nLength = 0;

            Debug.Assert(long.TryParse(in_str, out nLength));
            return FormatSize(nLength, bBytes);
        }

        public static String FormatSize(long nLength, bool bBytes = false)
        {
            double nT = nLength / 1024.0 / 1024.0 / 1024 / 1024 - .05;
            double nG = nLength / 1024.0 / 1024 / 1024 - .05;
            double nM = nLength / 1024.0 / 1024 - .05;
            double nK = nLength / 1024.0 - .05;     // Windows Explorer seems to not round
            String strFormat = "###,##0.0";
            String strSz = "";

            if (((int)nT) > 0) strSz = nT.ToString(strFormat) + " TB";
            else if (((int)nG) > 0) strSz = nG.ToString(strFormat) + " GB";
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

        public static String FormatString(String strDir = "", String strFile = "", DateTime? dtCreated = null, DateTime? dtModified = null, String strAttributes = "", long nLength = 0, String strError1 = "", String strError2 = "", int? nHeader = null)
        {
            String strLength = "";
            String strCreated = "";
            String strModified = "";

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

        protected const String m_strLINETYPE_Version = "V";
        protected const String m_strLINETYPE_Nickname = "N";
        protected const String m_strLINETYPE_Path = "P";
        protected const String m_strLINETYPE_DriveInfo = "I";
        protected const String m_strLINETYPE_Comment = "C";
        protected const String m_strLINETYPE_Start = "S";
        protected const String m_strLINETYPE_Directory = "D";
        protected const String m_strLINETYPE_File = "F";
        protected const String m_strLINETYPE_End = "E";
        protected const String m_strLINETYPE_Blank = "B";
        protected const String m_strLINETYPE_Error_Dir = "R";
        protected const String m_strLINETYPE_Error_File = "r";
        protected const String m_strLINETYPE_Length = "L";

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
            String strFile_01 = Path.GetDirectoryName(strFile) + Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(strFile) + "_01" + Path.GetExtension(strFile);

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
                        else if (strLine == "")
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
                            String strTab = "";
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
        String m_strVolumeName = "";
        String m_strPath = "";
        String m_strSaveAs = "";
        String m_strStatus = "";
        String m_strInclude = "";

        public String VolumeName { get { return m_strVolumeName; } }
        public String Path { get { return m_strPath; } }
        public String SaveAs { get { return m_strSaveAs; } }
        public String Status { get { return m_strStatus; } }
        public String Include { get { return m_strInclude; } }

        public LVvolStrings(ListViewItem lvItem)
        {
            m_strVolumeName = lvItem.SubItems[0].Text;
            m_strPath = lvItem.SubItems[1].Text;
            m_strSaveAs = lvItem.SubItems[2].Text;
            m_strStatus = lvItem.SubItems[3].Text;
            m_strInclude = lvItem.SubItems[4].Text;
        }
    }

    class SavePathInfo : Utilities
    {
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();
        long m_nTotalLength = 0;
        List<String> m_list_Errors = new List<string>();
        SavePathInfoStatusDelegate m_callbackStatus = null;
        SavePathInfoDoneDelegate m_callbackDone = null;

        public SavePathInfo(ListView.ListViewItemCollection lvItems,
            SavePathInfoStatusDelegate callbackStatus, SavePathInfoDoneDelegate callbackDone)
        {
            foreach (ListViewItem lvItem in lvItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_callbackStatus = callbackStatus;
            m_callbackDone = callbackDone;
        }

        private void WriteHeader(TextWriter fs, String strVolumeName, String strPath)
        {
            fs.WriteLine(m_str_HEADER_01);
            // assume SaveFields() by caller because SaveFields() has already prompted user
            fs.WriteLine(strVolumeName);
            fs.WriteLine(strPath);

            DriveInfo driveInfo = new DriveInfo(strPath.Substring(0, strPath.IndexOf(Path.DirectorySeparatorChar)));

            fs.WriteLine(m_str_DRIVE);
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
            bool bFileOK = true;
            String strFilenameCheck = strFile.Replace(":" + Path.DirectorySeparatorChar, "");
            Char chrErrorMessage = ' ';

            if (strFilenameCheck.Contains(chrErrorMessage = ':'))
            {
                bFileOK = false;
            }
            else if (strFilenameCheck.Contains(chrErrorMessage = '?'))
            {
                bFileOK = false;
            }

            else if (strFilenameCheck.Contains(chrErrorMessage = '"'))
            {
                bFileOK = false;
            }

            if (bFileOK == false)
            {
                String strErrorFile = "";
                String strErrorDir = "";

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

        // http://msdn.microsoft.com/en-us/library/bb513869.aspx
        public void TraverseTree(TextWriter fs, string root)
        {
            // Data structure to hold names of subfolders to be 
            // examined for files.
            Stack<string> dirs = new Stack<string>(64);
            if (!del.Directory.Exists(root))
            {
                throw new ArgumentException();
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs = null;

                if (CheckNTFS_chars(currentDir, false) == false)
                {
                    continue;
                }

                try
                {
                    subDirs = del.Directory.GetDirectories(currentDir);
                }
                // An UnauthorizedAccessException exception will be thrown if we do not have 
                // discovery permission on a folder or file. It may or may not be acceptable  
                // to ignore the exception and continue enumerating the remaining files and  
                // folders. It is also possible (but unlikely) that a DirectoryNotFound exception  
                // will be raised. This will happen if currentDir has been deleted by 
                // another application or thread after our call to del.Directory.Exists. The  
                // choice of which exceptions to catch depends entirely on the specific task  
                // you are intending to perform and also on how much you know with certainty  
                // about the systems on which this code will run.

                catch (PathTooLongException)
                {
                    String strOut = FormatString(strDir: currentDir, strError1: "PathTooLongException");
                    m_list_Errors.Add(strOut);
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

                string[] files = null;

                try
                {
                    files = del.Directory.GetFiles(currentDir);
                }

                catch (UnauthorizedAccessException e)
                {

                    m_list_Errors.Add(FormatString(strDir: currentDir, strFile: "GetFiles()", strError1: "UnauthorizedAccessException", strError2: e.Message));
                    continue;
                }

                catch (System.IO.DirectoryNotFoundException e)
                {
                    m_list_Errors.Add(FormatString(strDir: currentDir, strFile: "GetFiles()", strError1: "DirectoryNotFoundException", strError2: e.Message));
                    continue;
                }

                long nDirLength = 0;

                // Perform the required action on each file here. 
                // Modify this block to perform your required task. 
                foreach (string strFile in files)
                {
                    String strOut = "";

                    if (CheckNTFS_chars(strFile) == false)
                    {
                        continue;
                    }

                    try
                    {
                        // Perform whatever action is required in your scenario.
                        del.FileInfo fi = new del.FileInfo(strFile);

                        String strError1 = "";
                        String strError2 = "";

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
                    Console.Write(".");
                }

                {
                    del.DirectoryInfo di = new del.DirectoryInfo(currentDir);

                    String strError1 = "";
                    String strError2 = "";

                    if (currentDir.Length > 240)
                    {
                        strError1 = "Path Length";
                        strError2 = currentDir.Length.ToString();
                    }

                    fs.WriteLine(FormatString(strDir: currentDir, dtCreated: di.CreationTime, strAttributes: di.Attributes.ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: strError2));
                    Console.WriteLine();
                }

                // Push the subdirectories onto the stack for traversal. 
                // This could also be done before handing the files. 
                foreach (string str in subDirs)
                    dirs.Push(str);
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

            Console.Clear();

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

            //String strSavePathInfo = form_btn_SavePathInfo.Text;
            String strPathOrig = Directory.GetCurrentDirectory();

            //form_btn_SavePathInfo.Text = "Running Task...";
            //this.Enabled = false;

            m_list_Errors.Clear();
            m_nTotalLength = 0;

            using (TextWriter fs = File.CreateText(strSaveAs))
            {
                WriteHeader(fs, strVolumeName, strPath);
                fs.WriteLine();
                fs.WriteLine(FormatString());
                fs.WriteLine(m_str_START + " " + DateTime.Now.ToString());
                TraverseTree(fs, strPath);
                fs.WriteLine(m_str_END + " " + DateTime.Now.ToString());
                fs.WriteLine();
                fs.WriteLine(m_str_ERRORS_LOC);

                foreach (String strError in m_list_Errors)
                {
                    fs.WriteLine(strError);
                }

                fs.WriteLine();
                fs.WriteLine(FormatString(strDir: m_str_TOTAL_LENGTH_LOC, nLength: m_nTotalLength));
            }

            Directory.SetCurrentDirectory(strPathOrig);
            m_callbackDone();
            //form_btn_SavePathInfo.Text = strSavePathInfo;
            //this.Enabled = true;
            return true;
        }

        public void Go()
        {
            int nIndex = -1;

            foreach (LVvolStrings lvStrings in m_list_lvVolStrings)
            {
                ++nIndex;

                if ((m_str_USING_FILE + m_str_SAVED).Contains(lvStrings.Status))
                {
                    continue;
                }

                m_callbackStatus(nIndex, "Saving...");

                if (SaveDirectoryListing(lvStrings))
                {
                    m_callbackStatus(nIndex, m_str_SAVED);
                    MessageBox.Show("Completed.                           ", "Save Path Info");
                }
                else
                {
                    m_callbackStatus(nIndex, "Not saved.");
                }
            }
        }
    }

    public partial class Form1 : Form
    {
        private bool m_bThreadingSavePath = false;

        void SavePathInfoStatusCallback(int nIndex, String strText)
        {
            if (InvokeRequired) { Invoke(new SavePathInfoStatusDelegate(SavePathInfoStatusCallback), new object[] { nIndex, strText }); return; }

            form_LV_VolumesMain.Items[nIndex].SubItems[3].Text = strText;
        }

        void SavePathInfoDoneCallback()
        {
            if (InvokeRequired) { Invoke(new SavePathInfoDoneDelegate(SavePathInfoDoneCallback)); return; }

            m_bThreadingSavePath = false;
            m_bBrowseLoaded = false;
        }

        private void DoSavePathInfo()
        {
            if (m_bThreadingSavePath)
            {
                MessageBox.Show("Already in progress.                       ", "Save Path Info");
                return;
            }

            SavePathInfo savePathInfo = new SavePathInfo(form_LV_VolumesMain.Items,
                new SavePathInfoStatusDelegate(SavePathInfoStatusCallback),
                new SavePathInfoDoneDelegate(SavePathInfoDoneCallback));

            m_bThreadingSavePath = true;

            new Thread(new ThreadStart(savePathInfo.Go)).Start();
        }
    }
}
