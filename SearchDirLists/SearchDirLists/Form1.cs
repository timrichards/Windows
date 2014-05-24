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
//      search results interrupt wonky
//      compare file list
//      why need Blinky.static_clrDefault?

namespace SearchDirLists
{
    delegate DialogResult MessageBoxDelegate(String strMessage, String strTitle = null, MessageBoxButtons? buttons = null);
    delegate bool BoolAction();

    partial class Form1 : Form
    {
        TreeNode m_nodeCompare1 = null;
        Dictionary<TreeNode, TreeNode> m_dictCompareDiffs = new Dictionary<TreeNode, TreeNode>();
        UList<TreeNode> m_listTreeNodes_Compare1 = new UList<TreeNode>();
        UList<TreeNode> m_listTreeNodes_Compare2 = new UList<TreeNode>();

        TreeNode[] m_arrSearchResults = null;
        int m_nSearchResultsIndexer = 0;
        bool m_bNavToFile = false;

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

            m_arrSearchResults = null;
            m_nSearchResultsIndexer = 0;
            m_bNavToFile = false;

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
            ClearMem_SaveDirListings();
            ClearMem_Search();
            ClearMem_TreeForm();
        }

        String m_strSelectFile = null;
        String m_strVolumeName = null;
        String m_strPath = null;
        String m_strSaveAs = null;

        int m_nCompareIndex = 0;
        int m_nLVclonesClickIx = -1;
        int[] m_arrSelChgIx = new int[2];
        int m_nSelChgIx = 0;
        bool m_bLVclonesMouseDown = false;
        bool m_bLVclonesMouseSelChg = false;

        bool m_bCompareMode = false;
        bool m_bPutPathInFindEditBox = false;
        bool m_bCheckboxes = false;

        Control m_ctlLastFocusForCopyButton = null;
        Control m_ctlLastSearchSender = null;

        bool m_bHistoryDefer = false;
        bool m_bTreeViewIndirectSelChange = false;
        bool m_bChkCompare1IndirectCheckChange = false;
        bool m_bClonesLVindirectSelChange = false;
        bool m_bNavDropDown = false;
        bool m_btmapUserCtl_MouseDown = false;

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
        readonly String m_strBtnTreeCollapseOrig = null;
        readonly String m_strColFilesOrig = null;
        readonly String m_strColFileCompareOrig = null;
        readonly String m_strColDirDetailCompareOrig = null;
        readonly String m_strColDirDetailOrig = null;
        readonly String m_strColVolDetailOrig = null;
        readonly String m_strBtnCompareOrig = null;
        readonly String m_strChkCompareOrig = null;
        readonly String m_strVolGroupOrig = null;

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

            // Assert String-lookup form items exist
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

        bool AddVolume()
        {
            if (SaveFields(false) == false)
            {
                return false;
            }

            if (Utilities.StrValid(m_strSaveAs) == false)
            {
                FormError(form_cbSaveAs, "Must have a file to load or save directory listing to.", "Volume Save As");
                return false;
            }

            if (form_lvVolumesMain.FindItemWithText(m_strSaveAs) != null)
            {
                FormError(form_cbSaveAs, "File already in use in list of volumes.", "Volume Save As");
                return false;
            }

            bool bOpenedFile = (Utilities.StrValid(m_strPath) == false);

            if (File.Exists(m_strSaveAs) && Utilities.StrValid(m_strPath))
            {
                m_blinky.Go(form_cbSaveAs, clr: Color.Red);

                if (Form1MessageBox(m_strSaveAs + " already exists. Overwrite?", "Volume Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    m_blinky.Go(form_cbVolumeName, clr: Color.Yellow, Once: true);
                    form_cbVolumeName.Text = String.Empty;
                    m_blinky.Go(form_cbPath, clr: Color.Yellow, Once: true);
                    form_cbPath.Text = String.Empty;
                    Utilities.Assert(1308.9304, SaveFields(false));
                }
            }

            if ((File.Exists(m_strSaveAs) == false) && (Utilities.StrValid(m_strPath) == false))
            {
                m_blinky.Go(form_cbPath, clr: Color.Red);
                Form1MessageBox("Must have a path or existing directory listing file.", "Volume Source Path");
                m_blinky.Go(form_cbPath, clr: Color.Red, Once: true);
                return false;
            }

            if (Utilities.StrValid(m_strPath) && (Directory.Exists(m_strPath) == false))
            {
                m_blinky.Go(form_cbPath, clr: Color.Red);
                Form1MessageBox("Path does not exist.", "Volume Source Path");
                m_blinky.Go(form_cbPath, clr: Color.Red, Once: true);
                return false;
            }

            String strStatus = "Not Saved";
            bool bFileOK = false;

            if (File.Exists(m_strSaveAs))
            {
                if (Utilities.StrValid(m_strPath) == false)
                {
                    bFileOK = ReadHeader();

                    if (bFileOK)
                    {
                        strStatus = Utilities.m_str_USING_FILE;
                    }
                    else
                    {
                        if (Utilities.StrValid(m_strPath))
                        {
                            strStatus = "File is bad. Will overwrite.";
                        }
                        else
                        {
                            m_blinky.Go(form_cbPath, clr: Color.Red);
                            Form1MessageBox("File is bad and path does not exist.", "Volume Source Path");
                            m_blinky.Go(form_cbPath, clr: Color.Red, Once: true);
                            return false;
                        }
                    }
                }
                else
                {
                    strStatus = "Will overwrite.";
                }
            }

            if ((bFileOK == false) && (form_lvVolumesMain.Items.ContainsKey(m_strPath)))
            {
                FormError(form_cbPath, "Path already added.", "Volume Source Path");
                return false;
            }

            if (Utilities.StrValid(m_strVolumeName))
            {
                ListViewItem lvItem = form_lvVolumesMain.FindItemWithText(m_strVolumeName);

                if ((lvItem != null) && (lvItem.Text == m_strVolumeName))
                {
                    m_blinky.Go(form_cbVolumeName, clr: Color.Red);

                    if (Form1MessageBox("Nickname already in use. Use it for more than one volume?", "Volume Save As", MessageBoxButtons.YesNo)
                        != DialogResult.Yes)
                    {
                        m_blinky.Go(form_cbVolumeName, clr: Color.Red, Once: true);
                        return false;
                    }
                }
            }
            else if (bOpenedFile == false)
            {
                m_blinky.Go(form_cbVolumeName, clr: Color.Red);

                if (Form1MessageBox("Continue without entering a nickname for this volume?", "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.Yes)
                {
                    m_blinky.Go(form_cbVolumeName, clr: Color.Red, Once: true);
                    return false;
                }
            }

            {
                ListViewItem lvItem = new ListViewItem(new String[] { m_strVolumeName, m_strPath, m_strSaveAs, strStatus, "Yes" });

                if (bFileOK == false)
                {
                    lvItem.Name = m_strPath;    // indexing by path, only for unsaved volumes
                }

                form_lvVolumesMain.Items.Add(lvItem);
            }

            form_btnSaveDirList.Enabled = true;
            return bFileOK;
        }

        void ComboBoxItemsInsert(ComboBox comboBox, String strText = null, bool bTrimText = true)
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
            if (m_dictCompareDiffs.Count == 0)
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

        void CopyToClipboard()
        {
            Color? clr = null;

            if (Utilities.StrValid(form_cbFindbox.Text))
            {
                Clipboard.SetText(form_cbFindbox.Text);
            }
            else
            {
                clr = Color.Red;
            }

            m_blinky.Go(clr: clr, Once: true);
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

        TreeNode FindNode(String strSearch, TreeNode startNode = null, TreeView treeView = null)
        {
            if (Utilities.StrValid(strSearch) == false)
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
                treeView = form_treeViewBrowse;
            }

            TreeNode treeNode = GetNodeByPath(strSearch, treeView);

            if (treeNode == null)
            {
                // case sensitive only when user enters an uppercase character

                List<TreeNode> listTreeNodes = m_listTreeNodes.ToList();

                if (m_bCompareMode)
                {
                    listTreeNodes = ((treeView == form_treeCompare2) ? m_listTreeNodes_Compare2 : m_listTreeNodes_Compare1).ToList();
                }

                if (strSearch.ToLower() == strSearch)
                {
                    m_arrSearchResults = listTreeNodes.FindAll(node => node.Text.ToLower().Contains(strSearch)).ToArray();
                }
                else
                {
                    m_arrSearchResults = listTreeNodes.FindAll(node => node.Text.Contains(strSearch)).ToArray();
                }
            }
            else
            {
                m_arrSearchResults = new TreeNode[] { treeNode };
            }

            if ((m_arrSearchResults != null) && (m_arrSearchResults.Length > 0))
            {
                if (m_arrSearchResults.Contains(startNode))
                {
                    m_nSearchResultsIndexer = m_arrSearchResults.Count(node => node != startNode);
                    m_bNavToFile = false;
                    return startNode;
                }
                else
                {
                    return m_arrSearchResults[0];
                }
            }
            else
            {
                return null;
            }
        }

        DialogResult Form1MessageBox(String strMessage, String strTitle = null, MessageBoxButtons? buttons = null)
        {
            if (AppExit)
            {
                return DialogResult.None;
            }

            if (InvokeRequired) { return (DialogResult)Invoke(new MessageBoxDelegate(Form1MessageBox), new object[] { strMessage, strTitle, buttons }); }

            m_blinky.Reset();

            // make MessageBox modal from a worker thread
            if (buttons == null)
            {
                return MessageBox.Show(strMessage.PadRight(100), strTitle);
            }
            else
            {
                return MessageBox.Show(strMessage.PadRight(100), strTitle, buttons.Value);
            }
        }

        bool FormatPath(Control ctl, ref String strPath, bool bFailOnDirectory = true)
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
                form_tabControlMain.SelectedTab = form_tabPageVolumes;
                FormError(ctl, "Path does not exist.", "Save Fields");
                return false;
            }

            strPath = strPath.TrimEnd(Path.DirectorySeparatorChar);
            return true;
        }

        void FormError(Control control, String strError, String strTitle)
        {
            m_blinky.Go(control, clr: Color.Red);
            Form1MessageBox(strError, strTitle);
            m_blinky.Go(control, clr: Color.Red, Once: true);
        }

        static String FullPath(TreeNode treeNode)
        {
            if (treeNode == null)
            {
                return null;
            }

            StringBuilder stbFullPath = null;
            TreeNode parentNode = treeNode.Parent;
            String P = Path.DirectorySeparatorChar.ToString();
            String PP = P + P;

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

        TreeNode GetNodeByPath(String path, TreeView treeView)
        {
            return GetNodeByPath_A(path, treeView) ?? GetNodeByPath_A(path, treeView, bIgnoreCase: true);
        }

        TreeNode GetNodeByPath_A(String strPath, TreeView treeView, bool bIgnoreCase = false)
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
            String P = Path.DirectorySeparatorChar.ToString();
            String PP = P + P;

            foreach (TreeNode topNode in treeView.Nodes)
            {
                String[] arrPath = null;
                int nPathLevelLength = 0;
                int nLevel = 0;
                String strNode = topNode.Name.TrimEnd(Path.DirectorySeparatorChar).Replace(PP, P);

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

        TreeNode GetSubNode(TreeNode node, String[] pathLevel, int i, int nPathLevelLength, bool bIgnoreCase)
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

        void LoadIgnoreList(String strFile = null)
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

        bool LoadVolumeList(String strFile = null)
        {
            if (new SDL_VolumeFile(strFile).ReadList(form_lvVolumesMain) == false)
            {
                return false;
            }

            if (form_lvVolumesMain.Items.Count > 0)
            {
                form_btnSaveDirList.Enabled = true;
                form_tabControlMain.SelectedTab = form_tabPageBrowse;
            }

            return true;    // this kicks off the tree
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
                    return false;   // LV with just a marker item? assert?
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

            if (lv.SelectedItems.Count == 0)
            {
                return false;
            }

            return (lv.SelectedItems[0].Tag != null);
        }

        bool LV_VolumesItemInclude(ListViewItem lvItem)
        {
            return (lvItem.SubItems[4].Text == "Yes");
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

            return QueryLVselChange(sender, ref bUp);
        }

        bool QueryLVselChange(object sender, ref bool bUp)
        {
            if ((sender is ListView) == false)
            {
                Utilities.Assert(1308.9319, false);
                return false;
            }

            ListView lv = (ListView)sender;

            if (lv.SelectedItems.Count == 0)
            {
                return false;
            }

            ++m_nSelChgIx;
            m_arrSelChgIx[m_nSelChgIx %= 2] = lv.SelectedItems[0].Index;

            int nNow = m_arrSelChgIx[m_nSelChgIx % 2];  // extra modulus just to be sure
            int nPrev = m_arrSelChgIx[(m_nSelChgIx + 1) % 2];

            bUp = nNow < nPrev;
            return nNow != nPrev;
        }

        bool ReadHeader()
        {
            if (Utilities.ValidateFile(m_strSaveAs) == false)
            {
                return false;
            }

            using (StreamReader file = new StreamReader(m_strSaveAs))
            {
                String line = null;

                if ((line = file.ReadLine()) == null) return false;
                if ((line = file.ReadLine()) == null) return false;
                if (line.StartsWith(Utilities.m_strLINETYPE_Nickname) == false) return false;

                String[] arrLine = line.Split('\t');
                String strName = String.Empty;

                if (arrLine.Length > 2) strName = arrLine[2];
                form_cbVolumeName.Text = strName;
                if ((line = file.ReadLine()) == null) return false;
                if (line.StartsWith(Utilities.m_strLINETYPE_Path) == false) return false;
                arrLine = line.Split('\t');
                if (arrLine.Length < 3) return false;
                form_cbPath.Text = arrLine[2];
            }

            return SaveFields(false);
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

                    if (listClones.Count == 0)
                    {
                        m_dictNodes.Remove(nodeDatum.Key);
                    }
                }
            }
            while (bContinue && ((treeNode = treeNode.NextNode) != null));
        }

        bool SaveFields(bool bFailOnDirectory = true)
        {
            m_strVolumeName = form_cbVolumeName.Text.Trim();
            m_strPath = form_cbPath.Text.Trim();

            if (Utilities.StrValid(m_strPath))
            {
                m_strPath += Path.DirectorySeparatorChar;

                if (FormatPath(form_cbPath, ref m_strPath, bFailOnDirectory) == false)
                {
                    return false;
                }
            }

            if (Utilities.StrValid(form_cbSaveAs.Text))
            {
                try
                {
                    form_cbSaveAs.Text = m_strSaveAs = Path.GetFullPath(form_cbSaveAs.Text.Trim());
                }
                catch
                {
                    FormError(form_cbSaveAs, "Error in save listings filename.", "Save Fields");
                    return false;
                }

                if (Directory.Exists(Path.GetDirectoryName(m_strSaveAs)) == false)
                {
                    FormError(form_cbSaveAs, "Directory to save listings to doesn't exist.", "Save Fields");
                    return false;
                }

                if (Directory.Exists(m_strSaveAs))
                {
                    FormError(form_cbSaveAs, "Must specify save filename. Only directory entered.", "Save Fields");
                    return false;
                }

                if (FormatPath(form_cbSaveAs, ref m_strSaveAs, bFailOnDirectory) == false)
                {
                    return false;
                }
            }

            return true;
        }

        void SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;

            if (m_bCompareMode)
            {
                return;
            }

            if (lv.SelectedItems.Count == 0)
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

            if (treeNode.IsVisible)
            {
                return;
            }

            treeNode.EnsureVisible();
        }

        void SetLV_VolumesItemInclude(ListViewItem lvItem, bool bInclude)
        {
            lvItem.SubItems[4].Text = (bInclude) ? "Yes" : "No";
        }

        void EnableButtonsWhenVolsSel()
        {
            bool bHasSelection = (form_lvVolumesMain.SelectedIndices.Count > 0);

            form_btnVolGroup.Enabled = bHasSelection;
            form_btnRemoveVolume.Enabled = bHasSelection;
            form_btnToggleInclude.Enabled = bHasSelection;
            form_btnModifyFile.Enabled = (form_lvVolumesMain.SelectedIndices.Count == 1);
        }

        void form_btnAddVolume_Click(object sender, EventArgs e)
        {
            InterruptTreeTimerWithAction(new BoolAction(AddVolume));
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
                CompareNav();
                return;
            }
            
            if (form_chkCompare1.Checked == false)
            {
                form_chkCompare1.Checked = true;
                return;
            }

            m_arrSearchResults = null;
            m_nSearchResultsIndexer = 0;
            m_bNavToFile = false;

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
            form_splitTreeFind.Panel1Collapsed = false;
            form_splitTreeFind.Panel2Collapsed = true;
            form_splitCompareFiles.Panel2Collapsed = false;
            form_splitClones.Panel2Collapsed = true;

            RootNodeDatum rootNodeDatum1 = (RootNodeDatum)m_nodeCompare1.Root().Tag;
            RootNodeDatum rootNodeDatum2 = (RootNodeDatum)nodeCompare2.Root().Tag;
            String strFullPath1 = FullPath(m_nodeCompare1);
            String strFullPath2 = FullPath(nodeCompare2);
            String strFullPath1A = m_nodeCompare1.FullPath;
            String strFullPath2A = nodeCompare2.FullPath;

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
            m_listHistory.Clear();
            m_nIxHistory = -1;
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

        void form_btnCopy_Click(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                if (new object[] { form_lvFiles, form_lvFileCompare }.Contains(m_ctlLastFocusForCopyButton))
                {
                    ListView lv = (ListView)m_ctlLastFocusForCopyButton;

                    if (lv.SelectedItems.Count > 0)
                    {
                        PutTreeCompareNodePathIntoFindCombo((lv == form_lvFiles) ? form_treeCompare1 : form_treeCompare2);

                        ListViewItem lvItem = lv.SelectedItems[0];

                        Clipboard.SetText(Path.Combine(FullPath(form_treeViewBrowse.SelectedNode), lvItem.Text));
                        m_blinky.SelectLVitem(lvItem: lvItem);
                    }
                    else
                    {
                        m_ctlLastFocusForCopyButton = (lv == form_lvFiles) ? form_treeCompare1 : form_treeCompare2;
                    }
                }

                if (new object[] { form_treeCompare1, form_treeCompare2 }.Contains(m_ctlLastFocusForCopyButton))
                {
                    PutTreeCompareNodePathIntoFindCombo((TreeView)m_ctlLastFocusForCopyButton);
                    CopyToClipboard();
                }
                else
                {
                    CopyToClipboard();
                    m_blinky.Go(clr: Color.Yellow);
                }
            }
            else
            {
                CopyToClipboard();
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
                    lvItem.Remove();    // sorted by size
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
                ListViewItem lvItem = new ListViewItem(new String[] { treeNode.Text, (treeNode.Level + 1).ToString() });

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

        void form_btnLoadVolumeList_Click(object sender, EventArgs e)
        {
            InterruptTreeTimerWithAction(new BoolAction(() => { return LoadVolumeList(); }));
        }

        void form_btnModifyFile_Click(object sender, EventArgs e)
        {
            InterruptTreeTimerWithAction(new BoolAction(() =>
            {
                ListView.SelectedListViewItemCollection lvSelect = form_lvVolumesMain.SelectedItems;

                if (lvSelect.Count <= 0)
                {
                    return false;
                }

                if (lvSelect.Count > 1)
                {
                    Utilities.Assert(1308.9308, false);    // guaranteed by selection logic
                    Form1MessageBox("Only one file can be modified at a time.", "Modify file");
                    return false;
                }

                String strVolumeName_orig = form_lvVolumesMain.SelectedItems[0].Text;
                String strVolumeName = null;
                String strFileName = form_lvVolumesMain.SelectedItems[0].SubItems[2].Text;

                try { using (new StreamReader(strFileName)) { } }
                catch
                {
                    if (m_saveDirListings != null)
                    {
                        Form1MessageBox("Currently saving listings and can't open file yet. Please wait.", "Modify file");
                    }
                    else if (Utilities.StrValid(lvSelect[0].Name))
                    {
                        Form1MessageBox("File hasn't been saved yet.", "Modify file");
                    }
                    else
                    {
                        Form1MessageBox("Can't open file.", "Modify file");
                    }

                    return false;
                }

                {
                    InputBox inputBox = new InputBox();

                    inputBox.Text = "Step 1 of 2: Volume name";
                    inputBox.Prompt = "Enter a volume name. (Next: drive letter)";
                    inputBox.Entry = strVolumeName_orig;
                    inputBox.SetNextButtons();

                    if ((inputBox.ShowDialog() == DialogResult.OK) && (Utilities.StrValid(inputBox.Entry)))
                    {
                        strVolumeName = inputBox.Entry;
                    }
                }

                String strDriveLetter_orig = null;
                String strDriveLetter = null;

                while (true)
                {
                    InputBox inputBox = new InputBox();

                    inputBox.Text = "Step 2 of 2: Drive letter";
                    inputBox.Prompt = "Enter a drive letter.";

                    String str = form_lvVolumesMain.SelectedItems[0].SubItems[1].Text;

                    if (str.Length <= 0)
                    {
                        Utilities.Assert(1308.9309, false);
                        break;
                    }

                    strDriveLetter_orig = str[0].ToString();
                    inputBox.Entry = strDriveLetter_orig.ToUpper();
                    inputBox.SetNextButtons();

                    if (inputBox.ShowDialog() == DialogResult.OK)
                    {
                        if (inputBox.Entry.Length > 1)
                        {
                            Form1MessageBox("Drive letter must be one letter.", "Drive letter");
                            continue;
                        }

                        strDriveLetter = inputBox.Entry.ToUpper();
                    }

                    break;
                }

                if (((Utilities.StrValid(strVolumeName) == false) ||
                    (Utilities.NotNull(strVolumeName) == Utilities.NotNull(strVolumeName_orig)))
                    &&
                    ((Utilities.StrValid(strDriveLetter) == false) ||
                    (Utilities.NotNull(strDriveLetter) == Utilities.NotNull(strDriveLetter_orig))))
                {
                    Form1MessageBox("No changes made.", "Modify file");
                    return false;
                }

                StringBuilder sbFileConts = new StringBuilder();
                bool bDriveLetter = Utilities.StrValid(strDriveLetter);

                KillTreeBuilder(bJoin: true);

                using (StringReader reader = new StringReader(File.ReadAllText(strFileName)))
                {
                    String strLine = null;
                    bool bHitNickname = (Utilities.StrValid(strVolumeName) == false);

                    while ((strLine = reader.ReadLine()) != null)
                    {
                        StringBuilder sbLine = new StringBuilder(strLine);

                        if ((bHitNickname == false) && strLine.StartsWith(Utilities.m_strLINETYPE_Nickname))
                        {
                            if (Utilities.StrValid(strVolumeName_orig))
                            {
                                sbLine.Replace(strVolumeName_orig, Utilities.NotNull(strVolumeName));
                            }
                            else
                            {
                                Utilities.Assert(1308.93101, sbLine.ToString().Split('\t').Length == 2);
                                sbLine.Append('\t');
                                sbLine.Append(strVolumeName);
                            }

                            form_lvVolumesMain.SelectedItems[0].Text = strVolumeName;
                            bHitNickname = true;
                        }
                        else if (bDriveLetter)
                        {
                            sbLine.Replace("\t" + strDriveLetter_orig + @":\", "\t" + strDriveLetter + @":\");
                        }

                        sbFileConts.AppendLine(sbLine.ToString());
                    }
                }

                if (bDriveLetter)
                {
                    ListViewItem lvItem = form_lvVolumesMain.SelectedItems[0];

                    lvItem.SubItems[1].Text = strDriveLetter + ":";
                }

                File.WriteAllText(strFileName, sbFileConts.ToString());
                m_blinky.Go(form_btnSaveVolumeList);

                if (Form1MessageBox("Update the volume list?", "Modify file", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    form_btnSaveVolumeList_Click();
                }

                return true;
            }));
        }

        void form_btnPath_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                ComboBoxItemsInsert(form_cbPath);
                m_strPath = form_cbPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        void form_btnRemoveVolume_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvSelect = form_lvVolumesMain.SelectedItems;

            if (lvSelect.Count <= 0)
            {
                return;
            }

            m_bKillTree = (m_tree != null) || (m_bKillTree && timer_DoTree.Enabled);
            timer_DoTree.Enabled = false;
            KillTreeBuilder(bJoin: true);

            if (m_bKillTree == false)
            {
                uint nNumFoldersKeep = 0;
                uint nNumFoldersRemove = 0;

                foreach (ListViewItem lvItem in form_lvVolumesMain.Items)
                {
                    if (lvItem.Tag == null)
                    {
                        // scenario: unsaved file
                        continue;
                    }

                    RootNodeDatum rootNodeDatum = (RootNodeDatum)((TreeNode)lvItem.Tag).Tag;

                    if (lvSelect.Contains(lvItem))
                    {
                        nNumFoldersRemove += rootNodeDatum.nSubDirs;
                    }
                    else
                    {
                        nNumFoldersKeep += rootNodeDatum.nSubDirs;
                    }
                }

                m_bKillTree = (nNumFoldersRemove > nNumFoldersKeep);
            }

            if (m_bKillTree)
            {
                RestartTreeTimer();
            }
            else
            {
                List<ListViewItem> listLVvolItems = new List<ListViewItem>();

                foreach (ListViewItem lvItem in lvSelect)
                {
                    if (lvItem.Tag == null)
                    {
                        // scenario: unsaved file
                        continue;
                    }

                    listLVvolItems.Add(lvItem);
                    form_treeViewBrowse.Nodes.Remove((TreeNode)lvItem.Tag);
                }

                new Thread(new ThreadStart(() =>
                {
                    foreach (ListViewItem lvItem in listLVvolItems)
                    {
                        TreeNode rootNode = (TreeNode)lvItem.Tag;

                        RemoveCorrelation(rootNode);
                        m_listRootNodes.Remove(rootNode);
                    }

                    Invoke(new Action(() =>
                    {
                        RestartTreeTimer();
                    }));
                }))
                .Start();
            }

            foreach (ListViewItem lvItem in lvSelect)
            {
                lvItem.Remove();
            }

            EnableButtonsWhenVolsSel();
            form_btnSaveDirList.Enabled = (form_lvVolumesMain.Items.Count > 0);
        }

        void form_btnSaveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = SDL_File.FileAndDirListFileFilter + "|" + SDL_File.BaseFilter;

            if (Utilities.StrValid(m_strSaveAs))
            {
                saveFileDialog1.InitialDirectory = Path.GetDirectoryName(m_strSaveAs);
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ComboBoxItemsInsert(form_cbSaveAs);
                m_strSaveAs = form_cbSaveAs.Text = saveFileDialog1.FileName;

                if (File.Exists(m_strSaveAs))
                {
                    form_cbVolumeName.Text = null;
                    form_cbPath.Text = null;
                }
            }
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

        void form_btnSaveDirLists_Click(object sender, EventArgs e)
        {
            bool bRestartTreeTimer = timer_DoTree.Enabled;

            timer_DoTree.Stop();

            if ((DoSaveDirListings() == false) && bRestartTreeTimer)   // cancelled
            {
                RestartTreeTimer();
            }
        }

        void form_btnSaveDirLists_EnabledChanged(object sender, EventArgs e)
        {
            form_btnSaveVolumeList.Enabled = form_btnSaveDirList.Enabled;
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

        void form_btnSaveVolumeList_Click(object sender = null, EventArgs e = null)
        {
            if (form_lvVolumesMain.Items.Count > 0)
            {
                new SDL_VolumeFile().WriteList(form_lvVolumesMain.Items);
            }
            else
            {
                m_blinky.Go(ctl: form_lvVolumesMain, clr: Color.Red, Once: true);
                Utilities.Assert(1308.93103, false);    // shouldn't even be hit: this button gets dimmed
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
                    m_nSearchResultsIndexer = 0;
                }

                if ((m_nSearchResultsIndexer == 0) && new Button[] { form_btnFoldersAndFiles, form_btnFiles }.Contains(sender))
                {
                    m_blinky.Go(bProgress: true);
                    SearchFiles(form_cbFindbox.Text, bSearchFilesOnly: (sender == form_btnFiles));
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

        void form_btnToggleInclude_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvSelect = form_lvVolumesMain.SelectedItems;

            if (lvSelect.Count > 0)
            {
                foreach (ListViewItem lvItem in lvSelect)
                {
                    SetLV_VolumesItemInclude(lvItem, LV_VolumesItemInclude(lvItem) == false);
                }

                RestartTreeTimer();
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
                treeNode = ((m_ctlLastFocusForCopyButton is TreeView) ? (TreeView)m_ctlLastFocusForCopyButton : form_treeCompare1).SelectedNode;
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

        void form_btnVolGroup_Click(object sender, EventArgs e)
        {
            m_bKillTree &= timer_DoTree.Enabled;

            InterruptTreeTimerWithAction(new BoolAction(() =>
            {
                ListView.SelectedListViewItemCollection lvSelect = form_lvVolumesMain.SelectedItems;

                if (lvSelect.Count <= 0)
                {
                    return false;
                }

                InputBox inputBox = new InputBox();

                inputBox.Text = "Volume Group";
                inputBox.Prompt = "Enter a volume group name";

                if (form_lvVolumesMain.SelectedItems[0].SubItems.Count > 5)
                {
                    inputBox.Entry = form_lvVolumesMain.SelectedItems[0].SubItems[5].Text;
                }

                SortedDictionary<String, object> dictVolGroups = new SortedDictionary<String, object>();

                foreach (ListViewItem lvItem in form_lvVolumesMain.Items)
                {
                    if (lvItem.SubItems.Count <= 5)
                    {
                        continue;
                    }

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
                    return false;
                }

                foreach (ListViewItem lvItem in lvSelect)
                {
                    while (lvItem.SubItems.Count <= 5)
                    {
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem());
                    }

                    lvItem.SubItems[5].Text = inputBox.Entry;

                    if (lvItem.Tag == null)
                    {
                        m_bKillTree = true;
                    }
                    else if (m_bKillTree == false)
                    {
                        ((RootNodeDatum)((TreeNode)lvItem.Tag).Tag).StrVolumeGroup = inputBox.Entry;
                    }
                }

                return true;
            }));
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
            if (m_listSearchResults.Count > 0)
            {
                m_listSearchResults = new List<SearchResults>();
                GC.Collect();
            }

            m_nSearchResultsIndexer = 0;
            m_bNavToFile = false;
        }

        void form_cb_Path_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cbPath, m_strPath);
            m_strPath = form_cbPath.Text;
        }

        void form_cbSaveAs_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Keys[] { Keys.Enter, Keys.Return }.Contains((Keys)e.KeyChar))
            {
                form_btnAddVolume_Click(sender, e);
                e.Handled = true;
            }
        }

        void form_cb_SaveAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cbSaveAs, m_strSaveAs);
            m_strSaveAs = form_cbSaveAs.Text;
        }

        void form_cb_VolumeName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cbVolumeName, m_strPath);
            m_strPath = form_cbVolumeName.Text;
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
                    if (m_nSearchResultsIndexer > 0)
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

        void form_ctlEnterForCopyButton(object sender, EventArgs e)
        {
            m_ctlLastFocusForCopyButton = (Control)sender;
        }

        void form_lvClones_Enter(object sender, EventArgs e)
        {
            //if (Form.ActiveForm == null)
            //{
            //    return;
            //}

            m_nLVclonesClickIx = -1;
        }

        void form_lvClones_KeyDown(object sender, KeyEventArgs e)
        {
            Utilities.Assert(1308.93107, QueryLVselChange(sender) == false, bOnlyDebug: true);
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
            Utilities.Assert(1308.93109, m_bLVclonesMouseSelChg == false, bOnlyDebug: true);
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

            Utilities.Assert(1308.93111, QueryLVselChange(sender) == false, bOnlyDebug: true);

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

            if (QueryLVselChange(sender, ref bUp) == false)
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

        void form_lvFiles_Enter(object sender, EventArgs e)
        {
            if (Form.ActiveForm == null)
            {
                return;
            }

            form_ctlEnterForCopyButton(sender, e);

            if (m_bCompareMode == false)
            {
                return;
            }

            form_cbFindbox.Text = FullPath(((sender == form_lvFiles) ? form_treeCompare1 : form_treeCompare2).SelectedNode);
        }

        void form_lvTreeNodes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView lv = (ListView)sender;

            if (lv.Tag == null)
            {
                lv.Tag = SortOrder.None;
            }

            if (lv.Items.Count == 0)
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

        void form_lvVolumesMain_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            EnableButtonsWhenVolsSel();
        }

        void form_lvVolumesMain_KeyUp(object sender, KeyEventArgs e)
        {
            if (new Keys[] { Keys.Back, Keys.Delete }.Contains(e.KeyCode))
            {
                form_btnRemoveVolume.PerformClick();
                e.Handled = true;
            }
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

            form_ctlEnterForCopyButton(sender, e);
            form_cbFindbox.Text = FullPath(((TreeView)sender).SelectedNode);
        }

        void form_treeViewBrowse_AfterCheck(object sender, TreeViewEventArgs e)
        {
            String strPath = FullPath(e.Node);

            if (e.Node.Checked)
            {
                ListViewItem lvItem = new ListViewItem(new String[] { e.Node.Text, strPath });

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

            String strNode = e.Node.Text;

            Utilities.Assert(1308.9315, Utilities.StrValid(strNode));

            if (m_bCompareMode)
            {
                String strDirAndVolume = strNode;
                String strVolume = rootNode.ToolTipText;

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

            String strVolumeGroup = ((RootNodeDatum)rootNode.Tag).StrVolumeGroup;

            form_lblVolGroup.Text = Utilities.StrValid(strVolumeGroup) ? strVolumeGroup : "(no volume group set)";
            form_colVolDetail.Text = rootNode.Text;
            form_colDirDetail.Text = form_colFilename.Text = strNode;

            if (m_bPutPathInFindEditBox)
            {
                m_bPutPathInFindEditBox = false;
                form_cbFindbox.Text = FullPath(e.Node);
            }

            NodeDatum nodeDatum = (NodeDatum)e.Node.Tag;

            if (nodeDatum.nImmediateFiles == 0)
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

        void Form1_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            new AboutBox1().ShowDialog_Once(this);
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_bAppExit = true;
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
                if (m_bCompareMode || (m_nSearchResultsIndexer > 0))
                {
                    // L and R prevent the text cursor from walking in the find box, (and the tab-order of controls doesn't work.)
                    e.SuppressKeyPress = e.Handled = true;
                }
            }
            else if (new Keys[] { Keys.Enter, Keys.Return }.Contains(e.KeyCode))
            {
                if (m_nSearchResultsIndexer > 0)
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
                CopyToClipboard();
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
                else if (m_nSearchResultsIndexer > 0)
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

            String[] arrArgs = args.ActivationData;

            if (arrArgs == null)
            {
                // scenario: launched from Start menu
                return;
            }

            if (Utilities.Assert(1308.93165, arrArgs.Length > 0) == false)
            {
                return;
            }

            String strFile = arrArgs[0];

            switch (Path.GetExtension(strFile).Substring(1))
            {
                case Utilities.m_strFILEEXT_Listing:
                {
                    form_cbSaveAs.Text = strFile;
                    AddVolume();
                    form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    RestartTreeTimer();
                    break;
                }

                case Utilities.m_strFILEEXT_Volume:
                {
                    if (LoadVolumeList(strFile))
                    {
                        RestartTreeTimer();
                    }

                    break;
                }

                case Utilities.m_strFILEEXT_Copy:
                {
                    form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    form_tabControlCopyIgnore.SelectedTab = form_tabPageCopy;
                    m_blinky.Go(form_lvCopyScratchpad, clr: Color.Yellow, Once: true);
                    Form1MessageBox("The Copy scratchpad cannot be loaded with no directory listings.", "Load Copy scratchpad externally");
                    Application.Exit();
                    break;
                }

                case Utilities.m_strFILEEXT_Ignore:
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
