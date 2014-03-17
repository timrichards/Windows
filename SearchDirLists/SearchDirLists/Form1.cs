using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using del = Delimon.Win32.IO;
using System.Collections;
using System.Diagnostics;
using ListViewEmbeddedControls;
using System.Threading;

namespace SearchDirLists
{
    public partial class Form1 : Form
    {
        private const String m_str_HEADER               = "SearchDirLists 0.1";
        private const String m_str_START                = m_str_HEADER + " START";
        private const String m_str_END                  = m_str_HEADER + " END";
        private const String m_str_ERRORS_LOC           = m_str_HEADER + " ERRORS";
        private const String m_str_TOTAL_LENGTH_LOC     = m_str_HEADER + " LENGTH";
        private const String m_str_DRIVE                = m_str_HEADER + " DRIVE";
        private const String m_str_VOLUME_LIST_HEADER   = m_str_HEADER + " VOLUME LIST";

        private const String m_str_USING_FILE           = "Using file.";
        private const String m_str_SAVED                = "Saved.";

        private String m_strVolumeName = "";
        private String m_strPath = "";
        private String m_strSaveAs = "";
        private String m_strSearch = "";

        private List<String> m_list_Errors = new List<string>();

        private bool m_bBrowseLoaded = false;

        Hashtable m_hashCache = new Hashtable();

        public Form1()
        {
            InitializeComponent();
            Console.WindowWidth = Console.LargestWindowWidth;
        }

#region Selected Index Changed

        private bool FormatPath(Control ctl, ref String strPath, bool bFailOnDirectory = true)
        {
            if (Directory.Exists(Path.GetFullPath(strPath)))
            {
                String strCapDrive = strPath.Substring(0, strPath.IndexOf(":" + Path.DirectorySeparatorChar) + 2);

                strPath = Path.GetFullPath(strPath).Replace(strCapDrive, strCapDrive.ToUpper());

                if (strPath != strCapDrive.ToUpper())
                {
                    strPath = strPath.TrimEnd(Path.DirectorySeparatorChar);
                }

                ctl.Text = strPath;
            }
            else if (bFailOnDirectory)
            {
                form_tabControl.TabIndex = 0;
                FormError(ctl, "Path does not exist.                    ", "Save Fields");
                return false;
            }

            return true;
        }

        private bool SaveFields(bool bFailOnDirectory = true)
        {
            m_strVolumeName = form_cb_VolumeName.Text.Trim();
            m_strPath = form_cb_Path.Text.Trim();
            m_strSearch = form_cb_Search.Text;

            if (m_strPath.Length > 0)
            {
                m_strPath += Path.DirectorySeparatorChar;

                if (FormatPath(form_cb_Path, ref m_strPath, bFailOnDirectory) == false)
                {
                    return false;
                }
            }

            if (form_cb_SaveAs.Text.Length > 0)
            {
                form_cb_SaveAs.Text = m_strSaveAs = Path.GetFullPath(form_cb_SaveAs.Text.Trim());

                if (FormatPath(form_cb_SaveAs, ref m_strSaveAs, bFailOnDirectory) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private void ComboBoxItemsInsert(ComboBox comboBox, String strText = "")
        {
            if (strText.Length <= 0)
            {
                strText = comboBox.Text;
            }

            strText = strText.Trim();

            if (strText.Length <= 0)
            {
                return;
            }

            if (comboBox.Items.Contains(strText))
            {
                return;
            }

            comboBox.Items.Insert(0, strText);
        }

        private void form_cb_VolumeName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cb_VolumeName, m_strPath);
            m_strPath = form_cb_VolumeName.Text;
        }

        private void cb_Path_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cb_Path, m_strPath);
            m_strPath = form_cb_Path.Text;
        }

        private void cb_SaveAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cb_SaveAs, m_strSaveAs);
            m_strSaveAs = form_cb_SaveAs.Text;
        }

        private void cb_Search_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cb_Search, m_strSearch);
            m_strSearch = form_cb_Search.Text;
        }

#endregion //Selected Index Changed

        private void form_btn_Path_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ComboBoxItemsInsert(form_cb_Path);
            m_strPath = form_cb_Path.Text = folderBrowserDialog1.SelectedPath;
        }

        private void form_btn_SaveAs_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ComboBoxItemsInsert(form_cb_SaveAs);
            m_strSaveAs = form_cb_SaveAs.Text = saveFileDialog1.FileName;

            if (File.Exists(m_strSaveAs))
            {
                form_cb_VolumeName.Text = "";
                form_cb_Path.Text = "";
            }

            m_bBrowseLoaded = false;
        }

        private bool ReadHeader()
        {
            using (StreamReader file = new StreamReader(m_strSaveAs))
            {
                do
                {
                    String line = file.ReadLine();

                    if (line != m_str_HEADER) break;

                    line = file.ReadLine();

                    if (line == null) break;

                    form_cb_VolumeName.Text = line;
                    line = file.ReadLine();

                    if (line == null) break;

                    form_cb_Path.Text = line;
                    return SaveFields(false);
                }
                while (false);
            }

            return false;
        }

        void FormError(Control control, String strError, String strTitle)
        {
            control.BackColor = Color.Red;
            timer_killRed.Enabled = true;
            MessageBox.Show(strError, strTitle);
        }

        private void form_btn_AddVolume_Click(object sender, EventArgs e)
        {
            if (SaveFields(false) == false)
            {
                return;
            }

            form_cb_VolumeName.BackColor = Color.Empty;
            form_cb_Path.BackColor = Color.Empty;
            form_cb_SaveAs.BackColor = Color.Empty;

            if (m_strSaveAs.Length <= 0)
            {
                FormError(form_cb_SaveAs, "Must have a file to load or save directory listing to.", "Volume Save As");
                return;
            }

            if (form_lv_Volumes.FindItemWithText(m_strSaveAs) != null)
            {
                FormError(form_cb_SaveAs, "File already in use in list of volumes.            ", "Volume Save As");
                return;
            }

            if (File.Exists(m_strSaveAs) && (m_strPath.Length > 0))
            {
                if (MessageBox.Show(m_strSaveAs + " already exists. Overwrite?                 ", "Volume Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    form_cb_SaveAs.BackColor = Color.Red;
                    timer_killRed.Enabled = true;
                    return;
                }
            }

            if ((File.Exists(m_strSaveAs) == false) && form_lv_Volumes.Items.ContainsKey(m_strPath))
            {
                FormError(form_cb_Path, "Path already added.                                   ", "Volume Source Path");
                return;
            }

            if ((m_strVolumeName.Length > 0) && form_lv_Volumes.FindItemWithText(m_strVolumeName) != null)
            {
                form_cb_VolumeName.BackColor = Color.Red;

                if (MessageBox.Show("Nickname already in use. Use it for more than one volume?", "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.Yes)
                {
                    return;
                }
                else
                {
                    form_cb_VolumeName.BackColor = Color.Empty;
                }
            }

            if ((File.Exists(m_strSaveAs) == false) && (m_strPath.Length <= 0))
            {
                form_cb_Path.BackColor = Color.Red;
                MessageBox.Show("Must have a path or existing directory listing file.  ", "Volume Source Path");
                return;
            }

            if ((m_strPath.Length > 0) && (Directory.Exists(m_strPath) == false))
            {
                form_cb_Path.BackColor = Color.Red;
                MessageBox.Show("Path does not exist.                                  ", "Volume Source Path");
                return;
            }

            String strStatus = "Not Saved";

            if (File.Exists(m_strSaveAs))
            {
                if (m_strPath.Length <= 0)
                {
                    bool bFileOK = ReadHeader();

                    if (bFileOK)
                    {
                        strStatus = m_str_USING_FILE;
                    }
                    else
                    {
                        if (m_strPath.Length > 0)
                        {
                            strStatus = "File is bad. Will overwrite.";
                        }
                        else
                        {
                            form_cb_Path.BackColor = Color.Red;
                            MessageBox.Show("File is bad and path does not exist.           ", "Volume Source Path");
                            return;
                        }
                    }
                }
                else
                {
                    strStatus = "Will overwrite.";
                }
            }

            if (m_strVolumeName.Length == 0)
            {
                form_cb_VolumeName.BackColor = Color.Red;

                if (MessageBox.Show("Would you like to enter a nickname for this volume?", "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.No)
                {
                    return;
                }
                else
                {
                    form_cb_VolumeName.BackColor = Color.Empty;
                }
            }

            ListViewItem lvItem = new ListViewItem(new string[] { m_strVolumeName, m_strPath, m_strSaveAs, strStatus, "Yes" });

            lvItem.Name = m_strPath;
            form_lv_Volumes.Items.Add(lvItem);
            form_btn_SavePathInfo.Enabled = true;
            m_bBrowseLoaded = false;
        }

        private void form_btn_RemoveVolume_Click(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection lvSelect = form_lv_Volumes.SelectedIndices;

            if (lvSelect.Count <= 0)
            {
                return;
            }

            form_lv_Volumes.Items[lvSelect[0]].Remove();
            UpdateLV_VolumesSelection();
            form_btn_SavePathInfo.Enabled = (form_lv_Volumes.Items.Count > 0);
            m_bBrowseLoaded = false;
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

        private void SetLV_VolumesItemInclude(ListViewItem lvItem, bool bInclude)
        {
            lvItem.SubItems[4].Text = (bInclude) ? "Yes" : "No";
        }

        private void form_btn_ToggleInclude_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvSelect = form_lv_Volumes.SelectedItems;

            if (lvSelect.Count <= 0)
            {
                return;
            }

            SetLV_VolumesItemInclude(lvSelect[0], LV_VolumesItemInclude(lvSelect[0]) == false);
            m_bBrowseLoaded = false;
        }

        private bool LV_VolumesItemInclude(ListViewItem lvItem)
        {
            return (lvItem.SubItems[4].Text == "Yes");
        }

        class Utilities
        {
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

        #region Save path info

        private delegate void SavePathInfoStatusDelegate(int nIndex, String strText);

        class SavePathInfo : Utilities
        {
            long m_nTotalLength = 0;
            SavePathInfoStatusDelegate m_callbackStatus = null;
            List<String> m_list_Errors = new List<string>();

            List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();

            public SavePathInfo(ListView.ListViewItemCollection lvItems,
                SavePathInfoStatusDelegate callbackStatus)
            {
                foreach (ListViewItem lvItem in lvItems)
                {
                    m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
                }

                m_callbackStatus = callbackStatus;
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

        void SavePathInfoStatusCallback(int nIndex, String strText)
        {
            if (InvokeRequired)
            {
                // called on a worker thread. marshal the call to the user interface thread
                Invoke(new SavePathInfoStatusDelegate(SavePathInfoStatusCallback), new object[] { nIndex, strText });
                return;
            }

            form_lv_Volumes.Items[nIndex].SubItems[3].Text = strText;
        }

        private void form_btn_SavePathInfo_Click(object sender, EventArgs e)
        {
            SavePathInfo savePathInfo = new SavePathInfo(form_lv_Volumes.Items,
                new SavePathInfoStatusDelegate(SavePathInfoStatusCallback));

            new Thread(new ThreadStart(savePathInfo.Go)).Start();

            m_bBrowseLoaded = false;
        }

        #endregion Save path info

        private void form_btn_SaveVolumeList_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (File.Exists(saveFileDialog1.FileName))
            {
                if (MessageBox.Show(saveFileDialog1.FileName + " already exists. Overwrite?         ", "Volume List Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }
            }

            using (TextWriter fs = File.CreateText(saveFileDialog1.FileName))
            {
                fs.WriteLine(m_str_VOLUME_LIST_HEADER);

                foreach (ListViewItem lvItem in form_lv_Volumes.Items)
                {
                    foreach (ListViewItem.ListViewSubItem lvSubitem in lvItem.SubItems)
                    {
                        fs.Write(lvSubitem.Text + "\t");
                    }

                    fs.WriteLine();
                }
            }
        }

        private void form_btn_LoadVolumeList_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
            openFileDialog1.FileName = saveFileDialog1.FileName;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (TextReader fs = File.OpenText(openFileDialog1.FileName))
            {
                String strLine = fs.ReadLine();

                if (strLine != m_str_VOLUME_LIST_HEADER)
                {
                    MessageBox.Show(openFileDialog1.FileName + " is not a valid volume list file.", "Load Volume List");
                    return;
                }

                form_lv_Volumes.Items.Clear();

                while ((strLine = fs.ReadLine()) != null)
                {
                    String[] strArray = strLine.Split('\t');

                    strArray[3] = "Using file.";

                    if (File.Exists(strArray[2]) == false)
                    {
                        strArray[2] = Path.GetDirectoryName(openFileDialog1.FileName) + Path.DirectorySeparatorChar + Path.GetFileName(strArray[2]);

                        if (File.Exists(strArray[2]) == false)
                        {
                            strArray[3] = "No file. Will create.";
                        }
                    }

                    form_lv_Volumes.Items.Add(new ListViewItem(strArray));
                }
            }

            if (form_lv_Volumes.Items.Count > 0)
            {
                form_btn_SavePathInfo.Enabled = true;
            }

            m_bBrowseLoaded = false;
        }

        private delegate void SearchStatusDelegate(String[] strArray, bool bFillPaths);

        class Search : Utilities
        {
            List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();
            SearchStatusDelegate m_callbackStatus = null;
            String m_strSearch = null;
            bool m_bCaseSensitive = true;

            public enum FolderSpecialHandling { None, Outermost, Innermost };
            FolderSpecialHandling m_folderHandling;

            public Search(ListView.ListViewItemCollection lvItems, SearchStatusDelegate callbackStatus,
                String strSearch, bool bCaseSensitive, FolderSpecialHandling folderHandling)
            {
                foreach (ListViewItem lvItem in lvItems)
                {
                    m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
                }

                m_callbackStatus = callbackStatus;
                m_strSearch = strSearch;
                m_bCaseSensitive = bCaseSensitive;
                m_folderHandling = folderHandling;
            }

            public void Go()
            {
                Console.WriteLine("Searching for '" + m_strSearch + "'");

                DateTime dtStart = DateTime.Now;

                foreach (LVvolStrings lvStrings in m_list_lvVolStrings)
                {
                    if (LV_VolumesItemInclude(lvStrings) == false)
                    {
                        continue;
                    }

                    String strVolumeName = lvStrings.VolumeName;
                    String strPath = lvStrings.Path;
                    String strSaveAs = lvStrings.SaveAs;

                    if (FormatPath(ref strPath, ref strSaveAs, false) == false)
                    {
                        return; // false;
                    }

                    using (StreamReader file = new StreamReader(strSaveAs))
                    {
                        String line = "";
                        long counter = -1;
                        bool bFillPaths = false;

                        if (m_bCaseSensitive == false)
                        {
                            m_strSearch = m_strSearch.ToLower();
                        }

                        while ((line = file.ReadLine()) != null)
                        {
                            ++counter;

                            if (m_bCaseSensitive)
                            {
                                if (line.Contains(m_strSearch) == false)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (line.ToLower().Contains(m_strSearch) == false)
                                {
                                    continue;
                                }
                            }

                            String[] strArray = line.Split('\t');

                            if (strArray[0].Length > 0) // directory
                            {
                                if (m_folderHandling == FolderSpecialHandling.Outermost)
                                {
                                    if (strArray[0].EndsWith(m_strSearch) == false)
                                    {
                                        continue;
                                    }
                                }
                                else if (m_folderHandling == FolderSpecialHandling.Innermost)
                                {
                                    if (strArray.Length < 6)
                                    {
                                        continue;
                                    }

                                    long nParse = 0;

                                    if (long.TryParse(strArray[5], out nParse) == false)
                                    {
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                bFillPaths = true;
                            }

                            if ((strArray.Length > 5) && (strArray[5].Length > 0))
                            {
                                strArray[5] = FormatSize(strArray[5]);
                            }

                            m_callbackStatus(strArray, bFillPaths);
                            Console.WriteLine(counter.ToString() + ": " + line);
                        }
                    }
                }

                TimeSpan span = DateTime.Now - dtStart;
                String strTimeSpent = String.Format("Completed Search for {0} in {1} milliseconds.", m_strSearch, (int)span.TotalMilliseconds);

                Console.WriteLine(strTimeSpent);
                MessageBox.Show(strTimeSpent);
            }
        }

        void SearchStatusCallback(String[] strArray, bool bFillPaths)
        {
            if (InvokeRequired)
            {
                // called on a worker thread. marshal the call to the user interface thread
                Invoke(new SearchStatusDelegate(SearchStatusCallback), new object[] { strArray, bFillPaths });
                return;
            }

            form_lv_SearchResults.Items.Add(new ListViewItem(strArray));
            form_btn_Search_FillPaths.Enabled = bFillPaths;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            form_lv_SearchResults.Items.Clear();
            Console.Clear();
            form_btn_Search_FillPaths.Enabled = false;

            Search.FolderSpecialHandling folderHandling = Search.FolderSpecialHandling.None;

            if (form_rad_Folder_Outermost.Checked)
            {
                folderHandling = Search.FolderSpecialHandling.Outermost;
            }
            else if (form_rad_Folder_Innermost.Checked)
            {
                folderHandling = Search.FolderSpecialHandling.Innermost;
            }

            m_strSearch = form_cb_Search.Text;
            Search search = new Search(form_lv_Volumes.Items, new SearchStatusDelegate(SearchStatusCallback),
                m_strSearch, form_chk_SearchCase.Checked, folderHandling);

            new Thread(new ThreadStart(search.Go)).Start();
        }

        private void UpdateLV_VolumesSelection()
        {
            bool bHasSelection = (form_lv_Volumes.SelectedIndices.Count > 0);

            form_btn_RemoveVolume.Enabled = bHasSelection;
            form_btn_ToggleInclude.Enabled = bHasSelection;
        }

        private void form_lv_Volumes_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            UpdateLV_VolumesSelection();
        }

        class NodeDatum
        {
            long m_nPrevLineNo = 0;
            long m_nLineNo = 0;

            public long PrevlineNo { get { return m_nPrevLineNo; } }
            public long LineNo { get { return m_nLineNo; } }
            public NodeDatum(long nPrevLineNo, long nLineNo) { m_nPrevLineNo = nPrevLineNo; m_nLineNo = nLineNo; }
        }

        class Node
        {
            static SortedDictionary<String, Node> nodes = null;
            SortedDictionary<String, Node> subNodes = new SortedDictionary<string, Node>();
            String m_strPath = "";
            static long m_nStaticLineNo = 0;
            long m_nPrevLineNo = 0;
            long m_nLineNo = 0;
            bool bUseShortPath = true;

            public Node(String in_str, long nLineNo)
            {
                if (in_str.EndsWith(":" + Path.DirectorySeparatorChar) == false)
                {
                    Debug.Assert(in_str.Trim().EndsWith(Path.DirectorySeparatorChar.ToString()) == false);
                }

                m_strPath = in_str;
                m_nPrevLineNo = m_nStaticLineNo;
                m_nStaticLineNo = m_nLineNo = nLineNo;

                // Path.GetDirectoryName() does not preserve filesystem root

                String strParent = m_strPath;
                int nIndex = strParent.LastIndexOf(Path.DirectorySeparatorChar);

                if (nIndex < 0)
                {
                    return;
                }

                strParent = strParent.Remove(nIndex);

                if (nodes.ContainsKey(strParent) == false)
                {
                    nodes.Add(strParent, new Node(strParent, 0));
                }

                if (nodes[strParent].subNodes.ContainsKey(m_strPath) == false)
                {
                    nodes[strParent].subNodes.Add(m_strPath, this);
                }
            }

            public static void SetRootNode(SortedDictionary<String, Node> node)
            {
                nodes = node;
            }

            public TreeNode AddToTree(String strVolumeName = null)
            {
                int nIndex = m_strPath.LastIndexOf(Path.DirectorySeparatorChar);
                String strShortPath = bUseShortPath ? m_strPath.Substring(nIndex + 1) : m_strPath;

                if (strVolumeName != null)
                {
                    bool bNotRedundant = (strVolumeName.EndsWith(strShortPath) == false);

                    if (bNotRedundant)
                    {
                        strShortPath = strVolumeName + " (" + strShortPath + ")";
                    }
                    else
                    {
                        strShortPath = strVolumeName;
                    }
                }

                TreeNode treeNode = null;

                if (subNodes.Count == 1)
                {
                    Node subNode = subNodes.Values.First();

                    if (this == nodes.Values.First())
                    {
                        // cull all root node single-chains.
                        SetRootNode(subNodes);
                        subNode.m_strPath.Insert(0, m_strPath + Path.DirectorySeparatorChar);
                        subNode.bUseShortPath = false;
                        treeNode = subNode.AddToTree(strVolumeName);
                    }
                    else
                    {
                        treeNode = new TreeNode(strShortPath, new TreeNode[] { subNode.AddToTree() });
                    }
                }
                else if (subNodes.Count > 1)
                {
                    List<TreeNode> treeList = new List<TreeNode>();

                    foreach (Node node in subNodes.Values)
                    {
                        treeList.Add(node.AddToTree());
                    }

                    treeNode = new TreeNode(strShortPath, treeList.ToArray());
                }
                else
                {
                    treeNode = new TreeNode(strShortPath);
                }

                treeNode.Tag = new NodeDatum(m_nPrevLineNo, m_nLineNo);
                return treeNode;
            }
        }

        class DirData
        {
            TreeView form_treeView_Browse = null;
            SortedDictionary<String, Node> nodes = new SortedDictionary<string, Node>();

            public DirData(TreeView ctl)
            {
                form_treeView_Browse = ctl;
                Node.SetRootNode(nodes);
            }

            public void AddToTree(String in_str, long nLineNo)
            {
                if (nodes.ContainsKey(in_str))
                {
                    Node node = nodes[in_str];
                    Debug.Assert(false);
                }

                while (in_str.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    in_str = in_str.Remove(in_str.LastIndexOf(Path.DirectorySeparatorChar));
                }

                nodes.Add(in_str, new Node(in_str, nLineNo));
            }

            public TreeNode AddToTree(String strVolumeName)
            {
                TreeNode rootNode = nodes.Values.First().AddToTree(strVolumeName);

                form_treeView_Browse.Nodes.Add(rootNode);
                return rootNode;
            }
        }
        
        private void form_tabPage_Browse_Paint(object sender, PaintEventArgs e)
        {
            if (m_bBrowseLoaded)
            {
                return;
            }

        //    new Thread(new ThreadStart(CreateBrowsingTree)).Start();
        //}

        //private void CreateBrowsingTree()
        //{
            form_treeView_Browse.Nodes.Clear();
            form_LV_Files.Items.Clear();
            form_LV_Detail.Items.Clear();
            m_hashCache.Clear();

            Console.WriteLine();
            Console.WriteLine("Creating browsing tree.");

            DateTime dtStart = DateTime.Now;

            foreach (ListViewItem lvItem in form_lv_Volumes.Items)
            {
                if (LV_VolumesItemInclude(lvItem) == false)
                {
                    continue;
                }

                form_cb_VolumeName.Text = lvItem.SubItems[0].Text;
                form_cb_Path.Text = lvItem.SubItems[1].Text;
                form_cb_SaveAs.Text = lvItem.SubItems[2].Text;

                if (SaveFields(false) == false)
                {
                    return;
                }

                using (StreamReader file = new StreamReader(m_strSaveAs))
                {
                    String line = "";
                    DirData dirData = new DirData(form_treeView_Browse);
                    long nLineNo = -1;

                    while ((line = file.ReadLine()) != null)
                    {
                        ++nLineNo;

                        StringBuilder strDriveInfo = new StringBuilder();

                        if (line == m_str_DRIVE)
                        {
                            for (int i = 0; i < 8; ++i)
                            {
                                strDriveInfo.AppendLine(file.ReadLine());
                                ++nLineNo;
                            }

                            m_hashCache.Add("driveInfo" + m_strSaveAs, strDriveInfo.ToString().Trim());
                            continue;
                        }

                        if (line.Contains('\t') == false)
                        {
                            continue;
                        }

                        if (line.Contains(":" + Path.DirectorySeparatorChar) == false)
                        {
                            continue;
                        }

                        String[] strArray = line.Split('\t');
                        String strNew = strArray[0];

                        if (strNew.Length <= 0)
                        {
                            continue;
                        }

                        // directory
                        dirData.AddToTree(strNew, nLineNo);
                    }

                    dirData.AddToTree(m_strVolumeName).Tag = m_strSaveAs;
                }
            }

            Console.WriteLine(String.Format("Completed browsing tree in {0} seconds.", ((int) (DateTime.Now - dtStart).TotalMilliseconds/10)/100.0));
            m_bBrowseLoaded = true;
        }

        private void timer_killRed_Tick(object sender, EventArgs e)
        {
            form_cb_VolumeName.BackColor = Color.Empty;
            form_cb_Path.BackColor = Color.Empty;
            form_cb_SaveAs.BackColor = Color.Empty;
            timer_killRed.Enabled = false;
        }

        private void form_treeView_Browse_AfterSelect(object sender, TreeViewEventArgs e)
        {
            form_LV_Detail.Items.Clear();
            form_LV_Files.Items.Clear();

            TreeNode nodeParent = e.Node;

            while (nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            Debug.Assert(nodeParent.Tag is String);

            String strFile = (String)nodeParent.Tag;

            if (File.Exists(strFile) == false)
            {
                Debug.Assert(false);
                return;
            }

            if ((e.Node.Tag is NodeDatum) == false)
            {
                return;
            }

            NodeDatum nodeDatum = (NodeDatum) e.Node.Tag;

            if (nodeDatum.LineNo <= 0)
            {
                return;
            }

            long nPrevDir = nodeDatum.PrevlineNo;
            long nLineNo = nodeDatum.LineNo;
            String strLine = File.ReadLines(strFile).Skip((int)nLineNo).Take(1).ToArray()[0];
            String[] strArray = strLine.Split('\t');
            long nIx = 0;
            DateTime dt;


            // Directory detail

            nIx = 2; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) { form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Created\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() })); }
            nIx = 3; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Modified\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 4; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Attributes\t", strArray[nIx] }));
            nIx = 5;

            long nLengthDebug_A = 0;

            if ((strArray.Length > nIx) && (strArray[nIx].Length > 0))
            {
                Debug.Assert(long.TryParse(strArray[nIx], out nLengthDebug_A));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Size\t", Utilities.FormatSize(strArray[nIx], true) }));
            }

            nIx = 6; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Error 1\t", strArray[nIx] }));
            nIx = 7; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Error 2\t", strArray[nIx] }));

            Console.WriteLine(strLine);

            
            // Volume detail

            if (m_hashCache.ContainsKey("driveInfo" + strFile))
            {
                String strDriveInfo = (String)m_hashCache["driveInfo" + strFile];
                String[] arrDriveInfo = strDriveInfo.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None);

                Debug.Assert(new int[] { 7, 8 }.Contains(arrDriveInfo.Length));
                form_LV_Detail.Items.Add(new ListViewItem());

                ListViewItem lvItem = new ListViewItem("Volume detail");

                lvItem.BackColor = Color.DarkGray;
                lvItem.ForeColor = Color.White;
                form_LV_Detail.Items.Add(lvItem);
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Available Free Space", Utilities.FormatSize(arrDriveInfo[0], true) }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Drive Format", arrDriveInfo[1] }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Drive Type", arrDriveInfo[2] }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Name", arrDriveInfo[3] }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Root Directory", arrDriveInfo[4] }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Total Free Space", Utilities.FormatSize(arrDriveInfo[5], true) }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Total Size", Utilities.FormatSize(arrDriveInfo[6], true) }));

                if (arrDriveInfo.Length == 8)
                {
                    form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Volume Label", arrDriveInfo[7] }));
                }
            }


            // file list

            if (nPrevDir <= 0)
            {
                return;
            }

            if ((nLineNo - nPrevDir) <= 1)  // dir has no files
            {
                return;
            }

            long nLengthDebug = 0;

            if (m_hashCache.ContainsKey(strLine) == false)
            {
                DateTime dtStart = DateTime.Now;
                List<String> listLines = File.ReadLines(strFile)
                    .Skip((int)nPrevDir+1)
                    .Take((int)(nLineNo - nPrevDir - 1))
                    .ToList();

                listLines.Sort();

                for (int i = 0; i < listLines.Count; ++i)
                {
                    strArray = listLines[i].Split('\t');

                    if ((strArray.Length > 5) && (strArray[5].Length > 0))
                    {
                        nLengthDebug += long.Parse(strArray[5]);
                        strArray[5] = Utilities.FormatSize(strArray[5]);
                    }

                    form_LV_Files.Items.Add(new ListViewItem(strArray));
                }

                TimeSpan timeSpan = (DateTime.Now - dtStart);
                String strTimeSpan = (((int)timeSpan.TotalMilliseconds / 100) / 10.0).ToString();

                Console.WriteLine("File list took " + strTimeSpan + " seconds.");

                if (timeSpan.Seconds > 1)
                {
                    ListViewItem[] itemArray = new ListViewItem[form_LV_Files.Items.Count];

                    form_LV_Files.Items.CopyTo(itemArray, 0);
                    m_hashCache.Add(strLine, itemArray);
                    m_hashCache.Add("nLengthDebug" + strLine, nLengthDebug);
                    m_hashCache.Add("timeSpan" + strLine, strTimeSpan);
                    Console.WriteLine("Cached.");
                }

                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "# Files", (nLineNo - nPrevDir + 1).ToString() }));
            }
            else    // file list is cached
            {
                DateTime dtStart = DateTime.Now;
                ListViewItem[] itemArray = (ListViewItem[])m_hashCache[strLine];

                form_LV_Files.Items.AddRange(itemArray);

                if (itemArray.Length > 0)
                {
                    form_LV_Detail.Items.Add(new ListViewItem(new String[] { "# Files", itemArray.Length.ToString() }));
                }

                TimeSpan timeSpan = (DateTime.Now - dtStart);

                nLengthDebug = (long)m_hashCache["nLengthDebug" + strLine];
                Console.WriteLine("File list used to take " + (String)m_hashCache["timeSpan" + strLine] + " seconds before caching.");
                Console.WriteLine("Cache read took " + (int)timeSpan.TotalMilliseconds + " milliseconds.");
            }

            Debug.Assert(nLengthDebug == nLengthDebug_A);
        }

        private void form_cb_Search_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Keys[] {Keys.Enter, Keys.Return}.Contains((Keys) e.KeyChar))
            {
                btnSearch_Click(sender, e);
            }
        }
    }
}
