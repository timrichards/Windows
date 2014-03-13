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

        private long m_nTotalLength = 0;
        private List<String> m_list_Errors = new List<string>();

        private bool m_bBrowseLoaded = false;

        public Form1()
        {
            InitializeComponent();
            Console.WindowWidth = Console.LargestWindowWidth;
        }

#region Selected Index Changed

        private void SaveFields()
        {
            m_strVolumeName = form_cb_VolumeName.Text;
            m_strPath = form_cb_Path.Text;

            if (m_strPath.Length > 0)
            {
                form_cb_Path.Text = m_strPath = Path.GetFullPath(m_strPath);
            }

            m_strSaveAs = form_cb_SaveAs.Text;

            if (m_strSaveAs.Length > 0)
            {
                form_cb_SaveAs.Text = m_strSaveAs = Path.GetFullPath(m_strSaveAs);
            }

            m_strSearch = form_cb_Search.Text;
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

#region Button Click

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
            m_bBrowseLoaded = false;
        }

        private void WriteHeader(TextWriter fs)
        {
            fs.WriteLine(m_str_HEADER);
            // assume SaveFields() by caller because SaveFields() has already prompted user
            fs.WriteLine(m_strVolumeName);
            fs.WriteLine(m_strPath);

            DriveInfo driveInfo = new DriveInfo(m_strPath.Substring(0, m_strPath.IndexOf('\\')));
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

        private bool ReadHeader()
        {
            bool bFileOK = false;

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
                    SaveFields();
                    bFileOK = true;
                }
                while (false);
            }

            return bFileOK;
        }

        void FormError(Control control, String strError, String strTitle)
        {
            control.BackColor = Color.Red;
            timer_killRed.Enabled = true;
            MessageBox.Show(strError, strTitle);
        }

        private void form_btn_AddVolume_Click(object sender, EventArgs e)
        {
            SaveFields();
            form_cb_VolumeName.BackColor = Color.Empty;
            form_cb_Path.BackColor = Color.Empty;
            form_cb_SaveAs.BackColor = Color.Empty;

#region Save As

            if (m_strSaveAs.Length <= 0)
            {
                FormError(form_cb_SaveAs, "Must have a file to load or save directory listing to.", "Volume Save As");
                return;
            }

            if (form_lv_Volumes.Items.ContainsKey(m_strPath))
            {
                FormError(form_cb_Path, "Path already added.                                   ", "Volume Source Path");
                return;
            }

            if (form_lv_Volumes.FindItemWithText(m_strSaveAs) != null)
            {
                FormError(form_cb_SaveAs, "File already in use for another volume.               ", "Volume Save As");
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

            if (File.Exists(m_strSaveAs) && (m_strPath.Length > 0))
            {
                if (MessageBox.Show(m_strSaveAs + " already exists. Overwrite?         ", "Volume Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    form_cb_SaveAs.BackColor = Color.Red;
                    timer_killRed.Enabled = true;
                    return;
                }
            }

#endregion // Save As

            #region Path

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

            #endregion // path

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
        }

        private bool LV_VolumesItemInclude(ListViewItem lvItem)
        {
            return (lvItem.SubItems[4].Text == "Yes");
        }

        private void SetLV_VolumesItemInclude(ListViewItem lvItem, bool bInclude)
        {
            if (bInclude)
            {
                lvItem.SubItems[4].Text = "Yes";
            }
            else
            {
                lvItem.SubItems[4].Text = "No";
            }
        }

        private void form_btn_ToggleInclude_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvSelect = form_lv_Volumes.SelectedItems;

            if (lvSelect.Count <= 0)
            {
                return;
            }

            SetLV_VolumesItemInclude(lvSelect[0], LV_VolumesItemInclude(lvSelect[0]) == false);
        }

        private void form_btn_SavePathInfo_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvItem in form_lv_Volumes.Items)
            {
                if ((m_str_USING_FILE + m_str_SAVED).Contains(lvItem.SubItems[3].Text))
                {
                    continue;
                }

                form_cb_VolumeName.Text = lvItem.SubItems[0].Text;
                form_cb_Path.Text = lvItem.SubItems[1].Text;
                form_cb_SaveAs.Text = lvItem.SubItems[2].Text;
                lvItem.SubItems[3].Text = "Saving...";
                SaveDirectoryListing();
                lvItem.SubItems[3].Text = m_str_SAVED;
            }

            MessageBox.Show("Completed.                           ", "Save Path Info");
            m_bBrowseLoaded = false;
        }

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

                    ListViewItem lvItem = new ListViewItem(new string[] { strArray[0], strArray[1], strArray[2], strArray[3], strArray[4] });

                    form_lv_Volumes.Items.Add(lvItem);
                }
            }

            if (form_lv_Volumes.Items.Count > 0)
            {
                form_btn_SavePathInfo.Enabled = true;
            }

            m_bBrowseLoaded = false;
        }

#endregion // Button Click

        public String FormatString(String strDir = "", String strFile = "", DateTime? dtCreated = null, DateTime? dtModified = null, String strAttributes = "", long nLength = 0, String strError1 = "", String strError2 = "")
        {
            String strLength = "";

            if (nLength > 0)
            {
                strLength = nLength.ToString();
            }

            String strCreated = "";

            if (dtCreated != null)
            {
                strCreated = dtCreated.ToString();
            }

            String strModified = "";

            if (dtModified != null)
            {
                strModified = dtModified.ToString();
            }

            if ((strDir + strFile + strCreated + strModified + strAttributes + strLength + strError1 + strError2).Length <= 0)
            {
                return "0"        + "\t" + "1"    + "\t" + "2"        + "\t" + "3"        + "\t" + "4"        + "\t" + "5"        + "\t" + "6"        + "\t" + "7"        + "\n"
                     + "Dir"      + "\t" + "File" + "\t" + "Created"  + "\t" + "Modded"   + "\t" + "Attrib"   + "\t" + "Length"   + "\t" + "Error1"   + "\t" + "Error2";
            }

            return (strDir + "\t" + strFile + "\t" + strCreated + "\t" + strModified + "\t" + strAttributes + "\t" + strLength + "\t" + strError1 + "\t" + strError2).TrimEnd();
        }

        private void SaveDirectoryListing()
        {
            SaveFields();
            Console.Clear();

            if (Directory.Exists(m_strPath) == false)
            {
                MessageBox.Show("Source Path does not exist.");
                return;
            }

            if (m_strSaveAs.Length <= 0)
            {
                MessageBox.Show("Must specify save filename.");
                return;
            }

            String strSavePathInfo = form_btn_SavePathInfo.Text;

            form_btn_SavePathInfo.Text = "Running Task...";
            this.Enabled = false;

            m_list_Errors.Clear();
            m_nTotalLength = 0;
            String strPathOrig = Directory.GetCurrentDirectory();

            using (TextWriter fs = File.CreateText(m_strSaveAs))
            {
                WriteHeader(fs);
                fs.WriteLine();
                fs.WriteLine(m_str_START + " " + DateTime.Now.ToString());
                fs.WriteLine(FormatString());
                TraverseTree(fs, m_strPath);
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
            form_btn_SavePathInfo.Text = strSavePathInfo;
            this.Enabled = true;
        }

        bool CheckNTFS_chars(String strFile, bool bFile = true)
        {
            bool bFileOK = true;
            String strFilenameCheck = strFile.Replace(":\\", "");
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

                String strOut = FormatString(strFile: strErrorFile, strDir: strErrorDir, strError1: "NTFS Char", strError2: chrErrorMessage.ToString());
                m_list_Errors.Add(strOut);
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

                    m_list_Errors.Add(FormatString(strDir: currentDir, strFile:"GetFiles()", strError1: "UnauthorizedAccessException", strError2: e.Message));
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

                        strOut = FormatString(strFile: fi.Name, dtCreated: fi.CreationTime, strAttributes: fi.Attributes.ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Length, strError1:strError1, strError2:strError2);
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

                    Console.Write(".");
                    fs.WriteLine(strOut);
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

                    String strOut = FormatString(strDir: currentDir, dtCreated: di.CreationTime, strAttributes: di.Attributes.ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: strError2);

                    Console.WriteLine();
                    fs.WriteLine(strOut);
                }

                // Push the subdirectories onto the stack for traversal. 
                // This could also be done before handing the files. 
                foreach (string str in subDirs)
                    dirs.Push(str);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            Console.Clear();
            Console.WriteLine("Searching for '" + m_strSearch + "'");

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
                SaveFields();

                using (StreamReader file = new StreamReader(m_strSaveAs))
                {
                    String line = "";
                    int counter = 0;

                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.Contains(m_strSearch))
                        {
                            String[] strArray = line.Split('\t');

                            if (strArray[0].Length > 0) // directory
                            {
                                if (form_rad_Folder_Outermost.Checked)
                                {
                                    if (strArray[0].EndsWith(m_strSearch) == false)
                                    {
                                        continue;
                                    }
                                }
                                else if (form_rad_Folder_Innermost.Checked)
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

                            Console.WriteLine(counter.ToString() + ": " + line);
                        }

                        counter++;
                    }
                }
            }

            TimeSpan span = DateTime.Now - dtStart;
            String strTimeSpent = String.Format("Completed Search for {0} in {1} milliseconds.", m_strSearch, span.Milliseconds);

            Console.WriteLine(strTimeSpent);
            MessageBox.Show(strTimeSpent);
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

        class Node
        {
            static SortedDictionary<String, Node> nodes = null;
            SortedDictionary<String, Node> subNodes = new SortedDictionary<string, Node>();
            String m_strPath = "";
            int m_nLineNo = 0;

            public Node(String in_str, int nLineNo)
            {
                m_strPath = in_str;
                m_nLineNo = nLineNo;

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

            public TreeNode AddToTree()
            {
                int nIndex = m_strPath.LastIndexOf(Path.DirectorySeparatorChar);
                String strShortPath = m_strPath.Substring(nIndex + 1);

                TreeNode treeNode = null;

                if (subNodes.Count >= 1)
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

                treeNode.Tag = m_nLineNo;
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

            public void AddToTree(String in_str, int nLineNo)
            {
                in_str = in_str.Trim();

                while (in_str.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    in_str = in_str.Remove(in_str.LastIndexOf(Path.DirectorySeparatorChar));
                }

                nodes.Add(in_str, new Node(in_str, nLineNo));
            }

            public void AddToTree()
            {
                form_treeView_Browse.Nodes.Add(nodes.Values.FirstOrDefault().AddToTree());
            }
        }
        
        private void form_tabPage_Browse_Paint(object sender, PaintEventArgs e)
        {
            if (m_bBrowseLoaded)
            {
                return;
            }

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
                SaveFields();

                form_treeView_Browse.Nodes.Clear();

                using (StreamReader file = new StreamReader(m_strSaveAs))
                {
                    String line = "";
                    DirData dirData = new DirData(form_treeView_Browse);

                    int nLineNo = -1;

                    while ((line = file.ReadLine()) != null)
                    {
                        ++nLineNo;

                        if (line.Contains('\t') == false)
                        {
                            continue;
                        }

                        if (line.Contains(":\\") == false)
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

                    dirData.AddToTree();
                    form_treeView_Browse.Nodes[0].Tag = m_strSaveAs;
                }
            }

            m_bBrowseLoaded = true;
            TimeSpan span = DateTime.Now - dtStart;
            String strTimeSpent = String.Format("Completed browsing tree in {0} milliseconds.", span.Milliseconds);

            Console.WriteLine(strTimeSpent);
            MessageBox.Show(strTimeSpent);
        }

        private void timer_killRed_Tick(object sender, EventArgs e)
        {
            form_cb_VolumeName.BackColor = Color.Empty;
            form_cb_Path.BackColor = Color.Empty;
            form_cb_SaveAs.BackColor = Color.Empty;
            timer_killRed.Enabled = false;
        }

        String FormatSize(String in_str)
        {
            long nLength = 0;

            Debug.Assert(long.TryParse(in_str, out nLength));

            double nK = nLength / 1024.0 - .05;
            double nM = nLength / 1024.0 / 1024 - .05;
            double nG = nLength / 1024.0 / 1024 / 1024 - .05;
            String strSz = "";
            String strFormat = "###,##0.0";

            if (((int) nG) > 0) strSz = nG.ToString(strFormat) + " GB";
            else if (((int) nM) > 0) strSz = nM.ToString(strFormat) + " MB";
            else if (((int)nK) > 0) strSz = nK.ToString(strFormat) + " KB";
            else strSz = "1 KB";

            return strSz;
        }

        private void form_treeView_Browse_AfterSelect(object sender, TreeViewEventArgs e)
        {
            form_LV_Detail.Items.Clear();
            form_LV_Files.Items.Clear();

            if ((e.Node.Tag is int) == false)
            {
                return;
            }

            if (((int)e.Node.Tag) <= 0)
            {
                return;
            }

            int nLineNo = (int)e.Node.Tag;
            TreeNode nodeParent = e.Node;

            while (nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            Debug.Assert(nodeParent.Tag is String);

            String strFile = (String)nodeParent.Tag;
            String strLine = "";
            int nPrevDirPlus1 = 0;

            using (StreamReader file = new StreamReader(strFile))
            {
                for (int i = 0; i < nLineNo; ++i)
                {
                    strLine = file.ReadLine();
                    if (strLine.StartsWith('\t'.ToString()) == false)
                    {
                        if ((strLine.Contains(":\\") == false) || (strLine.Contains("\t") == false))
                        {
                            // header line, anyway not a directory path: doesn't start with the drive letter.
                            Debug.Assert(nPrevDirPlus1 == 0);
                            nPrevDirPlus1 = 0;
                        }
                        else
                        {
                            nPrevDirPlus1 = i+1;
                        }
                    }
                }

                strLine = file.ReadLine();
            }

            String[] strArray = strLine.Split('\t');
            int nIx = 0;
            DateTime dt;

            nIx = 2; if (strArray.Length > nIx) { form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Created\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() })); }
            nIx = 3; if (strArray.Length > nIx) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Modified\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 4; if (strArray.Length > nIx) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Attributes\t", strArray[nIx] }));

            nIx = 5;

            if (strArray.Length > nIx)
            {
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Size\t", FormatSize(strArray[nIx]) + " (" + long.Parse(strArray[nIx]).ToString("###,###,###,###,###") + " bytes)" }));
            }

            nIx = 6; if (strArray.Length > nIx) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Error 1\t", strArray[nIx] }));
            nIx = 7; if (strArray.Length > nIx) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Error 2\t", strArray[nIx] }));

            if (nPrevDirPlus1 <= 0)
            {
                return;
            }

            long nLengthDebug = 0;

            using (StreamReader file = new StreamReader(strFile))
            {
                for (int i = 0; i < nPrevDirPlus1; ++i)
                {
                    file.ReadLine();
                }

                for (int i = nPrevDirPlus1; i < nLineNo; ++i)
                {
                    strLine = file.ReadLine();
                    strArray = strLine.Split('\t');

                    if (strArray.Length > 5)
                    {
                        nLengthDebug += long.Parse(strArray[5]);
                        strArray[5] = FormatSize(strArray[5]);
                    }

                    form_LV_Files.Items.Add(new ListViewItem(strArray));
                }
            }

            if ((nLineNo - nPrevDirPlus1) > 0)
            {
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "# Files", (nLineNo - nPrevDirPlus1).ToString() }));
            }
            
            if (nLengthDebug > 0)
            {
                String strLengthDebug = nLengthDebug.ToString("###,###,###,###,###");

                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Files size", FormatSize(nLengthDebug.ToString()) + " (" + strLengthDebug + " bytes)" }));
            }
        }
    }
}
