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
    delegate void MessageBoxDelegate(String strMessage, String strTitle = null);
    delegate bool DoSomething();

    partial class Form1 : Form
    {
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

        TreeNode m_nodeCompare1 = null;
        TreeNode m_nodeCompare2 = null;
        TreeNode[] m_arrayTreeFound = null;

        Control m_ctlLastFocusForCopyButton = null;
        Control m_ctlLastSearchSender = null;
        Dictionary<TreeNode, TreeNode> dictCompareDiffs = new Dictionary<TreeNode, TreeNode>();
        List<TreeNode> m_listTreeNodes_Compare1 = new List<TreeNode>();
        List<TreeNode> m_listTreeNodes_Compare2 = new List<TreeNode>();

        List<TreeNode> m_listHistory = new List<TreeNode>();
        int m_nIxHistory = -1;
        bool m_bHistoryDefer = false;
        bool m_bTreeViewIndirectSelChange = false;

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

        class Form1TreeView : TreeView
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
            bool m_bProgress = false;

            internal void Go(Control ctl = null, ListViewItem lvItem = null, Color? clr = null, bool Once = false, bool bProgress = false)
            {
                m_lvItem = lvItem;

                if (m_timer.Enabled)
                {
                    Stop();
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

                m_bProgress = bProgress;
                m_clrBlink = clr ?? (bProgress ? Color.LightSalmon : Color.Turquoise);
                m_nBlink = 0;
                m_nNumBlinks = Once ? 2 : 10;
                m_timer.Interval = bProgress ? 500 : (Once ? 100 : 50);
                m_timer.Enabled = true;
            }

            internal void Stop()
            {
                Reset();
                SetCtlBackColor(m_clrControlOrig);
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

            internal void Tick()
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

            internal Blink(System.Windows.Forms.Timer timer, Control defaultControl)
            {
                m_timer = timer;
                m_defaultControl = defaultControl;
                m_clrControlOrig = defaultControl.BackColor;
            }
        }

        partial class SOTFile
        {
            internal static void WriteList(ListView.ListViewItemCollection lvItems, StreamWriter sw, String strHeader = null)
            {
                sw.WriteLine(strHeader ?? Utilities.m_str_VOLUME_LIST_HEADER);

                foreach (ListViewItem lvItem in lvItems)
                {
                    int nIx = 0;

                    foreach (ListViewItem.ListViewSubItem lvSubitem in lvItem.SubItems)
                    {
                        String str = lvSubitem.Text;

                        if (nIx == 1)
                        {
                            str = str.TrimEnd(Path.DirectorySeparatorChar);
                        }

                        sw.Write(str + '\t');
                    }

                    sw.WriteLine();
                }
            }

            internal static void ReadList(ListView.ListViewItemCollection lvItems, StreamReader sr, String strDir_in = null, String strHeader = null)
            {
                List<ListViewItem> listItems = new List<ListViewItem>();
                String strLine = sr.ReadLine();

                do
                {
                    if (strLine == null)
                    {
                        break;
                    }

                    if ((strHeader ?? (Utilities.m_str_VOLUME_LIST_HEADER_01 + Utilities.m_str_VOLUME_LIST_HEADER)).Contains(strLine) == false)
                    {
                        break;
                    }

                    while ((strLine = sr.ReadLine()) != null)
                    {
                        String[] strArray = strLine.Split('\t');

                        if (strHeader == null)
                        {
                            if (strArray.Length < 4)
                            {
                                break;
                            }

                            strArray[3] = "Using file.";

                            if (File.Exists(strArray[2]) == false)
                            {
                                strArray[2] = Path.Combine(strDir_in ?? Path.GetTempPath(), Path.GetFileName(strArray[2]));

                                if (File.Exists(strArray[2]) == false)
                                {
                                    strArray[3] = "No file. Will create.";
                                }
                            }

                            strArray[1] = strArray[1].TrimEnd(Path.DirectorySeparatorChar);
                        }

                        listItems.Add(new ListViewItem(strArray));
                    }
                }
                while (false);

                if (listItems.Count > 0)
                {
                    lvItems.Clear();
                    lvItems.AddRange(listItems.ToArray());
                }
                else
                {
                    if (strHeader == null)
                    {
                        MessageBox.Show("Not a valid volume list file.".PadRight(100), "Load Volume List");
                    }
                    else
                    {
                        MessageBox.Show("Not a valid file.".PadRight(100));
                    }
                }
            }

            internal bool Save(String strFile, ListView.ListViewItemCollection lvItems)
            {
                return false;
            }

            internal bool Load(String strFile, ListView.ListViewItemCollection lvItems)
            {
                return false;
            }
        }

        internal Form1()
        {
            InitializeComponent();

            // Assert String-lookup form items exist
            //    Debug.Assert(context_rclick_node.Items[m_strMARKFORCOPY] != null);

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
            splitIgnoreList.SplitterDistance = splitCopyList.SplitterDistance = form_btnCopyClear.Left + form_btnCopyClear.Width + 5;
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
            form_tmapUserCtl.ClearSelection();
        }

        void DoHistory(object sender, int nDirection)
        {
            int nIxHistory = m_nIxHistory + nDirection;

            do
            {
                if (nIxHistory < 0)
                {
                    break;
                }

                if (m_listHistory.Count <= 0)
                {
                    break;
                }

                if (nIxHistory > (m_listHistory.Count - 1))
                {
                    break;
                }

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
                return;
            }
            while (false);

            m_blink.Go((Control)sender, clr: Color.Red, Once: true);
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
                form_tabControl.SelectedTab = form_tabPageVolumes;
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
                        Debug.Assert((nPathLevelLength - nCount) > 0);
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

        void InterruptTreeTimerWithAction(DoSomething DoAction)
        {
            bool bTimer = timer_DoTree.Enabled;

            timer_DoTree.Stop();

            bool bKillTree = DoAction();

            if (bKillTree)
            {
                KillTreeBuilder();
            }

            if (bKillTree || bTimer)
            {
                RestartTreeTimer();
            }
        }

        bool LV_VolumesItemInclude(ListViewItem lvItem)
        {
            return (lvItem.SubItems[4].Text == "Yes");
        }

        void lvClonesClick(ListView lv, ref int nClickIndex)
        {
            if (lv.SelectedItems.Count == 0)
            {
                return;
            }

            if (lv.SelectedItems[0].Tag == null)
            {
                // marker item
                ListViewItem lvItem = lv.Items[lv.SelectedItems[0].Index + 1];

                lvItem.Selected = true;
                lvItem.Focused = true;
            }

            List<TreeNode> listTreeNodes = (List<TreeNode>)lv.SelectedItems[0].Tag;

            m_bPutPathInFindEditBox = true;
            m_bTreeViewIndirectSelChange = true;
            form_treeView_Browse.SelectedNode = listTreeNodes[++nClickIndex % listTreeNodes.Count];
            form_treeView_Browse.Select();
        }

        void MessageboxCallback(String strMessage, String strTitle)
        {
            if (InvokeRequired) { Invoke(new MessageBoxDelegate(MessageboxCallback), new object[] { strMessage, strTitle }); return; }

            // make MessageBox modal from a worker thread
            MessageBox.Show(strMessage.PadRight(100), strTitle);
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

        bool NavToFile(TreeView treeView)
        {
            int nCounter = -1;
            int nSearchLoop = -1;

            while (true)
            {
                int nSearchIx = -1;

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

                            bool bContinue = false;
                            TreeNode treeNode = NavToFile(resultDir.StrDir, treeView,
                                ref nSearchLoop, ref nSearchIx, ref bContinue);

                            if (bContinue)
                            {
                                continue;
                            }

                            if (treeNode != null)
                            {
                                m_bTreeViewIndirectSelChange = true;
                                treeNode.TreeView.SelectedNode = treeNode;
                                ++m_nTreeFindTextChanged;
                                m_blink.Stop();
                                m_blink.Go(Once: true);
                            }

                            return (treeNode != null);
                        }
                        else
                        {
                            foreach (String strFile in resultDir.ListFiles)
                            {
                                if (++nCounter < m_nTreeFindTextChanged)
                                {
                                    continue;
                                }

                                m_strMaybeFile = strFile;

                                bool bContinue = false;
                                TreeNode treeNode = NavToFile(resultDir.StrDir, treeView,
                                    ref nSearchLoop, ref nSearchIx, ref bContinue);

                                if (bContinue)
                                {
                                    continue;
                                }

                                if (treeNode != null)
                                {
                                    if (treeNode.TreeView.SelectedNode == treeNode)
                                    {
                                        SelectFoundFile();
                                    }
                                    else
                                    {
                                        m_bTreeViewIndirectSelChange = true;
                                        treeNode.TreeView.SelectedNode = treeNode;
                                    }

                                    ++m_nTreeFindTextChanged;
                                }

                                return (treeNode != null);
                            }
                        }
                    }
                }

                // Don't bother imposing a modulus. Just let m_nTreeFindTextChanged grow.
            }
        }

        TreeNode NavToFile(String strDir, TreeView treeView, ref int nSearchLoop, ref int nSearchIx, ref bool bContinue)
        {
            TreeNode treeNode = GetNodeByPath(strDir, treeView);
            bContinue = false;

            if (treeNode == null)
            {
                // compare mode
                Debug.Assert(treeView != form_treeView_Browse);

                if (treeView == form_treeView_Browse)
                {
                    return null;
                }

                if (nSearchIx >= nSearchLoop)
                {
                    return null;
                }

                nSearchLoop = nSearchIx;
                bContinue = true;
            }

            return treeNode;
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
                do
                {
                    String line = null;

                    if ((line = file.ReadLine()) == null) break;
                    if ((line = file.ReadLine()) == null) break;
                    if (line.StartsWith(Utilities.m_strLINETYPE_Nickname) == false) break;
                    String[] arrLine = line.Split('\t');
                    String strName = String.Empty;
                    if (arrLine.Length > 2) strName = arrLine[2];
                    form_cbVolumeName.Text = strName;
                    if ((line = file.ReadLine()) == null) break;
                    if (line.StartsWith(Utilities.m_strLINETYPE_Path) == false) break;
                    arrLine = line.Split('\t');
                    if (arrLine.Length < 3) break;
                    form_cbPath.Text = arrLine[2];
                    return SaveFields(false);
                }
                while (false);
            }

            return false;
        }

        void RestartTreeTimer()
        {
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

                if (FormatPath(form_cbSaveAs, ref m_strSaveAs, bFailOnDirectory) == false)
                {
                    return false;
                }
            }

            return true;
        }

        void Search(object sender)
        {
            if (form_cbNavigate.Text.Length == 0)
            {
                m_blink.Go(clr: Color.Red, Once: true);
                return;
            }

            m_ctlLastSearchSender = (Control)sender;

            TreeView treeView = form_treeView_Browse;

            if (m_bCompareMode)
            {
                treeView = (m_ctlLastFocusForCopyButton is TreeView) ? (TreeView)m_ctlLastFocusForCopyButton : form_treeCompare1;
            }

            while (true)
            {
                if (m_nTreeFindTextChanged == 0)
                {
                    FindNode(form_cbNavigate.Text, treeView.SelectedNode, treeView);
                }

                if (m_bFileFound)
                {
                    NavToFile(treeView);
                }
                else
                {
                    if ((m_arrayTreeFound != null) && (m_arrayTreeFound.Length > 0))
                    {
                        TreeNode treeNode = m_arrayTreeFound[m_nTreeFindTextChanged % m_arrayTreeFound.Length];

                        m_bTreeViewIndirectSelChange = true;
                        treeNode.TreeView.SelectedNode = treeNode;
                        ++m_nTreeFindTextChanged;
                        m_blink.Stop();
                        m_blink.Go(Once: true);
                    }
                    else if (treeView == form_treeCompare1)
                    {
                        treeView = form_treeCompare2;
                        continue;
                    }
                    else if (form_cbNavigate.Text.Contains(Path.DirectorySeparatorChar))
                    {
                        Debug.Assert(form_cbNavigate.Text.EndsWith(Path.DirectorySeparatorChar.ToString()) == false);

                        int nPos = form_cbNavigate.Text.LastIndexOf(Path.DirectorySeparatorChar);
                        String strMaybePath = form_cbNavigate.Text.Substring(0, nPos);
                        TreeNode treeNode = GetNodeByPath(strMaybePath, form_treeView_Browse);

                        m_strMaybeFile = form_cbNavigate.Text.Substring(nPos + 1);

                        if (treeNode != null)
                        {
                            m_bTreeViewIndirectSelChange = true;
                            treeNode.TreeView.SelectedNode = treeNode;
                        }
                        else
                        {
                            Debug.Assert(m_listSearchResults.Count <= 0);
                            SearchResultsCallback_Fail();
                        }
                    }
                    else
                    {
                        SearchFiles(form_cbNavigate.Text, new SearchResultsDelegate(SearchResultsCallback));
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

                TreeView treeView = form_treeView_Browse;

                if (m_bCompareMode)
                {
                    treeView = (m_ctlLastFocusForCopyButton is TreeView) ? (TreeView)m_ctlLastFocusForCopyButton : form_treeCompare1;

                    if (NavToFile(treeView) == false)
                    {
                        if (NavToFile((treeView == form_treeCompare1) ? form_treeCompare2 : form_treeCompare1) == false)
                        {
                            SearchResultsCallback_Fail();
                        }
                    }
                }
                else
                {
                    NavToFile(treeView);
                }
            }
            else
            {
                SearchResultsCallback_Fail();
            }
        }

        void SearchResultsCallback_Fail()
        {
            m_nTreeFindTextChanged = 0;
            m_bFileFound = false;
            m_strMaybeFile = null;
            m_blink.Stop();
            m_blink.Go(clr: Color.Red, Once: true);
            MessageBox.Show("Couldn't find the specified search parameter.".PadRight(100), "Search");
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

        void form_btn_AddVolume_Click(object sender, EventArgs e)
        {
            InterruptTreeTimerWithAction(new DoSomething(form_btn_AddVolume_Click));
        }

        bool form_btn_AddVolume_Click()
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

                if (MessageBox.Show(m_strSaveAs + " already exists. Overwrite?".PadRight(100), "Volume Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    m_blink.Go(form_cbSaveAs, clr: Color.Red, Once: true);
                    return false;
                }
            }

            if ((File.Exists(m_strSaveAs) == false) && form_lvVolumesMain.Items.ContainsKey(m_strPath))
            {
                FormError(form_cbPath, "Path already added.", "Volume Source Path");
                return false;
            }

            if (Utilities.StrValid(m_strVolumeName) && form_lvVolumesMain.FindItemWithText(m_strVolumeName) != null)
            {
                m_blink.Go(form_cbVolumeName, clr: Color.Red);

                if (MessageBox.Show("Nickname already in use. Use it for more than one volume?".PadRight(100), "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.Yes)
                {
                    m_blink.Go(form_cbVolumeName, clr: Color.Red, Once: true);
                    return false;
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

            ListViewItem lvItem = new ListViewItem(new String[] { m_strVolumeName, m_strPath, m_strSaveAs, strStatus, "Yes" });

            lvItem.Name = m_strPath;
            form_lvVolumesMain.Items.Add(lvItem);
            form_btnSaveDirList.Enabled = true;
            return true;
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

            KillTreeBuilder();
            RestartTreeTimer();
        }

        void form_btn_Compare_Click(object sender, EventArgs e)
        {
            if (m_bCompareMode)
            {
                form_btn_CompareNext_Click(sender, e);
            }
            else
            {
                form_cbNavigate.BackColor = Color.Empty;
                Debug.Assert(form_chkCompare1.Checked);
                Debug.Assert(Utilities.StrValid(m_strCompare1));

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
                    m_bCompareMode = true;
                    tabControl_FileList.SelectedTab = tabPage_FileList;
                    m_bTreeViewIndirectSelChange = true;
                    form_treeCompare1.SelectedNode = form_treeCompare1.Nodes[0];
                    m_bTreeViewIndirectSelChange = true;
                    form_treeCompare2.SelectedNode = form_treeCompare2.Nodes[0];
                }
            }
        }

        void form_btn_CompareNext_Click(object sender, EventArgs e)
        {
            if (dictCompareDiffs.Count == 0)
            {
                return;
            }

            m_nCompareIndex = Math.Min(dictCompareDiffs.Count - 1, ++m_nCompareIndex);
            CompareNav();
        }

        void form_btn_ComparePrev_Click(object sender, EventArgs e)
        {
            if (dictCompareDiffs.Count == 0)
            {
                return;
            }

            m_nCompareIndex = Math.Max(0, --m_nCompareIndex);
            CompareNav();
        }

        void form_btn_Copy_Click(object sender, EventArgs e)
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
        }

        void form_btnIgnoreTree_Click(object sender, EventArgs e)
        {
            KillTreeBuilder();
            RestartTreeTimer();
        }

        void form_btnLoadCopyDirs_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
            openFileDialog1.FileName = saveFileDialog1.FileName;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ListView lv = new ListView();   // Hack: check changed event loads the real listviewer

            using (StreamReader fs = File.OpenText(openFileDialog1.FileName))
            {
                SOTFile.ReadList(lv.Items, fs, Path.GetDirectoryName(openFileDialog1.FileName), Utilities.m_str_COPYDIRS_LIST_HEADER);
            }

            foreach (ListViewItem lvItem in lv.Items)
            {
                TreeNode treeNode = GetNodeByPath(lvItem.SubItems[1].Text, form_treeView_Browse);

                if (treeNode != null)
                {
                    treeNode.Checked = true;
                }
            }
        }

        void form_btnLoadIgnoreList_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
            openFileDialog1.FileName = saveFileDialog1.FileName;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (StreamReader fs = File.OpenText(openFileDialog1.FileName))
            {
                SOTFile.ReadList(form_lvIgnoreList.Items, fs, Path.GetDirectoryName(openFileDialog1.FileName), Utilities.m_str_IGNORE_LIST_HEADER);
            }

            foreach (ListViewItem lvItem in form_lvIgnoreList.Items)
            {
                lvItem.Name = lvItem.Text;
            }

            KillTreeBuilder();
            RestartTreeTimer();
        }

        void form_btn_LoadVolumeList_Click(object sender, EventArgs e)
        {
            InterruptTreeTimerWithAction(new DoSomething(form_btn_LoadVolumeList_Click));
        }

        bool form_btn_LoadVolumeList_Click()
        {
            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
            openFileDialog1.FileName = saveFileDialog1.FileName;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            if (new SOTFile().Load(openFileDialog1.FileName, form_lvVolumesMain.Items) == false)
            {
                using (StreamReader fs = File.OpenText(openFileDialog1.FileName))
                {
                    SOTFile.ReadList(form_lvVolumesMain.Items, fs, Path.GetDirectoryName(openFileDialog1.FileName));
                }
            }

            if (form_lvVolumesMain.Items.Count > 0)
            {
                form_btnSaveDirList.Enabled = true;
            }

            form_tabControl.SelectedTab = form_tabPageBrowse;
            return true;
        }

        void form_btnModifyFile_Click(object sender, EventArgs e)
        {
            InterruptTreeTimerWithAction(new DoSomething(form_btnModifyFile_Click));
        }

        bool form_btnModifyFile_Click()
        {
            ListView.SelectedListViewItemCollection lvSelect = form_lvVolumesMain.SelectedItems;

            if (lvSelect.Count <= 0)
            {
                return false;
            }

            if (lvSelect.Count > 1)
            {
                Debug.Assert(false);    // guaranteed by selection logic
                MessageBox.Show("Only one file can be modified at a time.".PadRight(100), "Modify file");
                return false;
            }

            String strVolumeName_orig = form_lvVolumesMain.SelectedItems[0].Text;
            String strVolumeName = null;

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
                    Debug.Assert(false);
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

            do
            {
                if ((Utilities.StrValid(strVolumeName)) &&
                    (Utilities.NotNull(strVolumeName) != Utilities.NotNull(strVolumeName_orig)))
                {
                    break;
                }

                if (Utilities.StrValid(strDriveLetter) &&
                    (Utilities.NotNull(strDriveLetter) != Utilities.NotNull(strDriveLetter_orig)))
                {
                    break;
                }

                MessageBox.Show("No changes made.".PadRight(100), "Modify file");
                return false;
            }
            while (false);

            String strFileName = form_lvVolumesMain.SelectedItems[0].SubItems[2].Text;
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
                            Debug.Assert(sbLine.ToString().Split('\t').Length == 2);
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
        }

        void form_btnNavigate_Click(object sender, EventArgs e)
        {
            Search(sender);
        }

        void form_btnPath_Click(object sender, EventArgs e)
        {
            InterruptTreeTimerWithAction(new DoSomething(form_btnPath_Click));
        }

        bool form_btnPath_Click()
        {
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            ComboBoxItemsInsert(form_cbPath);
            m_strPath = form_cbPath.Text = folderBrowserDialog1.SelectedPath;
            return true;
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
            InterruptTreeTimerWithAction(new DoSomething(form_btnSaveAs_Click));
        }

        bool form_btnSaveAs_Click()
        {
            if (Utilities.StrValid(m_strSaveAs))
            {
                saveFileDialog1.InitialDirectory = Path.GetDirectoryName(m_strSaveAs);
            }

            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            ComboBoxItemsInsert(form_cbSaveAs);
            m_strSaveAs = form_cbSaveAs.Text = saveFileDialog1.FileName;

            if (File.Exists(m_strSaveAs))
            {
                form_cbVolumeName.Text = null;
                form_cbPath.Text = null;
            }

            return true;
        }

        void form_btnSaveCopyDirs_Click(object sender, EventArgs e)
        {
            if (form_lvCopyList.Items.Count == 0)
            {
                m_blink.Go(ctl: form_btnSaveCopyDirs, clr: Color.Red, Once: true);
                return;
            }

            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if ((File.Exists(saveFileDialog1.FileName))
                && (MessageBox.Show(this, (saveFileDialog1.FileName + " already exists. Overwrite?").PadRight(100), "Save folders selected for copy", MessageBoxButtons.YesNo)
                != System.Windows.Forms.DialogResult.Yes))
            {
                return;
            }

            using (StreamWriter fs = File.CreateText(saveFileDialog1.FileName))
            {
                SOTFile.WriteList(form_lvCopyList.Items, fs, Utilities.m_str_COPYDIRS_LIST_HEADER);
            }
        }

        void form_btnSaveDirLists_Click(object sender, EventArgs e)
        {
            timer_DoTree.Stop();
            DoSaveDirListings();
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

            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if ((File.Exists(saveFileDialog1.FileName))
                && (MessageBox.Show(this, (saveFileDialog1.FileName + " already exists. Overwrite?").PadRight(100), "Save ignore list", MessageBoxButtons.YesNo)
                != System.Windows.Forms.DialogResult.Yes))
            {
                return;
            }

            using (StreamWriter fs = File.CreateText(saveFileDialog1.FileName))
            {
                SOTFile.WriteList(form_lvIgnoreList.Items, fs, Utilities.m_str_IGNORE_LIST_HEADER);
            }
        }

        void form_btnSaveVolumeList_Click(object sender, EventArgs e)
        {
            InterruptTreeTimerWithAction(new DoSomething(form_btnSaveVolumeList_Click));
        }

        bool form_btnSaveVolumeList_Click()
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            if ((File.Exists(saveFileDialog1.FileName))
                && (MessageBox.Show(this, (saveFileDialog1.FileName + " already exists. Overwrite?").PadRight(100), "Volume List Save As", MessageBoxButtons.YesNo)
                != System.Windows.Forms.DialogResult.Yes))
            {
                return false;
            }

            if (new SOTFile().Save(saveFileDialog1.FileName, form_lvVolumesMain.Items) == false)
            {
                using (StreamWriter fs = File.CreateText(saveFileDialog1.FileName))
                {
                    SOTFile.WriteList(form_lvVolumesMain.Items, fs);
                }
            }

            return false;   // Saving the volume list doesn't compel redrawing the tree.
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
                SearchFiles(form_cbNavigate.Text,
                    new SearchResultsDelegate(SearchResultsCallback),
                    bSearchFilesOnly: (sender == form_btnSearchFiles));
            }
            else
            {
                Search(sender);
            }
        }

        void form_btn_ToggleInclude_Click(object sender, EventArgs e)
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

        void form_btn_TreeCollapse_Click(object sender, EventArgs e)
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

        void form_btnUp_Click(object sender, EventArgs e)
        {
            TreeNode treeNode = form_treeView_Browse.SelectedNode;

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

        void form_btn_VolGroup_Click(object sender, EventArgs e)
        {
            InterruptTreeTimerWithAction(new DoSomething(form_btn_VolGroup_Click));
        }

        bool form_btn_VolGroup_Click()
        {
            ListView.SelectedListViewItemCollection lvSelect = form_lvVolumesMain.SelectedItems;

            if (lvSelect.Count <= 0)
            {
                return false;
            }

            InputBox inputBox = new InputBox();

            inputBox.Text = "Volume Group";
            inputBox.Prompt = "Enter a volume group name";
            inputBox.Entry = form_lvVolumesMain.SelectedItems[0].SubItems[5].Text;

            SortedDictionary<String, object> dictVolGroups = new SortedDictionary<String, object>();

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
                return false;
            }

            foreach (ListViewItem lvItem in lvSelect)
            {
                lvItem.SubItems[5].Text = inputBox.Entry;
            }

            return true;
        }

        void form_cbNavigate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Keys[] { Keys.Enter, Keys.Return }.Contains((Keys)e.KeyChar))
            {
                m_bPutPathInFindEditBox = false;    // because the search term may not be the complete path: Volume Group gets updated though.
                form_btnNavigate_Click(sender, e);
                e.Handled = true;
            }
        }

        void form_cbNavigate_MouseUp(object sender, MouseEventArgs e)
        {
            form_tmapUserCtl.ClearSelection();

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
                    form_chkCompare1.Checked = false;  // event retriggers this handler
                }
                else
                {
                    m_blink.Go();
                    m_bTreeViewIndirectSelChange = true;
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

        void form_chkLoose_CheckedChanged(object sender, EventArgs e)
        {
            if (form_lvIgnoreList.Items.Count <= 0)
            {
                return;
            }

            KillTreeBuilder();
            RestartTreeTimer();
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
                        listItems.Sort((x, y) => y.Text.CompareTo(x.Text));
                        break;
                    }

                case SortOrder.Descending:
                    {
                        if (bNullTags)
                        {
                            goto case SortOrder.None;
                        }

                        sortOrder = SortOrder.None;

                        if (listItems[0].Tag is List<TreeNode>)
                        {
                            listItems.Sort((x, y) => ((NodeDatum)((List<TreeNode>)y.Tag)[0].Tag).nTotalLength.CompareTo(((NodeDatum)((List<TreeNode>)x.Tag)[0].Tag).nTotalLength));
                        }
                        else
                        {
                            listItems.Sort((x, y) => ((NodeDatum)((TreeNode)y.Tag).Tag).nTotalLength.CompareTo(((NodeDatum)((TreeNode)x.Tag).Tag).nTotalLength));
                        }

                        TreeDone.InsertSizeMarkers(listItems);
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

        void form_lv_Unique_MouseClick(object sender, MouseEventArgs e)
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

        void form_lv_Volumes_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            UpdateLV_VolumesSelection();
        }

        void form_tmapUserCtl_Leave(object sender, EventArgs e)
        {
            form_tmapUserCtl.ClearSelection();
        }

        void form_tmapUserCtl_MouseUp(object sender, MouseEventArgs e)
        {
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
            if (m_bHistoryDefer == false)
            {
                m_bHistoryDefer = true;

                if ((m_listHistory.Count > 0) && (m_nIxHistory > -1) && ((m_listHistory.Count - 1) > m_nIxHistory))
                {
                    m_listHistory.RemoveRange(m_nIxHistory, m_listHistory.Count - m_nIxHistory - 1);
                }

                Debug.Assert(m_nIxHistory == (m_listHistory.Count - 1));

                if ((m_nIxHistory < 0) || (History_Equals(e.Node) == false))
                {
                    History_Add(e.Node);
                    ++m_nIxHistory;
                }
            }

            m_bHistoryDefer = false;

            if ((m_bTreeViewIndirectSelChange == false)
                && (e.Node.Parent == null))
            {
                ((RootNodeDatum)e.Node.Tag).VolumeView = true;
            }

            m_bTreeViewIndirectSelChange = false;
            form_tmapUserCtl.Render(e.Node);

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

            TreeNode rootNode = e.Node.Root();

            Debug.Assert((new object[] { form_treeCompare1, form_treeCompare2 }.Contains(sender)) == m_bCompareMode);
            DoTreeSelect(e.Node);

            String strNode = e.Node.Text;

            Debug.Assert(Utilities.StrValid(strNode));

            if (m_bCompareMode)
            {
                String strDirAndVolume = strNode + " (on " + rootNode.ToolTipText.Substring(0, rootNode.ToolTipText.IndexOf(Path.DirectorySeparatorChar)) + ")";

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

            form_lblVolGroup.Text = ((RootNodeDatum)rootNode.Tag).StrVolumeGroup;

            if (Utilities.StrValid(form_lblVolGroup.Text) == false)
            {
                form_lblVolGroup.Text = "(no volume group set)";
            }

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

            if ((nodeDatum.m_lvItem == null) || nodeDatum.m_lvItem.Selected)
            {
                return;
            }

            nodeDatum.m_lvItem.Selected = true;
            nodeDatum.m_lvItem.Focused = true;
            nodeDatum.m_lvItem.ListView.TopItem = nodeDatum.m_lvItem;
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
            else if (form_btnCompare.Enabled == false)
            {
                form_chkCompare1.Checked = true;            // enter first path to compare
            }
            else
            {
                form_btn_Compare_Click(sender, e);          // enter second path and start Compare mode
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

        void Form1_Load(object sender, EventArgs e)
        {
            form_tmapUserCtl.TooltipAnchor = form_cbNavigate;
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
            DoTree(bKill: true);
        }
    }
}
