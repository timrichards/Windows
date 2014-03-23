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
        String m_strCompare2 = null;
        TreeNode m_nodeCompare1 = null;
        TreeNode m_nodeCompare2 = null;
        int m_nCompareIndex = -1;
        String m_strBtnTreeCollapseOrig = null;
        String m_strFilesColOrig = null;
        String m_strFileCompareColOrig = null;
        String m_strVolDetailColOrig = null;
        String m_strBtnCompareOrigText = null;
        String m_strChkCompareOrigText = null;
        bool bComparing = false;
        Color clrBlink = Color.DarkTurquoise;
        int nBlink = 0;
        Dictionary<TreeNode, TreeNode> dictCompareDiffs = new Dictionary<TreeNode, TreeNode>();

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
                    String line = file.ReadLine();

                    if (line == null) break;
                    line = file.ReadLine();
                    if (line == null) break;
                    form_cb_VolumeName.Text = line.Split('\t')[2];
                    line = file.ReadLine();
                    if (line == null) break;
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
            DoTree(true);
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
            DoTree(true);
        }

        private bool LV_VolumesItemInclude(ListViewItem lvItem)
        {
            return (lvItem.SubItems[4].Text == "Yes");
        }

        private void form_btn_SavePathInfo_Click(object sender, EventArgs e)
        {
            DoSavePathInfo();
            DoTree(true);
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
            if (sender == tree_compare2)
            {
                Debug.Assert(bComparing);
                form_lv_FileCompare.Items.Clear();
                form_LV_DetailVol.Items.Clear();
            }
            else
            {
                form_LV_Files.Items.Clear();
                form_LV_Detail.Items.Clear();

                if (bComparing == false)
                {
                    form_LV_DetailVol.Items.Clear();
                }
            }

            String strFolder = null;

            if (sender == tree_compare1)
            {
                strFolder = tree_compare1.SelectedNode.Text;
            }
            else if (sender == tree_compare2)
            {
                strFolder = tree_compare2.SelectedNode.Text;
            }

            Debug.Assert((new object[] { tree_compare1, tree_compare2 }.Contains(sender)) == bComparing);
            Debug.Assert((strFolder != null) == bComparing);

            DoTreeSelect(e.Node, strFolder);

            if (bComparing)
            {
                return;
            }

            if (m_bPutPathInFindEditBox)
            {
                m_bPutPathInFindEditBox = false;
                form_cb_TreeFind.Text = e.Node.FullPath;
            }

            NodeDatum nodeDatum = (NodeDatum)e.Node.Tag;

            if ((nodeDatum.m_lvCloneItem != null) && (nodeDatum.m_lvCloneItem.Selected == false))
            {
                nodeDatum.m_lvCloneItem.Selected = true;

                bool bFirst = true;
                if (form_LV_Clones.Items.Contains(nodeDatum.m_lvCloneItem))
                {
                    form_LV_Clones.TopItem = nodeDatum.m_lvCloneItem;
                    bFirst = false;
                }

                bool bSecond = true;
                if (form_lv_Unique.Items.Contains(nodeDatum.m_lvCloneItem))
                {
                    form_lv_Unique.TopItem = nodeDatum.m_lvCloneItem;
                    bSecond = false;
                }

                Debug.Assert(bFirst || bSecond);    // just want to see what's obvious: listview item can't be in two lists
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
            //if (bComparing)
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

                if (strNode.Contains(Path.DirectorySeparatorChar))
                {
                    pathLevel = path.ToLower().Split(Path.DirectorySeparatorChar);
                    nPathLevelLength = pathLevel.Length;

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
                        Debug.Assert(nPathLevelLength > nCount + 1);
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

        TreeNode FindNode(String strSearch, TreeNode startNode = null)
        {
            if ((strSearch == null) || (strSearch.Length == 0))
            {
                return null;
            }

            TreeNode treeNode = GetNodeByPath(strSearch, form_treeView_Browse);

            if (treeNode == null)
            {
                // case sensitive only when user enters an uppercase character

                if (strSearch.ToLower() == strSearch)
                {
                    m_arrayTreeFound = m_listTreeNodes.FindAll(node => node.Text.ToLower().Contains(strSearch)).ToArray();
                }
                else
                {
                    m_arrayTreeFound = m_listTreeNodes.FindAll(node => node.Text.Contains(strSearch)).ToArray();
                }

                if ((m_arrayTreeFound == null) || (m_arrayTreeFound.Length == 0))
                {
                    MessageBox.Show("Couldn't find the specified search parameter in the tree.", "Search in Tree");
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
            if (m_nTreeFindTextChanged == 0)
            {
                FindNode(form_cb_TreeFind.Text, form_treeView_Browse.SelectedNode);
            }

            if ((m_arrayTreeFound != null) && (m_arrayTreeFound.Length > 0))
            {
                form_treeView_Browse.SelectedNode = m_arrayTreeFound[m_nTreeFindTextChanged % m_arrayTreeFound.Length];
                ++m_nTreeFindTextChanged;
            }
            else
            {
                m_nTreeFindTextChanged = 0;
            }
        }

        private void form_edit_TreeFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Keys[] { Keys.Enter, Keys.Return }.Contains((Keys)e.KeyChar))
            {
                m_bPutPathInFindEditBox = false;
                form_btn_TreeFind_Click(sender, e);
                e.Handled = true;
            }
        }

        private void form_btn_TreeCollapse_Click(object sender, EventArgs e)
        {
            if (bComparing)
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
            if (bComparing)
            {
                return;
            }

            if (form_lv_Unique.SelectedItems.Count == 0)
            {
                return;
            }

            m_bPutPathInFindEditBox = true;
            TreeNode treeNode = form_treeView_Browse.SelectedNode = (TreeNode)form_lv_Unique.SelectedItems[0].Tag;

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
            form_cb_TreeFind.BackColor = Color.Empty;
            nBlink = 0;

            if (bComparing)
            {
                form_chk_Compare1.Text = m_strChkCompareOrigText;
                form_btn_TreeCollapse.Text = m_strBtnTreeCollapseOrig;
                form_btn_Compare.Text = m_strBtnCompareOrigText;
                form_col_Filename.Text = m_strFilesColOrig;
                form_colFileCompare.Text = m_strFileCompareColOrig;
                form_colVolDetail.Text = m_strVolDetailColOrig;
                split_TreeFind.Panel1Collapsed = true;
                split_TreeFind.Panel2Collapsed = false;
                split_FileCompare.Panel2Collapsed = true;
                split_Clones.Panel2Collapsed = false;
                m_strCompare1 = null;
                m_strCompare2 = null;
                m_nodeCompare1 = null;
                m_nodeCompare2 = null;
                tree_compare1.Nodes.Clear();
                tree_compare2.Nodes.Clear();
                form_chk_Compare1.Checked = false;
                form_btn_Compare.Enabled = false;
                bComparing = false;
                form_treeView_Browse_AfterSelect(form_treeView_Browse, new TreeViewEventArgs(form_treeView_Browse.SelectedNode));
            }
            else if (form_chk_Compare1.Checked)
            {
                clrBlink = Color.DarkTurquoise;         // has to go here
                m_strCompare1 = form_cb_TreeFind.Text;

                bool bError = (m_strCompare1.Length == 0);

                if (bError == false)
                {
                    m_nodeCompare1 = FindNode(m_strCompare1, form_treeView_Browse.SelectedNode);
                    bError = (m_nodeCompare1 == null);
                }

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

                timer_blink.Enabled = true;
            }
            else
            {
                form_btn_Compare.Enabled = false;
                timer_blink.Enabled = false;
                m_strCompare1 = null;
            }
        }

        private void timer_blink_Tick(object sender, EventArgs e)
        {
            if (++nBlink >= 10)
            {
                form_cb_TreeFind.BackColor = Color.Empty;
                timer_blink.Enabled = false;
                nBlink = 0;
            }
            else if (form_cb_TreeFind.BackColor != clrBlink)
            {
                form_cb_TreeFind.BackColor = clrBlink;
            }
            else
            {
                form_cb_TreeFind.BackColor = Color.Empty;
            }
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

                if (bMin10M && (n1.LengthSubnodes < 10 * 1024 * 1024))
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
                    bCompare &= (Math.Abs(n1.Length - n2.Length) <= (bMin10M ? (100 * 1024) : 0));

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

        void NameNodes(TreeNode treeNode)
        {
            treeNode.Name = treeNode.Text;

            foreach (TreeNode subNode in treeNode.Nodes)
            {
                NameNodes(subNode);
            }
        }

        private void form_btn_Compare_Click(object sender, EventArgs e)
        {
            if (bComparing)
            {
                form_btn_CompareNext_Click(sender, e);
            }
            else
            {
                form_cb_TreeFind.BackColor = Color.Empty;
                nBlink = 0;
                Debug.Assert(form_chk_Compare1.Checked);
                Debug.Assert(m_strCompare1.Length > 0);
                clrBlink = Color.DarkTurquoise;
                m_strCompare2 = form_cb_TreeFind.Text;

                bool bError = (m_strCompare2.Length == 0);

                if (bError == false)
                {
                    m_nodeCompare2 = FindNode(m_strCompare2, form_treeView_Browse.SelectedNode);
                    bError = ((m_nodeCompare2 == null) || (m_nodeCompare2 == m_nodeCompare1));
                }

                if (bError)
                {
                    clrBlink = Color.Red;
                }
                else
                {
                    timer_blink.Enabled = true;
                    m_strBtnCompareOrigText = form_btn_Compare.Text;
                    m_strChkCompareOrigText = form_chk_Compare1.Text;
                    m_strBtnTreeCollapseOrig = form_btn_TreeCollapse.Text;
                    m_strVolDetailColOrig = form_colVolDetail.Text;
                    m_strFilesColOrig = form_col_Filename.Text;
                    m_strFileCompareColOrig = form_colFileCompare.Text;
                    form_btn_Compare.Text = "> >";
                    form_chk_Compare1.Text = "Comparing";
                    form_btn_TreeCollapse.Text = "< <";
                    form_colVolDetail.Text = "Directory detail";
                    split_TreeFind.Panel1Collapsed = false;
                    split_TreeFind.Panel2Collapsed = true;
                    split_FileCompare.Panel2Collapsed = false;
                    split_Clones.Panel2Collapsed = true;

                    form_treeView_Browse.SelectedNode = m_nodeCompare2;

                    String strFile1 = ((RootNodeDatum)TreeSelect.GetParentRoot(m_nodeCompare1).Tag).StrFile;
                    String strFile2 = ((RootNodeDatum)TreeSelect.GetParentRoot(m_nodeCompare2).Tag).StrFile;

                    m_nodeCompare1 = (TreeNode)m_nodeCompare1.Clone();
                    m_nodeCompare2 = (TreeNode)m_nodeCompare2.Clone();
                    tree_compare1.Nodes.Add(m_nodeCompare1);
                    tree_compare2.Nodes.Add(m_nodeCompare2);
                    NameNodes(m_nodeCompare1);
                    NameNodes(m_nodeCompare2);
                    m_nodeCompare1.Name = strFile1;
                    m_nodeCompare2.Name = strFile2;
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

                    m_nCompareIndex = -1;
                    form_btn_Compare.Select();
                    bComparing = true;
                    tree_compare1.SelectedNode = tree_compare1.Nodes[0];
                    tree_compare2.SelectedNode = tree_compare2.Nodes[0];
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

            tree_compare1.TopNode = tree_compare1.SelectedNode = treeNode;
            tree_compare2.TopNode = tree_compare2.SelectedNode = dictCompareDiffs.ToArray()[m_nCompareIndex].Value;

            if (tree_compare1.SelectedNode == null)
            {
                form_col_Filename.Text = "";
                tree_compare1.CollapseAll();
            }
            else
            {
                form_col_Filename.Text = tree_compare1.SelectedNode.Text;
                tree_compare1.SelectedNode.EnsureVisible();
            }

            if (tree_compare2.SelectedNode == null)
            {
                form_colFileCompare.Text = "";
                tree_compare2.CollapseAll();
            }
            else
            {
                form_colFileCompare.Text = tree_compare2.SelectedNode.Text;
                tree_compare2.SelectedNode.EnsureVisible();
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
                        }

                        e.Handled = true;
                    }
                    else if (e.KeyChar == ',')
                    {
                        if (nIx > 0)
                        {
                            form_lv_Unique.Items[nIx - 1].Selected = true;
                        }

                        e.Handled = true;
                    }
                }
            }

            if (e.Handled)
            {
                return;
            }
            else if (e.KeyChar == '!')
            {
                if (bComparing)
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

        private void form_btn_Compare_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (bComparing == false)
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
            }
        }

        private void form_treeView_Browse_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '!')
            {
                return;
            }

            e.Handled = true;

            if (bComparing)
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
    }
}
