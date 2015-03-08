using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleFile
{
    partial class Collate
    {
        internal Collate(GlobalData_Base gd_in,
            SortedDictionary<FolderKeyTuple, UList<TreeNode>> dictNodes,
            TreeView treeViewBrowse,
            SDL_ListView lvClones,
            SDL_ListView lvSameVol,
            SDL_ListView lvUnique,
            List<TreeNode> listRootNodes,
            UList<TreeNode> listTreeNodes,
            bool bCheckboxes,
            List<ListViewItem> list_lvIgnore,
            bool bLoose)
        {
            gd = gd_in;
            static_this = this;
            m_dictNodes = dictNodes;
            m_treeViewBrowse = treeViewBrowse;
            form_lvClones = lvClones;
            form_lvSameVol = lvSameVol;
            form_lvUnique = lvUnique;
            m_listRootNodes = listRootNodes;
            m_listTreeNodes = listTreeNodes;
            m_bCheckboxes = bCheckboxes;
            m_list_lvIgnore = list_lvIgnore;
            m_bLoose = bLoose;
        }

        internal static void ClearMem()
        {
            Abort();
            static_this = null;
        }

        internal static void Abort()
        {
            if (static_this != null)
            {
                static_this.m_bThreadAbort = true;
            }
        }

        internal static void InsertSizeMarkers(List<ListViewItem> listLVitems)
        {
            if (listLVitems.IsEmpty())
            {
                return;
            }

            bool bUnique = (listLVitems[0].Tag is TreeNode);
            int nCount = listLVitems.Count;
            int nInterval = (nCount < 100) ? 10 : (nCount < 1000) ? 25 : 50;

            InsertSizeMarkerStatic.Go(listLVitems, nCount - 1, bUnique, bAdd: true);

            int nInitial = nCount % nInterval;

            if (nInitial == 0)
            {
                nInitial = nInterval;
            }

            if (nCount - nInitial > nInterval / 2)
            {
                for (int i = nCount - nInitial; i > nInterval / 2; i -= nInterval)
                {
                    InsertSizeMarkerStatic.Go(listLVitems, i, bUnique);
                }
            }

            InsertSizeMarkerStatic.Go(listLVitems, 0, bUnique);            // Enter the Zeroth
        }

        internal void Step1_OnThread()
        {
            SDL_TreeView treeView = new SDL_TreeView();     // sets Level and NextNode

            if (m_listRootNodes.IsEmpty())
            {
                MBoxStatic.Assert(1305.6314, false);
                return;
            }

            if (m_listRootNodes[0].TreeView == null)
            {
                treeView.Nodes.AddRange(m_listRootNodes.ToArray());
            }

            if (false == m_list_lvIgnore.IsEmpty())
            {
                DateTime dtStart = DateTime.Now;
                int nMaxLevel = m_list_lvIgnore.Max(i => int.Parse(i.SubItems[1].Text) - 1);
                StringBuilder sbMatch = new StringBuilder();

                foreach (ListViewItem lvItem in m_list_lvIgnore)
                {
                    sbMatch.AppendLine(lvItem.Text);
                }

                IgnoreNodeQuery(sbMatch.ToString().ToLower(), nMaxLevel, m_listRootNodes[0]);
                UtilProject.WriteLine("IgnoreNode " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            }

            Dictionary<TreeNode, ListViewItem> dictIgnoreMark = new Dictionary<TreeNode, ListViewItem>();
            SortedDictionary<FolderKeyTuple, List<TreeNode>> dictNodes = new SortedDictionary<FolderKeyTuple, List<TreeNode>>();

            foreach (KeyValuePair<FolderKeyTuple, UList<TreeNode>> pair in m_dictNodes)  // clone to remove ignored
            {                                                                       // m_ vs local check is via List vs UList
                dictNodes.Add(pair.Key, pair.Value.ToList());                       // clone pair.Value to remove ignored, using ToList() 
            }

            foreach (KeyValuePair<TreeNode, ListViewItem> pair in dictIgnoreNodes)
            {
                TreeNode treeNode = pair.Key;
                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
                List<TreeNode> lsTreeNodes = null;

                if (false == dictNodes.TryGetValue(nodeDatum.Key, out lsTreeNodes))
                {
                    continue;
                }

                if (m_bLoose)
                {
                    foreach (TreeNode treeNode_A in lsTreeNodes)
                    {
                        dictIgnoreMark.Add(treeNode_A, pair.Value);
                    }

                    dictNodes.Remove(nodeDatum.Key);
                }
                else if (lsTreeNodes.Contains(treeNode))
                {
                    dictIgnoreMark.Add(treeNode, pair.Value);
                    lsTreeNodes.Remove(treeNode);

                    if (lsTreeNodes.IsEmpty())
                    {
                        dictNodes.Remove(nodeDatum.Key);
                    }
                }
            }

            SortedDictionary<FolderKeyTuple, TreeNode> dictUnique = new SortedDictionary<FolderKeyTuple, TreeNode>();

            foreach (KeyValuePair<FolderKeyTuple, List<TreeNode>> pair in dictNodes)
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                var listNodes = pair.Value;

                if (listNodes.Count < 1)
                {
                    MBoxStatic.Assert(1305.6315, false);
                    continue;
                }
                
                if (listNodes.Count > 1)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    UList<TreeNode> listKeep = new UList<TreeNode>();

                    foreach (TreeNode treeNode_A in listNodes)
                    {
                        if (m_bThreadAbort || gd.WindowClosed)
                        {
                            return;
                        }

                        NodeDatum nodeDatum = ((NodeDatum)treeNode_A.Tag);

                        MBoxStatic.Assert(1305.6316, nodeDatum.TotalLength > 100 * 1024);

                        if (listNodes.Contains(treeNode_A.Parent) == false)
                        {
                            listKeep.Add(treeNode_A);
                        }
                    }

                    if (listKeep.Count > 1)
                    {
                        foreach (TreeNode treeNode_A in listKeep)
                        {
                            ((NodeDatum)treeNode_A.Tag).Clones = listKeep;
                        }
                    }
                    else
                    {
                        listNodes = listKeep.ToList();  // kick off "else" logic below after deleting child clones
                    }
                }

                if (listNodes.Count == 1)               // "else"
                {
                    TreeNode treeNode = listNodes[0];

                    if (((NodeDatum)treeNode.Tag).ImmediateFiles > 0)
                    {
                        dictUnique.Add(pair.Key, treeNode);
                    }
                }
            }

            SortedDictionary<FolderKeyTuple, UList<TreeNode>> dictClones = new SortedDictionary<FolderKeyTuple, UList<TreeNode>>();

            foreach (TreeNode treeNode in m_listRootNodes)
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                DifferentVolsQuery(dictClones, treeNode);
            }

            m_listRootNodes.Sort((x, y) => string.Compare(x.Text, y.Text));

            foreach (KeyValuePair<FolderKeyTuple, UList<TreeNode>> listNodes in dictClones)
            {
                // load up listLVdiffVol

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                int nClones = listNodes.Value.Count;

                if (nClones == 0)
                {
                    MBoxStatic.Assert(1305.6317, false);
                    continue;
                }

                if (nClones == 1)
                {
                    continue;       // keep the same-vol
                }

                string str_nClones = null;

                if (nClones > 2)        // includes the subject node: only note three clones or more
                {
                    str_nClones = nClones.ToString("###,###");

                    foreach (TreeNode node in listNodes.Value)
                    {
                        node.ForeColor = Color.Blue;
                    }
                }

                ListViewItem lvItem = new ListViewItem(new string[] { string.Empty, str_nClones });

                lvItem.Tag = listNodes.Value;
                lvItem.ForeColor = listNodes.Value[0].ForeColor;

                TreeNode nameNode = null;
                int nLevel = int.MaxValue;

                foreach (TreeNode treeNode in listNodes.Value)
                {
                    if (m_bThreadAbort || gd.WindowClosed)
                    {
                        return;
                    }

                    if (treeNode.Level < nLevel)
                    {
                        nLevel = treeNode.Level;
                        nameNode = treeNode;
                    }

                    ((NodeDatum)treeNode.Tag).LVitem = lvItem;
                }

                lvItem.Text = nameNode.Text;
                MBoxStatic.Assert(1305.6318, false == string.IsNullOrWhiteSpace(lvItem.Text));
                listLVdiffVol.Add(lvItem);
            }

            foreach (KeyValuePair<TreeNode, ListViewItem> pair in dictIgnoreMark)
            {
                TreeNode treeNode = pair.Key;
                ListViewItem lvIgnoreItem = pair.Value;

                treeNode.ForeColor = Color.DarkGray;
                treeNode.BackColor = Color.Empty;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                nodeDatum.LVitem = lvIgnoreItem;
                MBoxStatic.Assert(1305.6319, nodeDatum.LVitem != null);
                nodeDatum.Clones.Remove(treeNode);
            }

            dictClones = null;
            InsertSizeMarkers(listLVdiffVol);

            foreach (KeyValuePair<FolderKeyTuple, TreeNode> listNodes in dictUnique)
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                TreeNode treeNode = listNodes.Value;

                MBoxStatic.Assert(1305.6321, false == string.IsNullOrWhiteSpace(treeNode.Text));

                ListViewItem lvItem = new ListViewItem(treeNode.Text);

                lvItem.Tag = treeNode;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                MBoxStatic.Assert(1305.6322, nodeDatum.ImmediateFiles > 0);
                SnowUniqueParents(treeNode);

                if (treeNode.ForeColor != Color.DarkOrange)
                {
                    MBoxStatic.Assert(1305.6323, treeNode.ForeColor == Color.Empty);
                    treeNode.ForeColor = Color.Red;
                }

                lvItem.ForeColor = treeNode.ForeColor;
                listLVunique.Add(lvItem);
                MBoxStatic.Assert(1305.6324, nodeDatum.LVitem == null);
                nodeDatum.LVitem = lvItem;
            }

            dictUnique = null;
            InsertSizeMarkers(listLVunique);

            List<TreeNode> listSameVol = new List<TreeNode>();

            if (false == m_listRootNodes.IsEmpty())
            {
                int nCount = CountNodes.Go(m_listRootNodes);
                int nCount_A = new AddTreeToList(m_listTreeNodes, listSameVol).Go(m_listRootNodes).Count;

                MBoxStatic.Assert(1305.6325, nCount_A == nCount);
                MBoxStatic.Assert(1305.6326, m_listTreeNodes.Count == nCount);
                MBoxStatic.Assert(1305.6327, CountNodes.Go(m_listRootNodes) == nCount);
                UtilProject.WriteLine("Step1_OnThread " + nCount);
            }

            listSameVol.Sort((y, x) => ((NodeDatum)x.Tag).TotalLength.CompareTo(((NodeDatum)y.Tag).TotalLength));

            foreach (TreeNode treeNode in listSameVol)
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                SnowUniqueParents(treeNode);

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
                int nClones = 0;

                if (null != nodeDatum.Clones)
                    nClones = nodeDatum.Clones.Count;

                if (nClones == 0)
                {
                    MBoxStatic.Assert(1305.6328, false);
                }

                string str_nClones = null;

                if (nClones > 2)
                {
                    str_nClones = nClones.ToString("###,###");
                }

                MBoxStatic.Assert(1305.6329, false == string.IsNullOrWhiteSpace(treeNode.Text));

                ListViewItem lvItem = new ListViewItem(new string[] { treeNode.Text, str_nClones });

                lvItem.Tag = nodeDatum.Clones;
                lvItem.ForeColor = Color.Firebrick;
                lvItem.BackColor = treeNode.BackColor;
                listLVsameVol.Add(lvItem);
                nodeDatum.LVitem = lvItem;
            }

            listSameVol = null;
            InsertSizeMarkers(listLVsameVol);
            treeView.Nodes.Clear();                             // prevents destroy nodes
        }

        internal void Step2_OnForm()
        {
            UtilAnalysis_DirList.Closure(() =>
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                    m_treeViewBrowse.Nodes.Clear();

                if (m_listRootNodes.IsEmpty())
                {
                    return;
                }

                m_treeViewBrowse.Enabled = true;
                m_treeViewBrowse.CheckBoxes = m_bCheckboxes;

                int nCount = CountNodes.Go(m_listRootNodes);

                UtilAnalysis_DirList.Write("A");
                m_treeViewBrowse.Nodes.AddRange(m_listRootNodes.ToArray());
                UtilProject.WriteLine("A");

                int nCount_A = CountNodes.Go(m_listRootNodes);

                MBoxStatic.Assert(1305.6331, nCount_A == nCount);
                MBoxStatic.Assert(1305.6332, m_treeViewBrowse.GetNodeCount(includeSubTrees: true) == nCount);
                UtilProject.WriteLine("Step2_OnForm_A " + nCount);

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                MBoxStatic.Assert(1305.6333, form_lvClones.Items.IsEmpty());
                UtilAnalysis_DirList.Write("B");
                form_lvClones.Items.AddRange(listLVdiffVol.ToArray());
                form_lvClones.Invalidate();
                UtilProject.WriteLine("B");

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                MBoxStatic.Assert(1305.6334, form_lvUnique.Items.IsEmpty());
                UtilAnalysis_DirList.Write("C");
                form_lvUnique.Items.AddRange(listLVunique.ToArray());
                form_lvUnique.Invalidate();
                UtilProject.WriteLine("C");

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                MBoxStatic.Assert(1305.6335, form_lvSameVol.Items.IsEmpty());
                UtilAnalysis_DirList.Write("D");
                form_lvSameVol.Items.AddRange(listLVsameVol.ToArray());
                form_lvSameVol.Invalidate();
                UtilProject.WriteLine("D");

                if (m_treeViewBrowse.SelectedNode != null)      // gd.m_bPutPathInFindEditBox is set in TreeDoneCallback()
                {
                    TreeNode treeNode = (TreeNode)m_treeViewBrowse.SelectedNode;

                    m_treeViewBrowse.SelectedNode = null;
                    m_treeViewBrowse.SelectedNode = treeNode;   // reselect in repopulated collation listviewers
                }
                else
                {
                    m_treeViewBrowse.SelectedNode = m_listRootNodes[0];
                }
            });

            static_this = null;
        }

        // If an outer directory is cloned then all the inner ones are part of the outer clone and their clone status is redundant.
        // Breadth-first.
        void DifferentVolsQuery(SortedDictionary<FolderKeyTuple, UList<TreeNode>> dictClones, TreeNode treeNode, TreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used as a bool).
            // provisional.

            NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
            UList<TreeNode> listClones = nodeDatum.Clones;
            ulong nLength = nodeDatum.TotalLength;

            if (nLength <= 100 * 1024)
            {
                treeNode.ForeColor = Color.LightGray;
                nodeDatum.Clones.Clear();
            }

            if ((false == listClones.IsEmpty()) && (rootClone == null))
            {
                rootClone = treeNode;

                UList<TreeNode> lsTreeNodes = null;

                if (dictClones.TryGetValue(nodeDatum.Key, out lsTreeNodes))
                {
                    MBoxStatic.Assert(1305.6305, lsTreeNodes == listClones);
                    MBoxStatic.Assert(1305.6306, ((NodeDatum)lsTreeNodes[0].Tag).SeparateVols == nodeDatum.SeparateVols);
                    MBoxStatic.Assert(1305.6307, lsTreeNodes[0].ForeColor == treeNode.ForeColor);
                }
                else
                {
                    dictClones.Add(nodeDatum.Key, listClones);

                    // Test to see if clones are on separate volumes.

                    TreeNode rootNode = treeNode.Root();
                    RootNodeDatum rootNodeDatum = (RootNodeDatum)rootNode.Tag;

                    MBoxStatic.Assert(1305.6308, Color.Empty == treeNode.ForeColor);
                    treeNode.ForeColor = Color.Firebrick;

                    bool bDifferentVols = false;

                    foreach (TreeNode subnode in listClones)
                    {
                        if (m_bThreadAbort || gd.WindowClosed)
                        {
                            return;
                        }

                        MBoxStatic.Assert(1305.6309, ((NodeDatum)subnode.Tag).Key.Equals(nodeDatum.Key));

                        TreeNode rootNode_A = subnode.Root();

                        if (rootNode == rootNode_A)
                        {
                            continue;
                        }

                        RootNodeDatum rootNodeDatum_A = (RootNodeDatum)rootNode_A.Tag;

                        if (false == string.IsNullOrWhiteSpace(rootNodeDatum.VolumeGroup) &&
                            (rootNodeDatum.VolumeGroup == rootNodeDatum_A.VolumeGroup))
                        {
                            continue;
                        }

                        MBoxStatic.Assert(1305.6311, treeNode.ForeColor == Color.Firebrick);
                        treeNode.ForeColor = Color.SteelBlue;

                        bDifferentVols = true;
                        break;
                    }

                    foreach (TreeNode subNode in listClones)
                    {
                        ((NodeDatum)subNode.Tag).SeparateVols = bDifferentVols;
                        subNode.ForeColor = treeNode.ForeColor;
                    }
                }
            }

            foreach (TreeNode subNode in treeNode.Nodes)
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                DifferentVolsQuery(dictClones, subNode, rootClone);
            }
        }

        void IgnoreNodeAndSubnodes(ListViewItem lvItem, TreeNode treeNode_in, bool bContinue = false)
        {
            TreeNode treeNode = treeNode_in;

            do
            {
                if (dictIgnoreNodes.ContainsKeyA(treeNode))
                {
                    continue;
                }

                MBoxStatic.Assert(1305.6312, lvItem != null);
                dictIgnoreNodes.Add(treeNode, lvItem);

                if (false == treeNode.Nodes.IsEmpty())
                {
                    IgnoreNodeAndSubnodes(lvItem, (TreeNode)treeNode.Nodes[0], bContinue: true);
                }
            }
            while (bContinue && ((treeNode = (TreeNode)treeNode.NextNode) != null));
        }

        void IgnoreNodeQuery(string sbMatch, int nMaxLevel, TreeNode treeNode_in)
        {
            if (treeNode_in.Level > nMaxLevel)
            {
                return;
            }

            TreeNode treeNode = treeNode_in;

            do
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                if (sbMatch.Contains(treeNode.Text.ToLower()))
                {
                    foreach (ListViewItem lvItem in m_list_lvIgnore)
                    {
                        if (treeNode.Level != (int.Parse(lvItem.SubItems[1].Text) - 1))
                        {
                            continue;
                        }

                        if (lvItem.Text.ToLower() == treeNode.Text.ToLower())
                        {
                            IgnoreNodeAndSubnodes((ListViewItem)lvItem.Tag, treeNode);
                            break;
                        }
                    }
                }

                if (false == treeNode.Nodes.IsEmpty())
                {
                    IgnoreNodeQuery(sbMatch, nMaxLevel, (TreeNode)treeNode.Nodes[0]);
                }
            }
            while ((treeNode = (TreeNode)treeNode.NextNode) != null);
        }

        static void SnowUniqueParents(TreeNode treeNode)
        {
            TreeNode parentNode = (TreeNode)treeNode.Parent;

            while (parentNode != null)
            {
                parentNode.BackColor = Color.Snow;

                NodeDatum nodeDatum = (NodeDatum)parentNode.Tag;

                if (nodeDatum.LVitem != null)
                {
                    nodeDatum.LVitem.BackColor = parentNode.BackColor;
                }

                if (parentNode.ForeColor != Color.DarkOrange)
                {
                    MBoxStatic.Assert(1305.6313, (parentNode.ForeColor == Color.Empty) == (nodeDatum.LVitem == null));
                }

                parentNode = (TreeNode)parentNode.Parent;
            }
        }

        // the following are form vars referenced internally, thus keeping their form_ and m_ prefixes
        readonly SortedDictionary<FolderKeyTuple, UList<TreeNode>> m_dictNodes = null;
        readonly TreeView m_treeViewBrowse = null;
        readonly SDL_ListView form_lvClones = null;
        readonly SDL_ListView form_lvSameVol = null;
        readonly SDL_ListView form_lvUnique = null;
        readonly List<TreeNode> m_listRootNodes = null;
        readonly UList<TreeNode> m_listTreeNodes = null;
        readonly bool m_bCheckboxes = false;
        readonly List<ListViewItem> m_list_lvIgnore = null;

        // the following are "local" to this object, and do not have m_ prefixes because they do not belong to the form.
        readonly List<ListViewItem> listLVunique = new List<ListViewItem>();
        readonly List<ListViewItem> listLVsameVol = new List<ListViewItem>();
        readonly List<ListViewItem> listLVdiffVol = new List<ListViewItem>();
        readonly Dictionary<TreeNode, ListViewItem> dictIgnoreNodes = new Dictionary<TreeNode, ListViewItem>();
        readonly bool m_bLoose = false;

        bool m_bThreadAbort = false;
        static Collate static_this = null;
        readonly GlobalData_Base gd = null;
    }
}
