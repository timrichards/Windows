using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Hosting;

namespace SearchDirLists
{
    delegate void MessageBoxDelegate(String strMessage, String strTitle = null);
    delegate bool BoolAction();

    partial class Form1 : Form
    {
        TreeNode m_nodeCompare1 = null;
        TreeNode m_nodeCompare2 = null;
        TreeNode[] m_arrayTreeFound = null;
        Dictionary<TreeNode, TreeNode> dictCompareDiffs = new Dictionary<TreeNode, TreeNode>();
        UList<TreeNode> m_listTreeNodes_Compare1 = new UList<TreeNode>();
        UList<TreeNode> m_listTreeNodes_Compare2 = new UList<TreeNode>();
        List<TreeNode> m_listHistory = new List<TreeNode>();
        int m_nIxHistory = -1;

        // Memory allocations occur just below all partial class Form1 : Form declarations, then ClearMem_...() for each.
        // Declarations continue below these two ClearMem() methods.

        void ClearMem_Form1()
        {
            Utilities.Assert(1308.9318, form_lvClones.Items.Count == 0);
            Utilities.Assert(1308.9301, form_lvSameVol.Items.Count == 0);
            Utilities.Assert(1308.9302, form_lvUnique.Items.Count == 0);

            form_lvClones.Items.Clear();
            form_lvSameVol.Items.Clear();
            form_lvUnique.Items.Clear();

            m_nodeCompare1 = null;
            m_nodeCompare2 = null;
            m_arrayTreeFound = null;
            dictCompareDiffs.Clear();
            m_listTreeNodes_Compare1.Clear();
            m_listTreeNodes_Compare2.Clear();
            m_listHistory.Clear();
            m_nIxHistory = -1;
            form_lvFiles.Items.Clear();
            form_lvFileCompare.Items.Clear();
            form_lvDetail.Items.Clear();
            form_lvDetailVol.Items.Clear();
            form_tmapUserCtl.Clear();
            form_treeView_Browse.Nodes.Clear();
        }

        void ClearMem()
        {
            Collate.ClearMem();
            ClearMem_Form1();
            ClearMem_SaveDirListings();
            ClearMem_Search();
            ClearMem_TreeForm();
        }

        String m_strCompare1 = null;
        String m_strMaybeFile = null;
        String m_strVolumeName = null;
        String m_strPath = null;
        String m_strSaveAs = null;

        int m_nCompareIndex = -1;
        int m_nLVclonesClickIx = -1;
        int m_nLVsameVolClickIx = -1;
        int m_nTreeFindTextChanged = 0;

        bool m_bCompareMode = false;
        bool m_bFileFound = false;
        bool m_bPutPathInFindEditBox = false;
        bool m_bCheckboxes = false;

        Control m_ctlLastFocusForCopyButton = null;
        Control m_ctlLastSearchSender = null;

        bool m_bHistoryDefer = false;
        bool m_bTreeViewIndirectSelChange = false;
        bool m_bChkCompare1IndirectCheckChange = false;
        bool m_bNavDropDown = false;
        bool m_btmapUserCtl_MouseDown = false;
        TabPage m_FileListTabPageBeforeCompare = null;
        bool m_bKillTree = true;
        bool m_bRestartTreeTimer = false;

        static bool m_bAppExit = false;
        public static bool AppExit { get { return m_bAppExit; } }
        public static Form static_form = null;

        // initialized in constructor:
        Blink m_blink = null;
        Color m_clrVolGroupOrig = Color.Empty;
        Font m_FontVolGroupOrig = null;
        String m_strBtnTreeCollapseOrig = null;
        String m_strColFilesOrig = null;
        String m_strColFileCompareOrig = null;
        String m_strColDirDetailCompareOrig = null;
        String m_strColDirDetailOrig = null;
        String m_strColVolDetailOrig = null;
        String m_strBtnCompareOrig = null;
        String m_strChkCompareOrig = null;
        String m_strVolGroupOrig = null;

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

            m_blink = new Blink(timer_blink, form_cbNavigate);
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
            m_bCheckboxes = form_treeView_Browse.CheckBoxes;
            Utilities.SetMessageBoxDelegate(MessageboxCallback);
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
                m_blink.Go(form_cbSaveAs, clr: Color.Red);

                if (MessageBox.Show((m_strSaveAs + " already exists. Overwrite?").PadRight(100), "Volume Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    m_blink.Go(form_cbVolumeName, clr: Color.Yellow, Once: true);
                    form_cbVolumeName.Text = String.Empty;
                    m_blink.Go(form_cbPath, clr: Color.Yellow, Once: true);
                    form_cbPath.Text = String.Empty;
                    Utilities.Assert(1308.9304, SaveFields(false));
                }
            }

            if ((File.Exists(m_strSaveAs) == false) && form_lvVolumesMain.Items.ContainsKey(m_strPath))
            {
                FormError(form_cbPath, "Path already added.", "Volume Source Path");
                return false;
            }

            if (Utilities.StrValid(m_strVolumeName))
            {
                ListViewItem lvItem = form_lvVolumesMain.FindItemWithText(m_strVolumeName);

                if ((lvItem != null) && (lvItem.Text == m_strVolumeName))
                {
                    m_blink.Go(form_cbVolumeName, clr: Color.Red);

                    if (MessageBox.Show("Nickname already in use. Use it for more than one volume?".PadRight(100), "Volume Save As", MessageBoxButtons.YesNo)
                        != DialogResult.Yes)
                    {
                        m_blink.Go(form_cbVolumeName, clr: Color.Red, Once: true);
                        return false;
                    }
                }
            }

            if ((File.Exists(m_strSaveAs) == false) && (Utilities.StrValid(m_strPath) == false))
            {
                m_blink.Go(form_cbPath, clr: Color.Red);
                MessageBox.Show("Must have a path or existing directory listing file.".PadRight(100), "Volume Source Path");
                m_blink.Go(form_cbPath, clr: Color.Red, Once: true);
                return false;
            }

            if (Utilities.StrValid(m_strPath) && (Directory.Exists(m_strPath) == false))
            {
                m_blink.Go(form_cbPath, clr: Color.Red);
                MessageBox.Show("Path does not exist.".PadRight(100), "Volume Source Path");
                m_blink.Go(form_cbPath, clr: Color.Red, Once: true);
                return false;
            }

            String strStatus = "Not Saved";

            if (File.Exists(m_strSaveAs))
            {
                if (Utilities.StrValid(m_strPath) == false)
                {
                    bool bFileOK = ReadHeader();

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
                            m_blink.Go(form_cbPath, clr: Color.Red);
                            MessageBox.Show("File is bad and path does not exist.".PadRight(100), "Volume Source Path");
                            m_blink.Go(form_cbPath, clr: Color.Red, Once: true);
                            return false;
                        }
                    }
                }
                else
                {
                    strStatus = "Will overwrite.";
                }
            }

            if ((bOpenedFile == false) && (Utilities.StrValid(m_strVolumeName) == false))
            {
                m_blink.Go(form_cbVolumeName, clr: Color.Red);

                if (MessageBox.Show("Continue without entering a nickname for this volume?".PadRight(100), "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.Yes)
                {
                    m_blink.Go(form_cbVolumeName, clr: Color.Red, Once: true);
                    return false;
                }
            }

            {
                ListViewItem lvItem = new ListViewItem(new String[] { m_strVolumeName, m_strPath, m_strSaveAs, strStatus, "Yes" });

                lvItem.Name = m_strPath;
                form_lvVolumesMain.Items.Add(lvItem);
            }

            form_btnSaveDirList.Enabled = true;
            return true;
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

        void CompareModeButtonKeyPress(object sender, KeyPressEventArgs e)
        {
            if (m_bCompareMode == false)
            {
                return;
            }

            if (e.KeyChar == '.')
            {
                form_btnCompareNext_Click();
                e.Handled = true;
            }
            else if (e.KeyChar == ',')
            {
                form_btnComparePrev_Click();
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

        void CompareNav()
        {
            Utilities.WriteLine(dictCompareDiffs.ToArray()[m_nCompareIndex].ToString());
            form_chkCompare1.Text = m_nCompareIndex + 1 + " of " + dictCompareDiffs.Count;
            form_lvFiles.Items.Clear();
            form_lvFileCompare.Items.Clear();
            form_lvDetail.Items.Clear();
            form_lvDetailVol.Items.Clear();
            form_treeCompare1.SelectedNode = null;
            form_treeCompare2.SelectedNode = null;

            TreeNode treeNode = dictCompareDiffs.ToArray()[m_nCompareIndex].Key;

            if (Utilities.StrValid(treeNode.Name) == false)  // can't have a null key in the dictionary so there's a new TreeNode there
            {
                treeNode = null;
            }

            m_bTreeViewIndirectSelChange = true;
            form_treeCompare1.TopNode = form_treeCompare1.SelectedNode = treeNode;
            m_bTreeViewIndirectSelChange = true;
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

        void CopyToClipboard()
        {
            m_blink.Stop();

            if (Utilities.StrValid(form_cbNavigate.Text) == false)
            {
                m_blink.Go(clr: Color.Red, Once: true);
                return;
            }

            Clipboard.SetText(form_cbNavigate.Text);
            m_blink.Go(Once: true);
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
                m_blink.Go((Control)sender, clr: Color.Red, Once: true);
            }
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
            m_blink.Stop();

            if (lv.SelectedItems.Count <= 0)
            {
                m_blink.Go(clr: Color.Red, Once: true);
                return;
            }

            if (m_bCompareMode)
            {
                PutTreeCompareNodePathIntoFindCombo((lv == form_lvFiles) ? form_treeCompare1 : form_treeCompare2);
            }

            Clipboard.SetText(Path.Combine(FullPath(form_treeView_Browse.SelectedNode), lv.SelectedItems[0].Text));
            m_blink.Go(lvItem: lv.SelectedItems[0], Once: true);
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
                treeView = form_treeView_Browse;
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
            m_blink.Go(control, clr: Color.Red);
            MessageBox.Show(strError.PadRight(100), strTitle);
            m_blink.Go(control, clr: Color.Red, Once: true);
        }

        static String FullPath(TreeNode treeNode)
        {
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
                TreeNode treeNode = GetNodeByPath(lvItem.SubItems[1].Text, form_treeView_Browse);

                if (treeNode != null)
                {
                    treeNode.Checked = true;
                    ++nLoaded;
                }

                ++nTotal;
            }

            if (nLoaded != nTotal)
            {
                MessageBox.Show((nLoaded + " of " + nTotal + " scratchpad folders found in the tree.").PadRight(100), "Load copy scratchpad");
                form_tabControlCopyIgnore.SelectedTab = form_tabPageCopy;
                m_blink.Go(form_lvCopyList, clr: Color.Yellow, Once: true);
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

            foreach (ListViewItem lvItem in form_lvVolumesMain.Items)
            {
                lvItem.Name = lvItem.Text;
            }

            if (form_lvVolumesMain.Items.Count > 0)
            {
                form_btnSaveDirList.Enabled = true;
                form_tabControlMain.SelectedTab = form_tabPageBrowse;
            }

            return true;    // this kicks off the tree
        }

        bool LV_VolumesItemInclude(ListViewItem lvItem)
        {
            return (lvItem.SubItems[4].Text == "Yes");
        }

        void lvClonesClick(ListView lv, ref int nClickIndex)
        {
            if (LVMarkerClick(lv) == false)
            {
                return;
            }

            UList<TreeNode> listTreeNodes = (UList<TreeNode>)lv.SelectedItems[0].Tag;

            m_bPutPathInFindEditBox = true;
            m_bTreeViewIndirectSelChange = true;
            form_treeView_Browse.SelectedNode = listTreeNodes[++nClickIndex % listTreeNodes.Count];
            form_treeView_Browse.Select();
        }

        bool LVMarkerClick(ListView lv)
        {
            if (lv.SelectedItems.Count <= 0)
            {
                return false;
            }

            if (lv.SelectedItems[0].Tag == null)
            {
                // marker item
                int nIx = lv.SelectedItems[0].Index + 1;

                if (nIx >= lv.Items.Count)
                {
                    nIx -= 2;

                    if (nIx < 0)
                    {
                        return false;
                    }
                }

                ListViewItem lvItem = lv.Items[nIx];

                lvItem.Selected = true;
                lvItem.Focused = true;
            }

            return true;
        }

        void MessageboxCallback(String strMessage, String strTitle)
        {
            if (AppExit)
            {
                return;
            }

            if (InvokeRequired) { Invoke(new MessageBoxDelegate(MessageboxCallback), new object[] { strMessage, strTitle }); return; }

            // make MessageBox modal from a worker thread
            MessageBox.Show(strMessage.PadRight(100), strTitle);
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

            form_cbNavigate.Text = FullPath(treeView.SelectedNode);
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

            TreeNode treeNode = form_treeView_Browse.SelectedNode = (TreeNode)lv.SelectedItems[0].Tag;

            if (treeNode == null)
            {
                return;
            }

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

        void SetLV_VolumesItemInclude(ListViewItem lvItem, bool bInclude)
        {
            lvItem.SubItems[4].Text = (bInclude) ? "Yes" : "No";
        }

        void UpdateLV_VolumesSelection()
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
                form_btnCompareNext_Click();
            }
            else if (form_chkCompare1.Checked == false)
            {
                form_chkCompare1.Checked = true;
            }
            else
            {
                form_cbNavigate.BackColor = Color.Empty;
                Utilities.Assert(1308.9306, form_chkCompare1.Checked);
                Utilities.Assert(1308.9307, Utilities.StrValid(m_strCompare1));

                String strCompare2 = form_cbNavigate.Text;
                bool bError = (Utilities.StrValid(strCompare2) == false);

                if (bError == false)
                {
                    m_nodeCompare2 = FindNode(strCompare2, form_treeView_Browse.SelectedNode);
                    bError = ((m_nodeCompare2 == null) || (m_nodeCompare2 == m_nodeCompare1));
                }

                if (bError)
                {
                    m_blink.Go(clr: Color.Red, Once: true);
                }
                else
                {
                    m_blink.Go();
                    form_splitTreeFind.Panel1Collapsed = false;
                    form_splitTreeFind.Panel2Collapsed = true;
                    form_splitCompareFiles.Panel2Collapsed = false;
                    form_splitClones.Panel2Collapsed = true;
                    m_bTreeViewIndirectSelChange = true;
                    form_treeView_Browse.SelectedNode = m_nodeCompare2;

                    RootNodeDatum rootNodeDatum1 = (RootNodeDatum)m_nodeCompare1.Root().Tag;
                    RootNodeDatum rootNodeDatum2 = (RootNodeDatum)m_nodeCompare2.Root().Tag;
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
                    m_nodeCompare1.Tag = new RootNodeDatum((NodeDatum)m_nodeCompare1.Tag, rootNodeDatum1);
                    m_nodeCompare2.Tag = new RootNodeDatum((NodeDatum)m_nodeCompare2.Tag, rootNodeDatum2);
                    m_nodeCompare2.Checked = true;    // hack to put it in the right file pane
                    dictCompareDiffs.Clear();
                    Compare(m_nodeCompare1, m_nodeCompare2);
                    Compare(m_nodeCompare2, m_nodeCompare1, bReverse: true);

                    if (dictCompareDiffs.Count < 15)
                    {
                        dictCompareDiffs.Clear();
                        Compare(m_nodeCompare1, m_nodeCompare2, nMin10M: 0);
                        Compare(m_nodeCompare2, m_nodeCompare1, bReverse: true, nMin10M: 0);
                    }

                    if (dictCompareDiffs.Count < 15)
                    {
                        dictCompareDiffs.Clear();
                        Compare(m_nodeCompare1, m_nodeCompare2, nMin10M: 0, nMin100K: 0);
                        Compare(m_nodeCompare2, m_nodeCompare1, bReverse: true, nMin10M: 0, nMin100K: 0);
                    }

                    SortedDictionary<ulong, KeyValuePair<TreeNode, TreeNode>> dictSort = new SortedDictionary<ulong, KeyValuePair<TreeNode, TreeNode>>();

                    foreach (KeyValuePair<TreeNode, TreeNode> pair in dictCompareDiffs)
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
                    m_listHistory.Clear();
                    m_nIxHistory = -1;
                    form_btnNavigate.Enabled = false;
                    form_btnSearchFiles.Enabled = false;
                    form_btnSearchFoldersAndFiles.Enabled = false;
                    m_FileListTabPageBeforeCompare = tabControl_FileList.SelectedTab;
                    m_bCompareMode = true;
                    tabControl_FileList.SelectedTab = tabPage_FileList;
                    m_bTreeViewIndirectSelChange = true;
                    form_treeCompare1.SelectedNode = form_treeCompare1.Nodes[0];
                    m_bTreeViewIndirectSelChange = true;
                    form_treeCompare2.SelectedNode = form_treeCompare2.Nodes[0];
                }
            }
        }

        void form_btnCompareNext_Click(object sender = null, EventArgs e = null)
        {
            if (dictCompareDiffs.Count == 0)
            {
                return;
            }

            m_nCompareIndex = Math.Min(dictCompareDiffs.Count - 1, ++m_nCompareIndex);
            CompareNav();
        }

        void form_btnComparePrev_Click(object sender = null, EventArgs e = null)
        {
            if (dictCompareDiffs.Count == 0)
            {
                return;
            }

            m_nCompareIndex = Math.Max(0, --m_nCompareIndex);
            CompareNav();
        }

        void form_btnCopy_Click(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                if (new object[] { form_treeCompare1, form_treeCompare2 }.Contains(m_ctlLastFocusForCopyButton))
                {
                    form_tree_compare_KeyPress(m_ctlLastFocusForCopyButton, new KeyPressEventArgs((char)3));
                }
                else if (new object[] { form_lvFiles, form_lvFileCompare }.Contains(m_ctlLastFocusForCopyButton))
                {
                    FileListKeyPress((ListView)m_ctlLastFocusForCopyButton, new KeyPressEventArgs((char)3));
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

        void form_btnCopyClear_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvItem in form_lvCopyList.Items)
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
            TreeNode treeNode = form_treeView_Browse.SelectedNode;

            if (treeNode == null)
            {
                m_blink.Go(form_btnIgnoreAdd, clr: Color.Red, Once: true);
                return;
            }

            if (form_lvIgnoreList.Items.ContainsKey(treeNode.Text))
            {
                ListViewItem lvItem = form_lvIgnoreList.Items[treeNode.Text];

                lvItem.EnsureVisible();
                m_blink.Go(lvItem: lvItem, Once: true);
                return;
            }

            {
                ListViewItem lvItem = new ListViewItem(new String[] { treeNode.Text, (treeNode.Level + 1).ToString() });

                lvItem.Name = lvItem.Text;
                form_lvIgnoreList.Items.Add(lvItem);
            }

            m_bKillTree &= timer_DoTree.Enabled;
            RestartTreeTimer();
        }

        void form_btnIgnoreDel_Click(object sender, EventArgs e)
        {
            if (form_lvIgnoreList.SelectedItems.Count <= 0)
            {
                m_blink.Go(form_btnIgnoreDel, clr: Color.Red, Once: true);
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

            foreach (ColumnHeader col in form_lvCopyList.Columns)
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
                    MessageBox.Show("Only one file can be modified at a time.".PadRight(100), "Modify file");
                    return false;
                }

                String strVolumeName_orig = form_lvVolumesMain.SelectedItems[0].Text;
                String strVolumeName = null;
                String strFileName = form_lvVolumesMain.SelectedItems[0].SubItems[2].Text;

                if (m_saveDirListings != null)
                {
                    try { using (new StreamReader(strFileName)) { } }
                    catch
                    {
                        MessageBox.Show("Currently saving listings and can't open file yet. Please wait.".PadRight(100), "Modify file");
                        return false;
                    }
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
                            MessageBox.Show("Drive letter must be one letter.".PadRight(100), "Drive letter");
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
                    MessageBox.Show("No changes made.".PadRight(100), "Modify file");
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

                    lvItem.Name = lvItem.SubItems[1].Text = strDriveLetter + ":";
                }

                File.WriteAllText(strFileName, sbFileConts.ToString());
                m_blink.Go(form_btnSaveVolumeList);

                if (MessageBox.Show("Update the volume list?".PadRight(100), "Modify file", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    form_btnSaveVolumeList_Click();
                }

                return true;
            }));
        }

        void form_btnNavigate_Click(object sender, EventArgs e = null)
        {
            DoSearch(sender);
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

            foreach (ListViewItem lvItem in lvSelect)
            {
                lvItem.Remove();
            }

            UpdateLV_VolumesSelection();
            form_btnSaveDirList.Enabled = (form_lvVolumesMain.Items.Count > 0);
            RestartTreeTimer();
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
            if (form_lvCopyList.Items.Count == 0)
            {
                m_blink.Go(ctl: form_btnSaveCopyDirs, clr: Color.Red, Once: true);
                return;
            }

            new SDL_CopyFile().WriteList(form_lvCopyList.Items);
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
            if (form_lvIgnoreList.Items.Count == 0)
            {
                m_blink.Go(ctl: form_btnSaveIgnoreList, clr: Color.Red, Once: true);
                return;
            }

            new SDL_IgnoreFile().WriteList(form_lvIgnoreList.Items);
        }

        void form_btnSaveVolumeList_Click(object sender = null, EventArgs e = null)
        {
            if (form_lvVolumesMain.Items.Count == 0)
            {
                m_blink.Go(ctl: form_lvVolumesMain, clr: Color.Red, Once: true);
                Utilities.Assert(1308.93103, false);    // shouldn't even be hit: this button gets dimmed
                return;
            }

            new SDL_VolumeFile().WriteList(form_lvVolumesMain.Items);
        }

        void form_btnSearchFoldersAndFiles_Click(object sender, EventArgs e)
        {
            if (form_cbNavigate.Text.Length == 0)
            {
                m_blink.Go(clr: Color.Red, Once: true);
                return;
            }

            if (m_ctlLastSearchSender != sender)
            {
                m_ctlLastSearchSender = (Control)sender;
                m_nTreeFindTextChanged = 0;
            }

            if (m_nTreeFindTextChanged == 0)
            {
                m_blink.Go(bProgress: true);
                SearchFiles(form_cbNavigate.Text, bSearchFilesOnly: (sender == form_btnSearchFiles));
            }
            else
            {
                DoSearch(sender);
            }
        }

        void form_btnToggleInclude_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvSelect = form_lvVolumesMain.SelectedItems;

            if (lvSelect.Count <= 0)
            {
                return;
            }

            foreach (ListViewItem lvItem in lvSelect)
            {
                SetLV_VolumesItemInclude(lvItem, LV_VolumesItemInclude(lvItem) == false);
            }

            RestartTreeTimer();
        }

        void form_btnTreeCollapse_Click(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                form_btnComparePrev_Click();
            }
            else
            {
                form_treeView_Browse.CollapseAll();
            }
        }

        void form_btnUp_Click(object sender, EventArgs e)
        {
            TreeNode treeNode = form_treeView_Browse.SelectedNode;

            if (m_bCompareMode)
            {
                treeNode = ((m_ctlLastFocusForCopyButton is TreeView) ? (TreeView)m_ctlLastFocusForCopyButton : form_treeCompare1).SelectedNode;
            }

            if (treeNode == null)
            {
                m_blink.Go(form_btnUp, clr: Color.Red, Once: true);
                return;
            }

            m_bPutPathInFindEditBox = true;
            m_bTreeViewIndirectSelChange = true;

            if ((treeNode.Parent != null) && (treeNode.Parent.Parent == null))
            {
                ((RootNodeDatum)treeNode.Parent.Tag).VolumeView = false;
            }
            else if (treeNode.Parent == null)
            {
                RootNodeDatum rootNodeDatum = (RootNodeDatum)treeNode.Tag;

                if (rootNodeDatum.VolumeView == true)
                {
                    m_blink.Go(form_btnUp, clr: Color.Red, Once: true);
                    return;
                }

                rootNodeDatum.VolumeView = true;
                treeNode.TreeView.SelectedNode = null;      // to kick in a change selection event
                treeNode.TreeView.SelectedNode = treeNode;
                return;
            }

            treeNode.TreeView.SelectedNode = treeNode.Parent;
        }

        void form_btnVolGroup_Click(object sender, EventArgs e)
        {
            m_bKillTree = (m_tree != null) || (m_bKillTree && timer_DoTree.Enabled);

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

                    if (m_tree == null)
                    {
                        ((RootNodeDatum)((TreeNode)lvItem.Tag).Tag).StrVolumeGroup = inputBox.Entry;
                    }
                }

                return true;
            }));
        }

        void form_cbNavigate_DropDown(object sender, EventArgs e)
        {
            m_bNavDropDown = true;
        }

        void form_cbNavigate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Keys[] { Keys.Enter, Keys.Return }.Contains((Keys)e.KeyChar))
            {
                m_bPutPathInFindEditBox = false;    // because the search term may not be the complete path: Volume Group gets updated though.
                form_btnNavigate_Click(sender);
                e.Handled = true;
            }
        }

        void form_cbNavigate_MouseUp(object sender, MouseEventArgs e)
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
            m_strMaybeFile = form_tmapUserCtl.Tooltip_Click();

            if (m_strMaybeFile != null)
            {
                m_bTreeViewIndirectSelChange = false;   // didn't hit a sel change
                tabControl_FileList.SelectedTab = tabPage_FileList;
                SelectFoundFile();
            }
        }

        void form_cbNavigate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cbNavigate, bTrimText: false);
        }

        void form_cbNavigate_TextChanged(object sender, EventArgs e)
        {
            if (m_listSearchResults.Count > 0)
            {
                m_listSearchResults = new List<SearchResults>();
                GC.Collect();
            }

            m_nTreeFindTextChanged = 0;
            m_bFileFound = false;
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
            if (m_bChkCompare1IndirectCheckChange == true)
            {
                m_bChkCompare1IndirectCheckChange = false;
                return;
            }

            if (m_bCompareMode)
            {
                Utilities.Assert(1308.93105, form_chkCompare1.Checked == false);
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
                m_listHistory.Clear();
                m_nIxHistory = -1;
                form_btnNavigate.Enabled = true;
                form_btnSearchFiles.Enabled = true;
                form_btnSearchFoldersAndFiles.Enabled = true;
                tabControl_FileList.SelectedTab = m_FileListTabPageBeforeCompare;

                m_bCompareMode = false;
                form_treeView_Browse_AfterSelect(form_treeView_Browse, new TreeViewEventArgs(form_treeView_Browse.SelectedNode));
            }
            else if (form_chkCompare1.Checked)
            {
                m_strCompare1 = form_cbNavigate.Text;

                bool bError = (Utilities.StrValid(m_strCompare1) == false);

                if (bError == false)
                {
                    m_nodeCompare1 = FindNode(m_strCompare1, form_treeView_Browse.SelectedNode);
                    bError = (m_nodeCompare1 == null);
                }

                m_blink.Stop();

                if (bError)
                {
                    m_blink.Go(clr: Color.Red, Once: true);
                    m_bChkCompare1IndirectCheckChange = true;
                    form_chkCompare1.Checked = false;
                }
                else
                {
                    m_blink.Go();
                    m_bTreeViewIndirectSelChange = true;
                    form_treeView_Browse.SelectedNode = m_nodeCompare1;
                }
            }
            else
            {
                m_strCompare1 = null;
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

        void formCtl_EnterForCopyButton(object sender, EventArgs e)
        {
            m_ctlLastFocusForCopyButton = (Control)sender;
        }

        void form_lvClones_MouseClick(object sender, MouseEventArgs e)
        {
            lvClonesClick(form_lvClones, ref m_nLVclonesClickIx);
        }

        void form_lvClones_SelectedIndexChanged(object sender, EventArgs e)
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

            m_nLVclonesClickIx = -1;
        }

        void form_lv_FileCompare_KeyPress(object sender, KeyPressEventArgs e)
        {
            FileListKeyPress(form_lvFileCompare, e);
        }

        void form_LV_Files_KeyPress(object sender, KeyPressEventArgs e)
        {
            FileListKeyPress(form_lvFiles, e);
        }

        void form_lvSameVol_MouseClick(object sender, MouseEventArgs e)
        {
            lvClonesClick(form_lvSameVol, ref m_nLVsameVolClickIx);
        }

        void form_lvSameVol_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_nLVsameVolClickIx = -1;
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
            lv.Tag = sortOrder;
            lv.SetSortIcon(0, sortOrder);

            if (lvSelectedItem != null)
            {
                lvSelectedItem.Selected = true;
                lvSelectedItem.EnsureVisible();
            }
        }

        void form_lv_Unique_KeyPress(object sender, KeyPressEventArgs e)
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
                    Utilities.Assert(1308.9311, false);    // the listviewer shouldn't even be visible
                    return;
                }

                e.Handled = true;

                if (form_chkCompare1.Checked == false)
                {
                    form_chkCompare1.Checked = true;
                }
                else
                {
                    form_btnCompare_Click();
                }
            }
        }

        void form_lv_Unique_MouseClick(object sender, MouseEventArgs e)
        {
            LVMarkerClick(form_lvUnique);
        }

        void form_lv_Volumes_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            UpdateLV_VolumesSelection();
        }

        void form_tmapUserCtl_Leave(object sender, EventArgs e)
        {
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

        void form_tree_compare_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 3)
            {
                PutTreeCompareNodePathIntoFindCombo((TreeView)sender);
            }

            CompareModeButtonKeyPress(sender, e);
        }

        void form_treeView_Browse_AfterCheck(object sender, TreeViewEventArgs e)
        {
            String strPath = FullPath(e.Node);

            if (e.Node.Checked)
            {
                ListViewItem lvItem = new ListViewItem(new String[] { e.Node.Text, strPath });

                lvItem.Name = strPath;
                lvItem.Tag = e.Node;
                form_lvCopyList.Items.Add(lvItem);
            }
            else
            {
                form_lvCopyList.Items.Remove(form_lvCopyList.Items.Find(strPath, false)[0]);
            }
        }

        void form_treeView_Browse_AfterSelect(object sender, TreeViewEventArgs e)
        {
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

                return;
            }

            String strVolumeGroup = ((RootNodeDatum)rootNode.Tag).StrVolumeGroup;

            form_lblVolGroup.Text = Utilities.StrValid(strVolumeGroup) ? strVolumeGroup : "(no volume group set)";
            form_colVolDetail.Text = rootNode.Text;
            form_colDirDetail.Text = form_colFilename.Text = strNode;

            if (m_bPutPathInFindEditBox)
            {
                m_bPutPathInFindEditBox = false;
                form_cbNavigate.Text = FullPath(e.Node);
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
                nodeDatum.m_lvItem.Selected = true;
                nodeDatum.m_lvItem.Focused = true;
                nodeDatum.m_lvItem.ListView.TopItem = nodeDatum.m_lvItem;
            }
        }

        void form_treeView_Browse_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 3)     // this actually means cancel (ctrl-C) in PC parlance. Keys.Cancel would misrepresent
            {
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
        }

        void form_treeView_Browse_MouseClick(object sender, MouseEventArgs e)
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

        void Form1_Load(object sender, EventArgs e)
        {
            form_tmapUserCtl.TooltipAnchor = form_cbNavigate;

#if (DEBUG)
//#warning DEBUG is defined.
            Utilities.Assert(0, false, "DEBUG is defined.");
            Utilities.Assert(0, System.Diagnostics.Debugger.IsAttached, "Debugger is not attached!");
#else
            Utilities.Closure(new Action(() =>
            {
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
                        m_blink.Go(form_lvCopyList, clr: Color.Yellow, Once: true);
                        MessageBox.Show("The Copy scratchpad cannot be loaded with no directory listings.", "Load Copy scratchpad externally");
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
            }));

#endif
        }

        void label_About_Click(object sender, EventArgs e)
        {
            new AboutBox1().ShowDialog_Once(this);
        }

        void timer_blink_Tick(object sender, EventArgs e)
        {
            m_blink.Tick();
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
