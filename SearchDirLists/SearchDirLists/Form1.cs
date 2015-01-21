using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Hosting;       // release mode
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// TODO:
//      compare file list
//      enable search for compare mode, like it had been
//      first compare item still doesn't go back
//      save treeNode prior to null - file list redraw

namespace SearchDirLists
{
    delegate bool BoolAction();
    delegate MBoxRet MBoxDelegate(string strMessage, string strTitle = null, MBoxBtns? buttons = null);

        [System.ComponentModel.DesignerCategory("Designer")]
   // [System.ComponentModel.DesignerCategory("Code")]
    partial class Form1 : Form
    {
        readonly GlobalData gd = null;

        // Memory allocations occur just below all partial class Form1 : Form declarations, then ClearMem_...() for each.
        // Declarations continue below these two ClearMem() methods.

        void ClearMem_Form1()
        {
            gd.ClearMem_Form1();

            Utilities.Assert(1308.9301, form_lvClones.Items.Count == 0, bTraceOnly: true);
            Utilities.Assert(1308.9302, form_lvSameVol.Items.Count == 0, bTraceOnly: true);
            Utilities.Assert(1308.9303, form_lvUnique.Items.Count == 0, bTraceOnly: true);

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
            ClearMem_Form1();
            gd.ClearMem_Search();
            ClearMem_TreeForm();
        }

        Control m_ctlLastSearchSender = null;
        TabPage m_FileListTabPageBeforeCompare = null;
        internal readonly Color m_clrVolGroupOrig = Color.Empty;
        internal readonly Font m_FontVolGroupOrig = null;

        public class Form1LayoutPanel : TableLayoutPanel
        {
            void SetStyle()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.UserPaint, true);
            }

            public Form1LayoutPanel()
            {
                SetStyle();
            }

            public Form1LayoutPanel(System.ComponentModel.IContainer container)
            {
                container.Add(this);
                SetStyle();
            }
        }

        internal Form1()
        {
            gd = GlobalData.GetInstance(this);
            InitializeComponent();

            gd.timer_DoTree.Interval = new System.TimeSpan(0, 0, 3);
            gd.timer_DoTree.Tick += new System.EventHandler((object sender, EventArgs e) =>
            {
                gd.timer_DoTree.Stop();

                if (gd.m_bCompareMode)
                {
                    Utilities.Assert(1308.9304, form_chkCompare1.Checked);
                    form_chkCompare1.Checked = false;
                }

                DoTree(bKill: gd.m_bKillTree);
                gd.m_bKillTree = true;
                gd.m_bRestartTreeTimer = false;
            });

            // Assert string-lookup form items exist
            //    Utilities.Assert(1308.9305, context_rclick_node.Items[m_strMARKFORCOPY] != null);

            gd.m_blinky = new Blinky(form_cbFindbox);
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

        void CompareNav(bool bNext = true)
        {
            if (gd.m_dictCompareDiffs.Count <= 0)
            {
                return;
            }

            gd.m_nCompareIndex = (bNext) ?
                Math.Min(gd.m_dictCompareDiffs.Count - 1, ++gd.m_nCompareIndex) :
                Math.Max(0, --gd.m_nCompareIndex);

            Utilities.WriteLine(gd.m_dictCompareDiffs.ToArray()[gd.m_nCompareIndex].ToString());
            form_chkCompare1.Text = gd.m_nCompareIndex + 1 + " of " + gd.m_dictCompareDiffs.Count;
            form_lvFiles.Items.Clear();
            form_lvFileCompare.Items.Clear();
            form_lvDetail.Items.Clear();
            form_lvDetailVol.Items.Clear();
            SDLWPF.treeViewCompare1.SelectedNode = null;
            SDLWPF.treeViewCompare2.SelectedNode = null;

            TreeNode treeNode = gd.m_dictCompareDiffs.ToArray()[gd.m_nCompareIndex].Key;

            if (string.IsNullOrWhiteSpace(treeNode.Name))  // can't have a null key in the dictionary so there's a new TreeNode there
            {
                treeNode = null;
            }

            gd.m_bTreeViewIndirectSelChange = true;
            SDLWPF.treeViewCompare1.TopNode = SDLWPF.treeViewCompare1.SelectedNode = treeNode;
            gd.m_bTreeViewIndirectSelChange = true;
            SDLWPF.treeViewCompare2.TopNode = SDLWPF.treeViewCompare2.SelectedNode = gd.m_dictCompareDiffs.ToArray()[gd.m_nCompareIndex].Value;

            if (SDLWPF.treeViewCompare1.SelectedNode == null)
            {
                form_colFilename.Text = gd.m_strColFilesOrig;
                form_colDirDetail.Text = gd.m_strColDirDetailOrig;
                SDLWPF.treeViewCompare1.CollapseAll();
            }
            else
            {
                SDLWPF.treeViewCompare1.SelectedNode.EnsureVisible();
            }

            if (SDLWPF.treeViewCompare2.SelectedNode == null)
            {
                form_colFileCompare.Text = gd.m_strColFileCompareOrig;
                form_colVolDetail.Text = gd.m_strColVolDetailOrig;
                SDLWPF.treeViewCompare2.CollapseAll();
            }
            else
            {
                SDLWPF.treeViewCompare2.SelectedNode.EnsureVisible();
            }
        }

        void ClearToolTip(object sender, EventArgs e)
        {
            if (form_tmapUserCtl.IsDisposed)
            {
                return;
            }

            form_tmapUserCtl.ClearSelection();
        }

        bool FormatPath(Control ctl, ref string strPath, bool bFailOnDirectory = true)
        {
            if (Directory.Exists(Path.GetFullPath(strPath)))
            {
                string strCapDrive = strPath.Substring(0, strPath.IndexOf(@":\") + 2);

                strPath = Path.GetFullPath(strPath).Replace(strCapDrive, strCapDrive.ToUpper());

                if (strPath != strCapDrive.ToUpper())
                {
                    strPath = strPath.TrimEnd('\\');
                }

                ctl.Text = strPath;
            }
            else if (bFailOnDirectory)
            {
      //          form_tabControlMain.SelectedTab = form_tabPageVolumes;
                gd.FormError(ctl, "Path does not exist.", "Save Fields");
                return false;
            }

            strPath = strPath.TrimEnd('\\');
            return true;
        }

        void LoadCopyScratchPad(ListView lvFake)
        {
            int nTotal = 0;
            int nLoaded = 0;

            foreach (SDL_ListViewItem lvItem in lvFake.Items)
            {
                TreeNode treeNode = gd.GetNodeByPath(lvItem.SubItems[1].Text, SDLWPF.treeViewMain);

                if (treeNode != null)
                {
                    treeNode.Checked = true;
                    ++nLoaded;
                }

                ++nTotal;
            }

            if (nLoaded != nTotal)
            {
                Utilities.MBox(nLoaded + " of " + nTotal + " scratchpad folders found in the tree.", "Load copy scratchpad");
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

            if (form_lvIgnoreList.Items.Count > 0)
            {
                gd.m_bKillTree &= gd.timer_DoTree.IsEnabled;
                gd.RestartTreeTimer();
            }
        }

        void LV_CloneSelNode(ListView lv)
        {
            if (Utilities.Assert(1308.9307, lv.SelectedItems.Count > 0, bTraceOnly: true) == false)
            {
                return;
            }

            UList<TreeNode> listTreeNodes = (UList<TreeNode>)((SDL_ListViewItem)lv.SelectedItems[0]).Tag;

            if (Utilities.Assert(1308.9308, listTreeNodes != null, bTraceOnly: true) == false)
            {
                return;
            }

            gd.m_bPutPathInFindEditBox = true;
            gd.m_bTreeViewIndirectSelChange = true;
            gd.m_bClonesLVindirectSelChange = true;                                        // TODO: Is this in the way or is it applicable?
            SDLWPF.treeViewMain.SelectedNode = listTreeNodes[++gd.m_nLVclonesClickIx % listTreeNodes.Count];
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

        void PutTreeCompareNodePathIntoFindCombo(TreeView treeView)
        {
            if (treeView.SelectedNode == null)
            {
                return;
            }

            form_cbFindbox.Text = GlobalData.FullPath((TreeNode)((SDL_TreeView)treeView).SelectedNode);
        }

        void SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;

            if (gd.m_bCompareMode)
            {
                return;
            }

            if (lv.SelectedItems.Count <= 0)
            {
                return;
            }

            if (lv.Focused == false)
            {
                return;
            }

            gd.m_bPutPathInFindEditBox = true;
            gd.m_bTreeViewIndirectSelChange = true;

            TreeNode treeNode = (TreeNode)(SDLWPF.treeViewMain.SelectedNode = (TreeNode)((SDL_ListViewItem)lv.SelectedItems[0]).Tag);

            if (treeNode == null)
            {
                return;
            }

            SDLWPF.treeViewMain.TopNode = (TreeNode)treeNode.Parent;
            treeNode.EnsureVisible();
        }

        void form_btnBack_Click(object sender, EventArgs e)
        {
            gd.DoHistory(sender, -1);
        }

        void form_btnClearIgnoreList_Click(object sender, EventArgs e)
        {
            if (form_lvIgnoreList.Items.Count <= 0)
            {
                return;
            }

            form_lvIgnoreList.Items.Clear();
            gd.m_bKillTree &= gd.timer_DoTree.IsEnabled;
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

            Utilities.Assert(1308.9309, form_chkCompare1.Checked);

            if (SDLWPF.treeViewMain.SelectedNode == null)
            {
                gd.m_blinky.Go(clr: Color.Red, Once: true);
                return;
            }

            TreeNode nodeCompare2 = (TreeNode)SDLWPF.treeViewMain.SelectedNode;

            if (nodeCompare2 == gd.m_nodeCompare1)
            {
                gd.m_blinky.Go(clr: Color.Red, Once: true);
                return;
            }

            gd.m_blinky.Go();
            gd.ClearMem_Search();
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

                while (dictSort.ContainsKey(lMax))
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
            SDLWPF.treeViewCompare1.Nodes.Add(gd.m_nodeCompare1);
            SDLWPF.treeViewCompare2.Nodes.Add(nodeCompare2);
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
            SDLWPF.treeViewCompare1.SelectedNode = (TreeNode)SDLWPF.treeViewCompare1.Nodes[0];
            gd.m_bTreeViewIndirectSelChange = true;
            SDLWPF.treeViewCompare2.SelectedNode = (TreeNode)SDLWPF.treeViewCompare2.Nodes[0];
        }

        void form_btnCopyToClipBoard_Click(object sender = null, EventArgs e = null)
        {
            SDL_TreeView treeView = gd.m_treeCopyToClipboard;

            if (gd.m_bCompareMode)
            {
                if (treeView == null)
                {
                    treeView = SDLWPF.treeViewCompare1;
                }

                if (treeView == SDLWPF.treeViewMain)
                {
                    treeView = SDLWPF.treeViewCompare1;
                }

                if (treeView.SelectedNode == null)
                {
                    treeView = (treeView == SDLWPF.treeViewCompare1) ? SDLWPF.treeViewCompare2 : SDLWPF.treeViewCompare1;
                }
            }
            else
            {
                treeView = SDLWPF.treeViewMain;
            }

            TreeNode treeNode = (TreeNode)treeView.SelectedNode;

            if (treeNode == null)
            {
                treeNode = gd.SearchType1_FindNode(form_cbFindbox.Text, (TreeNode)treeView.SelectedNode, treeView);
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
            foreach (SDL_ListViewItem lvItem in form_lvCopyScratchpad.Items)
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
            TreeNode treeNode = (TreeNode)SDLWPF.treeViewMain.SelectedNode;

            if (treeNode == null)
            {
                gd.m_blinky.Go(form_btnIgnoreAdd, clr: Color.Red, Once: true);
            }
            else if (form_lvIgnoreList.Items.ContainsKey(treeNode.Text))
            {
                gd.m_blinky.SelectLVitem(lvItem: (SDL_ListViewItem)form_lvIgnoreList.Items[treeNode.Text]);
            }
            else
            {
                SDL_ListViewItem lvItem = new SDL_ListViewItem(new string[] { treeNode.Text, (treeNode.Level + 1).ToString() });

                lvItem.Name = lvItem.Text;
                form_lvIgnoreList.Items.Add(lvItem);
                gd.m_bKillTree &= gd.timer_DoTree.IsEnabled;
                gd.RestartTreeTimer();
            }
        }

        void form_btnIgnoreDel_Click(object sender, EventArgs e)
        {
            if (form_lvIgnoreList.SelectedItems.Count <= 0)
            {
                gd.m_blinky.Go(form_btnIgnoreDel, clr: Color.Red, Once: true);
                return;
            }

            foreach (SDL_ListViewItem lvItem in form_lvIgnoreList.SelectedItems)
            {
                lvItem.Remove();
            }

            gd.m_bKillTree &= gd.timer_DoTree.IsEnabled;
            gd.RestartTreeTimer();
        }

        void form_btnLoadCopyScratchpad_Click(object sender, EventArgs e)
        {
            ListView lvFake = new ListView();   // Hack: check changed event loads the real listviewer

            foreach (ColumnHeader col in form_lvCopyScratchpad.Columns)
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
            if (form_lvCopyScratchpad.Items.Count > 0)
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
            if (form_lvIgnoreList.Items.Count > 0)
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
            gd.m_strSelectFile = null;

            if (form_cbFindbox.Text.Length > 0)
            {
                if (m_ctlLastSearchSender != sender)
                {
                    m_ctlLastSearchSender = (Control)sender;
                    gd.m_nSearchResultsIndexer = -1;
                }

                if ((gd.m_nSearchResultsIndexer < 0) && new Button[] { form_btnFoldersAndFiles, form_btnFiles }.Contains(sender))
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
                SDLWPF.treeViewMain.CollapseAll();
            }
        }

        void form_btnUp_Click(object sender, EventArgs e)
        {
            TreeNode treeNode = (TreeNode)SDLWPF.treeViewMain.SelectedNode;

            if (gd.m_bCompareMode)
            {
                treeNode = (TreeNode)((gd.m_treeCopyToClipboard != null) ? gd.m_treeCopyToClipboard : SDLWPF.treeViewCompare1).SelectedNode;
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
            if (form_cbFindbox.Text.Length <= 0)
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

        void form_cbFindbox_MouseUp(object sender, MouseEventArgs e)
        {
            form_tmapUserCtl.ClearSelection();

            if (gd.m_bNavDropDown)
            {
                gd.m_bNavDropDown = false;
                return;
            }

            if (Cursor.Current != Cursors.Arrow)        // hack: clicked in tooltip
            {
                return;
            }

            gd.m_bPutPathInFindEditBox = true;
            gd.m_bTreeViewIndirectSelChange = true;
            gd.m_strSelectFile = form_tmapUserCtl.Tooltip_Click();

            if (gd.m_strSelectFile != null)
            {
                gd.m_bTreeViewIndirectSelChange = false;   // didn't hit a sel change
                form_tabControlFileList.SelectedTab = form_tabPageFileList;
                SelectFoundFile();
            }
        }

        void form_cbFindbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            gd.ComboBoxItemsInsert(form_cbFindbox, bTrimText: false);
        }

        void form_cbFindbox_TextChanged(object sender, EventArgs e)
        {
            if (gd.m_SearchResultsType2_List.Count > 0)
            {
                gd.m_SearchResultsType2_List.Clear();
                GC.Collect();
            }

            gd.m_nSearchResultsIndexer = -1;
            gd.m_bSearchResultsType2_List = false;
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
                Utilities.Assert(1308.9315, form_chkCompare1.Checked == false);
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
                SDLWPF.treeViewCompare1.Nodes.Clear();
                SDLWPF.treeViewCompare2.Nodes.Clear();
                gd.m_listHistory.Clear();
                gd.m_nIxHistory = -1;
                form_btnFolder.Enabled = true;
                form_btnFiles.Enabled = true;
                form_btnFoldersAndFiles.Enabled = true;
                form_tabControlFileList.SelectedTab = m_FileListTabPageBeforeCompare;

                gd.m_bCompareMode = false;
                form_treeView_AfterSelect(SDLWPF.treeViewMain, new TreeViewEventArgs(SDLWPF.treeViewMain.SelectedNode));
            }
            else if (form_chkCompare1.Checked)
            {
                gd.m_nodeCompare1 = (TreeNode)SDLWPF.treeViewMain.SelectedNode;

                if (gd.m_nodeCompare1 != null)
                {
                    if (gd.m_nSearchResultsIndexer >= 0)
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
            if (form_lvIgnoreList.Items.Count <= 0)
            {
                return;
            }

            gd.m_bKillTree &= gd.timer_DoTree.IsEnabled;
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
                gd.m_treeCopyToClipboard = SDLWPF.treeViewCompare2;
            }
            else
            {
                gd.m_treeCopyToClipboard = gd.m_bCompareMode ? SDLWPF.treeViewCompare1 : SDLWPF.treeViewMain;
            }
        }

        void form_lvClones_Enter(object sender, EventArgs e)
        {
            //if (Form.ActiveForm == null)
            //{
            //    return;
            //}

            form_SetCopyToClipboardTree(SDLWPF.treeViewMain);
            gd.m_nLVclonesClickIx = -1;
        }

        void form_lvClones_KeyDown(object sender, KeyEventArgs e)
        {
            Utilities.Assert(1308.9316, gd.QueryLVselChange(sender) == false, bTraceOnly: true);
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

            Utilities.WriteLine("form_lvClones_KeyUp");
            LV_CloneSelNode((ListView)sender);
        }

        void form_lvClones_MouseDown(object sender, MouseEventArgs e)
        {
            gd.m_bLVclonesMouseDown = true;
            Utilities.Assert(1308.9317, gd.m_bLVclonesMouseSelChg == false, bTraceOnly: true);
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

            Utilities.Assert(1308.9318, gd.QueryLVselChange(sender) == false, bTraceOnly: true);

            if (gd.m_bLVclonesMouseDown == false)  // leave
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

            Utilities.WriteLine("form_lvClones_SelectedIndexChanged");

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

            form_SetCopyToClipboardTree((sender == form_lvFiles) ? SDLWPF.treeViewCompare1 : SDLWPF.treeViewCompare2);

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

            if (lv.Items.Count <= 0)
            {
                return;
            }

            lv.Sorting = SortOrder.None;    // initially eg the copy list may be autosorted. From then on use tag, bespoke.

            SortOrder sortOrder = (SortOrder)lv.Tag;
            List<SDL_ListViewItem> listItems = new List<SDL_ListViewItem>();

            foreach (SDL_ListViewItem lvItem in lv.Items.Cast<SDL_ListViewItem>().Where(i => i.Tag != null).ToList())
            {
                listItems.Add(lvItem);
            }

            SDL_ListViewItem lvSelectedItem = null;

            if ((lv.SelectedItems != null) && (lv.SelectedItems.Count > 0))
            {
                lvSelectedItem = (SDL_ListViewItem)lv.SelectedItems[0];
            }

            bool bNullTags = (listItems.Count <= 0);

            if (bNullTags)
            {
                listItems = lv.Items.Cast<SDL_ListViewItem>().ToList();
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
            Utilities.WriteLine("form_lvUnique_MouseClick");
            gd.LV_MarkerClick(form_lvUnique);
        }

        void form_tmapUserCtl_Leave(object sender, EventArgs e)
        {
            //if (Form.ActiveForm == null)
            //{
            //    return;
            //}

            form_tmapUserCtl.ClearSelection();
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
                SDL_ListViewItem lvItem = new SDL_ListViewItem(new string[] { e.Node.Text, strPath });

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
            if (Blinky.TreeSelect)
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

                if ((gd.m_listHistory.Count > 0) && (gd.m_nIxHistory > -1) && ((gd.m_listHistory.Count - 1) > gd.m_nIxHistory))
                {
                    gd.m_listHistory.RemoveRange(gd.m_nIxHistory, gd.m_listHistory.Count - gd.m_nIxHistory - 1);
                }

                Utilities.Assert(1308.9319, gd.m_nIxHistory == (gd.m_listHistory.Count - 1));

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

            if (sender == SDLWPF.treeViewCompare2)
            {
                Utilities.Assert(1308.9321, gd.m_bCompareMode);
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

            Utilities.Assert(1308.9322, (new object[] { SDLWPF.treeViewCompare1, SDLWPF.treeViewCompare2 }.Contains(sender)) == gd.m_bCompareMode);
            gd.DoTreeSelect((TreeNode)e.Node, TreeSelectStatusCallback, TreeSelectDoneCallback);

            string strNode = e.Node.Text;

            Utilities.Assert(1308.9323, false == string.IsNullOrWhiteSpace(strNode));

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

            if (nodeDatum.nImmediateFiles <= 0)
            {
                form_colFilename.Text = gd.m_strColFilesOrig;
            }

            if (nodeDatum.m_lvItem == null)
            {
                return;
            }

            if (nodeDatum.m_lvItem.ListView == null)    // during Corellate()
            {
                Utilities.Assert(1308.9324, gd.m_threadCollate != null, bTraceOnly: true);
                return;
            }

            if (nodeDatum.m_lvItem.ListView == form_lvIgnoreList)
            {
                foreach (SDL_ListViewItem lvItem in nodeDatum.m_lvItem.ListView.SelectedItems)
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

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (gd.m_bCompareMode)
            {
                Utilities.Assert(1308.9325, form_chkCompare1.Checked == true);
                form_chkCompare1.Checked = false;
                e.Cancel = true;
            }
            else
            {
                GlobalData.AppExit = true;
                Utilities.MessageBoxKill();
            }
        }

        void Form1_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            new AboutBox1().ShowDialog_Once(this);
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (form_tabControlMain.SelectedTab != form_tabPageBrowse)
            {
                return;
            }

            if (e.KeyCode == Keys.F2)
            {
                e.SuppressKeyPress = e.Handled = true;
            }
            else if (new Keys[] { Keys.Left, Keys.Right }.Contains(e.KeyCode))
            {
                if (gd.m_bCompareMode || (gd.m_nSearchResultsIndexer >= 0))
                {
                    // L and R prevent the text cursor from walking in the find box, (and the tab-order of controls doesn't work.)
                    e.SuppressKeyPress = e.Handled = true;
                }
            }
            else if (new Keys[] { Keys.Enter, Keys.Return }.Contains(e.KeyCode))
            {
                if (gd.m_nSearchResultsIndexer >= 0)
                {
                    e.SuppressKeyPress = e.Handled = true;
                }
            }
        }

        void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (form_tabControlMain.SelectedTab != form_tabPageBrowse)
            {
                return;
            }

            if (e.KeyChar == (char)0x3)                         // Ctrl-C
            {
                form_btnCopyToClipBoard_Click();
                e.Handled = true;
            }
        }

        void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (form_tabControlMain.SelectedTab != form_tabPageBrowse)
            {
                return;
            }

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
                else if (gd.m_nSearchResultsIndexer >= 0)
                {
                    e.SuppressKeyPress = e.Handled = true;

                    if (e.KeyCode == Keys.Left)
                    {
                        if (gd.m_nSearchResultsIndexer > 1)
                        {
                            gd.m_nSearchResultsIndexer -= 2;
                        }
                        else if (gd.m_nSearchResultsIndexer == 1)
                        {
                            gd.m_nSearchResultsIndexer = 0;
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

        void Form1_Load(object sender, EventArgs e)
        {
            form_tmapUserCtl.TooltipAnchor = (Control)form_cbFindbox;

#if (DEBUG)
//#warning DEBUG is defined.
            Utilities.Assert(0, false, "DEBUG is defined.");
            Utilities.Assert(0, System.Diagnostics.Debugger.IsAttached, "Debugger is not attached!");
#else
            if (Utilities.Assert(0, (System.Diagnostics.Debugger.IsAttached == false), "Debugger is attached but DEBUG is not defined.") == false)
            {
                return;
            }

            ActivationArguments args = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;

            if (Utilities.Assert(1308.9326, args != null, bTraceOnly: true) == false)
            {
                return;
            }

            string[] arrArgs = args.ActivationData;

            if (arrArgs == null)
            {
                // scenario: launched from Start menu
                return;
            }

            if (Utilities.Assert(1308.9327, arrArgs.Length > 0, bTraceOnly: true) == false)
            {
                return;
            }

            string strFile = arrArgs[0];

            switch (Path.GetExtension(strFile).Substring(1))
            {
                case Utilities.mSTRfileExt_Listing:
                {
                    form_cbSaveAs.Text = strFile;
                    AddVolume();
                    form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    gd.RestartTreeTimer();
                    break;
                }

                case Utilities.mSTRfileExt_Volume:
                {
                    if (LoadVolumeList(strFile))
                    {
                        gd.RestartTreeTimer();
                    }

                    break;
                }

                case Utilities.mSTRfileExt_Copy:
                {
                    form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    form_tabControlCopyIgnore.SelectedTab = form_tabPageCopy;
                    gd.m_blinky.Go(form_lvCopyScratchpad, clr: Color.Yellow, Once: true);
                    Utilities.MBox("The Copy scratchpad cannot be loaded with no directory listings.", "Load Copy scratchpad externally");
                    Application.Exit();
                    break;
                }

                case Utilities.mSTRfileExt_Ignore:
                {
                    LoadIgnoreList(strFile);
                    form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    form_tabControlCopyIgnore.SelectedTab = form_tabPageIgnore;
                    break;
                }
            }
#endif
        }

        void label_About_Click(object sender, EventArgs e)
        {
            new AboutBox1().ShowDialog_Once(this);
        }

        void timer_DoTree_Tick(object sender, EventArgs e)
        {
            gd.timer_DoTree.Stop();

            if (gd.m_bCompareMode)
            {
                Utilities.Assert(1308.9328, form_chkCompare1.Checked);
                form_chkCompare1.Checked = false;
            }

            DoTree(bKill: gd.m_bKillTree);
            gd.m_bKillTree = true;
            gd.m_bRestartTreeTimer = false;
        }

    }

    partial class GlobalData : Utilities
    {
        internal static Form1 static_wpfOrForm { get { return static_form_; } }
        internal static Form1 static_form { get { return static_form_; } } static Form1 static_form_ = null;
        internal static GlobalData GetInstance(Form1 form) { if (static_instance == null) static_instance = new GlobalData(form); return static_instance; }
        GlobalData(Form1 form) { static_form_ = form; }               // private constructor: singleton pattern
        internal SDL_TreeView m_treeCopyToClipboard = null;
        internal TreeNode m_nodeCompare1 = null;
        internal readonly Dictionary<TreeNode, TreeNode> m_dictCompareDiffs = new Dictionary<TreeNode, TreeNode>();
        internal readonly UList<TreeNode> m_listTreeNodes_Compare1 = new UList<TreeNode>();
        internal readonly UList<TreeNode> m_listTreeNodes_Compare2 = new UList<TreeNode>();
        internal readonly List<TreeNode> m_listHistory = new List<TreeNode>();
        internal int m_nIxHistory = -1;

        static GlobalData static_instance = null;

        internal static GlobalData GetInstance()
        {
            Utilities.Assert(1308.9329, static_instance != null);
            return static_instance;
        }

        internal void ClearMem_Form1()
        {
            m_nodeCompare1 = null;
            m_dictCompareDiffs.Clear();
            m_listTreeNodes_Compare1.Clear();
            m_listTreeNodes_Compare2.Clear();

            m_listHistory.Clear();
            m_nIxHistory = -1;

            SDLWPF.treeViewMain.Nodes.Clear();
        }

        internal readonly SDL_Timer timer_DoTree = new SDL_Timer();

        internal string m_strVolumeName = null;
        internal string m_strPath = null;
        internal string m_strSaveAs = null;

        internal int m_nCompareIndex = 0;
        internal int m_nLVclonesClickIx = -1;
        internal readonly int[] m_arrSelChgIx = new int[2];
        internal int m_nSelChgIx = 0;
        internal bool m_bLVclonesMouseDown = false;
        internal bool m_bLVclonesMouseSelChg = false;

        internal bool m_bCompareMode = false;
        internal bool m_bPutPathInFindEditBox = false;
        internal bool m_bCheckboxes = false;

        internal bool m_bHistoryDefer = false;
        internal bool m_bTreeViewIndirectSelChange = false;
        internal bool m_bChkCompare1IndirectCheckChange = false;
        internal bool m_bClonesLVindirectSelChange = false;
        internal bool m_bNavDropDown = false;
        internal bool m_btmapUserCtl_MouseDown = false;

        internal bool m_bKillTree = true;
        internal bool m_bRestartTreeTimer = false;

        internal static bool AppExit = false;

        // initialized in Form1 constructor:
        internal Blinky m_blinky = null;
        internal string m_strBtnTreeCollapseOrig = null;
        internal string m_strColFilesOrig = null;
        internal string m_strColFileCompareOrig = null;
        internal string m_strColDirDetailCompareOrig = null;
        internal string m_strColDirDetailOrig = null;
        internal string m_strColVolDetailOrig = null;
        internal string m_strBtnCompareOrig = null;
        internal string m_strChkCompareOrig = null;
        internal string m_strVolGroupOrig = null;

        internal void ComboBoxItemsInsert(ComboBox comboBox, string strText = null, bool bTrimText = true)
        {
            if (string.IsNullOrWhiteSpace(strText))
            {
                strText = comboBox.Text;
            }

            if (bTrimText)
            {
                strText = strText.Trim();
            }

            if (string.IsNullOrWhiteSpace(strText))
            {
                return;
            }

            if (comboBox.Items.Contains(strText))
            {
                return;
            }

            comboBox.Items.Insert(0, strText);
        }

        internal bool Compare(TreeNode t1, TreeNode t2, bool bReverse = false, ulong nMin10M = (10 * 1024 - 100) * 1024, ulong nMin100K = 100 * 1024)
        {
            bool bRet = true;

            foreach (TreeNode s1 in t1.Nodes)
            {
                bool bCompare = true;
                bool bCompareSub = true;
                TreeNode s2 = null;
                NodeDatum n1 = (NodeDatum)s1.Tag;

                if (n1.nTotalLength <= (nMin10M + nMin100K))
                {
                    s1.ForeColor = Color.LightGray;
                }
                else if (t2.Nodes.ContainsKey(s1.Name))
                {
                    s2 = (TreeNode)t2.Nodes[s1.Name];

                    if (Compare(s1, s2, bReverse, nMin10M, nMin100K) == false)
                    {
                        bCompareSub = false;
                    }

                    NodeDatum n2 = (NodeDatum)s2.Tag;

                    bCompare &= (n1.nImmediateFiles == n2.nImmediateFiles);
                    bCompare &= (Math.Abs((long)(n1.nLength - n2.nLength)) <= (long)(nMin10M + nMin100K));

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

                    if ((r1 != null) && (m_dictCompareDiffs.ContainsKey(r1) == false))
                    {
                        m_dictCompareDiffs.Add(r1, r2);
                    }
                    else if (m_dictCompareDiffs.ContainsValue(r2) == false)
                    {
                        m_dictCompareDiffs.Add(new TreeNode(), r2);
                    }
                }

                if (bCompare == false) { s1.ForeColor = Color.Red; }
                else if (bCompareSub == false) { s1.ForeColor = Color.DarkRed; }
                else if (s1.ForeColor == Color.Empty) { s1.ForeColor = Color.SteelBlue; }

                bRet &= (bCompare && bCompareSub);
            }

            return bRet;
        }

        internal void DoHistory(object sender, int nDirection)
        {
            int nIxHistory = m_nIxHistory + (nDirection > 0 ? 1 : -1);

            if ((nIxHistory >= 0) && (m_listHistory.Count > 0) && (nIxHistory <= (m_listHistory.Count - 1)))
            {
                TreeNode treeNode = History_GetAt(nIxHistory);

                if (treeNode.TreeView.SelectedNode == treeNode)
                {
                    treeNode.TreeView.SelectedNode = null;      // VolumeView needs refresh since it's the same node
                }

                m_nIxHistory = nIxHistory;
                m_bHistoryDefer = true;
                m_bPutPathInFindEditBox = true;
                m_bTreeViewIndirectSelChange = true;
                treeNode.TreeView.SelectedNode = treeNode;
            }
            else
            {
                m_blinky.Go((Control)sender, clr: Color.Red, Once: true);
            }
        }

        internal void FormError(Control control, string strError, string strTitle)
        {
            m_blinky.Go(control, clr: Color.Red, Once: true);
            MBox(strError, strTitle);
            m_blinky.Go(control, clr: Color.Red, Once: true);
        }

        internal static string FullPath(TreeNode treeNode)
        {
            if (treeNode == null)
            {
                return null;
            }

            StringBuilder sbFullPath = null;
            TreeNode parentNode = (TreeNode)treeNode.Parent;

            if (parentNode == null)
            {
                sbFullPath = new StringBuilder(treeNode.Name);
            }
            else
            {
                sbFullPath = new StringBuilder(treeNode.Text);
            }

            while ((parentNode != null) && (parentNode.Parent != null))
            {
                sbFullPath.Insert(0, '\\');
                sbFullPath.Insert(0, parentNode.Text.TrimEnd('\\'));
                parentNode = (TreeNode)parentNode.Parent;
            }

            if ((parentNode != null) && (parentNode.Parent == null))
            {
                sbFullPath.Insert(0, '\\');
                sbFullPath.Insert(0, parentNode.Name.TrimEnd('\\'));
            }

            return sbFullPath.ToString().Replace(@"\\", @"\");
        }

        internal TreeNode History_GetAt(int n)
        {
            TreeNode treeNode = m_listHistory[n];

            if (treeNode.Tag is TreeNode)
            {
                TreeNode treeNode_A = (TreeNode)treeNode.Tag;

                ((RootNodeDatum)treeNode_A.Tag).VolumeView = treeNode.Checked;
                return treeNode_A;
            }
            else
            {
                return treeNode;
            }
        }

        internal bool History_Equals(TreeNode treeNode)
        {
            if (treeNode.Tag is RootNodeDatum)
            {
                TreeNode treeNode_A = (TreeNode)m_listHistory[m_listHistory.Count - 1];

                if ((treeNode_A.Tag is TreeNode) == false)
                {
                    return false;
                }

                if (((RootNodeDatum)treeNode.Tag).VolumeView != treeNode_A.Checked)
                {
                    return false;
                }

                return (treeNode_A.Tag == treeNode);
            }
            else
            {
                return (treeNode == m_listHistory[m_listHistory.Count - 1]);
            }
        }

        internal void History_Add(TreeNode treeNode)
        {
            if (treeNode.TreeView == null)
            {
                return;
            }

            if (treeNode.Tag is RootNodeDatum)
            {
                TreeNode treeNode_A = new TreeNode();   // checked means VolumeView mode and necessitates History_Add() etc.

                treeNode_A.Checked = ((RootNodeDatum)treeNode.Tag).VolumeView;
                treeNode_A.Tag = treeNode;
                m_listHistory.Add(treeNode_A);
            }
            else
            {
                m_listHistory.Add(treeNode);
            }
        }

        internal void InterruptTreeTimerWithAction(BoolAction boolAction)
        {
            bool bTimer = timer_DoTree.IsEnabled;

            timer_DoTree.Stop();

            bool bKillTree = boolAction();

            if (bKillTree)
            {
                KillTreeBuilder();
            }

            if (bKillTree || bTimer)
            {
                RestartTreeTimer();
            }
        }

        internal bool LV_MarkerClick(ListView lv, bool bUp = false)     // returns true when selected tag is not null, and may change selection.
        {
            if (lv.SelectedItems.Count <= 0)
            {
                return false;
            }

            if (((ListViewItem)lv.SelectedItems[0]).Tag != null)
            {
                return true;
            }

            // marker item
            int nIx = ((SDL_ListViewItem)lv.SelectedItems[0]).Index + 1;
            bool bGt = (nIx >= lv.Items.Count);

            if (bUp || bGt)
            {
                if ((nIx - 2) >= 0)
                {
                    nIx -= 2;
                }
                else if (bGt)
                {
                    Utilities.Assert(1308.9331, false, bTraceOnly: true);
                    return false;   // LV with just a marker item?
                }
            }

            SDL_ListViewItem lvItem = (SDL_ListViewItem)lv.Items[nIx];

            if (Utilities.Assert(1308.9332, lvItem.Tag != null, bTraceOnly: true) == false)
            {
                return false;
            }

            m_bClonesLVindirectSelChange = true;

            Utilities.WriteLine("LVMarkerClick");
            lvItem.EnsureVisible();
            lvItem.Select();
            lvItem.Focused = true;

            if (lv.SelectedItems.Count <= 0)
            {
                return false;
            }

            return (((ListViewItem)lv.SelectedItems[0]).Tag != null);
        }

        internal void NameNodes(TreeNode treeNode, UList<TreeNode> listTreeNodes)
        {
            treeNode.Name = treeNode.Text;
            treeNode.ForeColor = Color.Empty;
            listTreeNodes.Add(treeNode);

            foreach (TreeNode subNode in treeNode.Nodes)
            {
                NameNodes(subNode, listTreeNodes);
            }
        }

        internal bool QueryLVselChange(object sender)
        {
            bool bUp = false;

            return QueryLVselChange(sender, out bUp);
        }

        internal bool QueryLVselChange(object sender, out bool bUp)
        {
            bUp = false;

            if ((sender is ListView) == false)
            {
                Utilities.Assert(1308.9333, false, bTraceOnly: true);
                return false;
            }

            ListView lv = (ListView)sender;

            if (lv.SelectedItems.Count <= 0)
            {
                return false;
            }

            ++m_nSelChgIx;

            int nNow =  m_arrSelChgIx[m_nSelChgIx %= 2] = ((SDL_ListViewItem)lv.SelectedItems[0]).Index;
            int nPrev = m_arrSelChgIx[(m_nSelChgIx + 1) % 2];

            bUp = nNow < nPrev;
            return nNow != nPrev;
        }

        internal void RestartTreeTimer()
        {
            m_bRestartTreeTimer = m_bKillTree;
            timer_DoTree.Stop();
            timer_DoTree.Start();
        }

        internal void RemoveCorrelation(TreeNode treeNode_in, bool bContinue = false)
        {
            TreeNode treeNode = treeNode_in;

            do
            {
                if ((treeNode.Nodes != null) && (treeNode.Nodes.Count > 0))
                {
                    RemoveCorrelation((TreeNode)treeNode.Nodes[0], bContinue: true);
                }

                if (m_listTreeNodes.Contains(treeNode))
                {
                    m_listTreeNodes.Remove(treeNode);
                }

                treeNode.ForeColor = Color.Empty;
                treeNode.BackColor = Color.Empty;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                if (m_dictNodes.ContainsKey(nodeDatum.Key) == false)
                {
                    // same scenario as empty parent:
                    // Search "Parent folder may contain only its clone subfolder, in which case unmark the subfolder"
                    continue;
                }

                UList<TreeNode> listClones = m_dictNodes[nodeDatum.Key];

                if (listClones.Contains(treeNode))
                {
                    listClones.Remove(treeNode);

                    if (listClones.Count <= 0)
                    {
                        m_dictNodes.Remove(nodeDatum.Key);
                    }
                }
            }
            while (bContinue && ((treeNode = (TreeNode)treeNode.NextNode) != null));
        }
    }
}
