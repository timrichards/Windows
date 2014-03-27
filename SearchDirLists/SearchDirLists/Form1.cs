using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace SearchDirLists
{
    public partial class Form1 : Form
    {
        private String m_strVolumeName = "";
        private String m_strPath = "";
        private String m_strSaveAs = "";
        private String m_strSearch = "";
        int m_nLVclonesClickIndex = -1;
        int m_nTreeFindTextChanged = 0;
        TreeNode[] m_arrayTreeFound = null;
        bool m_bPutPathInFindEditBox = false;
        String m_strCompare1 = null;
        TreeNode m_nodeCompare1 = null;
        TreeNode m_nodeCompare2 = null;
        int m_nCompareIndex = -1;
        bool m_bCompareMode = false;
        Dictionary<TreeNode, TreeNode> dictCompareDiffs = new Dictionary<TreeNode, TreeNode>();
        Control m_ctlLastFocusForCopyButton = null;
        List<TreeNode> m_listTreeNodes_Compare1 = new List<TreeNode>();
        List<TreeNode> m_listTreeNodes_Compare2 = new List<TreeNode>();

        // initialized in constructor:
        Blink blink = null;
        String m_strBtnTreeCollapseOrig = null;
        String m_strFilesColOrig = null;
        String m_strFileCompareColOrig = null;
        String m_strVolDetailColOrig = null;
        String m_strBtnCompareOrig = null;
        String m_strChkCompareOrig = null;
        String m_strVolGroupOrig = null;
        Font m_FontVolGroupOrig = null;

        public Form1()
        {
            InitializeComponent();
            blink = new Blink(timer_blink, form_cb_TreeFind);
            m_strBtnTreeCollapseOrig = form_btn_TreeCollapse.Text;
            m_strFilesColOrig = form_col_Filename.Text;
            m_strFileCompareColOrig = form_colFileCompare.Text;
            m_strVolDetailColOrig = form_colVolDetail.Text;
            m_strBtnCompareOrig = form_btn_Compare.Text;
            m_strChkCompareOrig = form_chk_Compare1.Text;
            m_strVolGroupOrig = form_lbl_VolGroup.Text;
            m_FontVolGroupOrig = form_lbl_VolGroup.Font;
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

        private void ComboBoxItemsInsert(ComboBox comboBox, String strText = "", bool bTrimText = true)
        {
            if (strText.Length <= 0)
            {
                strText = comboBox.Text;
            }

            if (bTrimText)
            {
                strText = strText.Trim();
            }

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
            {
                String strLine = File.ReadLines(m_strSaveAs).Take(1).ToArray()[0];

                if (strLine == Utilities.m_str_HEADER_01)
                {
                    Console.WriteLine("Converting " + m_strSaveAs);
                    Utilities.ConvertFile(m_strSaveAs);
                    Console.WriteLine("File converted to " + Utilities.m_str_HEADER);
                }
            }

            {
                String strLine = File.ReadLines(m_strSaveAs).Take(1).ToArray()[0].Split('\t')[2];

                Debug.Assert(strLine == Utilities.m_str_HEADER);

                if (strLine != Utilities.m_str_HEADER)
                {
                    return false;
                }
            }

            using (StreamReader file = new StreamReader(m_strSaveAs))
            {
                do
                {
                    String line = null;

                    if ((line = file.ReadLine()) == null) break;
                    if ((line = file.ReadLine()) == null) break;
                    form_cb_VolumeName.Text = line.Split('\t')[2];
                    if ((line = file.ReadLine()) == null) break;
                    form_cb_Path.Text = line.Split('\t')[2];
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

            if (form_LV_VolumesMain.FindItemWithText(m_strSaveAs) != null)
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

            if ((File.Exists(m_strSaveAs) == false) && form_LV_VolumesMain.Items.ContainsKey(m_strPath))
            {
                FormError(form_cb_Path, "Path already added.                                   ", "Volume Source Path");
                return;
            }

            if ((m_strVolumeName.Length > 0) && form_LV_VolumesMain.FindItemWithText(m_strVolumeName) != null)
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
                        strStatus = Utilities.m_str_USING_FILE;
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
            form_LV_VolumesMain.Items.Add(lvItem);
            form_btn_SavePathInfo.Enabled = true;
            m_bBrowseLoaded = false;
            DoTree(true);
        }

        private void form_btn_RemoveVolume_Click(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection lvSelect = form_LV_VolumesMain.SelectedIndices;

            if (lvSelect.Count <= 0)
            {
                return;
            }

            form_LV_VolumesMain.Items[lvSelect[0]].Remove();
            UpdateLV_VolumesSelection();
            form_btn_SavePathInfo.Enabled = (form_LV_VolumesMain.Items.Count > 0);
            m_bBrowseLoaded = false;
            RestartTreeTimer();
        }

        private void SetLV_VolumesItemInclude(ListViewItem lvItem, bool bInclude)
        {
            lvItem.SubItems[4].Text = (bInclude) ? "Yes" : "No";
        }

        private void form_btn_ToggleInclude_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvSelect = form_LV_VolumesMain.SelectedItems;

            if (lvSelect.Count <= 0)
            {
                return;
            }

            SetLV_VolumesItemInclude(lvSelect[0], LV_VolumesItemInclude(lvSelect[0]) == false);
            m_bBrowseLoaded = false;
            RestartTreeTimer();
        }

        private bool LV_VolumesItemInclude(ListViewItem lvItem)
        {
            return (lvItem.SubItems[4].Text == "Yes");
        }

        private void form_btn_SavePathInfo_Click(object sender, EventArgs e)
        {
            DoSaveDirListings();
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
                fs.WriteLine(Utilities.m_str_VOLUME_LIST_HEADER);

                foreach (ListViewItem lvItem in form_LV_VolumesMain.Items)
                {
                    foreach (ListViewItem.ListViewSubItem lvSubitem in lvItem.SubItems)
                    {
                        fs.Write(lvSubitem.Text + '\t');
                    }

                    fs.WriteLine();
                }
            }
        }

        private void form_btn_LoadVolumeList_Click(object sender, EventArgs e)
        {
            timer_DoTree.Stop();
            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
            openFileDialog1.FileName = saveFileDialog1.FileName;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (TextReader fs = File.OpenText(openFileDialog1.FileName))
            {
                String strLine = fs.ReadLine();

                if (new String[] { Utilities.m_str_VOLUME_LIST_HEADER_01, Utilities.m_str_VOLUME_LIST_HEADER}.Contains(strLine) == false)
                {
                    MessageBox.Show(openFileDialog1.FileName + " is not a valid volume list file.", "Load Volume List");
                    return;
                }

                form_LV_VolumesMain.Items.Clear();

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

                    form_LV_VolumesMain.Items.Add(new ListViewItem(strArray));
                }
            }

            if (form_LV_VolumesMain.Items.Count > 0)
            {
                form_btn_SavePathInfo.Enabled = true;
            }

            m_bBrowseLoaded = false;
            DoTree(true);
        }

        private void UpdateLV_VolumesSelection()
        {
            bool bHasSelection = (form_LV_VolumesMain.SelectedIndices.Count > 0);

            form_btn_VolGroup.Enabled = bHasSelection;
            form_btn_RemoveVolume.Enabled = bHasSelection;
            form_btn_ToggleInclude.Enabled = bHasSelection;
        }

        private void form_lv_Volumes_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            UpdateLV_VolumesSelection();
        }
      
        private void form_tabPage_Browse_Paint(object sender, PaintEventArgs e)
        {
            DoTree();
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
            if (sender == form_treeView_compare2)
            {
                Debug.Assert(m_bCompareMode);
                form_lv_FileCompare.Items.Clear();
                form_LV_DetailVol.Items.Clear();
            }
            else
            {
                form_LV_Files.Items.Clear();
                form_LV_Detail.Items.Clear();

                if (m_bCompareMode == false)
                {
                    form_LV_DetailVol.Items.Clear();
                }
            }

            String strCompareDir = null;

            if (sender == form_treeView_compare1)
            {
                strCompareDir = form_treeView_compare1.SelectedNode.Text;
            }
            else if (sender == form_treeView_compare2)
            {
                strCompareDir = form_treeView_compare2.SelectedNode.Text;
            }

            Debug.Assert((new object[] { form_treeView_compare1, form_treeView_compare2 }.Contains(sender)) == m_bCompareMode);
            Debug.Assert((strCompareDir != null) == m_bCompareMode);
            DoTreeSelect(e.Node, strCompareDir);

            if (m_bCompareMode)
            {
                return;
            }

            form_lbl_VolGroup.Text = ((RootNodeDatum)TreeSelect.GetParentRoot(e.Node).Tag).StrVolumeGroup;

            if (m_bPutPathInFindEditBox)
            {
                m_bPutPathInFindEditBox = false;
                form_cb_TreeFind.Text = e.Node.FullPath;
            }

            NodeDatum nodeDatum = (NodeDatum)e.Node.Tag;

            if (nodeDatum.NumImmediateFiles == 0)
            {
                form_col_Filename.Text = m_strFilesColOrig;
            }

            if ((nodeDatum.m_lvCloneItem != null) && (nodeDatum.m_lvCloneItem.Selected == false))
            {
                nodeDatum.m_lvCloneItem.Selected = true;
                nodeDatum.m_lvCloneItem.Focused = true;

                if (form_LV_Clones.Items.Contains(nodeDatum.m_lvCloneItem))
                {
                    form_LV_Clones.TopItem = nodeDatum.m_lvCloneItem;
                }
                else if (form_lv_Unique.Items.Contains(nodeDatum.m_lvCloneItem))
                {
                    form_lv_Unique.TopItem = nodeDatum.m_lvCloneItem;
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            DoSearch();
        }

        private void form_cb_Search_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Keys[] {Keys.Enter, Keys.Return}.Contains((Keys) e.KeyChar))
            {
                btnSearch_Click(sender, e);
            }
        }

        private void form_lvClones_SelectedIndexChanged(object sender, EventArgs e)
        {
            //use this code if amending this method to do more than just reset the clone click index.
            //if (bCompareMode)
            //{
            //    return;
            //}

            //if (form_lvClones.Focused == false)
            //{
            //    return;
            //}

            m_nLVclonesClickIndex = -1;
        }

        private void form_lvClones_MouseClick(object sender, MouseEventArgs e)
        {
            if (form_LV_Clones.SelectedItems.Count == 0)
            {
                return;
            }

            if (form_LV_Clones.SelectedItems[0].Tag == null)
            {
                // marker item
                ListViewItem lvItem = form_LV_Clones.Items[form_LV_Clones.SelectedItems[0].Index + 1];

                lvItem.Selected = true;
                lvItem.Focused = true;
            }

            ++m_nLVclonesClickIndex;

            List<TreeNode> listTreeNodes = (List<TreeNode>)form_LV_Clones.SelectedItems[0].Tag;

            m_bPutPathInFindEditBox = true;
            form_treeView_Browse.SelectedNode = listTreeNodes[m_nLVclonesClickIndex % listTreeNodes.Count];
            form_treeView_Browse.Select();
        }

        private TreeNode GetNodeByPath(string path, TreeView treeView)
        {
            if ((path == null) || (path.Length == 0))
            {
                return null;
            }

            TreeNode node = null;
            string[] pathLevel = path.ToLower().Split(Path.DirectorySeparatorChar);
            int i = 0;
            int nPathLevelLength = pathLevel.Length;

            foreach (TreeNode topNode in treeView.Nodes)
            {
                String strNode = topNode.Text.ToLower();

                pathLevel = path.ToLower().Split(Path.DirectorySeparatorChar);
                nPathLevelLength = pathLevel.Length;

                if (strNode.Contains(Path.DirectorySeparatorChar))
                {
                    int nCount = strNode.Count(c => c == Path.DirectorySeparatorChar);

                    for (int n = 0; n < nPathLevelLength - 1; ++n)
                    {
                        if (n < nCount)
                        {
                            pathLevel[0] += Path.DirectorySeparatorChar + pathLevel[n + 1];
                        }
                    }

                    for (int n = 1; n < nPathLevelLength-1; ++n)
                    {
                        if ((nCount + n) < pathLevel.Length)
                        {
                            pathLevel[n] = pathLevel[nCount + n];
                        }
                    }

                    if (nPathLevelLength > 1)
                    {
                        Debug.Assert((nPathLevelLength - nCount) > 0);
                        nPathLevelLength -= nCount;
                    }
                }
                
                if (strNode == pathLevel[i])
                {
                    node = topNode;
                    i++;
                    break;
                }
            }

            if ((i < nPathLevelLength) && node != null)
            {
                node = GetSubNode(node, pathLevel, i, nPathLevelLength);
            }

            return node;
        }

        private TreeNode GetSubNode(TreeNode node, string[] pathLevel, int i, int nPathLevelLength)
        {
            foreach (TreeNode subNode in node.Nodes)
            {
                if (subNode.Text.ToLower() != pathLevel[i])
                {
                    continue;
                }

                if (++i == nPathLevelLength)
                {
                    return subNode;
                }

                return GetSubNode(subNode, pathLevel, i, nPathLevelLength);
            }

            return null;
        }

        TreeNode FindNode(String strSearch, TreeNode startNode = null, TreeView treeView = null)
        {
            if ((strSearch == null) || (strSearch.Length == 0))
            {
                return null;
            }

            if (treeView == null)
            {
                treeView = form_treeView_Browse;
            }

            if (startNode != null)
            {
                treeView = startNode.TreeView;
            }
            else if (m_bCompareMode)
            {
                treeView = (m_ctlLastFocusForCopyButton is TreeView) ? (TreeView)m_ctlLastFocusForCopyButton : form_treeView_compare1;
            }

            TreeNode treeNode = GetNodeByPath(strSearch, treeView);

            if (treeNode == null)
            {
                // case sensitive only when user enters an uppercase character

                List<TreeNode> listTreeNodes = m_listTreeNodes;

                if (m_bCompareMode)
                {
                    listTreeNodes = (treeView == form_treeView_compare2) ? m_listTreeNodes_Compare2 : m_listTreeNodes_Compare1;
                }

                if (strSearch.ToLower() == strSearch)
                {
                    m_arrayTreeFound = listTreeNodes.FindAll(node => node.Text.ToLower().Contains(strSearch)).ToArray();
                }
                else
                {
                    m_arrayTreeFound = listTreeNodes.FindAll(node => node.Text.Contains(strSearch)).ToArray();
                }
            }
            else
            {
                m_arrayTreeFound = new TreeNode[] { treeNode };
            }

            if ((m_arrayTreeFound != null) && (m_arrayTreeFound.Length > 0))
            {
                if (m_arrayTreeFound.Contains(startNode))
                {
                    m_nTreeFindTextChanged = m_arrayTreeFound.Count(node => node != startNode);
                    return startNode;
                }
                else
                {
                    return m_arrayTreeFound[0];
                }
            }
            else
            {
                return null;
            }
        }

        private void form_btn_TreeFind_Click(object sender, EventArgs e)
        {
            TreeView treeView = form_treeView_Browse;

            if (m_bCompareMode)
            {
                treeView = (m_ctlLastFocusForCopyButton is TreeView) ? (TreeView)m_ctlLastFocusForCopyButton : form_treeView_compare1;
            }

            do
            {
                if (m_nTreeFindTextChanged == 0)
                {
                    FindNode(form_cb_TreeFind.Text, treeView.SelectedNode, treeView);
                }

                if ((m_arrayTreeFound != null) && (m_arrayTreeFound.Length > 0))
                {
                    TreeNode treeNode = m_arrayTreeFound[m_nTreeFindTextChanged % m_arrayTreeFound.Length];

                    treeNode.TreeView.SelectedNode = treeNode;
                    ++m_nTreeFindTextChanged;
                    blink.Go(Once: true);
                }
                else if (treeView == form_treeView_compare1)
                {
                    treeView = form_treeView_compare2;
                    continue;
                }
                else
                {
                    m_nTreeFindTextChanged = 0;
                    blink.Go(clr: Color.Red, Once: true);
                    MessageBox.Show("Couldn't find the specified search parameter in the tree.", "Search in Tree");
                }

                break;
            }
            while (true);
        }

        private void form_edit_TreeFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Keys[] { Keys.Enter, Keys.Return }.Contains((Keys)e.KeyChar))
            {
                m_bPutPathInFindEditBox = false;    // because the search term may not be the complete path: Volume Group gets updated though.
                form_btn_TreeFind_Click(sender, e);
                e.Handled = true;
            }
        }

        private void form_btn_TreeCollapse_Click(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                form_btn_ComparePrev_Click(sender, e);
            }
            else
            {
                form_treeView_Browse.CollapseAll();
            }
        }

        private void form_edit_TreeFind_TextChanged(object sender, EventArgs e)
        {
            m_nTreeFindTextChanged = 0;
        }

        private void form_treeView_Browse_MouseClick(object sender, MouseEventArgs e)
        {
            m_bPutPathInFindEditBox = true;
        }

        private void form_cb_TreeFind_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cb_TreeFind, bTrimText: false);
        }

        private void form_lv_Unique_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                return;
            }

            if (form_lv_Unique.SelectedItems.Count == 0)
            {
                return;
            }

            if (form_lv_Unique.Focused == false)
            {
                return;
            }

            m_bPutPathInFindEditBox = true;

            TreeNode treeNode = form_treeView_Browse.SelectedNode = (TreeNode)form_lv_Unique.SelectedItems[0].Tag;

            if (treeNode == null)
            {
                return;
            }

            treeNode.Expand();
            form_treeView_Browse.TopNode = treeNode.Parent;

            if (treeNode.IsVisible)
            {
                return;
            }

            for (int i = 0; i < 5; ++i)
            {
                TreeNode neighbor = treeNode;

                if (neighbor.PrevNode != null)
                {
                    neighbor = form_treeView_Browse.TopNode = neighbor.PrevNode;

                    if (treeNode.IsVisible == false)
                    {
                        neighbor.Collapse();
                    }
                }
                else
                {
                    form_treeView_Browse.TopNode = neighbor;
                    break;
                }
            }
        }

        private void form_chk_Compare1_CheckedChanged(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                form_chk_Compare1.Text = m_strChkCompareOrig;
                form_btn_TreeCollapse.Text = m_strBtnTreeCollapseOrig;
                form_btn_Compare.Text = m_strBtnCompareOrig;
                form_col_Filename.Text = m_strFilesColOrig;
                form_colFileCompare.Text = m_strFileCompareColOrig;
                form_colVolDetail.Text = m_strVolDetailColOrig;
                form_lbl_VolGroup.Text = m_strVolGroupOrig;
                form_lbl_VolGroup.Font = m_FontVolGroupOrig;
                split_TreeFind.Panel1Collapsed = true;
                split_TreeFind.Panel2Collapsed = false;
                split_FileCompare.Panel2Collapsed = true;
                split_Clones.Panel2Collapsed = false;
                m_strCompare1 = null;
                m_nodeCompare1 = null;
                m_nodeCompare2 = null;
                form_treeView_compare1.Nodes.Clear();
                form_treeView_compare2.Nodes.Clear();
                form_chk_Compare1.Checked = false;
                form_btn_Compare.Enabled = false;
                m_bCompareMode = false;
                form_treeView_Browse_AfterSelect(form_treeView_Browse, new TreeViewEventArgs(form_treeView_Browse.SelectedNode));
            }
            else if (form_chk_Compare1.Checked)
            {
                m_strCompare1 = form_cb_TreeFind.Text;

                bool bError = (m_strCompare1.Length == 0);

                if (bError == false)
                {
                    m_nodeCompare1 = FindNode(m_strCompare1, form_treeView_Browse.SelectedNode);
                    bError = (m_nodeCompare1 == null);
                }

                Color clrBlink = Color.DarkTurquoise;

                if (bError)
                {
                    clrBlink = Color.Red;
                    form_chk_Compare1.Checked = false;  // event retriggers this handler
                }
                else
                {
                    form_treeView_Browse.SelectedNode = m_nodeCompare1;
                    form_btn_Compare.Enabled = true;
                }

                blink.Go(clr: clrBlink);
            }
            else
            {
                form_btn_Compare.Enabled = false;
                m_strCompare1 = null;
            }
        }

        class Blink
        {
            System.Windows.Forms.Timer m_timer = null;
            Control m_defaultControl = null;
            Color m_clrControlOrig = Color.Empty;
            Color m_clrBlink = Color.DarkTurquoise;
            int m_nBlink = 0;
            Control m_ctlBlink = null;
            int m_nNumBlinks = 10;
            ListViewItem m_lvItem = null;

            public void Go(Control ctl = null, ListViewItem lvItem = null, Color? clr = null, bool Once = false)
            {
                m_lvItem = lvItem;

                if (m_timer.Enabled)
                {
                    Reset();
                    SetCtlBackColor(m_clrControlOrig);
                }

                if (m_lvItem != null)
                {
                    m_clrControlOrig = m_lvItem.BackColor;
                    m_lvItem.Selected = false;
                    m_ctlBlink = null;
                }
                else
                {
                    m_ctlBlink = ctl ?? m_defaultControl;
                    m_clrControlOrig = m_ctlBlink.BackColor;
                }

                m_clrBlink = clr ?? Color.Turquoise;
                m_nBlink = 0;
                m_nNumBlinks = Once ? 2 : 10;
                m_timer.Interval = Once ? 100 : 50;
                m_timer.Enabled = true;
            }

            void Reset()
            {
                m_timer.Enabled = false;
                m_nBlink = 0;

                if (m_lvItem != null)
                {
                    m_lvItem.Selected = true;
                }
            }

            void SetCtlBackColor(Color clr)
            {
                if (m_lvItem != null)
                {
                    m_lvItem.BackColor = clr;
                }
                else
                {
                    m_ctlBlink.BackColor = clr;
                }
            }

            public void Tick()
            {
                Color colorChg = m_clrControlOrig;

                if (++m_nBlink >= m_nNumBlinks)
                {
                    Reset();
                }
                else if (((m_lvItem != null) && (m_lvItem.BackColor != m_clrBlink)) ||
                    (m_ctlBlink.BackColor != m_clrBlink))
                {
                    colorChg = m_clrBlink;
                }

                SetCtlBackColor(colorChg);
            }

            public Blink(System.Windows.Forms.Timer timer, Control defaultControl)
            {
                m_timer = timer;
                m_defaultControl = defaultControl;
                m_clrControlOrig = defaultControl.BackColor;
            }
        }

        private void timer_blink_Tick(object sender, EventArgs e)
        {
            blink.Tick();
        }

        bool Compare(TreeNode t1, TreeNode t2, bool bReverse = false, bool bMin10M = true)
        {
            bool bRet = true;

            foreach (TreeNode s1 in t1.Nodes)
            {
                bool bCompare = true;
                bool bCompareSub = true;
                TreeNode s2 = null;
                NodeDatum n1 = (NodeDatum)s1.Tag;

                if (bMin10M && (n1.LengthSubnodes <= 10 * 1024 * 1024))
                {
                }
                else if (t2.Nodes.ContainsKey(s1.Name))
                {
                    s2 = t2.Nodes[s1.Name];

                    if (Compare(s1, s2, bReverse, bMin10M) == false)
                    {
                        bCompareSub = false;
                    }

                    NodeDatum n2 = (NodeDatum)s2.Tag;

                    bCompare &= (n1.NumImmediateFiles == n2.NumImmediateFiles);
                    bCompare &= (Math.Abs(n1.Length - n2.Length) <= (bMin10M ? (10 * 1024 * 1024) : 0));

                    if (bCompare == false)
                    {
                        s2.ForeColor = Color.Red;
                    }
                }
                else
                {
                    bCompare = false;
                }

                if (bCompare == false)
                {
                    s1.ForeColor = Color.Red;

                    TreeNode r1 = bReverse ? s2 : s1;
                    TreeNode r2 = bReverse ? s1 : s2;

                    if ((r1 != null) && dictCompareDiffs.ContainsKey(r1) == false)
                    {
                        dictCompareDiffs.Add(r1, r2);
                    }
                    else if (dictCompareDiffs.ContainsValue(r2) == false)
                    {
                        dictCompareDiffs.Add(new TreeNode(), r2);
                    }
                }
                else if (bCompareSub == false)
                {
                    s1.ForeColor = Color.DarkRed;
                }

                bRet &= (bCompare && bCompareSub);
            }

            return bRet;
        }

        void NameNodes(TreeNode treeNode, List<TreeNode> listTreeNodes)
        {
            treeNode.Name = treeNode.Text;
            listTreeNodes.Add(treeNode);

            foreach (TreeNode subNode in treeNode.Nodes)
            {
                NameNodes(subNode, listTreeNodes);
            }
        }

        private void form_btn_Compare_Click(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                form_btn_CompareNext_Click(sender, e);
            }
            else
            {
                form_cb_TreeFind.BackColor = Color.Empty;
                Debug.Assert(form_chk_Compare1.Checked);
                Debug.Assert(m_strCompare1.Length > 0);
                String strCompare2 = form_cb_TreeFind.Text;

                bool bError = (strCompare2.Length == 0);

                if (bError == false)
                {
                    m_nodeCompare2 = FindNode(strCompare2, form_treeView_Browse.SelectedNode);
                    bError = ((m_nodeCompare2 == null) || (m_nodeCompare2 == m_nodeCompare1));
                }

                if (bError)
                {
                    blink.Go(clr: Color.Red);
                }
                else
                {
                    blink.Go();
                    split_TreeFind.Panel1Collapsed = false;
                    split_TreeFind.Panel2Collapsed = true;
                    split_FileCompare.Panel2Collapsed = false;
                    split_Clones.Panel2Collapsed = true;

                    form_treeView_Browse.SelectedNode = m_nodeCompare2;

                    RootNodeDatum rootNodeDatum1=(RootNodeDatum)TreeSelect.GetParentRoot(m_nodeCompare1).Tag;
                    RootNodeDatum rootNodeDatum2=(RootNodeDatum)TreeSelect.GetParentRoot(m_nodeCompare2).Tag;
                    String strFullPath1 = m_nodeCompare1.FullPath;
                    String strFullPath2 = m_nodeCompare2.FullPath;
                    m_nodeCompare1 = (TreeNode)m_nodeCompare1.Clone();
                    m_nodeCompare2 = (TreeNode)m_nodeCompare2.Clone();
                    m_listTreeNodes_Compare1.Clear();
                    m_listTreeNodes_Compare2.Clear();
                    NameNodes(m_nodeCompare1, m_listTreeNodes_Compare1);
                    NameNodes(m_nodeCompare2, m_listTreeNodes_Compare2);
                    m_nodeCompare1.Name = strFullPath1;
                    m_nodeCompare2.Name = strFullPath2;
                    m_nodeCompare1.Tag = new RootNodeDatum((NodeDatum)m_nodeCompare1.Tag, rootNodeDatum1.StrFile, rootNodeDatum1.StrVolumeGroup);
                    m_nodeCompare2.Tag = new RootNodeDatum((NodeDatum)m_nodeCompare2.Tag, rootNodeDatum2.StrFile, rootNodeDatum2.StrVolumeGroup);
                    m_nodeCompare2.Checked = true;    // hack to put it in the right file pane
                    dictCompareDiffs.Clear();
                    Compare(m_nodeCompare1, m_nodeCompare2);
                    Compare(m_nodeCompare2, m_nodeCompare1, true);

                    if (dictCompareDiffs.Count < 15)
                    {
                        dictCompareDiffs.Clear();
                        Compare(m_nodeCompare1, m_nodeCompare2, bMin10M: false);
                        Compare(m_nodeCompare2, m_nodeCompare1, true, bMin10M: false);
                    }

                    SortedDictionary<long, KeyValuePair<TreeNode, TreeNode>> dictSort = new SortedDictionary<long, KeyValuePair<TreeNode, TreeNode>>();

                    foreach (KeyValuePair<TreeNode, TreeNode> pair in dictCompareDiffs)
                    {
                        long l1 = 0, l2 = 0;

                        if (pair.Key.Text.Length > 0)
                        {
                            l1 = ((NodeDatum)pair.Key.Tag).Length;
                        }

                        if (pair.Value != null)
                        {
                            l2 = ((NodeDatum)pair.Value.Tag).Length;
                        }

                        long lMax = Math.Max(l1, l2);

                        while (dictSort.ContainsKey(lMax))
                        {
                            --lMax;
                        }

                        dictSort.Add(lMax, pair);
                    }

                    dictCompareDiffs.Clear();

                    foreach (KeyValuePair<TreeNode, TreeNode> pair in dictSort.Values.Reverse())
                    {
                        dictCompareDiffs.Add(pair.Key, pair.Value);
                    }

                    form_treeView_compare1.Nodes.Add(m_nodeCompare1);
                    form_treeView_compare2.Nodes.Add(m_nodeCompare2);
                    m_nCompareIndex = -1;
                    form_btn_Compare.Select();
                    form_btn_Compare.Text = "> >";
                    form_chk_Compare1.Text = "0 of " + dictCompareDiffs.Count;
                    form_btn_TreeCollapse.Text = "< <";
                    form_colVolDetail.Text = "Directory detail";
                    form_lbl_VolGroup.Text = "Compare Mode";
                    form_lbl_VolGroup.Font = new Font(m_FontVolGroupOrig, FontStyle.Regular | FontStyle.Bold);
                    m_bCompareMode = true;
                    form_treeView_compare1.SelectedNode = form_treeView_compare1.Nodes[0];
                    form_treeView_compare2.SelectedNode = form_treeView_compare2.Nodes[0];
                }
            }
        }

        void CompareNav()
        {
            Console.WriteLine(dictCompareDiffs.ToArray()[m_nCompareIndex]);
            form_chk_Compare1.Text = m_nCompareIndex + 1 + " of " + dictCompareDiffs.Count;
            form_LV_Files.Items.Clear();
            form_lv_FileCompare.Items.Clear();
            form_LV_Detail.Items.Clear();
            form_LV_DetailVol.Items.Clear();

            TreeNode treeNode = dictCompareDiffs.ToArray()[m_nCompareIndex].Key;

            if (treeNode.Name.Length == 0)  // can't have a null key in the dictionary so there's a new TreeNode there
            {
                treeNode = null;
            }

            form_treeView_compare1.TopNode = form_treeView_compare1.SelectedNode = treeNode;
            form_treeView_compare2.TopNode = form_treeView_compare2.SelectedNode = dictCompareDiffs.ToArray()[m_nCompareIndex].Value;

            if (form_treeView_compare1.SelectedNode == null)
            {
                form_col_Filename.Text = m_strFilesColOrig;
                form_treeView_compare1.CollapseAll();
            }
            else
            {
                form_treeView_compare1.SelectedNode.EnsureVisible();
            }

            if (form_treeView_compare2.SelectedNode == null)
            {
                form_colFileCompare.Text = m_strFileCompareColOrig;
                form_treeView_compare2.CollapseAll();
            }
            else
            {
                form_treeView_compare2.SelectedNode.EnsureVisible();
            }
        }

        private void form_btn_ComparePrev_Click(object sender, EventArgs e)
        {
            --m_nCompareIndex;

            if (m_nCompareIndex < 0)
            {
                m_nCompareIndex = 0;
            }
            else
            {
                CompareNav();
            }
        }

        private void form_btn_CompareNext_Click(object sender, EventArgs e)
        {
            ++m_nCompareIndex;

            if (m_nCompareIndex > dictCompareDiffs.Count - 1)
            {
                m_nCompareIndex = dictCompareDiffs.Count - 1;
            }
            else
            {
                CompareNav();
            }
        }

        private void form_lv_Unique_KeyPress(object sender, KeyPressEventArgs e)
        {
            {
                ListView.SelectedListViewItemCollection lvSel = form_lv_Unique.SelectedItems;

                if (lvSel.Count > 0)
                {
                    int nIx = form_lv_Unique.SelectedItems[0].Index;

                    // accidentally trying to compare too early (using < and >)
                    if (e.KeyChar == '.')
                    {
                        if (nIx < form_lv_Unique.Items.Count - 1)
                        {
                            form_lv_Unique.Items[nIx + 1].Selected = true;
                            form_lv_Unique.Items[nIx + 1].Focused = true;
                        }

                        e.Handled = true;
                    }
                    else if (e.KeyChar == ',')
                    {
                        if (nIx > 0)
                        {
                            form_lv_Unique.Items[nIx - 1].Selected = true;
                            form_lv_Unique.Items[nIx - 1].Focused = true;
                        }

                        e.Handled = true;
                    }
                }
            }

            if (e.Handled)
            {
                return;
            }
            else if (e.KeyChar == 3)
            {
                // Copy
                CopyToClipboard();
                e.Handled = true;
            }
            else if (e.KeyChar == '!')
            {
                if (m_bCompareMode)
                {
                    Debug.Assert(false);    // the listviewer shouldn't even be visible
                    return;
                }

                e.Handled = true;

                if (form_btn_Compare.Enabled == false)
                {
                    form_chk_Compare1.Checked = true;
                }
                else
                {
                    form_btn_Compare_Click(sender, e);
                }
            }
        }

        void CompareModeButtonKeyPress(object sender, KeyPressEventArgs e)
        {
            if (m_bCompareMode == false)
            {
                return;
            }

            if (e.KeyChar == '.')
            {
                form_btn_CompareNext_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyChar == ',')
            {
                form_btn_ComparePrev_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyChar == '!')
            {
                form_chk_Compare1.Checked = false;
                form_lv_Unique.Select();
                e.Handled = true;
            }
            else if (e.KeyChar == 3)
            {
                // Copy
                CopyToClipboard();
                e.Handled = true;
            }
        }

        private void form_treeView_Browse_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 3)
            {
                // Copy
                CopyToClipboard();
                e.Handled = true;
                return;
            }
            else if (e.KeyChar != '!')
            {
                return;
            }

            e.Handled = true;

            if (m_bCompareMode)
            {
                form_chk_Compare1.Checked = false;          // leave Compare mode
            }
            else if (form_btn_Compare.Enabled == false)
            {
                form_chk_Compare1.Checked = true;           // enter first path to compare
            }
            else
            {
                form_btn_Compare_Click(sender, e);          // enter second path and start Compare mode
            }
        }

        void CopyToClipboard()
        {
            if (form_cb_TreeFind.Text.Length <= 0)
            {
                blink.Go(clr: Color.Red, Once: true);
                return;
            }

            Clipboard.SetText(form_cb_TreeFind.Text);
            blink.Go(Once: true);
        }

        private void form_btn_Copy_Click(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                if (new object[] { form_treeView_compare1, form_treeView_compare2 }.Contains(m_ctlLastFocusForCopyButton))
                {
                    form_tree_compare_KeyPress(m_ctlLastFocusForCopyButton, new KeyPressEventArgs((char)3));
                }
                else if (new object[] { form_LV_Files, form_lv_FileCompare }.Contains(m_ctlLastFocusForCopyButton))
                {
                    FileListKeyPress((ListView) m_ctlLastFocusForCopyButton, new KeyPressEventArgs((char)3));
                }
                else
                {
                    CopyToClipboard();
                    blink.Go(clr: Color.Yellow);
                }
            }
            else
            {
                CopyToClipboard();
            }
        }

        private void form_lv_Unique_MouseClick(object sender, MouseEventArgs e)
        {
            if (form_lv_Unique.SelectedItems.Count <= 0)
            {
                return;
            }

            if (form_lv_Unique.SelectedItems[0].Tag == null)
            {
                // marker item
                form_lv_Unique.Select();

                ListViewItem lvItem = form_lv_Unique.Items[form_lv_Unique.SelectedItems[0].Index + 1];
                
                lvItem.Selected = true;
                lvItem.Focused = true;
                return;
            }
        }

        private void form_btn_VolGroup_Click(object sender, EventArgs e)
        {
            if (form_LV_VolumesMain.SelectedItems.Count == 0)
            {
                return;
            }

            timer_DoTree.Stop();
            InputBox inputBox = new InputBox();

            inputBox.Text = "Volume Group";
            inputBox.Prompt = "Enter a volume group name";
            inputBox.Entry = form_LV_VolumesMain.SelectedItems[0].SubItems[5].Text;

            SortedDictionary<String, object> dictVolGroups = new SortedDictionary<string, object>();

            foreach (ListViewItem lvItem in form_LV_VolumesMain.Items)
            {
                String strVolGroup = lvItem.SubItems[5].Text;

                if (dictVolGroups.ContainsKey(strVolGroup) == false)
                {
                    dictVolGroups.Add(strVolGroup, null);
                }
            }

            foreach (KeyValuePair<String, object> entry in dictVolGroups)
            {
                inputBox.AddSelector(entry.Key);
            }

            if (inputBox.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            form_LV_VolumesMain.SelectedItems[0].SubItems[5].Text = inputBox.Entry;
            RestartTreeTimer();
        }

        private void timer_DoTree_Tick(object sender, EventArgs e)
        {
            timer_DoTree.Stop();
            DoTree(true);
        }

        void RestartTreeTimer()
        {
            timer_DoTree.Stop();
            timer_DoTree.Start();
        }

        private void form_btn_SavePathInfo_EnabledChanged(object sender, EventArgs e)
        {
            form_btn_SaveVolumeList.Enabled = form_btn_SavePathInfo.Enabled;
        }

        void PutTreeCompareNodePathIntoFindCombo(TreeView treeView)
        {
            if (treeView.SelectedNode == null)
            {
                return;
            }

            TreeNode treeNode = treeView.SelectedNode;

            form_cb_TreeFind.Text = TreeSelect.GetParentRoot(treeNode).Name;

            if (treeNode.Level <= 0)
            {
                return;
            }

            String strFullPath = treeNode.FullPath;

            form_cb_TreeFind.Text += strFullPath.Substring(strFullPath.IndexOf(Path.DirectorySeparatorChar));
        }

        private void form_tree_compare_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 3)
            {
                PutTreeCompareNodePathIntoFindCombo((TreeView)sender);
            }

            CompareModeButtonKeyPress(sender, e);
        }

        void FileListKeyPress(ListView lv, KeyPressEventArgs e)
        {
            if ((e.KeyChar != 3) && m_bCompareMode)
            {
                CompareModeButtonKeyPress(lv, e);
            }

            if ((e.KeyChar != 3) || (lv.SelectedItems.Count <= 0))
            {
                return;
            }

            if (m_bCompareMode)
            {
                PutTreeCompareNodePathIntoFindCombo((lv == form_LV_Files) ? form_treeView_compare1 : form_treeView_compare2);
            }

            Clipboard.SetText(form_cb_TreeFind.Text + Path.DirectorySeparatorChar + lv.SelectedItems[0].Text);
            blink.Go(lvItem: lv.SelectedItems[0], Once: true);
            e.Handled = true;
        }

        private void form_LV_Files_KeyPress(object sender, KeyPressEventArgs e)
        {
            FileListKeyPress(form_LV_Files, e);
        }

        private void form_lv_FileCompare_KeyPress(object sender, KeyPressEventArgs e)
        {
            FileListKeyPress(form_lv_FileCompare, e);
        }

        private void formCtl_EnterForCopyButton(object sender, EventArgs e)
        {
            m_ctlLastFocusForCopyButton = (Control) sender;
            m_nTreeFindTextChanged = 0;
        }
    }
}
