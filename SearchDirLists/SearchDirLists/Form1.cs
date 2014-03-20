﻿using System;
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
            using (StreamReader file = new StreamReader(m_strSaveAs))
            {
                do
                {
                    String line = file.ReadLine();

                    if (line != Utilities.m_str_HEADER) break;

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

                if (strLine != Utilities.m_str_VOLUME_LIST_HEADER)
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
            form_LV_Detail.Items.Clear();
            form_LV_DetailVol.Items.Clear();
            form_LV_Files.Items.Clear();

            if (bTreeClick)
            {
                form_cb_TreeFind.Text = e.Node.FullPath;
                bTreeClick = false;
            }

            DoTreeSelect(e.Node);

            NodeDatum nodeDatum = (NodeDatum)e.Node.Tag;

            if ((nodeDatum.m_lvCloneItem != null) && (nodeDatum.m_lvCloneItem.Selected == false))
            {
                nodeDatum.m_lvCloneItem.Selected = true;

                if (form_LV_Clones.Items.Contains(nodeDatum.m_lvCloneItem))
                {
                    form_LV_Clones.TopItem = nodeDatum.m_lvCloneItem;
                }

                if (form_lv_Unique.Items.Contains(nodeDatum.m_lvCloneItem))
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

        int nLVexttrClickIndex = -1;
        private void form_lvClones_SelectedIndexChanged(object sender, EventArgs e)
        {
            nLVexttrClickIndex = -1;
        }

        private void form_lvClones_MouseClick(object sender, MouseEventArgs e)
        {
            if (form_LV_Clones.SelectedItems.Count == 0)
            {
                return;
            }

            ++nLVexttrClickIndex;

            List<TreeNode> listTreeNodes = (List<TreeNode>)form_LV_Clones.SelectedItems[0].Tag;

            form_treeView_Browse.SelectedNode = listTreeNodes[nLVexttrClickIndex % listTreeNodes.Count];
            form_treeView_Browse.Select();
        }

        private TreeNode GetNodeByPath(string path, TreeView treeView)
        {
            TreeNode node = null;
            string[] pathLevel = path.ToLower().Split(Path.DirectorySeparatorChar);
            int i = 0;
            int nPathLevelLength = pathLevel.Length;

            foreach (TreeNode topNode in treeView.Nodes)
            {
                String strNode = topNode.Text.ToLower();

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
        
        int nTreeFindTextChanged = 0;
        TreeNode[] arrayTreeFound = null;
        private void form_btn_TreeFind_Click(object sender, EventArgs e)
        {
            if (nTreeFindTextChanged == 0)
            {
                TreeNode treeNode = GetNodeByPath(form_cb_TreeFind.Text, form_treeView_Browse);

                if (treeNode == null)
                {
                    // case sensitive only when user enters an uppercase character

                    if (form_cb_TreeFind.Text.ToLower() == form_cb_TreeFind.Text)
                    {
                        arrayTreeFound = m_listTreeNodes.FindAll(node => node.Text.ToLower().Contains(form_cb_TreeFind.Text)).ToArray();
                    }
                    else
                    {
                        arrayTreeFound = m_listTreeNodes.FindAll(node => node.Text.Contains(form_cb_TreeFind.Text)).ToArray();
                    }

                    if ((arrayTreeFound == null) || (arrayTreeFound.Length == 0))
                    {
                        MessageBox.Show("Couldn't find the specified search parameter in the tree.", "Search in Tree");
                    }
                }
                else
                {
                    arrayTreeFound = new TreeNode[] { treeNode };
                }
            }

            if ((arrayTreeFound != null) && (arrayTreeFound.Length > 0))
            {
                form_treeView_Browse.SelectedNode = arrayTreeFound[nTreeFindTextChanged % arrayTreeFound.Length];
                ++nTreeFindTextChanged;
            }
            else
            {
                nTreeFindTextChanged = 0;
            }
        }

        private void form_edit_TreeFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Keys[] { Keys.Enter, Keys.Return }.Contains((Keys)e.KeyChar))
            {
                form_btn_TreeFind_Click(sender, e);
                bTreeClick = false;
                e.Handled = true;
            }
        }

        private void form_btn_TreeCollapse_Click(object sender, EventArgs e)
        {
            form_treeView_Browse.CollapseAll();
        }

        private void form_edit_TreeFind_TextChanged(object sender, EventArgs e)
        {
            nTreeFindTextChanged = 0;
        }

        bool bTreeClick = false;
        private void form_treeView_Browse_MouseClick(object sender, MouseEventArgs e)
        {
            bTreeClick = true;
        }

        private void form_cb_TreeFind_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cb_TreeFind, bTrimText: false);
        }

        private void form_lv_Unique_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (form_lv_Unique.SelectedItems.Count == 0)
            {
                return;
            }

            TreeNode treeNode = form_treeView_Browse.SelectedNode = (TreeNode)form_lv_Unique.SelectedItems[0].Tag;

            treeNode.Expand();
            form_treeView_Browse.TopNode = treeNode.Parent;

            if (treeNode.IsVisible == false)
            {
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
        }
    }
}
