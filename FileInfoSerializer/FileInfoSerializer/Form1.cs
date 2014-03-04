using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace FileInfoSerializer
{
    public partial class Form1 : Form
    {
        String m_strVolumeName = "";
        String m_strPath = "";
        String m_strSaveAs = "";
        String m_strSearch = "";

        long m_nTotalLength = 0;
        List<String> m_list_Errors = new List<string>();

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
            folderBrowserDialog1.ShowDialog();
            ComboBoxItemsInsert(form_cb_Path);
            m_strPath = form_cb_Path.Text = folderBrowserDialog1.SelectedPath;
        }

        private void form_btn_SaveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            ComboBoxItemsInsert(form_cb_SaveAs);
            m_strSaveAs = form_cb_SaveAs.Text = saveFileDialog1.FileName;
        }

        private bool IsLV_VolumeItemUnique()
        {
            if (form_lv_Volumes.Items.ContainsKey(m_strPath))
            {
                form_cb_Path.BackColor = Color.Red;
                MessageBox.Show("Path already added.                                   ", "Volume Source Path");
                return false;
            }

            if (form_lv_Volumes.FindItemWithText(m_strSaveAs) != null)
            {
                form_cb_SaveAs.BackColor = Color.Red;
                MessageBox.Show("File already in use for another volume.               ", "Volume Save As");
                return false;
            }

            if ((m_strVolumeName.Length > 0) && form_lv_Volumes.FindItemWithText(m_strVolumeName) != null)
            {
                form_cb_VolumeName.BackColor = Color.Red;
                if (MessageBox.Show("Nickname already in use. Use it for more than one volume?", "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.Yes)
                {
                    return false;
                }
                else
                {
                    form_cb_VolumeName.BackColor = Color.Empty;
                }
            }

            return true;
        }

        private const String m_str_HEADER = "FileInfoSerializer";

        private void WriteHeader(TextWriter fs)
        {
            fs.WriteLine(m_str_HEADER);
            // assume SaveFields() by caller because SaveFields() has already prompted user
            fs.WriteLine(m_strVolumeName);
            fs.WriteLine(m_strPath);
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

        const String m_str_USING_FILE = "Using file.";
        const String m_str_SAVED = "Saved.";

        private void form_btn_AddVolume_Click(object sender, EventArgs e)
        {
            SaveFields();
            form_cb_Path.BackColor = Color.Empty;
            form_cb_SaveAs.BackColor = Color.Empty;

#region Save As

            if (m_strSaveAs.Length <= 0)
            {
                form_cb_SaveAs.BackColor = Color.Red;
                MessageBox.Show("Must have a file to load or save directory listing to.", "Volume Save As");
                return;
            }

            if (IsLV_VolumeItemUnique() == false)
            {
                return;
            }

            if (File.Exists(m_strSaveAs) && (m_strPath.Length > 0))
            {
                if (MessageBox.Show(m_strSaveAs + " already exists. Overwrite?         ", "Volume Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    form_cb_SaveAs.BackColor = Color.Red;
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

                    if (IsLV_VolumeItemUnique() == false)
                    {
                        return;
                    }

                    strStatus = bFileOK ? m_str_USING_FILE : "File is bad. Will overwrite.";
                }
                else
                {
                    strStatus = "Will overwrite.";
                }
            }

            ListViewItem lvItem = new ListViewItem(new string[] { m_strVolumeName, m_strPath, m_strSaveAs, strStatus, "Yes" });

            lvItem.Name = m_strPath;
            form_lv_Volumes.Items.Add(lvItem);
            form_btn_SavePathInfo.Enabled = true;
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

            using (TextWriter fs = File.CreateText(m_strSaveAs))
            {
                WriteHeader(fs);
                fs.WriteLine(FormatString());
                TraverseTree(fs, m_strPath);
                fs.WriteLine();

                foreach (String strError in m_list_Errors)
                {
                    fs.WriteLine(strError);
                }

                fs.WriteLine();
                fs.WriteLine(FormatString(nLength: m_nTotalLength));
            }

            form_btn_SavePathInfo.Text = strSavePathInfo;
            this.Enabled = true;
        }

        // http://msdn.microsoft.com/en-us/library/bb513869.aspx
        public void TraverseTree(TextWriter fs, string root)
        {
            // Data structure to hold names of subfolders to be 
            // examined for files.
            Stack<string> dirs = new Stack<string>(64);
            if (!System.IO.Directory.Exists(root))
            {
                throw new ArgumentException();
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;

                {
                    bool bFileOK = true;
                    String strFilenameCheck = currentDir.Replace(":\\", "");
                    Char chrErrorMessage = ' ';

                    if (strFilenameCheck.Contains(chrErrorMessage = ':'))
                    {
                        bFileOK = false;
                    }
                    else if (strFilenameCheck.Contains(chrErrorMessage = '?'))
                    {
                        bFileOK = false;
                    }

                    if (bFileOK == false)
                    {
                        String strOut = FormatString(strDir: currentDir, strError1: "NTFS Char", strError2: chrErrorMessage.ToString());
                        m_list_Errors.Add(strOut);
                        continue;
                    }
                }

                try
                {
                    subDirs = System.IO.Directory.GetDirectories(currentDir);
                }
                // An UnauthorizedAccessException exception will be thrown if we do not have 
                // discovery permission on a folder or file. It may or may not be acceptable  
                // to ignore the exception and continue enumerating the remaining files and  
                // folders. It is also possible (but unlikely) that a DirectoryNotFound exception  
                // will be raised. This will happen if currentDir has been deleted by 
                // another application or thread after our call to Directory.Exists. The  
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

                string[] files = null;
                try
                {
                    files = System.IO.Directory.GetFiles(currentDir);
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
                foreach (string file in files)
                {
                    String strOut = "";
                    bool bFileOK = true;
                    String strFilenameCheck = file.Replace(":\\", "");
                    Char chrErrorMessage = ' ';

                    if (strFilenameCheck.Contains(chrErrorMessage = ':'))
                    {
                        bFileOK = false;
                    }
                    else if (strFilenameCheck.Contains(chrErrorMessage = '?'))
                    {
                        bFileOK = false;
                    }

                    if (bFileOK == false)
                    {
                        strOut = FormatString(strFile: file, strError1: "NTFS Char", strError2: chrErrorMessage.ToString());
                        m_list_Errors.Add(strOut);
                        continue;
                    }

                    try
                    {
                        // Perform whatever action is required in your scenario.
                        System.IO.FileInfo fi = new System.IO.FileInfo(file);

                        strOut = FormatString(strFile: fi.Name, dtCreated: fi.CreationTime, strAttributes: fi.Attributes.ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Length);
                        m_nTotalLength += fi.Length;
                        nDirLength += fi.Length;
                    }
                    catch (PathTooLongException)
                    {
                        strOut = FormatString(strFile: file, strError1: "PathTooLongException");
                        m_list_Errors.Add(strOut);
                        continue;
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        // If file was deleted by a separate application 
                        //  or thread since the call to TraverseTree() 
                        // then just continue.
                        strOut = FormatString(strFile: file, strError1: "FileNotFoundException");
                        m_list_Errors.Add(strOut);
                        continue;
                    }

                    Console.Write(".");
                    fs.WriteLine(strOut);
                }

                {
                    DirectoryInfo di = new DirectoryInfo(currentDir);
                    String strOut = FormatString(strDir: currentDir, dtCreated: di.CreationTime, strAttributes: di.Attributes.ToString("X"), dtModified:di.LastWriteTime, nLength: nDirLength);

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
            SaveFields();

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
                            if (form_chk_FoldersWithLength.Checked)
                            {
                                String[] strArray = line.Split('\t');

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

                            Console.WriteLine(counter.ToString() + ": " + line);
                        }

                        counter++;
                    }
                }
            }

            TimeSpan span = DateTime.Now - dtStart;

            MessageBox.Show(String.Format("Completed Search for {0} in {1} milliseconds.", m_strSearch, span.Milliseconds));
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
    }
}
