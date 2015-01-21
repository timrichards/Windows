using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DoubleFile;

namespace SearchDirLists
{
    class Collate : Utilities
    {
        static Collate static_this = null;

        public static void ClearMem()
        {
            Utilities.Assert(1305.6301, static_this == null);
            static_this = null;
        }

        // the following are form vars referenced internally, thus keeping their form_ and m_ prefixes
        readonly SortedDictionary<Correlate, UList<TreeNode>> m_dictNodes = null;
        readonly TreeView m_treeViewBrowse = null;
        readonly SDL_ListView form_lvClones = null;
        readonly SDL_ListView form_lvSameVol = null;
        readonly SDL_ListView form_lvUnique = null;
        readonly List<TreeNode> m_listRootNodes = null;
        readonly UList<TreeNode> m_listTreeNodes = null;
        readonly bool m_bCheckboxes = false;
        readonly List<SDL_ListViewItem> m_list_lvIgnore = null;

        // the following are "local" to this object, and do not have m_ prefixes because they do not belong to the form.
        readonly List<SDL_ListViewItem> listLVunique = new List<SDL_ListViewItem>();
        readonly List<SDL_ListViewItem> listLVsameVol = new List<SDL_ListViewItem>();
        readonly List<SDL_ListViewItem> listLVdiffVol = new List<SDL_ListViewItem>();
        readonly Dictionary<TreeNode, SDL_ListViewItem> dictIgnoreNodes = new Dictionary<TreeNode, SDL_ListViewItem>();
        readonly bool m_bLoose = false;
        bool m_bThreadAbort = false;

        public static void Abort()
        {
            if (static_this != null)
            {
                static_this.m_bThreadAbort = true;
            }
        }

        class AddTreeToList
        {
            UList<TreeNode> m_listTreeNodes = null;
            List<TreeNode> m_listSameVol = null;

            int m_nCount = 0;
            internal int Count { get { return m_nCount; } }

            public AddTreeToList(UList<TreeNode> listTreeNodes, List<TreeNode> listSameVol)
            {
                m_listTreeNodes = listTreeNodes;
                m_listSameVol = listSameVol;
            }

            public AddTreeToList Go(List<TreeNode> listNodes)
            {
                foreach (TreeNode treeNode in listNodes)
                {
                    Go(treeNode, bNextNode: false);
                }

                return this;
            }

            void Go(TreeNode treeNode_in, bool bCloneOK = false, bool bNextNode = true)
            {
                if (treeNode_in == null)
                {
                    Utilities.Assert(1305.6302, false);
                }

                TreeNode treeNode = treeNode_in;

                do
                {
                    m_listTreeNodes.Add(treeNode);
                    ++m_nCount;

                    NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                    if (nodeDatum == null)
                    {
                        Utilities.Assert(1305.6303, false);
                        continue;
                    }

                    if ((treeNode.ForeColor == Color.Firebrick) && (treeNode == nodeDatum.m_listClones[0]))
                    {
                        Utilities.Assert(1305.6304, (nodeDatum.m_listClones.Count > 0) && (nodeDatum.m_bDifferentVols == false));
                        m_listSameVol.Add(treeNode);
                    }

                    if (bCloneOK)
                    {
                        treeNode.BackColor = Color.LightGoldenrodYellow;

                        if ((nodeDatum.m_lvItem != null) && (nodeDatum.m_lvItem.ListView == null))  // ignore LV
                        {
                            nodeDatum.m_lvItem.BackColor = treeNode.BackColor;
                        }
                    }

                    if (treeNode.FirstNode != null)
                    {
                        Go((TreeNode)treeNode.FirstNode, bCloneOK || (new Color[] { Color.SteelBlue, Color.DarkBlue }.Contains(treeNode.ForeColor)));
                    }
                }
                while (bNextNode && ((treeNode = (TreeNode)treeNode.NextNode) != null));
            }
        }

        class InsertSizeMarker
        {
            readonly static SDL_ListViewItem lvMarker = new SDL_ListViewItem();
            static bool bInit = false;

            static void Init()
            {
                if (bInit == false)
                {
                    lvMarker.BackColor = Color.DarkSlateGray;
                    lvMarker.ForeColor = Color.White;
                    lvMarker.Font = new Font(lvMarker.Font, FontStyle.Bold);
                    lvMarker.Tag = null;
                    bInit = true;
                }
            }

            public static void Go(List<SDL_ListViewItem> listLVitems, int nIx, bool bUnique, bool bAdd = false)
            {
                Init();

                SDL_ListViewItem lvItem = (SDL_ListViewItem)lvMarker.Clone();

                lvItem.Text = ((Utilities.FormatSize(((NodeDatum)((TreeNode)(bUnique ? listLVitems[nIx].Tag : ((UList<TreeNode>)listLVitems[nIx].Tag)[0])).Tag).nTotalLength, bNoDecimal: true)));

                if (bAdd)
                {
                    listLVitems.Add(lvItem);
                }
                else
                {
                    listLVitems.Insert(nIx, lvItem);
                }
            }
        }

        public Collate(SortedDictionary<Correlate, UList<TreeNode>> dictNodes,
            TreeView treeViewBrowse,
            SDL_ListView lvClones, SDL_ListView lvSameVol, SDL_ListView lvUnique,
            List<TreeNode> listRootNodes, UList<TreeNode> listTreeNodes, bool bCheckboxes,
            List<SDL_ListViewItem> list_lvIgnore, bool bLoose)
        {
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

        // If an outer directory is cloned then all the inner ones are part of the outer clone and their clone status is redundant.
        // Breadth-first.
        void DifferentVolsQuery(SortedDictionary<Correlate, UList<TreeNode>> dictClones, TreeNode treeNode, TreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used as a bool).
            // provisional.

            NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
            UList<TreeNode> listClones = nodeDatum.m_listClones;
            ulong nLength = nodeDatum.nTotalLength;

            if (nLength <= 100 * 1024)
            {
                treeNode.ForeColor = Color.LightGray;
                nodeDatum.m_listClones.Clear();
            }

            if ((listClones.Count > 0) && (rootClone == null))
            {
                rootClone = treeNode;

                if (dictClones.ContainsKey(nodeDatum.Key))
                {
                    Utilities.Assert(1305.6305, dictClones[nodeDatum.Key] == listClones);
                    Utilities.Assert(1305.6306, ((NodeDatum)dictClones[nodeDatum.Key][0].Tag).m_bDifferentVols == nodeDatum.m_bDifferentVols);
                    Utilities.Assert(1305.6307, dictClones[nodeDatum.Key][0].ForeColor == treeNode.ForeColor);
                }
                else
                {
                    dictClones.Add(nodeDatum.Key, listClones);

                    // Test to see if clones are on separate volumes.

                    TreeNode rootNode = treeNode.Root();
                    RootNodeDatum rootNodeDatum = (RootNodeDatum)rootNode.Tag;

                    Utilities.Assert(1305.6308, new Color[] { Color.Empty, Color.DarkBlue }.Contains(treeNode.ForeColor));
                    treeNode.ForeColor = Color.Firebrick;

                    bool bDifferentVols = false;

                    foreach (TreeNode subnode in listClones)
                    {
                        if (m_bThreadAbort || GlobalData.AppExit)
                        {
                            return;
                        }

                        Utilities.Assert(1305.6309, ((NodeDatum)subnode.Tag).Key == nodeDatum.Key);

                        TreeNode rootNode_A = subnode.Root();

                        if (rootNode == rootNode_A)
                        {
                            continue;
                        }

                        RootNodeDatum rootNodeDatum_A = (RootNodeDatum)rootNode_A.Tag;

                        if (false == string.IsNullOrWhiteSpace(rootNodeDatum.StrVolumeGroup) &&
                            (rootNodeDatum.StrVolumeGroup == rootNodeDatum_A.StrVolumeGroup))
                        {
                            continue;
                        }

                        if (treeNode.ForeColor != Color.DarkBlue)
                        {
                            Utilities.Assert(1305.6311, treeNode.ForeColor == Color.Firebrick);
                            treeNode.ForeColor = Color.SteelBlue;
                        }

                        bDifferentVols = true;
                        break;
                    }

                    foreach (TreeNode subNode in listClones)
                    {
                        ((NodeDatum)subNode.Tag).m_bDifferentVols = bDifferentVols;
                        subNode.ForeColor = treeNode.ForeColor;
                    }
                }
            }

            foreach (TreeNode subNode in treeNode.Nodes)
            {
                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                DifferentVolsQuery(dictClones, subNode, rootClone);
            }
        }

        public static void InsertSizeMarkers(List<SDL_ListViewItem> listLVitems)
        {
            if (listLVitems.Count <= 0)
            {
                return;
            }

            bool bUnique = (listLVitems[0].Tag is TreeNode);
            int nCount = listLVitems.Count;
            int nInterval = (nCount < 100) ? 10 : (nCount < 1000) ? 25 : 50;

            InsertSizeMarker.Go(listLVitems, nCount - 1, bUnique, bAdd: true);

            int nInitial = nCount % nInterval;

            if (nInitial <= 0)
            {
                nInitial = nInterval;
            }

            if (nCount - nInitial > nInterval / 2)
            {
                for (int i = nCount - nInitial; i > nInterval / 2; i -= nInterval)
                {
                    InsertSizeMarker.Go(listLVitems, i, bUnique);
                }
            }

            InsertSizeMarker.Go(listLVitems, 0, bUnique);            // Enter the Zeroth
        }

        void IgnoreNodeAndSubnodes(SDL_ListViewItem lvItem, TreeNode treeNode_in, bool bContinue = false)
        {
            TreeNode treeNode = treeNode_in;

            do
            {
                if (dictIgnoreNodes.ContainsKey(treeNode))
                {
                    continue;
                }

                Utilities.Assert(1305.6312, lvItem != null);
                dictIgnoreNodes.Add(treeNode, lvItem);

                if (treeNode.Nodes.Count > 0)
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
                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;     
                }

                if (sbMatch.Contains(treeNode.Text.ToLower()))
                {
                    foreach (SDL_ListViewItem lvItem in m_list_lvIgnore)
                    {
                        if (treeNode.Level != (int.Parse(lvItem.SubItems[1].Text) - 1))
                        {
                            continue;
                        }

                        if (lvItem.Text.ToLower() == treeNode.Text.ToLower())
                        {
                            IgnoreNodeAndSubnodes((SDL_ListViewItem)lvItem.Tag, treeNode);
                            break;
                        }
                    }
                }

                if (treeNode.Nodes.Count > 0)
                {
                    IgnoreNodeQuery(sbMatch, nMaxLevel, (TreeNode)treeNode.Nodes[0]);
                }
            }
            while ((treeNode = (TreeNode)treeNode.NextNode) != null);
        }

        void SnowUniqueParents(TreeNode treeNode)
        {
            TreeNode parentNode = (TreeNode)treeNode.Parent;

            while (parentNode != null)
            {
                parentNode.BackColor = Color.Snow;

                NodeDatum nodeDatum = (NodeDatum)parentNode.Tag;

                if (nodeDatum.m_lvItem != null)
                {
                    nodeDatum.m_lvItem.BackColor = parentNode.BackColor;
                }

                if (parentNode.ForeColor != Color.DarkOrange)
                {
                    Utilities.Assert(1305.6313, (parentNode.ForeColor == Color.Empty) == (nodeDatum.m_lvItem == null));
                }

                parentNode = (TreeNode)parentNode.Parent;
            }
        }

        public void Step1_OnThread()
        {
            SDL_TreeView treeView = new SDL_TreeView();     // sets Level and NextNode

            if (m_listRootNodes.Count <= 0)
            {
                Utilities.Assert(1305.6314, false);
                return;
            }

            if (m_listRootNodes[0].TreeView == null)
            {
                treeView.Nodes.AddRange(m_listRootNodes.ToArray());
            }

            if (m_list_lvIgnore.Count > 0)
            {
                DateTime dtStart = DateTime.Now;
                int nMaxLevel = m_list_lvIgnore.Max(i => int.Parse(i.SubItems[1].Text) - 1);
                StringBuilder sbMatch = new StringBuilder();

                foreach (SDL_ListViewItem lvItem in m_list_lvIgnore)
                {
                    sbMatch.AppendLine(lvItem.Text);
                }

                IgnoreNodeQuery(sbMatch.ToString().ToLower(), nMaxLevel, m_listRootNodes[0]);
                Utilities.WriteLine("IgnoreNode " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            }

            Dictionary<TreeNode, SDL_ListViewItem> dictIgnoreMark = new Dictionary<TreeNode, SDL_ListViewItem>();
            SortedDictionary<Correlate, List<TreeNode>> dictNodes = new SortedDictionary<Correlate, List<TreeNode>>();

            foreach (KeyValuePair<Correlate, UList<TreeNode>> pair in m_dictNodes)  // clone to remove ignored
            {                                                                       // m_ vs local check is via List vs UList
                dictNodes.Add(pair.Key, pair.Value.ToList());                       // clone pair.Value to remove ignored, using ToList() 
            }

            foreach (KeyValuePair<TreeNode, SDL_ListViewItem> pair in dictIgnoreNodes)
            {
                TreeNode treeNode = pair.Key;
                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                if (dictNodes.ContainsKey(nodeDatum.Key) == false)
                {
                    continue;
                }

                if (m_bLoose)
                {
                    foreach (TreeNode treeNode_A in dictNodes[nodeDatum.Key])
                    {
                        dictIgnoreMark.Add(treeNode_A, pair.Value);
                    }

                    dictNodes.Remove(nodeDatum.Key);
                }
                else if (dictNodes[nodeDatum.Key].Contains(treeNode))
                {
                    dictIgnoreMark.Add(treeNode, pair.Value);
                    dictNodes[nodeDatum.Key].Remove(treeNode);

                    if (dictNodes[nodeDatum.Key].Count <= 0)
                    {
                        dictNodes.Remove(nodeDatum.Key);
                    }
                }
            }

            SortedDictionary<Correlate, TreeNode> dictUnique = new SortedDictionary<Correlate, TreeNode>();

            foreach (KeyValuePair<Correlate, List<TreeNode>> pair in dictNodes)
            {
                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                List<TreeNode> listNodes = (List<TreeNode>)pair.Value;

                if (listNodes.Count < 1)
                {
                    Utilities.Assert(1305.6315, false);
                    continue;
                }
                
                if (listNodes.Count > 1)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    UList<TreeNode> listKeep = new UList<TreeNode>();

                    foreach (TreeNode treeNode_A in listNodes)
                    {
                        if (m_bThreadAbort || GlobalData.AppExit)
                        {
                            return;
                        }

                        NodeDatum nodeDatum = ((NodeDatum)treeNode_A.Tag);

                        Utilities.Assert(1305.6316, nodeDatum.nTotalLength > 100 * 1024);

                        if (listNodes.Contains(treeNode_A.Parent) == false)
                        {
                            listKeep.Add(treeNode_A);
                        }
                    }

                    if (listKeep.Count > 1)
                    {
                        foreach (TreeNode treeNode_A in listKeep)
                        {
                            ((NodeDatum)treeNode_A.Tag).m_listClones = listKeep;
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

                    if (((NodeDatum)treeNode.Tag).nImmediateFiles > 0)
                    {
                        dictUnique.Add(pair.Key, treeNode);
                    }
                }
            }

            SortedDictionary<Correlate, UList<TreeNode>> dictClones = new SortedDictionary<Correlate, UList<TreeNode>>();

            foreach (TreeNode treeNode in m_listRootNodes)
            {
                DifferentVolsQuery(dictClones, treeNode);
            }

            m_listRootNodes.Sort((x, y) => string.Compare(x.Text, y.Text));

            foreach (KeyValuePair<Correlate, UList<TreeNode>> listNodes in dictClones)
            {
                // load up listLVdiffVol

                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                int nClones = listNodes.Value.Count;

                if (nClones <= 0)
                {
                    Utilities.Assert(1305.6317, false);
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

                SDL_ListViewItem lvItem = new SDL_ListViewItem(new string[] { string.Empty, str_nClones });

                lvItem.Tag = listNodes.Value;
                lvItem.ForeColor = listNodes.Value[0].ForeColor;

                TreeNode nameNode = null;
                int nLevel = int.MaxValue;

                foreach (TreeNode treeNode in listNodes.Value)
                {
                    if (m_bThreadAbort || GlobalData.AppExit)
                    {
                        return;
                    }

                    TreeNode parentNode = (TreeNode)treeNode.Parent;

                    if (treeNode.Level < nLevel)
                    {
                        nLevel = treeNode.Level;
                        nameNode = treeNode;
                    }

                    ((NodeDatum)treeNode.Tag).m_lvItem = lvItem;
                }

                lvItem.Text = nameNode.Text;
                Utilities.Assert(1305.6318, false == string.IsNullOrWhiteSpace(lvItem.Text));
                listLVdiffVol.Add(lvItem);
            }

            foreach (KeyValuePair<TreeNode, SDL_ListViewItem> pair in dictIgnoreMark)
            {
                TreeNode treeNode = pair.Key;
                SDL_ListViewItem lvIgnoreItem = pair.Value;

                treeNode.ForeColor = Color.DarkGray;
                treeNode.BackColor = Color.Empty;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                nodeDatum.m_lvItem = lvIgnoreItem;
                Utilities.Assert(1305.6319, nodeDatum.m_lvItem != null);
                nodeDatum.m_listClones.Remove(treeNode);
            }

            dictClones = null;
            InsertSizeMarkers(listLVdiffVol);

            foreach (KeyValuePair<Correlate, TreeNode> listNodes in dictUnique)
            {
                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                TreeNode treeNode = listNodes.Value;

                Utilities.Assert(1305.6321, false == string.IsNullOrWhiteSpace(treeNode.Text));

                SDL_ListViewItem lvItem = new SDL_ListViewItem(treeNode.Text);

                lvItem.Tag = treeNode;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                Utilities.Assert(1305.6322, nodeDatum.nImmediateFiles > 0);
                SnowUniqueParents(treeNode);

                if (treeNode.ForeColor != Color.DarkOrange)
                {
                    Utilities.Assert(1305.6323, treeNode.ForeColor == Color.Empty);
                    treeNode.ForeColor = Color.Red;
                }

                lvItem.ForeColor = treeNode.ForeColor;
                listLVunique.Add(lvItem);
                Utilities.Assert(1305.6324, nodeDatum.m_lvItem == null);
                nodeDatum.m_lvItem = lvItem;
            }

            dictUnique = null;
            InsertSizeMarkers(listLVunique);

            List<TreeNode> listSameVol = new List<TreeNode>();

            if (m_listRootNodes.Count > 0)
            {
                int nCount = Utilities.CountNodes(m_listRootNodes);
                int nCount_A = new AddTreeToList(m_listTreeNodes, listSameVol).Go(m_listRootNodes).Count;

                Utilities.Assert(1305.6325, nCount_A == nCount);
                Utilities.Assert(1305.6326, m_listTreeNodes.Count == nCount);
                Utilities.Assert(1305.6327, Utilities.CountNodes(m_listRootNodes) == nCount);
                Utilities.WriteLine("Step1_OnThread " + nCount);
            }

            listSameVol.Sort((y, x) => ((NodeDatum)x.Tag).nTotalLength.CompareTo(((NodeDatum)y.Tag).nTotalLength));

            foreach (TreeNode treeNode in listSameVol)
            {
                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                SnowUniqueParents(treeNode);

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
                int nClones = nodeDatum.m_listClones.Count;

                if (nClones <= 0)
                {
                    Utilities.Assert(1305.6328, false);
                }

                string str_nClones = null;

                if (nClones > 2)
                {
                    str_nClones = nClones.ToString("###,###");
                }

                Utilities.Assert(1305.6329, false == string.IsNullOrWhiteSpace(treeNode.Text));

                SDL_ListViewItem lvItem = new SDL_ListViewItem(new string[] { treeNode.Text, str_nClones });

                lvItem.Tag = nodeDatum.m_listClones;
                lvItem.ForeColor = Color.Firebrick;
                lvItem.BackColor = treeNode.BackColor;
                listLVsameVol.Add(lvItem);
                nodeDatum.m_lvItem = lvItem;
            }

            listSameVol = null;
            InsertSizeMarkers(listLVsameVol);
            treeView.Nodes.Clear();                             // prevents destroy nodes
        }

        public void Step2_OnForm()
        {
            Utilities.Closure(new Action(() =>
            {
                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                if (m_treeViewBrowse.Enabled == false)      // stays enabled when DoCollation() is called directly
                {
                    m_treeViewBrowse.Nodes.Clear();
                }

                if (m_listRootNodes.Count <= 0)
                {
                    return;
                }

                if (m_treeViewBrowse.Enabled == false)
                {
                    m_treeViewBrowse.Enabled = true;
                    m_treeViewBrowse.CheckBoxes = m_bCheckboxes;

                    int nCount = Utilities.CountNodes(m_listRootNodes);

                    Utilities.Write("A");
                    m_treeViewBrowse.Nodes.AddRange(m_listRootNodes.ToArray());
                    Utilities.WriteLine("A");

                    int nCount_A = Utilities.CountNodes(m_listRootNodes);

                    Utilities.Assert(1305.6331, nCount_A == nCount);
                    Utilities.Assert(1305.6332, m_treeViewBrowse.GetNodeCount(includeSubTrees: true) == nCount);
                    Utilities.WriteLine("Step2_OnForm_A " + nCount);
                }

                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                Utilities.Assert(1305.6333, form_lvClones.Items.Count <= 0);
                Utilities.Write("B");
                form_lvClones.Items.AddRange(listLVdiffVol.ToArray());
                form_lvClones.Invalidate();
                Utilities.WriteLine("B");

                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                Utilities.Assert(1305.6334, form_lvUnique.Items.Count <= 0);
                Utilities.Write("C");
                form_lvUnique.Items.AddRange(listLVunique.ToArray());
                form_lvUnique.Invalidate();
                Utilities.WriteLine("C");

                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                Utilities.Assert(1305.6335, form_lvSameVol.Items.Count <= 0);
                Utilities.Write("D");
                form_lvSameVol.Items.AddRange(listLVsameVol.ToArray());
                form_lvSameVol.Invalidate();
                Utilities.WriteLine("D");

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
            }));

            static_this = null;
        }
    }
}
