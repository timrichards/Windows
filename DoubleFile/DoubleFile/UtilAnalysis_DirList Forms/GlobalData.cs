using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DoubleFile
{
    partial class GlobalData
    {
        internal SDL_TreeView m_treeCopyToClipboard = null;
        internal TreeNode m_nodeCompare1 = null;
        internal readonly Dictionary<TreeNode, TreeNode> m_dictCompareDiffs = new Dictionary<TreeNode, TreeNode>();
        internal readonly UList<TreeNode> m_listTreeNodes_Compare1 = new UList<TreeNode>();
        internal readonly UList<TreeNode> m_listTreeNodes_Compare2 = new UList<TreeNode>();
        internal readonly List<TreeNode> m_listHistory = new List<TreeNode>();
        internal int m_nIxHistory = -1;

        internal void ClearMem_FormAnalysis_DirList()
        {
            m_nodeCompare1 = null;
            m_dictCompareDiffs.Clear();
            m_listTreeNodes_Compare1.Clear();
            m_listTreeNodes_Compare2.Clear();

            m_listHistory.Clear();
            m_nIxHistory = -1;
        }

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

        // initialized in FormAnalysis_DirList constructor:
        internal BlinkyStruct m_blinky;
        internal string m_strBtnTreeCollapseOrig = null;
        internal string m_strColFilesOrig = null;
        internal string m_strColFileCompareOrig = null;
        internal string m_strColDirDetailCompareOrig = null;
        internal string m_strColDirDetailOrig = null;
        internal string m_strColVolDetailOrig = null;
        internal string m_strBtnCompareOrig = null;
        internal string m_strChkCompareOrig = null;
        internal string m_strVolGroupOrig = null;

        static internal void ComboBoxItemsInsert(ComboBox comboBox, string strText = null, bool bTrimText = true)
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

                    if ((r1 != null) && (m_dictCompareDiffs.ContainsKeyA(r1) == false))
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

            if ((nIxHistory >= 0) && (false == m_listHistory.IsEmpty()) && (nIxHistory <= (m_listHistory.Count - 1)))
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

        internal static string FullPath(TreeNode treeNode)
        {
            if (treeNode == null)
            {
                return null;
            }

            StringBuilder sbFullPath = null;
            TreeNode ownerNode = (TreeNode)treeNode.Parent;

            if (ownerNode == null)
            {
                sbFullPath = new StringBuilder(treeNode.Name);
            }
            else
            {
                sbFullPath = new StringBuilder(treeNode.Text);
            }

            while ((ownerNode != null) && (ownerNode.Parent != null))
            {
                sbFullPath.Insert(0, '\\');
                sbFullPath.Insert(0, ownerNode.Text.TrimEnd('\\'));
                ownerNode = (TreeNode)ownerNode.Parent;
            }

            if ((ownerNode != null) && (ownerNode.Parent == null))
            {
                sbFullPath.Insert(0, '\\');
                sbFullPath.Insert(0, ownerNode.Name.TrimEnd('\\'));
            }

            return sbFullPath.ToString().Replace(@"\\", @"\");
        }

        internal TreeNode History_GetAt(int n)
        {
            TreeNode treeNode = m_listHistory[n];

            var treeNode_A = treeNode.Tag as TreeNode;

            if (null != treeNode_A)
            {
                var rootNodeDatum = (treeNode_A.Tag as RootNodeDatum);

                if (null == rootNodeDatum)      // this check is new 2/17/15 and has never been hit
                {
                    MBoxStatic.Assert(99980, false);
                    return treeNode;
                }

                rootNodeDatum.VolumeView = treeNode.Checked;
                return treeNode_A;
            }
            else
            {
                return treeNode;
            }
        }

        internal bool History_Equals(TreeNode treeNode)
        {
            var rootNodeDatum = (treeNode.Tag as RootNodeDatum);

            if (null != rootNodeDatum)
            {
                TreeNode treeNode_A = (TreeNode)m_listHistory[m_listHistory.Count - 1];

                if ((treeNode_A.Tag is TreeNode) == false)
                {
                    return false;
                }

                if (rootNodeDatum.VolumeView != treeNode_A.Checked)
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

            var rootNodeDatum = (treeNode.Tag as RootNodeDatum);

            if (null != rootNodeDatum)
            {
                m_listHistory.Add(new TreeNode()   // checked means VolumeView mode and necessitates History_Add() etc.
                {
                    Checked = rootNodeDatum.VolumeView,
                    Tag = treeNode
                });
            }
            else
            {
                m_listHistory.Add(treeNode);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void InterruptTreeTimerWithAction(BoolAction boolAction)
        {
            bool bTimer = m_tmrDoTree.Enabled;

            m_tmrDoTree.Stop();

            bool bKillTree = boolAction();

            if (bKillTree)
            {
                gd_Tree.KillTreeBuilder();
            }

            if (bKillTree || bTimer)
            {
                RestartTreeTimer();
            }
        }

        internal bool LV_MarkerClick(ListView lv, bool bUp = false)     // returns true when selected tag is not null, and may change selection.
        {
            if (lv.SelectedItems.IsEmpty())
            {
                return false;
            }

            if (((ListViewItem)lv.SelectedItems[0]).Tag != null)
            {
                return true;
            }

            // marker item
            int nIx = ((ListViewItem)lv.SelectedItems[0]).Index + 1;
            bool bGt = (nIx >= lv.Items.Count);

            if (bUp || bGt)
            {
                if ((nIx - 2) >= 0)
                {
                    nIx -= 2;
                }
                else if (bGt)
                {
                    MBoxStatic.Assert(1308.9331, false, bTraceOnly: true);
                    return false;   // LV with just a marker item?
                }
            }

            ListViewItem lvItem = (ListViewItem)lv.Items[nIx];

            if (MBoxStatic.Assert(1308.9332, lvItem.Tag != null, bTraceOnly: true) == false)
            {
                return false;
            }

            m_bClonesLVindirectSelChange = true;

            UtilProject.WriteLine("LVMarkerClick");
            lvItem.EnsureVisible();
            lvItem.Selected = true;
            lvItem.Focused = true;

            if (lv.SelectedItems.IsEmpty())
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

            var lv = (sender as ListView);

            if (null == lv)
            {
                MBoxStatic.Assert(1308.9333, false, bTraceOnly: true);
                return false;
            }

            if (lv.SelectedItems.IsEmpty())
            {
                return false;
            }

            ++m_nSelChgIx;

            int nNow =  m_arrSelChgIx[m_nSelChgIx %= 2] = ((ListViewItem)lv.SelectedItems[0]).Index;
            int nPrev = m_arrSelChgIx[(m_nSelChgIx + 1) % 2];

            bUp = nNow < nPrev;
            return nNow != nPrev;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void RemoveCorrelation(TreeNode treeNode_in, bool bContinue = false)
        {
            TreeNode treeNode = treeNode_in;

            do
            {
                if (false == treeNode.Nodes.IsEmpty())
                {
                    RemoveCorrelation((TreeNode)treeNode.Nodes[0], bContinue: true);
                }

                if (gd_Tree.m_listTreeNodes.Contains(treeNode))
                {
                    gd_Tree.m_listTreeNodes.Remove(treeNode);
                }

                treeNode.ForeColor = Color.Empty;
                treeNode.BackColor = Color.Empty;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                if (gd_Tree.m_dictNodes.ContainsKeyA(nodeDatum.Key) == false)
                {
                    // same scenario as empty owner:
                    // Search "Parent folder may contain only its clone subfolder, in which case unmark the subfolder"
                    continue;
                }

                UList<TreeNode> listClones = gd_Tree.m_dictNodes[nodeDatum.Key];

                if (listClones.Contains(treeNode))
                {
                    listClones.Remove(treeNode);

                    if (listClones.IsEmpty())
                    {
                        gd_Tree.m_dictNodes.Remove(nodeDatum.Key);
                    }
                }
            }
            while (bContinue && ((treeNode = (TreeNode)treeNode.NextNode) != null));
        }
    }
}
