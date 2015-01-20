using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Hosting;
using System.Threading;

// TODO:
//      compare file list
//      enable search for compare mode, like it had been
//      first compare item still doesn't go back
//      save treeNode prior to null - file list redraw

namespace SearchDirLists
{
    delegate DialogResult MessageBoxDelegate(string strMessage, string strTitle = null, MessageBoxButtons? buttons = null);
    delegate bool BoolAction();

    partial class Form1 : Form
    {
        TreeNode m_nodeCompare1 = null;
        Dictionary<TreeNode, TreeNode> m_dictCompareDiffs = new Dictionary<TreeNode, TreeNode>();
        UList<TreeNode> m_listTreeNodes_Compare1 = new UList<TreeNode>();
        UList<TreeNode> m_listTreeNodes_Compare2 = new UList<TreeNode>();

        List<TreeNode> m_listHistory = new List<TreeNode>();
        int m_nIxHistory = -1;

        // Memory allocations occur just below all partial class Form1 : Form declarations, then ClearMem_...() for each.
        // Declarations continue below these two ClearMem() methods.

        void ClearMem_Form1()
        {
            Utilities.Assert(1308.9322, form_lvClones.Items.Count == 0);
            Utilities.Assert(1308.9301, form_lvSameVol.Items.Count == 0);
            Utilities.Assert(1308.9302, form_lvUnique.Items.Count == 0);

            form_lvClones.Items.Clear();
            form_lvSameVol.Items.Clear();
            form_lvUnique.Items.Clear();

            m_nodeCompare1 = null;
            m_dictCompareDiffs.Clear();
            m_listTreeNodes_Compare1.Clear();
            m_listTreeNodes_Compare2.Clear();

            m_listHistory.Clear();
            m_nIxHistory = -1;

            form_lvFiles.Items.Clear();
            form_lvFileCompare.Items.Clear();
            form_lvDetail.Items.Clear();
            form_lvDetailVol.Items.Clear();
            form_tmapUserCtl.Clear();
            form_treeViewBrowse.Nodes.Clear();
        }

        void ClearMem()
        {
            Collate.ClearMem();
            ClearMem_Form1();
            ClearMem_Search();
            ClearMem_TreeForm();
        }

        string m_strSelectFile = null;

        int m_nCompareIndex = 0;
        int m_nLVclonesClickIx = -1;
        int[] m_arrSelChgIx = new int[2];
        int m_nSelChgIx = 0;
        bool m_bLVclonesMouseDown = false;
        bool m_bLVclonesMouseSelChg = false;

        bool m_bCompareMode = false;
        bool m_bPutPathInFindEditBox = false;
        bool m_bCheckboxes = false;

        TreeView m_treeCopyToClipboard = null;
        Control m_ctlLastSearchSender = null;

        bool m_bHistoryDefer = false;
        bool m_bTreeViewIndirectSelChange = false;
        bool m_bChkCompare1IndirectCheckChange = false;
        bool m_bClonesLVindirectSelChange = false;
        bool m_bNavDropDown = false;
        bool m_btmapUserCtl_MouseDown = false;

        Form m_form1MessageBoxOwner = null;
        TabPage m_FileListTabPageBeforeCompare = null;
        bool m_bKillTree = true;
        bool m_bRestartTreeTimer = false;

        static bool m_bAppExit = false;
        public static bool AppExit { get { return m_bAppExit; } }

        // initialized in constructor:
        public static Form static_form = null;
        readonly Blinky m_blinky = null;
        readonly Color m_clrVolGroupOrig = Color.Empty;
        readonly Font m_FontVolGroupOrig = null;
        readonly string m_strBtnTreeCollapseOrig = null;
        readonly string m_strColFilesOrig = null;
        readonly string m_strColFileCompareOrig = null;
        readonly string m_strColDirDetailCompareOrig = null;
        readonly string m_strColDirDetailOrig = null;
        readonly string m_strColVolDetailOrig = null;
        readonly string m_strBtnCompareOrig = null;
        readonly string m_strChkCompareOrig = null;
        readonly string m_strVolGroupOrig = null;

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

        public class Form1TreeView : TreeView
        {
            // enable double buffer

            public Form1TreeView()
            {
                DoubleBuffered = true;
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
                base.OnHandleCreated(e);
            }
            private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
            private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
            private const int TVS_EX_DOUBLEBUFFER = 0x0004;
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

            // suppress double click bug in treeview. Affects checkboxes

            protected override void WndProc(ref Message m)
            {
                // Suppress WM_LBUTTONDBLCLK
                if (m.Msg == 0x203) { m.Result = IntPtr.Zero; }
                else base.WndProc(ref m);
            }
        }

        internal Form1()
        {
            static_form = this;
            InitializeComponent();

            // Assert string-lookup form items exist
            //    Utilities.Assert(1308.9303, context_rclick_node.Items[m_strMARKFORCOPY] != null);

            m_blinky = new Blinky(timer_blinky, form_cbFindbox);
            m_strBtnTreeCollapseOrig = form_btnCollapse.Text;
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
            m_bCheckboxes = form_treeViewBrowse.CheckBoxes;
            Utilities.SetMessageBoxDelegate(Form1MessageBox);
            SDL_File.SetFileDialogs(openFileDialog1, saveFileDialog1);
        }

        void ComboBoxItemsInsert(ComboBox comboBox, string strText = null, bool bTrimText = true)
        {
            if (Utilities.StrValid(strText) == false)
            {
                strText = comboBox.Text;
            }

            if (bTrimText)
            {
                strText = strText.Trim();
            }

            if (Utilities.StrValid(strText) == false)
            {
                return;
            }

            if (comboBox.Items.Contains(strText))
            {
                return;
            }

            comboBox.Items.Insert(0, strText);
        }

        bool Compare(TreeNode t1, TreeNode t2, bool bReverse = false, ulong nMin10M = (10 * 1024 - 100) * 1024, ulong nMin100K = 100 * 1024)
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
                    s2 = t2.Nodes[s1.Name];

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

        void CompareNav(bool bNext = true)
        {
            if (m_dictCompareDiffs.Count <= 0)
            {
                return;
            }

            m_nCompareIndex = (bNext) ?
                Math.Min(m_dictCompareDiffs.Count - 1, ++m_nCompareIndex) :
                Math.Max(0, --m_nCompareIndex);

            Utilities.WriteLine(m_dictCompareDiffs.ToArray()[m_nCompareIndex].ToString());
            form_chkCompare1.Text = m_nCompareIndex + 1 + " of " + m_dictCompareDiffs.Count;
            form_lvFiles.Items.Clear();
            form_lvFileCompare.Items.Clear();
            form_lvDetail.Items.Clear();
            form_lvDetailVol.Items.Clear();
            form_treeCompare1.SelectedNode = null;
            form_treeCompare2.SelectedNode = null;

            TreeNode treeNode = m_dictCompareDiffs.ToArray()[m_nCompareIndex].Key;

            if (Utilities.StrValid(treeNode.Name) == false)  // can't have a null key in the dictionary so there's a new TreeNode there
            {
                treeNode = null;
            }

            m_bTreeViewIndirectSelChange = true;
            form_treeCompare1.TopNode = form_treeCompare1.SelectedNode = treeNode;
            m_bTreeViewIndirectSelChange = true;
            form_treeCompare2.TopNode = form_treeCompare2.SelectedNode = m_dictCompareDiffs.ToArray()[m_nCompareIndex].Value;

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

        void ClearToolTip(object sender, EventArgs e)
        {
            if (form_tmapUserCtl.IsDisposed)
            {
                return;
            }

            form_tmapUserCtl.ClearSelection();
        }

        void DoHistory(object sender, int nDirection)
        {
            int nIxHistory = m_nIxHistory + nDirection;

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

        // make MessageBox modal from a worker thread
        DialogResult Form1MessageBox(string strMessage, string strTitle = null, MessageBoxButtons? buttons = null)
        {
            if (AppExit)
            {
                return DialogResult.None;
            }

            if (InvokeRequired) { return (DialogResult)Invoke(new MessageBoxDelegate(Form1MessageBox), new object[] { strMessage, strTitle, buttons }); }

            m_blinky.Reset();

            if (m_form1MessageBoxOwner != null)
            {
                m_form1MessageBoxOwner.Dispose();
                m_form1MessageBoxOwner = null;
            }

            m_form1MessageBoxOwner = new Form();
            m_form1MessageBoxOwner.Owner = this;
            m_form1MessageBoxOwner.Text = strTitle;

            DialogResult dlgRet = DialogResult.None;

            if (buttons == null)
            {
                dlgRet = MessageBox.Show(m_form1MessageBoxOwner, strMessage.PadRight(100), strTitle);
            }
            else
            {
                dlgRet = MessageBox.Show(m_form1MessageBoxOwner, strMessage.PadRight(100), strTitle, buttons.Value);
            }

            if (m_form1MessageBoxOwner != null)
            {
                m_form1MessageBoxOwner.Dispose();
                m_form1MessageBoxOwner = null;
                return dlgRet;
            }

            // cancelled externally
            return DialogResult.None;
        }

        void FormError(Control control, string strError, string strTitle)
        {
            m_blinky.Go(control, clr: Color.Red);
            Form1MessageBox(strError, strTitle);
            m_blinky.Go(control, clr: Color.Red, Once: true);
        }

        static string FullPath(TreeNode treeNode)
        {
            if (treeNode == null)
            {
                return null;
            }

            StringBuilder stbFullPath = null;
            TreeNode parentNode = treeNode.Parent;
            string P = Path.DirectorySeparatorChar.ToString();
            string PP = P + P;

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
                stbFullPath.Insert(0, P);
                stbFullPath.Insert(0, parentNode.Text.TrimEnd(Path.DirectorySeparatorChar));
                parentNode = parentNode.Parent;
            }

            if ((parentNode != null) && (parentNode.Parent == null))
            {
                stbFullPath.Insert(0, P);
                stbFullPath.Insert(0, parentNode.Name.TrimEnd(Path.DirectorySeparatorChar));
            }

            return stbFullPath.ToString().Replace(PP, P);
        }

        TreeNode GetNodeByPath(string path, TreeView treeView)
        {
            return GetNodeByPath_A(path, treeView) ?? GetNodeByPath_A(path, treeView, bIgnoreCase: true);
        }

        TreeNode GetNodeByPath_A(string strPath, TreeView treeView, bool bIgnoreCase = false)
        {
            if (Utilities.StrValid(strPath) == false)
            {
                return null;
            }

            if (bIgnoreCase)
            {
                strPath = strPath.ToLower();
            }

            TreeNode nodeRet = null;
            string P = Path.DirectorySeparatorChar.ToString();
            string PP = P + P;

            foreach (TreeNode topNode in treeView.Nodes)
            {
                string[] arrPath = null;
                int nPathLevelLength = 0;
                int nLevel = 0;
                string strNode = topNode.Name.TrimEnd(Path.DirectorySeparatorChar).Replace(PP, P);

                if (bIgnoreCase)
                {
                    strNode = strNode.ToLower();
                }

                arrPath = strPath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                nPathLevelLength = arrPath.Length;

                if (strNode.Contains(Path.DirectorySeparatorChar))
                {
                    int nCount = strNode.Count(c => c == Path.DirectorySeparatorChar);

                    for (int n = 0; n < nPathLevelLength - 1; ++n)
                    {
                        if (n < nCount)
                        {
                            arrPath[0] += Path.DirectorySeparatorChar + arrPath[n + 1];
                        }
                    }

                    for (int n = 1; n < nPathLevelLength - 1; ++n)
                    {
                        if ((nCount + n) < arrPath.Length)
                        {
                            arrPath[n] = arrPath[nCount + n];
                        }
                    }

                    if (nPathLevelLength > 1)
                    {
                        Utilities.Assert(1308.9305, (nPathLevelLength - nCount) > 0);
                        nPathLevelLength -= nCount;
                    }
                }

                if (strNode == arrPath[nLevel])
                {
                    nodeRet = topNode;
                    nLevel++;

                    if ((nLevel < nPathLevelLength) && nodeRet != null)
                    {
                        nodeRet = GetSubNode(nodeRet, arrPath, nLevel, nPathLevelLength, bIgnoreCase);

                        if (nodeRet != null)
                        {
                            return nodeRet;
                        }
                    }
                }
            }

            return nodeRet;
        }

        TreeNode GetSubNode(TreeNode node, string[] pathLevel, int i, int nPathLevelLength, bool bIgnoreCase)
        {
            foreach (TreeNode subNode in node.Nodes)
            {
                string strText = bIgnoreCase ? subNode.Text.ToLower() : subNode.Text;

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

        TreeNode History_GetAt(int n)
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

        bool History_Equals(TreeNode treeNode)
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

        void History_Add(TreeNode treeNode)
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

        void InterruptTreeTimerWithAction(BoolAction boolAction)
        {
            bool bTimer = timer_DoTree.Enabled;

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

        void LoadCopyScratchPad(ListView lvFake)
        {
            int nTotal = 0;
            int nLoaded = 0;

            foreach (ListViewItem lvItem in lvFake.Items)
            {
                TreeNode treeNode = GetNodeByPath(lvItem.SubItems[1].Text, form_treeViewBrowse);

                if (treeNode != null)
                {
                    treeNode.Checked = true;
                    ++nLoaded;
                }

                ++nTotal;
            }

            if (nLoaded != nTotal)
            {
                Form1MessageBox(nLoaded + " of " + nTotal + " scratchpad folders found in the tree.", "Load copy scratchpad");
                form_tabControlCopyIgnore.SelectedTab = form_tabPageCopy;
                m_blinky.Go(form_lvCopyScratchpad, clr: Color.Yellow, Once: true);
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
                m_bKillTree &= timer_DoTree.Enabled;
                RestartTreeTimer();
            }
        }

        void LV_CloneSelNode(ListView lv)
        {
            if (Utilities.Assert(1308.93183, lv.SelectedItems.Count > 0) == false)
            {
                return;
            }

            UList<TreeNode> listTreeNodes = (UList<TreeNode>)lv.SelectedItems[0].Tag;

            if (Utilities.Assert(1308.93187, listTreeNodes != null) == false)
            {
                return;
            }

            m_bPutPathInFindEditBox = true;
            m_bTreeViewIndirectSelChange = true;
            m_bClonesLVindirectSelChange = true;                                        // TODO: Is this in the way or is it applicable?
            form_treeViewBrowse.SelectedNode = listTreeNodes[++m_nLVclonesClickIx % listTreeNodes.Count];
        }

        void LV_ClonesSelChange(ListView lv, bool bUp = false)
        {
            if (LV_MarkerClick(lv, bUp) == false)
            {
                return;
            }

            m_nLVclonesClickIx = -1;
            LV_CloneSelNode(lv);
        }

        bool LV_MarkerClick(ListView lv, bool bUp = false)     // returns true when selected tag is not null, and may change selection.
        {
            if (lv.SelectedItems.Count <= 0)
            {
                return false;
            }

            if (lv.SelectedItems[0].Tag != null)
            {
                return true;
            }

            // marker item
            int nIx = lv.SelectedItems[0].Index + 1;
            bool bGt = (nIx >= lv.Items.Count);

            if (bUp || bGt)
            {
                if ((nIx - 2) >= 0)
                {
                    nIx -= 2;
                }
                else if (bGt)
                {
                    Utilities.Assert(1308.93189, false, bTraceOnly: true);
                    return false;   // LV with just a marker item?
                }
            }

            ListViewItem lvItem = lv.Items[nIx];

            if (Utilities.Assert(1308.9318, lvItem.Tag != null) == false)
            {
                return false;
            }

            m_bClonesLVindirectSelChange = true;

            Utilities.WriteLine("LVMarkerClick");
            lvItem.EnsureVisible();
            lvItem.Selected = true;
            lvItem.Focused = true;

            if (lv.SelectedItems.Count <= 0)
            {
                return false;
            }

            return (lv.SelectedItems[0].Tag != null);
        }

        void NameNodes(TreeNode treeNode, UList<TreeNode> listTreeNodes)
        {
            treeNode.Name = treeNode.Text;
            treeNode.ForeColor = Color.Empty;
            listTreeNodes.Add(treeNode);

            foreach (TreeNode subNode in treeNode.Nodes)
            {
                NameNodes(subNode, listTreeNodes);
            }
        }

        void PutTreeCompareNodePathIntoFindCombo(TreeView treeView)
        {
            if (treeView.SelectedNode == null)
            {
                return;
            }

            form_cbFindbox.Text = FullPath(treeView.SelectedNode);
        }

        bool QueryLVselChange(object sender)
        {
            bool bUp = false;

            return QueryLVselChange(sender, out bUp);
        }

        bool QueryLVselChange(object sender, out bool bUp)
        {
            bUp = false;

            if ((sender is ListView) == false)
            {
                Utilities.Assert(1308.9319, false);
                return false;
            }

            ListView lv = (ListView)sender;

            if (lv.SelectedItems.Count <= 0)
            {
                return false;
            }

            ++m_nSelChgIx;

            int nNow = m_arrSelChgIx[m_nSelChgIx %= 2] = lv.SelectedItems[0].Index;
            int nPrev = m_arrSelChgIx[(m_nSelChgIx + 1) % 2];

            bUp = nNow < nPrev;
            return nNow != nPrev;
        }

        void RestartTreeTimer()
        {
            m_bRestartTreeTimer = m_bKillTree;
            timer_DoTree.Stop();
            timer_DoTree.Start();
        }

        void RemoveCorrelation(TreeNode treeNode_in, bool bContinue = false)
        {
            TreeNode treeNode = treeNode_in;

            do
            {
                if ((treeNode.Nodes != null) && (treeNode.Nodes.Count > 0))
                {
                    RemoveCorrelation(treeNode.Nodes[0], bContinue: true);
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
                    // same scenario as empty parent.
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
            while (bContinue && ((treeNode = treeNode.NextNode) != null));
        }

        void SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;

            if (m_bCompareMode)
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

            m_bPutPathInFindEditBox = true;
            m_bTreeViewIndirectSelChange = true;

            TreeNode treeNode = form_treeViewBrowse.SelectedNode = (TreeNode)lv.SelectedItems[0].Tag;

            if (treeNode == null)
            {
                return;
            }

            form_treeViewBrowse.TopNode = treeNode.Parent;
            treeNode.EnsureVisible();
        }

        void form_btnBack_Click(object sender, EventArgs e)
        {
            DoHistory(sender, -1);
        }

        void form_btnClearIgnoreList_Click(object sender, EventArgs e)
        {
            if (form_lvIgnoreList.Items.Count <= 0)
            {
                return;
            }

            form_lvIgnoreList.Items.Clear();
            m_bKillTree &= timer_DoTree.Enabled;
            RestartTreeTimer();
        }

        void form_btnCompare_Click(object sender = null, EventArgs e = null)
        {
            if (m_bCompareMode)
            {
                CompareNav();       // doubles as forward button  > >
                return;
            }
            
            if (form_chkCompare1.Checked == false)
            {
                form_chkCompare1.Checked = true;
                return;
            }

            Utilities.Assert(1308.9306, form_chkCompare1.Checked);

            if (form_treeViewBrowse.SelectedNode == null)
            {
                m_blinky.Go(clr: Color.Red, Once: true);
                return;
            }

            TreeNode nodeCompare2 = form_treeViewBrowse.SelectedNode;

            if (nodeCompare2 == m_nodeCompare1)
            {
                m_blinky.Go(clr: Color.Red, Once: true);
                return;
            }

            m_blinky.Go();
            ClearMem_Search();
            m_listHistory.Clear();
            m_nIxHistory = -1;
            form_splitTreeFind.Panel1Collapsed = false;
            form_splitTreeFind.Panel2Collapsed = true;
            form_splitCompareFiles.Panel2Collapsed = false;
            form_splitClones.Panel2Collapsed = true;

            RootNodeDatum rootNodeDatum1 = (RootNodeDatum)m_nodeCompare1.Root().Tag;
            RootNodeDatum rootNodeDatum2 = (RootNodeDatum)nodeCompare2.Root().Tag;
            string strFullPath1 = FullPath(m_nodeCompare1);
            string strFullPath2 = FullPath(nodeCompare2);
            string strFullPath1A = m_nodeCompare1.FullPath;
            string strFullPath2A = nodeCompare2.FullPath;

            m_nodeCompare1 = (TreeNode)m_nodeCompare1.Clone();
            nodeCompare2 = (TreeNode)nodeCompare2.Clone();
            NameNodes(m_nodeCompare1, m_listTreeNodes_Compare1);
            NameNodes(nodeCompare2, m_listTreeNodes_Compare2);
            Compare(m_nodeCompare1, nodeCompare2);
            Compare(nodeCompare2, m_nodeCompare1, bReverse: true);

            if (m_dictCompareDiffs.Count < 15)
            {
                m_dictCompareDiffs.Clear();
                Compare(m_nodeCompare1, nodeCompare2, nMin10M: 0);
                Compare(nodeCompare2, m_nodeCompare1, bReverse: true, nMin10M: 0);
            }

            if (m_dictCompareDiffs.Count < 15)
            {
                m_dictCompareDiffs.Clear();
                Compare(m_nodeCompare1, nodeCompare2, nMin10M: 0, nMin100K: 0);
                Compare(nodeCompare2, m_nodeCompare1, bReverse: true, nMin10M: 0, nMin100K: 0);
            }

            SortedDictionary<ulong, KeyValuePair<TreeNode, TreeNode>> dictSort = new SortedDictionary<ulong, KeyValuePair<TreeNode, TreeNode>>();

            foreach (KeyValuePair<TreeNode, TreeNode> pair in m_dictCompareDiffs)
            {
                ulong l1 = 0, l2 = 0;

                if (Utilities.StrValid(pair.Key.Text))
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

            m_dictCompareDiffs.Clear();

            if (rootNodeDatum1.nLength != rootNodeDatum2.nLength)
            {
                m_dictCompareDiffs.Add(m_nodeCompare1, nodeCompare2);
            }

            foreach (KeyValuePair<TreeNode, TreeNode> pair in dictSort.Values.Reverse())
            {
                m_dictCompareDiffs.Add(pair.Key, pair.Value);
            }

            m_nodeCompare1.Name = strFullPath1;
            nodeCompare2.Name = strFullPath2;
            m_nodeCompare1.ToolTipText = strFullPath1A;
            nodeCompare2.ToolTipText = strFullPath2A;
            m_nodeCompare1.Tag = new RootNodeDatum((NodeDatum)m_nodeCompare1.Tag, rootNodeDatum1);
            nodeCompare2.Tag = new RootNodeDatum((NodeDatum)nodeCompare2.Tag, rootNodeDatum2);
            nodeCompare2.Checked = true;    // hack to put it in the right file pane
            form_treeCompare1.Nodes.Add(m_nodeCompare1);
            form_treeCompare2.Nodes.Add(nodeCompare2);
            m_nCompareIndex = 0;
            form_btnCompare.Select();
            form_btnCompare.Text = "> >";
            form_chkCompare1.Text = "1 of " + m_dictCompareDiffs.Count;
            form_btnCollapse.Text = "< <";
            form_colDirDetailCompare.Text = "Directory detail";
            form_lblVolGroup.Text = "Compare Mode";
            form_lblVolGroup.BackColor = Color.LightGoldenrodYellow;
            form_lblVolGroup.Font = new Font(m_FontVolGroupOrig, FontStyle.Regular);
            form_btnFolder.Enabled = false;
            form_btnFiles.Enabled = false;
            form_btnFoldersAndFiles.Enabled = false;
            m_FileListTabPageBeforeCompare = form_tabControlFileList.SelectedTab;
            m_bCompareMode = true;
            form_tabControlFileList.SelectedTab = form_tabPageFileList;
            m_bTreeViewIndirectSelChange = true;
            form_treeCompare1.SelectedNode = form_treeCompare1.Nodes[0];
            m_bTreeViewIndirectSelChange = true;
            form_treeCompare2.SelectedNode = form_treeCompare2.Nodes[0];
        }

        void form_btnCopyToClipBoard_Click(object sender = null, EventArgs e = null)
        {
            TreeView treeView = m_treeCopyToClipboard;

            if (m_bCompareMode)
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

            TreeNode treeNode = treeView.SelectedNode;

            if (treeNode != null)
            {
                m_blinky.SelectTreeNode(treeNode, Once: true);
                Clipboard.SetText(FullPath(treeNode));
            }
            else
            {
                m_blinky.Go(clr: Color.Red, Once: true);
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
            DoHistory(sender, +1);
        }

        void form_btnIgnoreAdd_Click(object sender, EventArgs e)
        {
            TreeNode treeNode = form_treeViewBrowse.SelectedNode;

            if (treeNode == null)
            {
                m_blinky.Go(form_btnIgnoreAdd, clr: Color.Red, Once: true);
            }
            else if (form_lvIgnoreList.Items.ContainsKey(treeNode.Text))
            {
                m_blinky.SelectLVitem(lvItem: form_lvIgnoreList.Items[treeNode.Text]);
            }
            else
            {
                ListViewItem lvItem = new ListViewItem(new string[] { treeNode.Text, (treeNode.Level + 1).ToString() });

                lvItem.Name = lvItem.Text;
                form_lvIgnoreList.Items.Add(lvItem);
                m_bKillTree &= timer_DoTree.Enabled;
                RestartTreeTimer();
            }
        }

        void form_btnIgnoreDel_Click(object sender, EventArgs e)
        {
            if (form_lvIgnoreList.SelectedItems.Count <= 0)
            {
                m_blinky.Go(form_btnIgnoreDel, clr: Color.Red, Once: true);
                return;
            }

            foreach (ListViewItem lvItem in form_lvIgnoreList.SelectedItems)
            {
                lvItem.Remove();
            }

            m_bKillTree &= timer_DoTree.Enabled;
            RestartTreeTimer();
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
                m_blinky.Go(ctl: form_btnSaveCopyDirs, clr: Color.Red, Once: true);
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
                m_blinky.Go(ctl: form_btnSaveIgnoreList, clr: Color.Red, Once: true);
            }
        }

        // form_btnFolder; form_btnFoldersAndFiles; form_btnFiles
        void form_btnFind_Click(object sender, EventArgs e = null)
        {
            m_strSelectFile = null;

            if (form_cbFindbox.Text.Length > 0)
            {
                if (m_ctlLastSearchSender != sender)
                {
                    m_ctlLastSearchSender = (Control)sender;
                    m_nSearchResultsIndexer = -1;
                }

                if ((m_nSearchResultsIndexer < 0) && new Button[] { form_btnFoldersAndFiles, form_btnFiles }.Contains(sender))
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
                m_blinky.Go(clr: Color.Red, Once: true);
            }
        }

        void form_btnTreeCollapse_Click(object sender, EventArgs e)
        {
            if (m_bCompareMode)
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
            TreeNode treeNode = form_treeViewBrowse.SelectedNode;

            if (m_bCompareMode)
            {
                treeNode = ((m_treeCopyToClipboard != null) ? m_treeCopyToClipboard : form_treeCompare1).SelectedNode;
            }

            if (treeNode != null)
            {
                m_bPutPathInFindEditBox = true;
                m_bTreeViewIndirectSelChange = true;

                if ((treeNode.Parent != null) && (treeNode.Parent.Parent == null))
                {
                    ((RootNodeDatum)treeNode.Parent.Tag).VolumeView = false;
                }
                else if (treeNode.Parent == null)
                {
                    RootNodeDatum rootNodeDatum = (RootNodeDatum)treeNode.Tag;

                    if (rootNodeDatum.VolumeView)
                    {
                        m_blinky.Go(form_btnUp, clr: Color.Red, Once: true);
                    }
                    else
                    {
                        rootNodeDatum.VolumeView = true;
                        treeNode.TreeView.SelectedNode = null;      // to kick in a change selection event
                        treeNode.TreeView.SelectedNode = treeNode;
                        return;
                    }
                }

                treeNode.TreeView.SelectedNode = treeNode.Parent;
            }
            else
            {
                m_blinky.Go(form_btnUp, clr: Color.Red, Once: true);
            }
        }

        void form_cbFindbox_DropDown(object sender, EventArgs e)
        {
            m_bNavDropDown = true;
        }

        void form_cbFindbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (form_cbFindbox.Text.Length <= 0)
            {
                return;
            }

            if (new Keys[] { Keys.Enter, Keys.Return }.Contains((Keys)e.KeyCode))
            {
                m_bPutPathInFindEditBox = false;    // search term is usually not the complete path.
                DoSearch(m_ctlLastSearchSender);
                e.Handled = true;
            }
        }

        void form_cbFindbox_MouseUp(object sender, MouseEventArgs e)
        {
            form_tmapUserCtl.ClearSelection();

            if (m_bNavDropDown)
            {
                m_bNavDropDown = false;
                return;
            }

            if (Cursor.Current != Cursors.Arrow)        // hack: clicked in tooltip
            {
                return;
            }

            m_bPutPathInFindEditBox = true;
            m_bTreeViewIndirectSelChange = true;
            m_strSelectFile = form_tmapUserCtl.Tooltip_Click();

            if (m_strSelectFile != null)
            {
                m_bTreeViewIndirectSelChange = false;   // didn't hit a sel change
                form_tabControlFileList.SelectedTab = form_tabPageFileList;
                SelectFoundFile();
            }
        }

        void form_cbFindbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cbFindbox, bTrimText: false);
        }

        void form_cbFindbox_TextChanged(object sender, EventArgs e)
        {
            if (m_SearchResultsType2_List.Count > 0)
            {
                m_SearchResultsType2_List = new List<SearchResults>();
                GC.Collect();
            }

            m_nSearchResultsIndexer = -1;
            m_bSearchResultsType2_List = false;
        }

        void form_chk_Compare1_CheckedChanged(object sender, EventArgs e)
        {
            if (m_bChkCompare1IndirectCheckChange)
            {
                m_bChkCompare1IndirectCheckChange = false;
                return;
            }

            if (m_bCompareMode)
            {
                Utilities.Assert(1308.93105, form_chkCompare1.Checked == false);
                form_chkCompare1.Text = m_strChkCompareOrig;
                form_btnCollapse.Text = m_strBtnTreeCollapseOrig;
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
                m_nodeCompare1 = null;
                m_listTreeNodes_Compare1.Clear();
                m_listTreeNodes_Compare2.Clear();
                m_dictCompareDiffs.Clear();
                form_treeCompare1.Nodes.Clear();
                form_treeCompare2.Nodes.Clear();
                m_listHistory.Clear();
                m_nIxHistory = -1;
                form_btnFolder.Enabled = true;
                form_btnFiles.Enabled = true;
                form_btnFoldersAndFiles.Enabled = true;
                form_tabControlFileList.SelectedTab = m_FileListTabPageBeforeCompare;

                m_bCompareMode = false;
                form_treeView_AfterSelect(form_treeViewBrowse, new TreeViewEventArgs(form_treeViewBrowse.SelectedNode));
            }
            else if (form_chkCompare1.Checked)
            {
                m_nodeCompare1 = form_treeViewBrowse.SelectedNode;

                if (m_nodeCompare1 != null)
                {
                    if (m_nSearchResultsIndexer >= 0)
                    {
                        m_blinky.SelectTreeNode(m_nodeCompare1, Once: false);
                    }
                    else
                    {
                        m_blinky.Go();
                    }
                }
                else
                {
                    m_blinky.Go(clr: Color.Red, Once: true);
                    m_bChkCompare1IndirectCheckChange = true;
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

            m_bKillTree &= timer_DoTree.Enabled;
            RestartTreeTimer();
        }

        void form_chkSpacer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), e.ClipRectangle);
        }

        void form_SetCopyToClipboardTree(object sender, EventArgs e = null)
        {
            if (sender is TreeView)
            {
                m_treeCopyToClipboard = (TreeView) sender;
            }
            else if (sender == form_lvFileCompare)
            {
                m_treeCopyToClipboard = form_treeCompare2;
            }
            else
            {
                m_treeCopyToClipboard = m_bCompareMode ? form_treeCompare1 : form_treeViewBrowse;
            }
        }

        void form_lvClones_Enter(object sender, EventArgs e)
        {
            //if (Form.ActiveForm == null)
            //{
            //    return;
            //}

            form_SetCopyToClipboardTree(form_treeViewBrowse);
            m_nLVclonesClickIx = -1;
        }

        void form_lvClones_KeyDown(object sender, KeyEventArgs e)
        {
            Utilities.Assert(1308.93107, QueryLVselChange(sender) == false, bTraceOnly: true);
        }

        void form_lvClones_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Left) && (m_nLVclonesClickIx > 0))
            {
                m_nLVclonesClickIx -= 2;
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
            m_bLVclonesMouseDown = true;
            Utilities.Assert(1308.93109, m_bLVclonesMouseSelChg == false, bTraceOnly: true);
            m_bLVclonesMouseSelChg = false;
        }

        void form_lvClones_MouseLeave(object sender, EventArgs e)
        {
            if (Form.ActiveForm == null)
            {
                return;
            }

            form_lvClones_MouseUp(sender);
            m_bLVclonesMouseSelChg = false;
        }

        void form_lvClones_MouseUp(object sender, MouseEventArgs e = null)
        {
            if (Form.ActiveForm == null)
            {
                return;
            }

            Utilities.Assert(1308.93111, QueryLVselChange(sender) == false, bTraceOnly: true);

            if (m_bLVclonesMouseDown == false)  // leave
            {
                return;
            }

            if (m_bLVclonesMouseSelChg)
            {
                LV_ClonesSelChange((ListView)sender);
                m_bLVclonesMouseSelChg = false;
            }
            else
            {
                LV_CloneSelNode((ListView)sender);
            }

            m_bLVclonesMouseDown = false;
        }

        void form_lvClones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                return;
            }

            if (m_bClonesLVindirectSelChange)
            {
                m_bClonesLVindirectSelChange = false;
                return;
            }

            ListView lv = (ListView)sender;

            if (lv.Focused == false)
            {
                return;
            }

            bool bUp = false;

            if (QueryLVselChange(sender, out bUp) == false)
            {
                return;
            }

            Utilities.WriteLine("form_lvClones_SelectedIndexChanged");

            if (m_bLVclonesMouseDown)
            {
                m_bLVclonesMouseSelChg = true;
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

            if (m_bCompareMode == false)
            {
                return;
            }

            form_cbFindbox.Text = FullPath(m_treeCopyToClipboard.SelectedNode);
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
            List<ListViewItem> listItems = new List<ListViewItem>();

            foreach (ListViewItem lvItem in lv.Items.Cast<ListViewItem>().Where(i => i.Tag != null).ToList())
            {
                listItems.Add(lvItem);
            }

            ListViewItem lvSelectedItem = null;

            if ((lv.SelectedItems != null) && (lv.SelectedItems.Count > 0))
            {
                lvSelectedItem = lv.SelectedItems[0];
            }

            bool bNullTags = (listItems.Count <= 0);

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
            Utilities.WriteLine("form_lvUnique_MouseClick");
            LV_MarkerClick(form_lvUnique);
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
            m_btmapUserCtl_MouseDown = true;
        }

        void form_tmapUserCtl_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_btmapUserCtl_MouseDown == false)
            {
                return;
            }

            m_btmapUserCtl_MouseDown = false;

            TreeNode treeNode = form_tmapUserCtl.DoToolTip(e.Location);

            if (treeNode == null)
            {
                return;
            }

            m_bPutPathInFindEditBox = true;
            m_bTreeViewIndirectSelChange = true;
            treeNode.TreeView.SelectedNode = treeNode;
        }

        void form_treeCompare_Enter(object sender, EventArgs e)
        {
            if (Form.ActiveForm == null)
            {
                return;
            }

            form_SetCopyToClipboardTree(sender);
            form_cbFindbox.Text = FullPath(((TreeView)sender).SelectedNode);
        }

        void form_treeViewBrowse_AfterCheck(object sender, TreeViewEventArgs e)
        {
            string strPath = FullPath(e.Node);

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
            if (Blinky.TreeSelect)
            {
                return;
            }

            if (m_bRestartTreeTimer)
            {
                return;
            }

            if (m_bHistoryDefer == false)
            {
                m_bHistoryDefer = true;

                if ((m_listHistory.Count > 0) && (m_nIxHistory > -1) && ((m_listHistory.Count - 1) > m_nIxHistory))
                {
                    m_listHistory.RemoveRange(m_nIxHistory, m_listHistory.Count - m_nIxHistory - 1);
                }

                Utilities.Assert(1308.9312, m_nIxHistory == (m_listHistory.Count - 1));

                if ((m_nIxHistory < 0) || (History_Equals(e.Node) == false))
                {
                    History_Add(e.Node);
                    ++m_nIxHistory;
                }
            }

            m_bHistoryDefer = false;

            if ((m_bTreeViewIndirectSelChange == false) && (e.Node.Parent == null))
            {
                ((RootNodeDatum)e.Node.Tag).VolumeView = true;
            }

            m_bTreeViewIndirectSelChange = false;
            form_tmapUserCtl.Render(e.Node);

            if (sender == form_treeCompare2)
            {
                Utilities.Assert(1308.9313, m_bCompareMode);
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

            TreeNode rootNode = e.Node.Root();

            Utilities.Assert(1308.9314, (new object[] { form_treeCompare1, form_treeCompare2 }.Contains(sender)) == m_bCompareMode);
            DoTreeSelect(e.Node);

            string strNode = e.Node.Text;

            Utilities.Assert(1308.9315, Utilities.StrValid(strNode));

            if (m_bCompareMode)
            {
                string strDirAndVolume = strNode;
                string strVolume = rootNode.ToolTipText;

                if (strVolume.Contains(Path.DirectorySeparatorChar))
                {
                    strVolume = strVolume.Substring(0, strVolume.IndexOf(Path.DirectorySeparatorChar));
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

                form_cbFindbox.Text = FullPath(e.Node);
                return;
            }

            string strVolumeGroup = ((RootNodeDatum)rootNode.Tag).StrVolumeGroup;

            form_lblVolGroup.Text = Utilities.StrValid(strVolumeGroup) ? strVolumeGroup : "(no volume group set)";
            form_colVolDetail.Text = rootNode.Text;
            form_colDirDetail.Text = form_colFilename.Text = strNode;

            if (m_bPutPathInFindEditBox)
            {
                m_bPutPathInFindEditBox = false;
                form_cbFindbox.Text = FullPath(e.Node);
            }

            NodeDatum nodeDatum = (NodeDatum)e.Node.Tag;

            if (nodeDatum.nImmediateFiles <= 0)
            {
                form_colFilename.Text = m_strColFilesOrig;
            }

            if (nodeDatum.m_lvItem == null)
            {
                return;
            }

            if (nodeDatum.m_lvItem.ListView == null)    // during Corellate()
            {
                Utilities.Assert(1308.9316, m_threadCollate != null);
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
                m_bClonesLVindirectSelChange = true;
                nodeDatum.m_lvItem.Selected = true;
                nodeDatum.m_lvItem.Focused = true;
                nodeDatum.m_lvItem.ListView.TopItem = nodeDatum.m_lvItem;
            }
        }

        void form_treeViewBrowse_MouseClick(object sender, MouseEventArgs e)
        {
            m_bPutPathInFindEditBox = true;
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_bCompareMode)
            {
                Utilities.Assert(1308.9317, form_chkCompare1.Checked == true);
                form_chkCompare1.Checked = false;
                e.Cancel = true;
            }
            else
            {
                m_bAppExit = true;
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
                e.Handled = true;
            }
            else if (new Keys[] { Keys.Left, Keys.Right }.Contains(e.KeyCode))
            {
                if (m_bCompareMode || (m_nSearchResultsIndexer >= 0))
                {
                    // L and R prevent the text cursor from walking in the find box, (and the tab-order of controls doesn't work.)
                    e.SuppressKeyPress = e.Handled = true;
                }
            }
            else if (new Keys[] { Keys.Enter, Keys.Return }.Contains(e.KeyCode))
            {
                if (m_nSearchResultsIndexer >= 0)
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
                if (m_bCompareMode)
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
                if (m_bCompareMode)
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
                else if (m_nSearchResultsIndexer >= 0)
                {
                    e.SuppressKeyPress = e.Handled = true;

                    if (e.KeyCode == Keys.Left)
                    {
                        if (m_nSearchResultsIndexer > 1)
                        {
                            m_nSearchResultsIndexer -= 2;
                        }
                        else if (m_nSearchResultsIndexer == 1)
                        {
                            m_nSearchResultsIndexer = 0;
                        }
                        else
                        {
                            return;                     // zeroth item, handled
                        }
                    }

                    // Keys.Left with above processing
                    // Keys.Right, Keys.Enter, Keys.Return
                    m_bPutPathInFindEditBox = false;    // search term is usually not the complete path.
                    DoSearch(m_ctlLastSearchSender);
                }

                // plenty of fall-through for form_cbFindbox etc.
            }
        }

        void Form1_Load(object sender, EventArgs e)
        {
            form_tmapUserCtl.TooltipAnchor = form_cbFindbox;

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

            if (Utilities.Assert(1308.93165, args != null) == false)
            {
                return;
            }

            string[] arrArgs = args.ActivationData;

            if (arrArgs == null)
            {
                // scenario: launched from Start menu
                return;
            }

            if (Utilities.Assert(1308.93165, arrArgs.Length > 0) == false)
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
                    RestartTreeTimer();
                    break;
                }

                case Utilities.mSTRfileExt_Volume:
                {
                    if (LoadVolumeList(strFile))
                    {
                        RestartTreeTimer();
                    }

                    break;
                }

                case Utilities.mSTRfileExt_Copy:
                {
                    form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    form_tabControlCopyIgnore.SelectedTab = form_tabPageCopy;
                    m_blinky.Go(form_lvCopyScratchpad, clr: Color.Yellow, Once: true);
                    Form1MessageBox("The Copy scratchpad cannot be loaded with no directory listings.", "Load Copy scratchpad externally");
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

        void timer_blinky_Tick(object sender, EventArgs e)
        {
            m_blinky.Tick();
        }

        void timer_DoTree_Tick(object sender, EventArgs e)
        {
            timer_DoTree.Stop();

            if (m_bCompareMode)
            {
                Utilities.Assert(1308.9317, form_chkCompare1.Checked);
                form_chkCompare1.Checked = false;
            }

            DoTree(bKill: m_bKillTree);
            m_bKillTree = true;
            m_bRestartTreeTimer = false;
        }
   }
}
