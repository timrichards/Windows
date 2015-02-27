using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System;
using System.Collections.Generic;

// TODO:
//      compare file list
//      enable search for compare mode, like it had been
//      first compare item still doesn't go back
//      save treeNode prior to null - file list redraw

namespace DoubleFile
{
    //    [System.ComponentModel.DesignerCategory("Designer")]
    [System.ComponentModel.DesignerCategory("Code")]
    partial class FormAnalysis_DirList : Form
    {
        bool m_bFindBoxMouseEnter = false;

        readonly GlobalData gd = null;
        GlobalData_Tree gd_Tree = null;
        GlobalData_Search_Path gd_Search_Path = null;
        GlobalData_Search_1_2 gd_Search_1_2 = null;
        readonly MainWindow m_ownerWindow = null;
        LV_ProjectVM LVprojectVM { get; set; }

        internal FormAnalysis_DirList(MainWindow ownerWindow, LV_ProjectVM lvProjectVM)
        {
            InitializeComponent();

            m_ownerWindow = ownerWindow;
            LVprojectVM = new LV_ProjectVM(lvProjectVM: lvProjectVM);

            gd = GlobalData.Reset();
            gd_Tree = new GlobalData_Tree(gd);
            gd.gd_Tree = gd_Tree;
            gd_Search_Path = new GlobalData_Search_Path(gd);
            gd_Search_1_2 = new GlobalData_Search_1_2(gd, gd_Search_Path, gd_Tree);

            gd.m_tmrDoTree.Elapsed += (o, e) =>
            {
                if (null == gd)
                {
                    return;
                }

                gd.m_tmrDoTree.Stop();
                gd.m_bRestartTreeTimer = false;

                if (IsDisposed)
                {
                    return;
                }

                UtilAnalysis_DirList.UIthread(this, () =>
                {
                    if (gd.m_bCompareMode)
                    {
                        MBoxStatic.Assert(1308.9304, form_chkCompare1.Checked);
                        form_chkCompare1.Checked = false;
                    }

                    DoTree(bKill: gd.m_bKillTree);
                });

                gd.m_bKillTree = true;
            };

            // Assert string-lookup form items exist
            //    Utilities.Assert(1308.9305, context_rclick_node.Items[m_strMARKFORCOPY] != null);

            gd.m_blinky = new BlinkyStruct(form_cbFindbox);
            gd.m_strBtnTreeCollapseOrig = form_btnCollapse.Text;
            gd.m_strColFilesOrig = form_colFilename.Text;
            gd.m_strColFileCompareOrig = form_colFileCompare.Text;
            gd.m_strColDirDetailCompareOrig = form_colDirDetailCompare.Text;
            gd.m_strColDirDetailOrig = form_colDirDetail.Text;
            gd.m_strColVolDetailOrig = form_colVolDetail.Text;
            gd.m_strBtnCompareOrig = form_btnCompare.Text;
            gd.m_strChkCompareOrig = form_chkCompare1.Text;
            gd.m_strVolGroupOrig = form_lblVolGroup.Text;
            gd.m_bCheckboxes = form_treeViewBrowse.CheckBoxes;

            m_FontVolGroupOrig = form_lblVolGroup.Font;
            m_clrVolGroupOrig = form_lblVolGroup.BackColor;
        }

        void FormAnalysis_DirList_Load(object sender, EventArgs e)
        {
            gd.RestartTreeTimer();
            form_tmapUserCtl.TooltipAnchor = (Control)form_cbFindbox;
        }

        static internal void RestartTreeTimer(FormAnalysis_DirList form1, LV_ProjectVM lvProjectVM)
        {
            if (form1 != null)
            {
                form1.LVprojectVM = new LV_ProjectVM(lvProjectVM: lvProjectVM);
                form1.gd.RestartTreeTimer();
            }
        }

        // Memory allocations occur just below all partial class FormAnalysis_DirList : Form declarations, then ClearMem_...() for each.
        // Declarations continue below these two ClearMem() methods.

        void ClearMem_FormAnalysis_DirList()
        {
            gd.ClearMem_FormAnalysis_DirList();

            MBoxStatic.Assert(1308.9301, form_lvClones.Items.IsEmpty(), bTraceOnly: true);
            MBoxStatic.Assert(1308.9302, form_lvSameVol.Items.IsEmpty(), bTraceOnly: true);
            MBoxStatic.Assert(1308.9303, form_lvUnique.Items.IsEmpty(), bTraceOnly: true);

            form_lvClones.Items.Clear();
            form_lvSameVol.Items.Clear();
            form_lvUnique.Items.Clear();

            form_lvFiles.Items.Clear();
            form_lvFileCompare.Items.Clear();
            form_lvDetail.Items.Clear();
            form_lvDetailVol.Items.Clear();
            form_tmapUserCtl.Clear();
        }

        void ClearMem()
        {
            Collate.ClearMem();
            ClearMem_FormAnalysis_DirList();
            gd_Search_1_2.ClearMem_Search();
            ClearMem_TreeForm();
        }

        Control m_ctlLastSearchSender = null;
        TabPage m_FileListTabPageBeforeCompare = null;
        internal readonly Color m_clrVolGroupOrig = Color.Empty;
        internal readonly Font m_FontVolGroupOrig = null;

        public class FormAnalysis_DirListLayoutPanel : TableLayoutPanel
        {
            void SetStyle()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.UserPaint, true);
            }

            public FormAnalysis_DirListLayoutPanel()
            {
                SetStyle();
            }

            public FormAnalysis_DirListLayoutPanel(System.ComponentModel.IContainer container)
            {
                container.Add(this);
                SetStyle();
            }
        }

        void CompareNav(bool bNext = true)
        {
            if (gd.m_dictCompareDiffs.IsEmpty())
            {
                return;
            }

            gd.m_nCompareIndex = (bNext) ?
                Math.Min(gd.m_dictCompareDiffs.Count - 1, ++gd.m_nCompareIndex) :
                Math.Max(0, --gd.m_nCompareIndex);

            UtilProject.WriteLine(gd.m_dictCompareDiffs.ToArray()[gd.m_nCompareIndex].ToString());
            form_chkCompare1.Text = gd.m_nCompareIndex + 1 + " of " + gd.m_dictCompareDiffs.Count;
            form_lvFiles.Items.Clear();
            form_lvFileCompare.Items.Clear();
            form_lvDetail.Items.Clear();
            form_lvDetailVol.Items.Clear();
            form_treeCompare1.SelectedNode = null;
            form_treeCompare2.SelectedNode = null;

            TreeNode treeNode = gd.m_dictCompareDiffs.ToArray()[gd.m_nCompareIndex].Key;

            if (string.IsNullOrWhiteSpace(treeNode.Name))  // can't have a null key in the dictionary so there's a new TreeNode there
            {
                treeNode = null;
            }

            gd.m_bTreeViewIndirectSelChange = true;
            form_treeCompare1.TopNode = form_treeCompare1.SelectedNode = treeNode;
            gd.m_bTreeViewIndirectSelChange = true;
            form_treeCompare2.TopNode = form_treeCompare2.SelectedNode = gd.m_dictCompareDiffs.ToArray()[gd.m_nCompareIndex].Value;

            if (form_treeCompare1.SelectedNode == null)
            {
                form_colFilename.Text = gd.m_strColFilesOrig;
                form_colDirDetail.Text = gd.m_strColDirDetailOrig;
                form_treeCompare1.CollapseAll();
            }
            else
            {
                form_treeCompare1.SelectedNode.EnsureVisible();
            }

            if (form_treeCompare2.SelectedNode == null)
            {
                form_colFileCompare.Text = gd.m_strColFileCompareOrig;
                form_colVolDetail.Text = gd.m_strColVolDetailOrig;
                form_treeCompare2.CollapseAll();
            }
            else
            {
                form_treeCompare2.SelectedNode.EnsureVisible();
            }
        }

        void ClearToolTip(object sender, EventArgs e)
        {
            if (form_tmapUserCtl.IsDisposed)
            {
                return;
            }

            UtilProject.WriteLine(DateTime.Now + " ClearToolTip();");
            form_tmapUserCtl.ClearSelection();
        }

        void LoadCopyScratchPad(ListView lvFake)
        {
            int nTotal = 0;
            int nLoaded = 0;

            foreach (ListViewItem lvItem in lvFake.Items)
            {
                TreeNode treeNode = gd_Search_Path.GetNodeByPath(lvItem.SubItems[1].Text, form_treeViewBrowse);

                if (treeNode != null)
                {
                    treeNode.Checked = true;
                    ++nLoaded;
                }

                ++nTotal;
            }

            if (nLoaded != nTotal)
            {
                MBoxStatic.ShowDialog(nLoaded + " of " + nTotal + " scratchpad folders found in the tree.", "Load copy scratchpad");
                form_tabControlCopyIgnore.SelectedTab = form_tabPageCopy;
                gd.m_blinky.Go(form_lvCopyScratchpad, clr: Color.Yellow, Once: true);
            }
        }

        void LoadIgnoreList(string strFile = null)
        {
            if (new SDL_IgnoreFile(strFile).ReadList(form_lvIgnoreList) == false)
            {
                return;
            }

            if (false == form_lvIgnoreList.Items.IsEmpty())
            {
                gd.m_bKillTree &= gd.m_tmrDoTree.Enabled;
                gd.RestartTreeTimer();
            }
        }

        void LV_CloneSelNode(ListView lv)
        {
            if (MBoxStatic.Assert(1308.9307, false == lv.SelectedItems.IsEmpty(), bTraceOnly: true) == false)
            {
                return;
            }

            UList<TreeNode> listTreeNodes = (UList<TreeNode>)((ListViewItem)lv.SelectedItems[0]).Tag;

            if (MBoxStatic.Assert(1308.9308, listTreeNodes != null, bTraceOnly: true) == false)
            {
                return;
            }

            gd.m_bPutPathInFindEditBox = true;
            gd.m_bTreeViewIndirectSelChange = true;
            gd.m_bClonesLVindirectSelChange = true;                                        // TODO: Is this in the way or is it applicable?
            form_treeViewBrowse.SelectedNode = listTreeNodes[++gd.m_nLVclonesClickIx % listTreeNodes.Count];
        }

        void LV_ClonesSelChange(ListView lv, bool bUp = false)
        {
            if (gd.LV_MarkerClick(lv, bUp) == false)
            {
                return;
            }

            gd.m_nLVclonesClickIx = -1;
            LV_CloneSelNode(lv);
        }

        void SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;

            if (gd.m_bCompareMode)
            {
                return;
            }

            if (lv.SelectedItems.IsEmpty())
            {
                return;
            }

            if (lv.Focused == false)
            {
                return;
            }

            gd.m_bPutPathInFindEditBox = true;
            gd.m_bTreeViewIndirectSelChange = true;

            TreeNode treeNode = (TreeNode)(form_treeViewBrowse.SelectedNode = (TreeNode)((ListViewItem)lv.SelectedItems[0]).Tag);

            if (treeNode == null)
            {
                return;
            }

            form_treeViewBrowse.TopNode = (TreeNode)treeNode.Parent;
            treeNode.EnsureVisible();
        }

        void form_btnBack_Click(object sender, EventArgs e)
        {
            gd.DoHistory(sender, -1);
        }

        void form_btnClearIgnoreList_Click(object sender, EventArgs e)
        {
            if (form_lvIgnoreList.Items.IsEmpty())
            {
                return;
            }

            form_lvIgnoreList.Items.Clear();
            gd.m_bKillTree &= gd.m_tmrDoTree.Enabled;
            gd.RestartTreeTimer();
        }

        void form_btnCompare_Click(object sender = null, EventArgs e = null)
        {
            if (gd.m_bCompareMode)
            {
                CompareNav();       // doubles as forward button  > >
                return;
            }
            
            if (form_chkCompare1.Checked == false)
            {
                form_chkCompare1.Checked = true;
                return;
            }

            MBoxStatic.Assert(1308.9309, form_chkCompare1.Checked);

            if (form_treeViewBrowse.SelectedNode == null)
            {
                gd.m_blinky.Go(clr: Color.Red, Once: true);
                return;
            }

            TreeNode nodeCompare2 = (TreeNode)form_treeViewBrowse.SelectedNode;

            if (nodeCompare2 == gd.m_nodeCompare1)
            {
                gd.m_blinky.Go(clr: Color.Red, Once: true);
                return;
            }

            gd.m_blinky.Go();
            gd_Search_1_2.ClearMem_Search();
            gd.m_listHistory.Clear();
            gd.m_nIxHistory = -1;
            form_splitTreeFind.Panel1Collapsed = false;
            form_splitTreeFind.Panel2Collapsed = true;
            form_splitCompareFiles.Panel2Collapsed = false;
            form_splitClones.Panel2Collapsed = true;

            RootNodeDatum rootNodeDatum1 = (RootNodeDatum)gd.m_nodeCompare1.Root().Tag;
            RootNodeDatum rootNodeDatum2 = (RootNodeDatum)nodeCompare2.Root().Tag;
            string strFullPath1 = GlobalData.FullPath(gd.m_nodeCompare1);
            string strFullPath2 = GlobalData.FullPath(nodeCompare2);
            string strFullPath1A = gd.m_nodeCompare1.FullPath;
            string strFullPath2A = nodeCompare2.FullPath;

            gd.m_nodeCompare1 = (TreeNode)gd.m_nodeCompare1.Clone();
            nodeCompare2 = (TreeNode)nodeCompare2.Clone();
            gd.NameNodes(gd.m_nodeCompare1, gd.m_listTreeNodes_Compare1);
            gd.NameNodes(nodeCompare2, gd.m_listTreeNodes_Compare2);
            gd.Compare(gd.m_nodeCompare1, nodeCompare2);
            gd.Compare(nodeCompare2, gd.m_nodeCompare1, bReverse: true);

            if (gd.m_dictCompareDiffs.Count < 15)
            {
                gd.m_dictCompareDiffs.Clear();
                gd.Compare(gd.m_nodeCompare1, nodeCompare2, nMin10M: 0);
                gd.Compare(nodeCompare2, gd.m_nodeCompare1, bReverse: true, nMin10M: 0);
            }

            if (gd.m_dictCompareDiffs.Count < 15)
            {
                gd.m_dictCompareDiffs.Clear();
                gd.Compare(gd.m_nodeCompare1, nodeCompare2, nMin10M: 0, nMin100K: 0);
                gd.Compare(nodeCompare2, gd.m_nodeCompare1, bReverse: true, nMin10M: 0, nMin100K: 0);
            }

            SortedDictionary<ulong, KeyValuePair<TreeNode, TreeNode>> dictSort = new SortedDictionary<ulong, KeyValuePair<TreeNode, TreeNode>>();

            foreach (KeyValuePair<TreeNode, TreeNode> pair in gd.m_dictCompareDiffs)
            {
                ulong l1 = 0, l2 = 0;

                if (false == string.IsNullOrWhiteSpace(pair.Key.Text))
                {
                    l1 = ((NodeDatum)pair.Key.Tag).nTotalLength;
                }

                if (pair.Value != null)
                {
                    l2 = ((NodeDatum)pair.Value.Tag).nTotalLength;
                }

                ulong lMax = Math.Max(l1, l2);

                while (dictSort.ContainsKeyA(lMax))
                {
                    --lMax;
                }

                dictSort.Add(lMax, pair);
            }

            gd.m_dictCompareDiffs.Clear();

            if (rootNodeDatum1.nLength != rootNodeDatum2.nLength)
            {
                gd.m_dictCompareDiffs.Add(gd.m_nodeCompare1, nodeCompare2);
            }

            foreach (KeyValuePair<TreeNode, TreeNode> pair in dictSort.Values.Reverse())
            {
                gd.m_dictCompareDiffs.Add(pair.Key, pair.Value);
            }

            gd.m_nodeCompare1.Name = strFullPath1;
            nodeCompare2.Name = strFullPath2;
            gd.m_nodeCompare1.ToolTipText = strFullPath1A;
            nodeCompare2.ToolTipText = strFullPath2A;
            gd.m_nodeCompare1.Tag = new RootNodeDatum((NodeDatum)gd.m_nodeCompare1.Tag, rootNodeDatum1);
            nodeCompare2.Tag = new RootNodeDatum((NodeDatum)nodeCompare2.Tag, rootNodeDatum2);
            nodeCompare2.Checked = true;    // hack to put it in the right file pane
            form_treeCompare1.Nodes.Add(gd.m_nodeCompare1);
            form_treeCompare2.Nodes.Add(nodeCompare2);
            gd.m_nCompareIndex = 0;
            form_btnCompare.Select();
            form_btnCompare.Text = "> >";
            form_chkCompare1.Text = "1 of " + gd.m_dictCompareDiffs.Count;
            form_btnCollapse.Text = "< <";
            form_colDirDetailCompare.Text = "Directory detail";
            form_lblVolGroup.Text = "Compare Mode";
            form_lblVolGroup.BackColor = Color.LightGoldenrodYellow;
            form_lblVolGroup.Font = new Font(m_FontVolGroupOrig, FontStyle.Regular);
            form_btnFolder.Enabled = false;
            form_btnFiles.Enabled = false;
            form_btnFoldersAndFiles.Enabled = false;
            m_FileListTabPageBeforeCompare = form_tabControlFileList.SelectedTab;
            gd.m_bCompareMode = true;
            form_tabControlFileList.SelectedTab = form_tabPageFileList;
            gd.m_bTreeViewIndirectSelChange = true;
            form_treeCompare1.SelectedNode = (TreeNode)form_treeCompare1.Nodes[0];
            gd.m_bTreeViewIndirectSelChange = true;
            form_treeCompare2.SelectedNode = (TreeNode)form_treeCompare2.Nodes[0];
        }

        void form_btnCopyToClipBoard_Click(object sender = null, EventArgs e = null)
        {
            SDL_TreeView treeView = gd.m_treeCopyToClipboard;

            if (gd.m_bCompareMode)
            {
                if (treeView == null)
                {
                    treeView = form_treeCompare1;
                }

                if (treeView == form_treeViewBrowse)
                {
                    treeView = form_treeCompare1;
                }

                if (treeView.SelectedNode == null)
                {
                    treeView = (treeView == form_treeCompare1) ? form_treeCompare2 : form_treeCompare1;
                }
            }
            else
            {
                treeView = form_treeViewBrowse;
            }

            TreeNode treeNode = (TreeNode)treeView.SelectedNode;

            if (treeNode == null)
            {
                treeNode = gd_Search_1_2.SearchType1_FindNode(form_cbFindbox.Text, (TreeNode)treeView.SelectedNode, treeView);
            }

            if (treeNode != null)
            {
                gd.m_blinky.SelectTreeNode(treeNode, Once: true);
                Clipboard.SetText(GlobalData.FullPath(treeNode));
            }
            else
            {
                gd.m_blinky.Go(clr: Color.Red, Once: true);
            }
        }

        void form_btnCopyScratchpadClear_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvItem in form_lvCopyScratchpad.Items)
            {
                TreeNode treeNode = (TreeNode)lvItem.Tag;

                if (treeNode != null)
                {
                    treeNode.Checked = false;
                }
                else
                {
                    lvItem.Remove();    // 1. sorted by size. 2. ClearMem_TreeForm() does null lvItems.
                }
            }
        }

        void form_btnForward_Click(object sender, EventArgs e)
        {
            gd.DoHistory(sender, +1);
        }

        void form_btnIgnoreAdd_Click(object sender, EventArgs e)
        {
            TreeNode treeNode = (TreeNode)form_treeViewBrowse.SelectedNode;

            if (treeNode == null)
            {
                gd.m_blinky.Go(form_btnIgnoreAdd, clr: Color.Red, Once: true);
            }
            else if (form_lvIgnoreList.Items.ContainsKey(treeNode.Text))
            {
                gd.m_blinky.SelectLVitem(lvItem: (ListViewItem)form_lvIgnoreList.Items[treeNode.Text]);
            }
            else
            {
                ListViewItem lvItem = new ListViewItem(new string[] { treeNode.Text, (treeNode.Level + 1).ToString() });

                lvItem.Name = lvItem.Text;
                form_lvIgnoreList.Items.Add(lvItem);
                gd.m_bKillTree &= gd.m_tmrDoTree.Enabled;
                gd.RestartTreeTimer();
            }
        }

        void form_btnIgnoreDel_Click(object sender, EventArgs e)
        {
            if (form_lvIgnoreList.SelectedItems.IsEmpty())
            {
                gd.m_blinky.Go(form_btnIgnoreDel, clr: Color.Red, Once: true);
                return;
            }

            foreach (ListViewItem lvItem in form_lvIgnoreList.SelectedItems)
            {
                lvItem.Remove();
            }

            gd.m_bKillTree &= gd.m_tmrDoTree.Enabled;
            gd.RestartTreeTimer();
        }

        void form_btnLoadCopyScratchpad_Click(object sender, EventArgs e)
        {
            ListView lvFake = new ListView();   // Hack: check changed event loads the real listviewer
            var nCount = form_lvCopyScratchpad.Columns.Count;

            for (int i = 0; i < nCount; ++i)
            {
                lvFake.Columns.Add(new ColumnHeader());
            }

            if (new SDL_CopyFile().ReadList(lvFake) == false)
            {
                return;
            }

            LoadCopyScratchPad(lvFake);
        }

        void form_btnLoadIgnoreList_Click(object sender, EventArgs e)
        {
            LoadIgnoreList();
        }

        void form_btnSaveCopyDirs_Click(object sender, EventArgs e)
        {
            if (false == form_lvCopyScratchpad.Items.IsEmpty())
            {
                new SDL_CopyFile().WriteList(form_lvCopyScratchpad.Items);
            }
            else
            {
                gd.m_blinky.Go(ctl: form_btnSaveCopyDirs, clr: Color.Red, Once: true);
            }
        }

        void form_btnSaveIgnoreList_Click(object sender, EventArgs e)
        {
            if (false == form_lvIgnoreList.Items.IsEmpty())
            {
                new SDL_IgnoreFile().WriteList(form_lvIgnoreList.Items);
            }
            else
            {
                gd.m_blinky.Go(ctl: form_btnSaveIgnoreList, clr: Color.Red, Once: true);
            }
        }

        // form_btnFolder; form_btnFoldersAndFiles; form_btnFiles
        void form_btnFind_Click(object sender, EventArgs e = null)
        {
            gd_Search_1_2.m_strSelectFile = null;

            if (form_cbFindbox.Text.Length > 0)
            {
                if (m_ctlLastSearchSender != sender)
                {
                    m_ctlLastSearchSender = (Control)sender;
                    gd_Search_1_2.m_nSearchResultsIndexer = -1;
                }

                if ((gd_Search_1_2.m_nSearchResultsIndexer < 0) && new Button[] { form_btnFoldersAndFiles, form_btnFiles }.Contains(sender))
                {
                    DoSearchType2(form_cbFindbox.Text, bSearchFilesOnly: (sender == form_btnFiles));
                }
                else
                {
                    DoSearch(sender);
                }
            }
            else
            {
                gd.m_blinky.Go(clr: Color.Red, Once: true);
            }
        }

        void form_btnTreeCollapse_Click(object sender, EventArgs e)
        {
            if (gd.m_bCompareMode)
            {
                CompareNav(bNext: false);   // doubles as the compare prev button   < <
            }
            else
            {
                form_treeViewBrowse.CollapseAll();
            }
        }

        void form_btnUp_Click(object sender, EventArgs e)
        {
            TreeNode treeNode = (TreeNode)form_treeViewBrowse.SelectedNode;

            if (gd.m_bCompareMode)
            {
                treeNode = (TreeNode)((gd.m_treeCopyToClipboard != null) ? gd.m_treeCopyToClipboard : form_treeCompare1).SelectedNode;
            }

            if (treeNode != null)
            {
                gd.m_bPutPathInFindEditBox = true;
                gd.m_bTreeViewIndirectSelChange = true;

                if (treeNode.Parent != null)
                {
                    if (((TreeNode)treeNode.Parent).Parent == null)
                    {
                        ((RootNodeDatum)treeNode.Parent.Tag).VolumeView = false;
                    }

                    treeNode.TreeView.SelectedNode = treeNode.Parent;
                }
                else
                {
                    RootNodeDatum rootNodeDatum = (RootNodeDatum)treeNode.Tag;

                    if (rootNodeDatum.VolumeView == false)
                    {
                        rootNodeDatum.VolumeView = true;
                        treeNode.TreeView.SelectedNode = null;      // to kick in a change selection event
                        treeNode.TreeView.SelectedNode = treeNode;
                    }
                    else
                    {
                        gd.m_blinky.Go(form_btnUp, clr: Color.Red, Once: true);
                    }
                }
            }
            else
            {
                gd.m_blinky.Go(form_btnUp, clr: Color.Red, Once: true);
            }
        }

        void form_cbFindbox_DropDown(object sender, EventArgs e)
        {
            gd.m_bNavDropDown = true;
        }

        void form_cbFindbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (form_cbFindbox.Text.Length == 0)
            {
                return;
            }

            if (new Keys[] { Keys.Enter, Keys.Return }.Contains((Keys)e.KeyCode))
            {
                gd.m_bPutPathInFindEditBox = false;    // search term is usually not the complete path.
                DoSearch(m_ctlLastSearchSender);
                e.Handled = true;
            }
        }

        void form_cbFindbox_MouseEnter(object sender, EventArgs e)
        {
            UtilProject.WriteLine(DateTime.Now + " form_cbFindbox_MouseEnter");
            m_bFindBoxMouseEnter = true;
        }

        void form_cbFindbox_MouseLeave(object sender, EventArgs e)
        {
            UtilProject.WriteLine(DateTime.Now + " form_cbFindbox_MouseLeave");
            m_bFindBoxMouseEnter = false;
        }

        void form_cbFindbox_MouseUp(object sender, MouseEventArgs e)
        {
            var bToolTip = form_tmapUserCtl.ToolTipActive;

            UtilProject.WriteLine(DateTime.Now + " form_cbFindbox_MouseUp");
            form_tmapUserCtl.ClearSelection();      // resets ToolTipActive

            if (gd.m_bNavDropDown)
            {
                gd.m_bNavDropDown = false;
                return;
            }

            if (false == bToolTip)
            {
                return;
            }

            gd.m_bPutPathInFindEditBox = true;
            gd.m_bTreeViewIndirectSelChange = true;
            gd_Search_1_2.m_strSelectFile = form_tmapUserCtl.Tooltip_Click();

            if (gd_Search_1_2.m_strSelectFile != null)
            {
                gd.m_bTreeViewIndirectSelChange = false;   // didn't hit a sel change
                form_tabControlFileList.SelectedTab = form_tabPageFileList;
                SelectFoundFile();
            }
        }

        void form_cbFindbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlobalData.ComboBoxItemsInsert(form_cbFindbox, bTrimText: false);
        }

        void form_cbFindbox_TextChanged(object sender, EventArgs e)
        {
            if (false == gd_Search_1_2.m_SearchResultsType2_List.IsEmpty())
            {
                gd_Search_1_2.m_SearchResultsType2_List.Clear();
            }

            gd_Search_1_2.m_nSearchResultsIndexer = -1;
            gd_Search_1_2.m_bSearchResultsType2_List = false;
        }

        void form_chk_Compare1_CheckedChanged(object sender, EventArgs e)
        {
            if (gd.m_bChkCompare1IndirectCheckChange)
            {
                gd.m_bChkCompare1IndirectCheckChange = false;
                return;
            }

            if (gd.m_bCompareMode)
            {
                MBoxStatic.Assert(1308.9315, form_chkCompare1.Checked == false);
                form_chkCompare1.Text = gd.m_strChkCompareOrig;
                form_btnCollapse.Text = gd.m_strBtnTreeCollapseOrig;
                form_btnCompare.Text = gd.m_strBtnCompareOrig;
                form_colFilename.Text = gd.m_strColFilesOrig;
                form_colFileCompare.Text = gd.m_strColFileCompareOrig;
                form_colDirDetailCompare.Text = gd.m_strColDirDetailCompareOrig;
                form_lblVolGroup.Text = gd.m_strVolGroupOrig;
                form_lblVolGroup.Font = m_FontVolGroupOrig;
                form_lblVolGroup.BackColor = m_clrVolGroupOrig;
                form_colDirDetail.Text = gd.m_strColDirDetailOrig;
                form_colVolDetail.Text = gd.m_strColVolDetailOrig;
                form_splitTreeFind.Panel1Collapsed = true;
                form_splitTreeFind.Panel2Collapsed = false;
                form_splitCompareFiles.Panel2Collapsed = true;
                form_splitClones.Panel2Collapsed = false;
                gd.m_nodeCompare1 = null;
                gd.m_listTreeNodes_Compare1.Clear();
                gd.m_listTreeNodes_Compare2.Clear();
                gd.m_dictCompareDiffs.Clear();
                form_treeCompare1.Nodes.Clear();
                form_treeCompare2.Nodes.Clear();
                gd.m_listHistory.Clear();
                gd.m_nIxHistory = -1;
                form_btnFolder.Enabled = true;
                form_btnFiles.Enabled = true;
                form_btnFoldersAndFiles.Enabled = true;
                form_tabControlFileList.SelectedTab = m_FileListTabPageBeforeCompare;

                gd.m_bCompareMode = false;
                form_treeView_AfterSelect(form_treeViewBrowse, new TreeViewEventArgs(form_treeViewBrowse.SelectedNode));
            }
            else if (form_chkCompare1.Checked)
            {
                gd.m_nodeCompare1 = (TreeNode)form_treeViewBrowse.SelectedNode;

                if (gd.m_nodeCompare1 != null)
                {
                    if (gd_Search_1_2.m_nSearchResultsIndexer >= 0)
                    {
                        gd.m_blinky.SelectTreeNode(gd.m_nodeCompare1, Once: false);
                    }
                    else
                    {
                        gd.m_blinky.Go();
                    }
                }
                else
                {
                    gd.m_blinky.Go(clr: Color.Red, Once: true);
                    gd.m_bChkCompare1IndirectCheckChange = true;
                    form_chkCompare1.Checked = false;
                }
            }
        }

        void form_chkLoose_CheckedChanged(object sender, EventArgs e)
        {
            if (form_lvIgnoreList.Items.IsEmpty())
            {
                return;
            }

            gd.m_bKillTree &= gd.m_tmrDoTree.Enabled;
            gd.RestartTreeTimer();
        }

        void form_chkSpacer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), e.ClipRectangle);
        }

        void form_SetCopyToClipboardTree(object sender, EventArgs e = null)
        {
            if (sender is TreeView)
            {
                gd.m_treeCopyToClipboard = (SDL_TreeView)sender;
            }
            else if (sender == form_lvFileCompare)
            {
                gd.m_treeCopyToClipboard = form_treeCompare2;
            }
            else
            {
                gd.m_treeCopyToClipboard = gd.m_bCompareMode ? form_treeCompare1 : form_treeViewBrowse;
            }
        }

        void form_lvClones_Enter(object sender, EventArgs e)
        {
            //if (Form.ActiveForm == null)
            //{
            //    return;
            //}

            form_SetCopyToClipboardTree(form_treeViewBrowse);
            gd.m_nLVclonesClickIx = -1;
        }

        void form_lvClones_KeyDown(object sender, KeyEventArgs e)
        {
            MBoxStatic.Assert(1308.9316, gd.QueryLVselChange(sender) == false, bTraceOnly: true);
        }

        void form_lvClones_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Left) && (gd.m_nLVclonesClickIx > 0))
            {
                gd.m_nLVclonesClickIx -= 2;
            }
            else if (e.KeyCode != Keys.Right)
            {
                return;
            }

            UtilProject.WriteLine("form_lvClones_KeyUp");
            LV_CloneSelNode((ListView)sender);
        }

        void form_lvClones_MouseDown(object sender, MouseEventArgs e)
        {
            gd.m_bLVclonesMouseDown = true;
            MBoxStatic.Assert(1308.9317, gd.m_bLVclonesMouseSelChg == false, bTraceOnly: true);
            gd.m_bLVclonesMouseSelChg = false;
        }

        void form_lvClones_MouseLeave(object sender, EventArgs e)
        {
            if (Form.ActiveForm == null)
            {
                return;
            }

            form_lvClones_MouseUp(sender);
            gd.m_bLVclonesMouseSelChg = false;
        }

        void form_lvClones_MouseUp(object sender, MouseEventArgs e = null)
        {
            if (Form.ActiveForm == null)
            {
                return;
            }

       //     Utilities.Assert(1308.9318, gd.QueryLVselChange(sender) == false, bTraceOnly: true);

            if (gd.m_bLVclonesMouseDown == false)  // leave
            {
                return;
            }

            var lv = (ListView)sender;

            if (lv.SelectedItems.IsEmpty())
            {
                return;
            }

            if (gd.m_bLVclonesMouseSelChg)
            {
                LV_ClonesSelChange((ListView)sender);
                gd.m_bLVclonesMouseSelChg = false;
            }
            else
            {
                LV_CloneSelNode((ListView)sender);
            }

            gd.m_bLVclonesMouseDown = false;
        }

        void form_lvClones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gd.m_bCompareMode)
            {
                return;
            }

            if (gd.m_bClonesLVindirectSelChange)
            {
                gd.m_bClonesLVindirectSelChange = false;
                return;
            }

            ListView lv = (ListView)sender;

            if (lv.Focused == false)
            {
                return;
            }

            bool bUp = false;

            if (gd.QueryLVselChange(sender, out bUp) == false)
            {
                return;
            }

            UtilProject.WriteLine("form_lvClones_SelectedIndexChanged");

            if (gd.m_bLVclonesMouseDown)
            {
                gd.m_bLVclonesMouseSelChg = true;
            }
            else
            {
                LV_ClonesSelChange(lv, bUp);
            }
        }

        void form_lvFiles_Enter(object sender, EventArgs e)     // both file listviewers
        {
            if (Form.ActiveForm == null)
            {
                return;
            }

            form_SetCopyToClipboardTree((sender == form_lvFiles) ? form_treeCompare1 : form_treeCompare2);

            if (gd.m_bCompareMode == false)
            {
                return;
            }

            form_cbFindbox.Text = GlobalData.FullPath((TreeNode)gd.m_treeCopyToClipboard.SelectedNode);
        }

        void form_lvTreeNodes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView lv = (ListView)sender;

            if (lv.Tag == null)
            {
                lv.Tag = SortOrder.None;
            }

            if (lv.Items.IsEmpty())
            {
                return;
            }

            lv.Sorting = SortOrder.None;    // initially eg the copy list may be autosorted. From then on use tag, bespoke.

            SortOrder sortOrder = (SortOrder)lv.Tag;
            List<ListViewItem> listItems = new List<ListViewItem>();

            foreach (ListViewItem lvItem in lv.Items.Cast<ListViewItem>().Where(i => i.Tag != null).ToList())
            {
                listItems.Add(lvItem);
            }

            ListViewItem lvSelectedItem = null;

            if (false == lv.SelectedItems.IsEmpty())
            {
                lvSelectedItem = (ListViewItem)lv.SelectedItems[0];
            }

            bool bNullTags = (listItems.IsEmpty());

            if (bNullTags)
            {
                listItems = lv.Items.Cast<ListViewItem>().ToList();
            }

            lv.Items.Clear();

            switch (sortOrder)
            {
                case SortOrder.Ascending:
                {
                    sortOrder = SortOrder.Descending;
                    listItems.Sort((y, x) => x.Text.CompareTo(y.Text));
                    break;
                }

                case SortOrder.Descending:
                {
                    if (bNullTags)
                    {
                        goto case SortOrder.None;
                    }

                    sortOrder = SortOrder.None;

                    if (listItems[0].Tag is UList<TreeNode>)
                    {
                        listItems.Sort((y, x) => ((NodeDatum)((UList<TreeNode>)x.Tag)[0].Tag).nTotalLength.CompareTo(((NodeDatum)((UList<TreeNode>)y.Tag)[0].Tag).nTotalLength));
                    }
                    else
                    {
                        listItems.Sort((y, x) => ((NodeDatum)((TreeNode)x.Tag).Tag).nTotalLength.CompareTo(((NodeDatum)((TreeNode)y.Tag).Tag).nTotalLength));
                    }

                    Collate.InsertSizeMarkers(listItems);
                    break;
                }

                case SortOrder.None:
                {
                    sortOrder = SortOrder.Ascending;
                    listItems.Sort((x, y) => x.Text.CompareTo(y.Text));
                    break;
                }
            }

            lv.Items.AddRange(listItems.ToArray());
            lv.Invalidate();
            lv.Tag = sortOrder;
            lv.SetSortIcon(0, sortOrder);

            if (lvSelectedItem != null)
            {
                lvSelectedItem.Selected = true;
                lvSelectedItem.EnsureVisible();
            }
        }

        void form_lvUnique_MouseClick(object sender, MouseEventArgs e)
        {
            UtilProject.WriteLine("form_lvUnique_MouseClick");
            gd.LV_MarkerClick(form_lvUnique);
        }

        void form_tmapUserCtl_Leave(object sender, EventArgs e)
        {
            //if (Form.ActiveForm == null)
            //{
            //    return;
            //}

            UtilProject.WriteLine(DateTime.Now + " form_tmapUserCtl_Leave();");
            form_tmapUserCtl.ClearSelection(m_bFindBoxMouseEnter);
        }

        void form_tmapUserCtl_MouseDown(object sender, MouseEventArgs e)
        {
            gd.m_btmapUserCtl_MouseDown = true;
        }

        void form_tmapUserCtl_MouseUp(object sender, MouseEventArgs e)
        {
            if (gd.m_btmapUserCtl_MouseDown == false)
            {
                return;
            }

            gd.m_btmapUserCtl_MouseDown = false;

            TreeNode treeNode = form_tmapUserCtl.DoToolTip(e.Location);

            if (treeNode == null)
            {
                return;
            }

            gd.m_bPutPathInFindEditBox = true;
            gd.m_bTreeViewIndirectSelChange = true;
            treeNode.TreeView.SelectedNode = treeNode;
        }

        void form_treeCompare_Enter(object sender, EventArgs e)
        {
            if (Form.ActiveForm == null)
            {
                return;
            }

            form_SetCopyToClipboardTree(sender);
            form_cbFindbox.Text = GlobalData.FullPath((TreeNode)((TreeView)sender).SelectedNode);
        }

        void form_treeViewBrowse_AfterCheck(object sender, TreeViewEventArgs e)
        {
            string strPath = GlobalData.FullPath((TreeNode)e.Node);

            if (e.Node.Checked)
            {
                ListViewItem lvItem = new ListViewItem(new string[] { e.Node.Text, strPath });

                lvItem.Name = strPath;
                lvItem.Tag = e.Node;
                form_lvCopyScratchpad.Items.Add(lvItem);
            }
            else
            {
                form_lvCopyScratchpad.Items.Remove(form_lvCopyScratchpad.Items.Find(strPath, false)[0]);
            }
        }

        void form_treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (gd_Tree == null)
            {
                return;
            }

            if (gd_Tree.m_tree != null)
            {
                // still building the tree: items added to show progress
                return;
            }

            if (BlinkyStruct.TreeSelect)
            {
                return;
            }

            if (gd.m_bRestartTreeTimer)
            {
                return;
            }

            if (gd.m_bHistoryDefer == false)
            {
                gd.m_bHistoryDefer = true;

                if ((false == gd.m_listHistory.IsEmpty()) && (gd.m_nIxHistory > -1) && ((gd.m_listHistory.Count - 1) > gd.m_nIxHistory))
                {
                    gd.m_listHistory.RemoveRange(gd.m_nIxHistory, gd.m_listHistory.Count - gd.m_nIxHistory - 1);
                }

                MBoxStatic.Assert(1308.9319, gd.m_nIxHistory == (gd.m_listHistory.Count - 1));

                if ((gd.m_nIxHistory < 0) || (gd.History_Equals((TreeNode)e.Node) == false))
                {
                    gd.History_Add((TreeNode)e.Node);
                    ++gd.m_nIxHistory;
                }
            }

            gd.m_bHistoryDefer = false;

            if ((gd.m_bTreeViewIndirectSelChange == false) && (e.Node.Parent == null))
            {
                ((RootNodeDatum)e.Node.Tag).VolumeView = true;
            }

            gd.m_bTreeViewIndirectSelChange = false;
            form_tmapUserCtl.Render((TreeNode)e.Node);

            if (sender == form_treeCompare2)
            {
                MBoxStatic.Assert(1308.9321, gd.m_bCompareMode);
                form_lvFileCompare.Items.Clear();
                form_lvDetailVol.Items.Clear();
            }
            else
            {
                form_lvFiles.Items.Clear();
                form_lvDetail.Items.Clear();

                if (gd.m_bCompareMode == false)
                {
                    form_lvDetailVol.Items.Clear();
                }
            }

            TreeNode rootNode = ((TreeNode)e.Node).Root();

            MBoxStatic.Assert(1308.9322, (new object[] { form_treeCompare1, form_treeCompare2 }.Contains(sender)) == gd.m_bCompareMode);
            gd_Tree.DoTreeSelect(e.Node, TreeSelectStatusCallback, TreeSelectDoneCallback);

            string strNode = e.Node.Text;

            MBoxStatic.Assert(1308.9323, false == string.IsNullOrWhiteSpace(strNode));

            if (gd.m_bCompareMode)
            {
                string strDirAndVolume = strNode;
                string strVolume = rootNode.ToolTipText;

                if (strVolume.Contains('\\'))
                {
                    strVolume = strVolume.Substring(0, strVolume.IndexOf('\\'));
                    strDirAndVolume += " (on " + strVolume + ")";
                }

                if (rootNode.Checked)   // hack to denote second compare pane
                {
                    form_colVolDetail.Text = form_colFileCompare.Text = strDirAndVolume;
                }
                else
                {
                    form_colDirDetail.Text = form_colFilename.Text = strDirAndVolume;
                }

                form_cbFindbox.Text = GlobalData.FullPath((TreeNode)e.Node);
                return;
            }

            string strVolumeGroup = ((RootNodeDatum)rootNode.Tag).StrVolumeGroup;

            form_lblVolGroup.Text = string.IsNullOrWhiteSpace(strVolumeGroup) ? "(no volume group set)" : strVolumeGroup;
            form_colVolDetail.Text = rootNode.Text;
            form_colDirDetail.Text = form_colFilename.Text = strNode;

            if (gd.m_bPutPathInFindEditBox)
            {
                gd.m_bPutPathInFindEditBox = false;
                form_cbFindbox.Text = GlobalData.FullPath((TreeNode)e.Node);
            }

            NodeDatum nodeDatum = (NodeDatum)e.Node.Tag;

            if (nodeDatum.nImmediateFiles == 0)
            {
                form_colFilename.Text = gd.m_strColFilesOrig;
            }

            if (nodeDatum.m_lvItem == null)
            {
                return;
            }

            if (nodeDatum.m_lvItem.ListView == null)    // during Corellate()
            {
                MBoxStatic.Assert(1308.9324, gd_Tree.m_threadCollate != null, bTraceOnly: true);
                return;
            }

            if (nodeDatum.m_lvItem.ListView == form_lvIgnoreList)
            {
                foreach (ListViewItem lvItem in nodeDatum.m_lvItem.ListView.SelectedItems)
                {
                    lvItem.Selected = false;
                }
            }
            else if (nodeDatum.m_lvItem.Selected == false)
            {
                gd.m_bClonesLVindirectSelChange = true;
                nodeDatum.m_lvItem.Selected = true;
                nodeDatum.m_lvItem.Focused = true;
                nodeDatum.m_lvItem.EnsureVisible();
            }
        }

        void form_treeViewBrowse_MouseClick(object sender, MouseEventArgs e)
        {
            gd.m_bPutPathInFindEditBox = true;
        }

        void FormAnalysis_DirList_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (gd.m_bCompareMode)
            {
                MBoxStatic.Assert(1308.9325, form_chkCompare1.Checked == true);
                form_chkCompare1.Checked = false;
                e.Cancel = true;
            }
            else
            {
                MBoxStatic.MessageBoxKill();
            }

            Collate.ClearMem();
            GlobalData.Reset();
            m_ownerWindow.Activate();
        }

        private void FormAnalysis_DirList_FormDisposed(object sender, EventArgs e)
        {
            form_tmapUserCtl.Dispose();
        }

        void FormAnalysis_DirList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                e.SuppressKeyPress = e.Handled = true;
            }
            else if (new Keys[] { Keys.Left, Keys.Right }.Contains(e.KeyCode))
            {
                if (gd.m_bCompareMode || (gd_Search_1_2.m_nSearchResultsIndexer >= 0))
                {
                    // L and R prevent the text cursor from walking in the find box, (and the tab-order of controls doesn't work.)
                    e.SuppressKeyPress = e.Handled = true;
                }
            }
            else if (new Keys[] { Keys.Enter, Keys.Return }.Contains(e.KeyCode))
            {
                if (gd_Search_1_2.m_nSearchResultsIndexer >= 0)
                {
                    e.SuppressKeyPress = e.Handled = true;
                }
            }
        }

        void FormAnalysis_DirList_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)0x3)                         // Ctrl-C
            {
                form_btnCopyToClipBoard_Click();
                e.Handled = true;
            }
        }

        void FormAnalysis_DirList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                if (gd.m_bCompareMode)
                {
                    form_chkCompare1.Checked = false;           // leave Compare mode
                }
                else if (form_chkCompare1.Checked == false)
                {
                    form_chkCompare1.Checked = true;            // enter first path to compare
                }
                else
                {
                    form_btnCompare_Click();                    // enter second path and start Compare mode
                }

                e.SuppressKeyPress = e.Handled = true;
            }
            else if (new Keys[] { Keys.Left, Keys.Right, Keys.Enter, Keys.Return }.Contains(e.KeyCode))
            {
                if (gd.m_bCompareMode)
                {
                    if (e.KeyCode == Keys.Left)
                    {
                        CompareNav(bNext: false);
                        e.SuppressKeyPress = e.Handled = true;
                    }
                    else if (e.KeyCode == Keys.Right)
                    {
                        CompareNav();
                        e.SuppressKeyPress = e.Handled = true;
                    }
                }
                else if (gd_Search_1_2.m_nSearchResultsIndexer >= 0)
                {
                    e.SuppressKeyPress = e.Handled = true;

                    if (e.KeyCode == Keys.Left)
                    {
                        if (gd_Search_1_2.m_nSearchResultsIndexer > 1)
                        {
                            gd_Search_1_2.m_nSearchResultsIndexer -= 2;
                        }
                        else if (gd_Search_1_2.m_nSearchResultsIndexer == 1)
                        {
                            gd_Search_1_2.m_nSearchResultsIndexer = 0;
                        }
                        else
                        {
                            return;                         // zeroth item, handled
                        }
                    }

                    // Keys.Left with above processing
                    // Keys.Right, Keys.Enter, Keys.Return
                    gd.m_bPutPathInFindEditBox = false;     // search term is usually not the complete path.
                    DoSearch(m_ctlLastSearchSender);
                }

                // plenty of fall-through for form_cbFindbox etc.
            }
        }
    }
}
