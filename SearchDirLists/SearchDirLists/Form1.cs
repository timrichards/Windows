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
        int m_nLVclonesClickIndex = -1;
        int m_nTreeFindTextChanged = 0;
        bool m_bFileFound = false;
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
        Blink m_blink = null;
        String m_strBtnTreeCollapseOrig = null;
        String m_strColFilesOrig = null;
        String m_strColFileCompareOrig = null;
        String m_strColDirDetailCompareOrig = null;
        String m_strColDirDetailOrig = null;
        String m_strColVolDetailOrig = null;
        String m_strBtnCompareOrig = null;
        String m_strChkCompareOrig = null;
        String m_strVolGroupOrig = null;
        Font m_FontVolGroupOrig = null;
        Color m_clrVolGroupOrig = Color.Empty;

        public Form1()
        {
            InitializeComponent();
            m_blink = new Blink(timer_blink, form_cb_TreeFind);
            m_strBtnTreeCollapseOrig = form_btn_TreeCollapse.Text;
            m_strColFilesOrig = form_colFilename.Text;
            m_strColFileCompareOrig = form_colFileCompare.Text;
            m_strColDirDetailCompareOrig = form_colDirDetailCompare.Text;
            m_strColDirDetailOrig = form_colDirDetail.Text;
            m_strColVolDetailOrig = form_colVolDetail.Text;
            m_strBtnCompareOrig = form_btnCompare.Text;
            m_strChkCompareOrig = form_chkCompare1.Text;
            m_strVolGroupOrig = form_lblVolGroup.Text;
            m_FontVolGroupOrig = form_lblVolGroup.Font;
            m_clrVolGroupOrig = form_lblVolGroup.BackColor;
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
            m_strVolumeName = form_cbVolumeName.Text.Trim();
            m_strPath = form_cbPath.Text.Trim();

            if (m_strPath.Length > 0)
            {
                m_strPath += Path.DirectorySeparatorChar;

                if (FormatPath(form_cbPath, ref m_strPath, bFailOnDirectory) == false)
                {
                    return false;
                }
            }

            if (form_cbSaveAs.Text.Length > 0)
            {
                form_cbSaveAs.Text = m_strSaveAs = Path.GetFullPath(form_cbSaveAs.Text.Trim());

                if (FormatPath(form_cbSaveAs, ref m_strSaveAs, bFailOnDirectory) == false)
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
            ComboBoxItemsInsert(form_cbVolumeName, m_strPath);
            m_strPath = form_cbVolumeName.Text;
        }

        private void cb_Path_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cbPath, m_strPath);
            m_strPath = form_cbPath.Text;
        }

        private void cb_SaveAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cbSaveAs, m_strSaveAs);
            m_strSaveAs = form_cbSaveAs.Text;
        }

#endregion //Selected Index Changed

        private void form_btn_Path_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ComboBoxItemsInsert(form_cbPath);
            m_strPath = form_cbPath.Text = folderBrowserDialog1.SelectedPath;
        }

        private void form_btn_SaveAs_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ComboBoxItemsInsert(form_cbSaveAs);
            m_strSaveAs = form_cbSaveAs.Text = saveFileDialog1.FileName;

            if (File.Exists(m_strSaveAs))
            {
                form_cbVolumeName.Text = "";
                form_cbPath.Text = "";
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
                    form_cbVolumeName.Text = line.Split('\t')[2];
                    if ((line = file.ReadLine()) == null) break;
                    form_cbPath.Text = line.Split('\t')[2];
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

            form_cbVolumeName.BackColor = Color.Empty;
            form_cbPath.BackColor = Color.Empty;
            form_cbSaveAs.BackColor = Color.Empty;

            if (m_strSaveAs.Length <= 0)
            {
                FormError(form_cbSaveAs, "Must have a file to load or save directory listing to.", "Volume Save As");
                return;
            }

            if (form_lvVolumesMain.FindItemWithText(m_strSaveAs) != null)
            {
                FormError(form_cbSaveAs, "File already in use in list of volumes.            ", "Volume Save As");
                return;
            }

            if (File.Exists(m_strSaveAs) && (m_strPath.Length > 0))
            {
                if (MessageBox.Show(m_strSaveAs + " already exists. Overwrite?                 ", "Volume Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    form_cbSaveAs.BackColor = Color.Red;
                    timer_killRed.Enabled = true;
                    return;
                }
            }

            if ((File.Exists(m_strSaveAs) == false) && form_lvVolumesMain.Items.ContainsKey(m_strPath))
            {
                FormError(form_cbPath, "Path already added.                                   ", "Volume Source Path");
                return;
            }

            if ((m_strVolumeName.Length > 0) && form_lvVolumesMain.FindItemWithText(m_strVolumeName) != null)
            {
                form_cbVolumeName.BackColor = Color.Red;

                if (MessageBox.Show("Nickname already in use. Use it for more than one volume?", "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.Yes)
                {
                    return;
                }
                else
                {
                    form_cbVolumeName.BackColor = Color.Empty;
                }
            }

            if ((File.Exists(m_strSaveAs) == false) && (m_strPath.Length <= 0))
            {
                form_cbPath.BackColor = Color.Red;
                MessageBox.Show("Must have a path or existing directory listing file.  ", "Volume Source Path");
                return;
            }

            if ((m_strPath.Length > 0) && (Directory.Exists(m_strPath) == false))
            {
                form_cbPath.BackColor = Color.Red;
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
                            form_cbPath.BackColor = Color.Red;
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
                form_cbVolumeName.BackColor = Color.Red;

                if (MessageBox.Show("Would you like to enter a nickname for this volume?", "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.No)
                {
                    return;
                }
                else
                {
                    form_cbVolumeName.BackColor = Color.Empty;
                }
            }

            ListViewItem lvItem = new ListViewItem(new string[] { m_strVolumeName, m_strPath, m_strSaveAs, strStatus, "Yes" });

            lvItem.Name = m_strPath;
            form_lvVolumesMain.Items.Add(lvItem);
            form_btnSavePathInfo.Enabled = true;
            m_bBrowseLoaded = false;
            DoTree(true);
        }

        private void form_btn_RemoveVolume_Click(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection lvSelect = form_lvVolumesMain.SelectedIndices;

            if (lvSelect.Count <= 0)
            {
                return;
            }

            form_lvVolumesMain.Items[lvSelect[0]].Remove();
            UpdateLV_VolumesSelection();
            form_btnSavePathInfo.Enabled = (form_lvVolumesMain.Items.Count > 0);
            m_bBrowseLoaded = false;
            RestartTreeTimer();
        }

        private void SetLV_VolumesItemInclude(ListViewItem lvItem, bool bInclude)
        {
            lvItem.SubItems[4].Text = (bInclude) ? "Yes" : "No";
        }

        private void form_btn_ToggleInclude_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvSelect = form_lvVolumesMain.SelectedItems;

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

            if ((File.Exists(saveFileDialog1.FileName))
                && (MessageBox.Show(saveFileDialog1.FileName + " already exists. Overwrite?         ", "Volume List Save As", MessageBoxButtons.YesNo)
                != System.Windows.Forms.DialogResult.Yes))
            {
                return;
            }

            using (TextWriter fs = File.CreateText(saveFileDialog1.FileName))
            {
                fs.WriteLine(Utilities.m_str_VOLUME_LIST_HEADER);

                foreach (ListViewItem lvItem in form_lvVolumesMain.Items)
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

                form_lvVolumesMain.Items.Clear();

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

                    form_lvVolumesMain.Items.Add(new ListViewItem(strArray));
                }
            }

            if (form_lvVolumesMain.Items.Count > 0)
            {
                form_btnSavePathInfo.Enabled = true;
            }

            m_bBrowseLoaded = false;
            DoTree(true);
        }

        private void UpdateLV_VolumesSelection()
        {
            bool bHasSelection = (form_lvVolumesMain.SelectedIndices.Count > 0);

            form_btnVolGroup.Enabled = bHasSelection;
            form_btn_RemoveVolume.Enabled = bHasSelection;
            form_btnToggleInclude.Enabled = bHasSelection;
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
            form_cbVolumeName.BackColor = Color.Empty;
            form_cbPath.BackColor = Color.Empty;
            form_cbSaveAs.BackColor = Color.Empty;
            timer_killRed.Enabled = false;
        }

        String FullPath(TreeNode treeNode)
        {
            StringBuilder stbFullPath = null;
            TreeNode parentNode = treeNode.Parent;

            if (parentNode == null)
            {
                stbFullPath = new StringBuilder(treeNode.Name);
            }
            else
            {
                stbFullPath = new StringBuilder(treeNode.Text);
            }
            
            while ((parentNode != null) && (parentNode.Parent != null))
            {
                stbFullPath.Insert(0, Path.DirectorySeparatorChar);
                stbFullPath.Insert(0, parentNode.Text);
                parentNode = parentNode.Parent;
            }

            if ((parentNode != null) && (parentNode.Parent == null))
            {
                stbFullPath.Insert(0, Path.DirectorySeparatorChar);
                stbFullPath.Insert(0, parentNode.Name);
            }

            return stbFullPath.ToString();
        }

        private void form_treeView_Browse_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (sender == form_treeCompare2)
            {
                Debug.Assert(m_bCompareMode);
                form_lvFileCompare.Items.Clear();
                form_lvDetailVol.Items.Clear();
            }
            else
            {
                form_lvFiles.Items.Clear();
                form_lvDetail.Items.Clear();

                if (m_bCompareMode == false)
                {
                    form_lvDetailVol.Items.Clear();
                }
            }

            TreeNode rootNode = TreeSelect.GetParentRoot(e.Node);

            Debug.Assert((new object[] { form_treeCompare1, form_treeCompare2 }.Contains(sender)) == m_bCompareMode);
            DoTreeSelect(e.Node);

            String strNode = e.Node.Text;

            Debug.Assert(strNode.Length > 0);

            if (m_bCompareMode)
            {
                if (rootNode.Checked)   // hack to denote second compare pane
                {
                    form_colVolDetail.Text = form_colFileCompare.Text = strNode;
                }
                else
                {
                    form_colDirDetail.Text = form_colFilename.Text = strNode;
                }

                return;
            }

            form_lblVolGroup.Text = ((RootNodeDatum)rootNode.Tag).StrVolumeGroup;

            if (form_lblVolGroup.Text.Length == 0)
            {
                form_lblVolGroup.Text = "(no volume group set)";
            }

            form_colVolDetail.Text = rootNode.Text;
            form_colDirDetail.Text = form_colFilename.Text = strNode;

            if (m_bPutPathInFindEditBox)
            {
                m_bPutPathInFindEditBox = false;
                form_cb_TreeFind.Text = FullPath(e.Node);
            }

            NodeDatum nodeDatum = (NodeDatum)e.Node.Tag;

            if (nodeDatum.NumImmediateFiles == 0)
            {
                form_colFilename.Text = m_strColFilesOrig;
            }

            if ((nodeDatum.m_lvCloneItem == null) || nodeDatum.m_lvCloneItem.Selected)
            {
                return;
            }

            nodeDatum.m_lvCloneItem.Selected = true;
            nodeDatum.m_lvCloneItem.Focused = true;

            if (form_lvClones.Items.Contains(nodeDatum.m_lvCloneItem))
            {
                form_lvClones.TopItem = nodeDatum.m_lvCloneItem;
            }
            else if (form_lvUnique.Items.Contains(nodeDatum.m_lvCloneItem))
            {
                form_lvUnique.TopItem = nodeDatum.m_lvCloneItem;
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
            if (form_lvClones.SelectedItems.Count == 0)
            {
                return;
            }

            if (form_lvClones.SelectedItems[0].Tag == null)
            {
                // marker item
                ListViewItem lvItem = form_lvClones.Items[form_lvClones.SelectedItems[0].Index + 1];

                lvItem.Selected = true;
                lvItem.Focused = true;
            }

            List<TreeNode> listTreeNodes = (List<TreeNode>)form_lvClones.SelectedItems[0].Tag;

            m_bPutPathInFindEditBox = true;
            form_treeView_Browse.SelectedNode = listTreeNodes[++m_nLVclonesClickIndex % listTreeNodes.Count];
            form_treeView_Browse.Select();
        }

        private TreeNode GetNodeByPath(string path, TreeView treeView)
        {
            return GetNodeByPath_A(path, treeView) ?? GetNodeByPath_A(path, treeView, true);
        }

        private TreeNode GetNodeByPath_A(string strPath, TreeView treeView, bool bIgnoreCase = false)
        {
            if ((strPath == null) || (strPath.Length == 0))
            {
                return null;
            }

            if (bIgnoreCase)
            {
                strPath = strPath.ToLower();
            }

            TreeNode node = null;
            string[] pathLevel = null;
            int i = 0;
            int nPathLevelLength = 0;

            foreach (TreeNode topNode in treeView.Nodes)
            {
                String strNode = topNode.Name;

                if (bIgnoreCase)
                {
                    strNode = strNode.ToLower();
                }

                pathLevel = strPath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
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

                    for (int n = 1; n < nPathLevelLength - 1; ++n)
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
                node = GetSubNode(node, pathLevel, i, nPathLevelLength, bIgnoreCase);
            }

            return node;
        }

        private TreeNode GetSubNode(TreeNode node, string[] pathLevel, int i, int nPathLevelLength, bool bIgnoreCase)
        {
            foreach (TreeNode subNode in node.Nodes)
            {
                String strText = bIgnoreCase ? subNode.Text.ToLower() : subNode.Text;

                if (strText != pathLevel[i])
                {
                    continue;
                }

                if (++i == nPathLevelLength)
                {
                    return subNode;
                }

                return GetSubNode(subNode, pathLevel, i, nPathLevelLength, bIgnoreCase);
            }

            return null;
        }

        TreeNode FindNode(String strSearch, TreeNode startNode = null, TreeView treeView = null)
        {
            if ((strSearch == null) || (strSearch.Length == 0))
            {
                return null;
            }

            if (startNode != null)
            {
                treeView = startNode.TreeView;
            }
            else if (m_bCompareMode)
            {
                treeView = (m_ctlLastFocusForCopyButton is TreeView) ? (TreeView)m_ctlLastFocusForCopyButton : form_treeCompare1;
            }
            else if (treeView == null)
            {
                treeView = form_treeView_Browse;
            }

            TreeNode treeNode = GetNodeByPath(strSearch, treeView);

            if (treeNode == null)
            {
                // case sensitive only when user enters an uppercase character

                List<TreeNode> listTreeNodes = m_listTreeNodes;

                if (m_bCompareMode)
                {
                    listTreeNodes = (treeView == form_treeCompare2) ? m_listTreeNodes_Compare2 : m_listTreeNodes_Compare1;
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
                    m_bFileFound = false;
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

        void NavToFile()
        {
            int nCounter = -1;

            while (true)
            {
                foreach (SearchResults searchResults in m_listSearchResults)
                {
                    foreach (SearchResultsDir resultDir in searchResults.Results)
                    {
                        if (resultDir.ListFiles.Count == 0)
                        {
                            if (++nCounter < m_nTreeFindTextChanged)
                            {
                                continue;
                            }

                            TreeNode treeNode = GetNodeByPath(resultDir.StrDir, form_treeView_Browse);

                            Debug.Assert(treeNode != null);
                            treeNode.TreeView.SelectedNode = treeNode;
                            ++m_nTreeFindTextChanged;
                            m_blink.Go(Once: true);
                            return;
                        }

                        foreach (String strFile in resultDir.ListFiles)
                        {
                            if (++nCounter < m_nTreeFindTextChanged)
                            {
                                continue;
                            }

                            m_strMaybeFile = strFile;

                            TreeNode treeNode = GetNodeByPath(resultDir.StrDir, form_treeView_Browse);

                            Debug.Assert(treeNode != null);

                            if (treeNode.TreeView.SelectedNode == treeNode)
                            {
                                SelectFoundFile();
                            }
                            else
                            {
                                treeNode.TreeView.SelectedNode = treeNode;
                            }

                            ++m_nTreeFindTextChanged;
                            return;
                        }
                    }
                }

                // Don't bother imposing a modulus. Just let m_nTreeFindTextChanged grow.
            }
        }

        String m_strMaybeFile = null;

        private void form_btn_TreeFind_Click(object sender, EventArgs e)
        {
            TreeView treeView = form_treeView_Browse;

            if (m_bCompareMode)
            {
                treeView = (m_ctlLastFocusForCopyButton is TreeView) ? (TreeView)m_ctlLastFocusForCopyButton : form_treeCompare1;
            }

            while (true)
            {
                if (m_nTreeFindTextChanged == 0)
                {
                    if (form_cb_TreeFind.Text.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        // special code for find file and dir
                        String strSearch = form_cb_TreeFind.Text.Substring(1);
                        bool bSearchFileOnly = false;

                        if (strSearch.StartsWith(Path.DirectorySeparatorChar.ToString()))
                        {
                            strSearch = strSearch.Substring(1);
                            bSearchFileOnly = true;
                        }

                        SearchFiles(form_cb_TreeFind.Text.Substring(1),
                            new SearchResultsDelegate(SearchResultsCallback),
                            bSearchFilesOnly: bSearchFileOnly);
                        // async
                        return;
                    }
                    else
                    {
                        FindNode(form_cb_TreeFind.Text, treeView.SelectedNode, treeView);
                    }
                }

                if (m_bFileFound)
                {
                    NavToFile();
                }
                else
                {
                    if ((m_arrayTreeFound != null) && (m_arrayTreeFound.Length > 0))
                    {
                        TreeNode treeNode = m_arrayTreeFound[m_nTreeFindTextChanged % m_arrayTreeFound.Length];

                        treeNode.TreeView.SelectedNode = treeNode;
                        ++m_nTreeFindTextChanged;
                        m_blink.Go(Once: true);
                    }
                    else if (treeView == form_treeCompare1)
                    {
                        treeView = form_treeCompare2;
                        continue;
                    }
                    else if (form_cb_TreeFind.Text.Contains(Path.DirectorySeparatorChar))
                    {
                        Debug.Assert(form_cb_TreeFind.Text.EndsWith(Path.DirectorySeparatorChar.ToString()) == false);

                        int nPos = form_cb_TreeFind.Text.LastIndexOf(Path.DirectorySeparatorChar);
                        String strMaybePath = form_cb_TreeFind.Text.Substring(0, nPos);
                        TreeNode treeNode = GetNodeByPath(strMaybePath, form_treeView_Browse);

                        m_strMaybeFile = form_cb_TreeFind.Text.Substring(nPos + 1);

                        if (treeNode != null)
                        {
                            treeNode.TreeView.SelectedNode = treeNode;
                        }
                        else
                        {
                            Debug.Assert(m_listSearchResults.Count <= 0);
                            SearchResultsCallback();    // hack: consolidates reporting the error.
                        }
                    }
                    else
                    {
                        SearchFiles(form_cb_TreeFind.Text, new SearchResultsDelegate(SearchResultsCallback));
                    }
                }

                break;
            }
        }

        void SearchResultsCallback()
        {
            if (m_listSearchResults.Count > 0)
            {
                m_bFileFound = true;
                NavToFile();
            }
            else
            {
                m_nTreeFindTextChanged = 0;
                m_bFileFound = false;
                m_strMaybeFile = null;
                m_blink.Go(clr: Color.Red, Once: true);
                MessageBox.Show("Couldn't find the specified search parameter.", "Search");
                return;
            }
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
            if (m_listSearchResults != null)
            {
                m_listSearchResults = null;
                GC.Collect();
            }

            m_nTreeFindTextChanged = 0;
            m_bFileFound = false;
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

            if (form_lvUnique.SelectedItems.Count == 0)
            {
                return;
            }

            if (form_lvUnique.Focused == false)
            {
                return;
            }

            m_bPutPathInFindEditBox = true;

            TreeNode treeNode = form_treeView_Browse.SelectedNode = (TreeNode)form_lvUnique.SelectedItems[0].Tag;

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
                form_chkCompare1.Text = m_strChkCompareOrig;
                form_btn_TreeCollapse.Text = m_strBtnTreeCollapseOrig;
                form_btnCompare.Text = m_strBtnCompareOrig;
                form_colFilename.Text = m_strColFilesOrig;
                form_colFileCompare.Text = m_strColFileCompareOrig;
                form_colDirDetailCompare.Text = m_strColDirDetailCompareOrig;
                form_lblVolGroup.Text = m_strVolGroupOrig;
                form_lblVolGroup.Font = m_FontVolGroupOrig;
                form_lblVolGroup.BackColor = m_clrVolGroupOrig;
                form_colDirDetail.Text = m_strColDirDetailOrig;
                form_colVolDetail.Text = m_strColVolDetailOrig;
                form_splitTreeFind.Panel1Collapsed = true;
                form_splitTreeFind.Panel2Collapsed = false;
                form_splitCompareFiles.Panel2Collapsed = true;
                form_splitClones.Panel2Collapsed = false;
                m_strCompare1 = null;
                m_nodeCompare1 = null;
                m_nodeCompare2 = null;
                form_treeCompare1.Nodes.Clear();
                form_treeCompare2.Nodes.Clear();
                form_chkCompare1.Checked = false;
                form_btnCompare.Enabled = false;
                m_bCompareMode = false;
                form_treeView_Browse_AfterSelect(form_treeView_Browse, new TreeViewEventArgs(form_treeView_Browse.SelectedNode));
            }
            else if (form_chkCompare1.Checked)
            {
                m_strCompare1 = form_cb_TreeFind.Text;

                bool bError = (m_strCompare1.Length == 0);

                if (bError == false)
                {
                    m_nodeCompare1 = FindNode(m_strCompare1, form_treeView_Browse.SelectedNode);
                    bError = (m_nodeCompare1 == null);
                }

                if (bError)
                {
                    m_blink.Go(clr: Color.Red, Once: true);
                    form_chkCompare1.Checked = false;  // event retriggers this handler
                }
                else
                {
                    m_blink.Go();
                    form_treeView_Browse.SelectedNode = m_nodeCompare1;
                    form_btnCompare.Enabled = true;
                }
            }
            else
            {
                form_btnCompare.Enabled = false;
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
                else if (m_ctlBlink != null)
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
            m_blink.Tick();
        }

        bool Compare(TreeNode t1, TreeNode t2, bool bReverse = false, long nMin10M = (10 * 1024 - 100) * 1024, long nMin100K = 100 * 1024)
        {
            bool bRet = true;

            foreach (TreeNode s1 in t1.Nodes)
            {
                bool bCompare = true;
                bool bCompareSub = true;
                TreeNode s2 = null;
                NodeDatum n1 = (NodeDatum)s1.Tag;

                if (n1.LengthSubnodes <= (nMin10M + nMin100K))
                {
                    s1.ForeColor = Color.LightGray;
                }
                else if (t2.Nodes.ContainsKey(s1.Name))
                {
                    s2 = t2.Nodes[s1.Name];

                    if (Compare(s1, s2, bReverse, nMin10M, nMin100K) == false)
                    {
                        bCompareSub = false;
                    }

                    NodeDatum n2 = (NodeDatum)s2.Tag;

                    bCompare &= (n1.NumImmediateFiles == n2.NumImmediateFiles);
                    bCompare &= (Math.Abs(n1.Length - n2.Length) <= (nMin10M + nMin100K));

                    if (bCompare == false) { s2.ForeColor = Color.Red; }
                    else if (s2.ForeColor == Color.Empty) { s2.ForeColor = Color.SteelBlue; }
                }
                else
                {
                    bCompare = false;
                }

                if (bCompare == false)
                {
                    TreeNode r1 = bReverse ? s2 : s1;
                    TreeNode r2 = bReverse ? s1 : s2;

                    if ((r1 != null) && (dictCompareDiffs.ContainsKey(r1) == false))
                    {
                        dictCompareDiffs.Add(r1, r2);
                    }
                    else if (dictCompareDiffs.ContainsValue(r2) == false)
                    {
                        dictCompareDiffs.Add(new TreeNode(), r2);
                    }
                }

                if (bCompare == false) { s1.ForeColor = Color.Red; }
                else if (bCompareSub == false) { s1.ForeColor = Color.DarkRed; }
                else if (s1.ForeColor == Color.Empty) { s1.ForeColor = Color.SteelBlue; }
                
                bRet &= (bCompare && bCompareSub);
            }

            return bRet;
        }

        void NameNodes(TreeNode treeNode, List<TreeNode> listTreeNodes)
        {
            treeNode.Name = treeNode.Text;
            treeNode.ForeColor = Color.Empty;
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
                Debug.Assert(form_chkCompare1.Checked);
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
                    m_blink.Go(clr: Color.Red, Once:true);
                }
                else
                {
                    m_blink.Go();
                    form_splitTreeFind.Panel1Collapsed = false;
                    form_splitTreeFind.Panel2Collapsed = true;
                    form_splitCompareFiles.Panel2Collapsed = false;
                    form_splitClones.Panel2Collapsed = true;
                    form_treeView_Browse.SelectedNode = m_nodeCompare2;

                    RootNodeDatum rootNodeDatum1 = (RootNodeDatum)TreeSelect.GetParentRoot(m_nodeCompare1).Tag;
                    RootNodeDatum rootNodeDatum2 = (RootNodeDatum)TreeSelect.GetParentRoot(m_nodeCompare2).Tag;
                    String strFullPath1 = FullPath(m_nodeCompare1);
                    String strFullPath2 = FullPath(m_nodeCompare2);
                    String strFullPath1A = m_nodeCompare1.FullPath;
                    String strFullPath2A = m_nodeCompare2.FullPath;

                    m_nodeCompare1 = (TreeNode)m_nodeCompare1.Clone();
                    m_nodeCompare2 = (TreeNode)m_nodeCompare2.Clone();
                    m_listTreeNodes_Compare1.Clear();
                    m_listTreeNodes_Compare2.Clear();
                    NameNodes(m_nodeCompare1, m_listTreeNodes_Compare1);
                    NameNodes(m_nodeCompare2, m_listTreeNodes_Compare2);
                    m_nodeCompare1.Name = strFullPath1;
                    m_nodeCompare2.Name = strFullPath2;
                    m_nodeCompare1.ToolTipText = strFullPath1A;
                    m_nodeCompare2.ToolTipText = strFullPath2A;
                    m_nodeCompare1.Tag = new RootNodeDatum((NodeDatum)m_nodeCompare1.Tag, rootNodeDatum1.StrFile, rootNodeDatum1.StrVolumeGroup);
                    m_nodeCompare2.Tag = new RootNodeDatum((NodeDatum)m_nodeCompare2.Tag, rootNodeDatum2.StrFile, rootNodeDatum2.StrVolumeGroup);
                    m_nodeCompare2.Checked = true;    // hack to put it in the right file pane
                    dictCompareDiffs.Clear();
                    Compare(m_nodeCompare1, m_nodeCompare2);
                    Compare(m_nodeCompare2, m_nodeCompare1, true);

                    if (dictCompareDiffs.Count < 15)
                    {
                        dictCompareDiffs.Clear();
                        Compare(m_nodeCompare1, m_nodeCompare2, nMin10M: 0);
                        Compare(m_nodeCompare2, m_nodeCompare1, true, nMin10M: 0);
                    }

                    if (dictCompareDiffs.Count < 15)
                    {
                        dictCompareDiffs.Clear();
                        Compare(m_nodeCompare1, m_nodeCompare2, nMin10M: 0, nMin100K: 0);
                        Compare(m_nodeCompare2, m_nodeCompare1, true, nMin10M: 0, nMin100K: 0);
                    }

                    SortedDictionary<long, KeyValuePair<TreeNode, TreeNode>> dictSort = new SortedDictionary<long, KeyValuePair<TreeNode, TreeNode>>();

                    foreach (KeyValuePair<TreeNode, TreeNode> pair in dictCompareDiffs)
                    {
                        long l1 = 0, l2 = 0;

                        if (pair.Key.Text.Length > 0)
                        {
                            l1 = ((NodeDatum)pair.Key.Tag).LengthSubnodes;
                        }

                        if (pair.Value != null)
                        {
                            l2 = ((NodeDatum)pair.Value.Tag).LengthSubnodes;
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

                    form_treeCompare1.Nodes.Add(m_nodeCompare1);
                    form_treeCompare2.Nodes.Add(m_nodeCompare2);
                    m_nCompareIndex = -1;
                    form_btnCompare.Select();
                    form_btnCompare.Text = "> >";
                    form_chkCompare1.Text = "0 of " + dictCompareDiffs.Count;
                    form_btn_TreeCollapse.Text = "< <";
                    form_colDirDetailCompare.Text = "Directory detail";
                    form_lblVolGroup.Text = "Compare Mode";
                    form_lblVolGroup.BackColor = Color.LightGoldenrodYellow;
                    form_lblVolGroup.Font = new Font(m_FontVolGroupOrig, FontStyle.Regular);
                    m_bCompareMode = true;
                    form_treeCompare1.SelectedNode = form_treeCompare1.Nodes[0];
                    form_treeCompare2.SelectedNode = form_treeCompare2.Nodes[0];
                }
            }
        }

        void CompareNav()
        {
            Console.WriteLine(dictCompareDiffs.ToArray()[m_nCompareIndex]);
            form_chkCompare1.Text = m_nCompareIndex + 1 + " of " + dictCompareDiffs.Count;
            form_lvFiles.Items.Clear();
            form_lvFileCompare.Items.Clear();
            form_lvDetail.Items.Clear();
            form_lvDetailVol.Items.Clear();
            form_treeCompare1.SelectedNode = null;
            form_treeCompare2.SelectedNode = null;

            TreeNode treeNode = dictCompareDiffs.ToArray()[m_nCompareIndex].Key;

            if (treeNode.Name.Length == 0)  // can't have a null key in the dictionary so there's a new TreeNode there
            {
                treeNode = null;
            }

            form_treeCompare1.TopNode = form_treeCompare1.SelectedNode = treeNode;
            form_treeCompare2.TopNode = form_treeCompare2.SelectedNode = dictCompareDiffs.ToArray()[m_nCompareIndex].Value;

            if (form_treeCompare1.SelectedNode == null)
            {
                form_colFilename.Text = m_strColFilesOrig;
                form_colDirDetail.Text = m_strColDirDetailOrig;
                form_treeCompare1.CollapseAll();
            }
            else
            {
                form_treeCompare1.SelectedNode.EnsureVisible();
            }

            if (form_treeCompare2.SelectedNode == null)
            {
                form_colFileCompare.Text = m_strColFileCompareOrig;
                form_colVolDetail.Text = m_strColVolDetailOrig;
                form_treeCompare2.CollapseAll();
            }
            else
            {
                form_treeCompare2.SelectedNode.EnsureVisible();
            }
        }

        private void form_btn_ComparePrev_Click(object sender, EventArgs e)
        {
            if (dictCompareDiffs.Count == 0)
            {
                return;
            }

            m_nCompareIndex = Math.Max(0, --m_nCompareIndex);
            CompareNav();
        }

        private void form_btn_CompareNext_Click(object sender, EventArgs e)
        {
            if (dictCompareDiffs.Count == 0)
            {
                return;
            }

            m_nCompareIndex = Math.Min(dictCompareDiffs.Count - 1, ++m_nCompareIndex);
            CompareNav();
        }

        private void form_lv_Unique_KeyPress(object sender, KeyPressEventArgs e)
        {
            ListView.SelectedListViewItemCollection lvSel = form_lvUnique.SelectedItems;

            if (lvSel.Count > 0)
            {
                int nIx = form_lvUnique.SelectedItems[0].Index;
                ListViewItem lvItem = null;

                // accidentally trying to compare too early (using < and >)
                if (e.KeyChar == '.')
                {
                    if (nIx < form_lvUnique.Items.Count - 1)
                    {
                        lvItem = form_lvUnique.Items[nIx + 1];
                    }

                    e.Handled = true;
                }
                else if (e.KeyChar == ',')
                {
                    if (nIx > 0)
                    {
                        lvItem = form_lvUnique.Items[nIx - 1];
                    }

                    e.Handled = true;
                }

                if (e.Handled)
                {
                    lvItem.Selected = true;
                    lvItem.Focused = true;
                    lvItem.EnsureVisible();
                    return;
                }
            }

            if (e.KeyChar == 3)
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

                if (form_btnCompare.Enabled == false)
                {
                    form_chkCompare1.Checked = true;
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
                form_chkCompare1.Checked = false;
                form_lvUnique.Select();
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
                form_chkCompare1.Checked = false;          // leave Compare mode
            }
            else if (form_btnCompare.Enabled == false)
            {
                form_chkCompare1.Checked = true;           // enter first path to compare
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
                m_blink.Go(clr: Color.Red, Once: true);
                return;
            }

            Clipboard.SetText(form_cb_TreeFind.Text);
            m_blink.Go(Once: true);
        }

        private void form_btn_Copy_Click(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                if (new object[] { form_treeCompare1, form_treeCompare2 }.Contains(m_ctlLastFocusForCopyButton))
                {
                    form_tree_compare_KeyPress(m_ctlLastFocusForCopyButton, new KeyPressEventArgs((char)3));
                }
                else if (new object[] { form_lvFiles, form_lvFileCompare }.Contains(m_ctlLastFocusForCopyButton))
                {
                    FileListKeyPress((ListView) m_ctlLastFocusForCopyButton, new KeyPressEventArgs((char)3));
                }
                else
                {
                    CopyToClipboard();
                    m_blink.Go(clr: Color.Yellow);
                }
            }
            else
            {
                CopyToClipboard();
            }
        }

        private void form_lv_Unique_MouseClick(object sender, MouseEventArgs e)
        {
            if (form_lvUnique.SelectedItems.Count <= 0)
            {
                return;
            }

            if (form_lvUnique.SelectedItems[0].Tag == null)
            {
                // marker item
                form_lvUnique.Select();

                ListViewItem lvItem = form_lvUnique.Items[form_lvUnique.SelectedItems[0].Index + 1];
                
                lvItem.Selected = true;
                lvItem.Focused = true;
                return;
            }
        }

        private void form_btn_VolGroup_Click(object sender, EventArgs e)
        {
            if (form_lvVolumesMain.SelectedItems.Count == 0)
            {
                return;
            }

            timer_DoTree.Stop();

            InputBox inputBox = new InputBox();

            inputBox.Text = "Volume Group";
            inputBox.Prompt = "Enter a volume group name";
            inputBox.Entry = form_lvVolumesMain.SelectedItems[0].SubItems[5].Text;

            SortedDictionary<String, object> dictVolGroups = new SortedDictionary<string, object>();

            foreach (ListViewItem lvItem in form_lvVolumesMain.Items)
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

            form_lvVolumesMain.SelectedItems[0].SubItems[5].Text = inputBox.Entry;
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
            form_btn_SaveVolumeList.Enabled = form_btnSavePathInfo.Enabled;
        }

        void PutTreeCompareNodePathIntoFindCombo(TreeView treeView)
        {
            if (treeView.SelectedNode == null)
            {
                return;
            }

            form_cb_TreeFind.Text = FullPath(treeView.SelectedNode);
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

            if (e.KeyChar != 3)
            {
                return;
            }

            e.Handled = true;

            if (lv.SelectedItems.Count <= 0)
            {
                m_blink.Go(clr:Color.Red, Once: true);
                return;
            }

            if (m_bCompareMode)
            {
                PutTreeCompareNodePathIntoFindCombo((lv == form_lvFiles) ? form_treeCompare1 : form_treeCompare2);
            }

            Clipboard.SetText(FullPath(form_treeView_Browse.SelectedNode) + Path.DirectorySeparatorChar + lv.SelectedItems[0].Text);
            m_blink.Go(lvItem: lv.SelectedItems[0], Once: true);
        }

        private void form_LV_Files_KeyPress(object sender, KeyPressEventArgs e)
        {
            FileListKeyPress(form_lvFiles, e);
        }

        private void form_lv_FileCompare_KeyPress(object sender, KeyPressEventArgs e)
        {
            FileListKeyPress(form_lvFileCompare, e);
        }

        private void formCtl_EnterForCopyButton(object sender, EventArgs e)
        {
            m_ctlLastFocusForCopyButton = (Control) sender;
        }
    }
}
