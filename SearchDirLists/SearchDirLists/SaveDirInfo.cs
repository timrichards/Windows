﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using del = Delimon.Win32.IO;
using System.Diagnostics;
using System.Threading;

namespace SearchDirLists
{
    class Utilities
    {
        public const String m_str_HEADER = "SearchDirLists 0.1";
        public const String m_str_START = m_str_HEADER + " START";
        public const String m_str_END = m_str_HEADER + " END";
        public const String m_str_ERRORS_LOC = m_str_HEADER + " ERRORS";
        public const String m_str_TOTAL_LENGTH_LOC = m_str_HEADER + " LENGTH";
        public const String m_str_DRIVE = m_str_HEADER + " DRIVE";
        public const String m_str_VOLUME_LIST_HEADER = m_str_HEADER + " VOLUME LIST";

        public const String m_str_USING_FILE = "Using file.";
        public const String m_str_SAVED = "Saved.";

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

            double nG = nLength / 1024.0 / 1024 / 1024 - .05;
            double nM = nLength / 1024.0 / 1024 - .05;
            double nK = nLength / 1024.0 - .05;     // Windows Explorer seems to not round
            String strFormat = "###,##0.0";
            String strSz = "";

            if (((int)nG) > 0) strSz = nG.ToString(strFormat) + " GB";
            else if (((int)nM) > 0) strSz = nM.ToString(strFormat) + " MB";
            else if (((int)nK) > 0) strSz = nK.ToString(strFormat) + " KB";
            else strSz = "1 KB";                    // Windows Explorer mins at 1K

            return strSz + (bBytes ? (" (" + nLength.ToString("###,###,###,###,###") + " bytes)") : "");
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

    delegate void SavePathInfoStatusDelegate(int nIndex, String strText);
    delegate void SavePathInfoDoneDelegate();

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
            fs.WriteLine(m_str_HEADER);
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

        public String FormatString(String strDir = "", String strFile = "", DateTime? dtCreated = null, DateTime? dtModified = null, String strAttributes = "", long nLength = 0, String strError1 = "", String strError2 = "")
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
                return "0" + "\t" + "1" + "\t" + "2" + "\t" + "3" + "\t" + "4" + "\t" + "5" + "\t" + "6" + "\t" + "7" + "\n"
                     + "Dir" + "\t" + "File" + "\t" + "Created" + "\t" + "Modded" + "\t" + "Attrib" + "\t" + "Length" + "\t" + "Error1" + "\t" + "Error2";
            }

            if ((strDir.TrimEnd() != strDir) || (strFile.TrimEnd() != strFile))
            {
                strError1 += " Trailing whitespace";
                strError1.Trim();
            }

            return (strDir + "\t" + strFile + "\t" + strCreated + "\t" + strModified + "\t" + strAttributes + "\t" + strLength + "\t" + strError1 + "\t" + strError2).TrimEnd();
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
                fs.WriteLine(m_str_START + " " + DateTime.Now.ToString());
                fs.WriteLine(FormatString());
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

            form_lv_Volumes.Items[nIndex].SubItems[3].Text = strText;
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

            SavePathInfo savePathInfo = new SavePathInfo(form_lv_Volumes.Items,
                new SavePathInfoStatusDelegate(SavePathInfoStatusCallback),
                new SavePathInfoDoneDelegate(SavePathInfoDoneCallback));

            m_bThreadingSavePath = true;

            new Thread(new ThreadStart(savePathInfo.Go)).Start();
        }
    }
}
