using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace SearchDirLists
{
    class Correlate : Utilities
    {
        // the following are form vars referenced internally, thus keeping their form_ and m_ prefixes
        TreeView form_treeView_Browse = null;
        SortedDictionary<HashKey, List<TreeNode>> m_dictNodes = null;
        ListView form_lvClones = null;
        ListView form_lvSameVol = null;
        ListView form_lvUnique = null;
        List<TreeNode> m_listRootNodes = null;
        List<TreeNode> m_listTreeNodes = null;
        bool m_bCheckboxes = false;
        List<ListViewItem> m_list_lvIgnore = null;

        // the following are "local" to this object, and do not have m_ prefixes because they do not belong to the form.
        List<ListViewItem> listLVitems = new List<ListViewItem>();
        List<ListViewItem> listLVunique = new List<ListViewItem>();
        List<ListViewItem> listLVsameVol = new List<ListViewItem>();
        Dictionary<TreeNode, ListViewItem> dictIgnoreNodes = new Dictionary<TreeNode, ListViewItem>();
        bool m_bLoose = false;

        bool m_bThreadAbort = false;
        static Correlate static_this = null;

        public static void Abort()
        {
            if (static_this != null)
            {
                static_this.m_bThreadAbort = true;
            }
        }

        class AddTreeToList
        {
            List<TreeNode> m_listTreeNodes = null;
            List<TreeNode> m_listSameVol = null;

            public AddTreeToList(List<TreeNode> listTreeNodes, List<TreeNode> listSameVol)
            {
                m_listTreeNodes = listTreeNodes;
                m_listSameVol = listSameVol;
            }

            public void Go(TreeNode treeNode_in, bool bCloneOK = false)
            {
                TreeNode treeNode = treeNode_in;

                do
                {
                    NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                    if ((treeNode.ForeColor == Color.Firebrick) && (treeNode == nodeDatum.m_listClones[0]))
                    {
                        Utilities.Assert(1305.6319, (nodeDatum.m_listClones != null) && (nodeDatum.m_bDifferentVols == false));
                        m_listSameVol.Add(treeNode);
                    }

                    m_listTreeNodes.Add(treeNode);

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
                        Go(treeNode.FirstNode, bCloneOK || (treeNode.ForeColor == Color.SteelBlue));
                    }

                    treeNode = treeNode.NextNode;
                }
                while (treeNode != null);
            }
        }

        class InsertSizeMarker
        {
            static ListViewItem lvMarker = new ListViewItem();
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

            public static void Go(List<ListViewItem> listLVitems, int nIx, bool bUnique, bool bAdd = false)
            {
                Init();

                ListViewItem lvItem = (ListViewItem)lvMarker.Clone();

                lvItem.Text = (Utilities.FormatSize(((NodeDatum)((TreeNode)(bUnique ? listLVitems[nIx].Tag : ((List<TreeNode>)listLVitems[nIx].Tag)[0])).Tag).nTotalLength, bNoDecimal: true));

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

        public Correlate(TreeView treeView_Browse, SortedDictionary<HashKey, List<TreeNode>> dictNodes,
            ListView lvClones, ListView lvSameVol, ListView lvUnique,
            List<TreeNode> listRootNodes, List<TreeNode> listTreeNodes, bool bCheckboxes,
            List<ListViewItem> list_lvIgnore, bool bLoose)
        {
            static_this = this;
            form_treeView_Browse = treeView_Browse;
            m_dictNodes = dictNodes;
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
        void FixClones(SortedDictionary<HashKey, List<TreeNode>> dictClones, TreeNode treeNode, TreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used as a bool).
            // provisional.

            NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
            List<TreeNode> listClones = nodeDatum.m_listClones;
            ulong nLength = nodeDatum.nTotalLength;

            if (nLength <= 100 * 1024)
            {
                treeNode.ForeColor = Color.LightGray;

                if (listClones != null)
                {
                    foreach (TreeNode otherNode in listClones)
                    {
                        ((NodeDatum)otherNode.Tag).m_listClones = null;
                    }
                }

                listClones = nodeDatum.m_listClones = null;
            }

            if ((listClones != null) && (rootClone == null))
            {
                rootClone = treeNode;

                if (dictClones.ContainsKey(nodeDatum.Key))
                {
                    Utilities.Assert(1305.6301, dictClones[nodeDatum.Key] == listClones);
                    Utilities.Assert(1305.6302, ((NodeDatum)dictClones[nodeDatum.Key][0].Tag).m_bDifferentVols == nodeDatum.m_bDifferentVols);
                    Utilities.Assert(1305.6303, dictClones[nodeDatum.Key][0].ForeColor == treeNode.ForeColor);
                }
                else
                {
                    dictClones.Add(nodeDatum.Key, listClones);

                    // Test to see if clones are on separate volumes.

                    TreeNode rootNode = treeNode.Root();
                    RootNodeDatum rootNodeDatum = (RootNodeDatum)rootNode.Tag;

                    Utilities.Assert(1305.6304, treeNode.ForeColor == Color.Empty);
                    treeNode.ForeColor = Color.Firebrick;

                    bool bDifferentVols = false;

                    foreach (TreeNode subnode in listClones)
                    {
                        if (m_bThreadAbort || Form1.AppExit)
                        {
                            return;
                        }

                        Utilities.Assert(1305.6305, ((NodeDatum)subnode.Tag).Key == nodeDatum.Key);

                        TreeNode rootNode_A = subnode.Root();

                        if (rootNode == rootNode_A)
                        {
                            continue;
                        }

                        RootNodeDatum rootNodeDatum_A = (RootNodeDatum)rootNode_A.Tag;

                        if (Utilities.StrValid(rootNodeDatum.StrVolumeGroup) &&
                            (rootNodeDatum.StrVolumeGroup == rootNodeDatum_A.StrVolumeGroup))
                        {
                            continue;
                        }

                        treeNode.ForeColor = Color.SteelBlue;
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
                if (m_bThreadAbort || Form1.AppExit)
                {
                    return;
                }

                FixClones(dictClones, subNode, rootClone);
            }
        }

        public static void InsertSizeMarkers(List<ListViewItem> listLVitems)
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

            if (nInitial == 0)
            {
                nInitial = nInterval;
            }

            for (int i = nCount - nInitial; i > nInterval / 2; i -= nInterval)
            {
                InsertSizeMarker.Go(listLVitems, i, bUnique);
            }

            InsertSizeMarker.Go(listLVitems, 0, bUnique);            // Enter the Zeroth
        }

        void IgnoreNodeAndSubnodes(ListViewItem lvItem, TreeNode treeNode, bool bContinue)
        {
            do
            {
                if (dictIgnoreNodes.ContainsKey(treeNode))
                {
                    continue;
                }

                dictIgnoreNodes.Add(treeNode, lvItem);

                if (treeNode.Nodes.Count > 0)
                {
                    IgnoreNodeAndSubnodes(lvItem, treeNode.Nodes[0], bContinue: true);
                }
            }
            while (bContinue && ((treeNode = treeNode.NextNode) != null));
        }

        void IgnoreNode_Query(String sbMatch, int nMaxLevel, TreeNode treeNode)
        {
            if (treeNode.Level > nMaxLevel)
            {
                return;
            }

            do
            {
                if (m_bThreadAbort || Form1.AppExit)
                {
                    return;
                }

                if (sbMatch.Contains(treeNode.Text))
                {
                    foreach (ListViewItem lvItem in m_list_lvIgnore)
                    {
                        if (lvItem.Text != treeNode.Text)
                        {
                            continue;
                        }

                        if (treeNode.Level == (int.Parse(lvItem.SubItems[1].Text) - 1))
                        {
                            IgnoreNodeAndSubnodes((ListViewItem)lvItem.Tag, treeNode, bContinue: false);
                            break;
                        }
                    }
                }

                if (treeNode.Nodes.Count > 0)
                {
                    IgnoreNode_Query(sbMatch, nMaxLevel, treeNode.Nodes[0]);
                }
            }
            while ((treeNode = treeNode.NextNode) != null);
        }

        void IgnoreNode_Mark(TreeNode treeNode, ListViewItem lvIgnoreItem = null)
        {
            ((NodeDatum)treeNode.Tag).m_lvItem = lvIgnoreItem ?? dictIgnoreNodes[treeNode];
            treeNode.ForeColor = Color.DarkGray;
            treeNode.BackColor = Color.Empty;
        }

        NodeDatum UnsetIgnoredParent(TreeNode parentNode)
        {
            NodeDatum nodeDatum_A = (NodeDatum)parentNode.Tag;

            if ((nodeDatum_A.m_lvItem != null) && (nodeDatum_A.m_lvItem.ListView != null)) // the ignore list
            {
                Utilities.Assert(1305.6306, parentNode.ForeColor == Color.DarkGray);
                nodeDatum_A.m_lvItem = null;
                parentNode.ForeColor = Color.Empty;
            }

            return nodeDatum_A;
        }

        public void Step1_OnThread()
        {
            TreeView treeView = new TreeView();     // sets Level and Next.

            if (m_listRootNodes[0].TreeView == null)
            {
                treeView.Nodes.AddRange(m_listRootNodes.ToArray());
            }

            if (m_list_lvIgnore.Count > 0)
            {
                DateTime dtStart = DateTime.Now;
                int nMaxLevel = m_list_lvIgnore.Max(i => int.Parse(i.SubItems[1].Text) - 1);
                StringBuilder sbMatch = new StringBuilder();

                foreach (ListViewItem lvItem in m_list_lvIgnore)
                {
                    sbMatch.AppendLine(lvItem.Text);
                }

                IgnoreNode_Query(sbMatch.ToString(), nMaxLevel, m_listRootNodes[0]);
                Console.WriteLine("IgnoreNode " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            }

            SortedDictionary<HashKey, TreeNode> dictUnique = new SortedDictionary<HashKey, TreeNode>();

            foreach (KeyValuePair<HashKey, List<TreeNode>> pair in m_dictNodes)
            {
                if (m_bThreadAbort || Form1.AppExit)
                {
                    return;
                }

                List<TreeNode> listNodes = (List<TreeNode>)pair.Value;

                if (listNodes.Count < 1)
                {
                    Utilities.Assert(1305.6307, false);
                    continue;
                }

                if (listNodes.Count > 1)
                {
                    bool bIgnore = true;
                    bool bAny = false;
                    ListViewItem lvIgnoreItem = null;

                    foreach (TreeNode treeNode in listNodes)
                    {
                        if (m_bThreadAbort || Form1.AppExit)
                        {
                            return;
                        }

                        if (dictIgnoreNodes.ContainsKey(treeNode) == false)
                        {
                            bIgnore = false;
                            if (m_bLoose == false) break;
                        }
                        else
                        {
                            bAny = true;
                            lvIgnoreItem = dictIgnoreNodes[treeNode];
                            if (m_bLoose) break;
                        }
                    }

                    if (bIgnore || (m_bLoose && bAny))
                    {
                        foreach (TreeNode treeNode in listNodes)
                        {
                            IgnoreNode_Mark(treeNode, lvIgnoreItem);
                        }

                        continue;
                    }

                    List<TreeNode> listKeep = new List<TreeNode>();

                    foreach (TreeNode treeNode in listNodes)
                    {
                        if (m_bThreadAbort || Form1.AppExit)
                        {
                            return;
                        }

                        NodeDatum nodeDatum = ((NodeDatum)treeNode.Tag);

                        Utilities.Assert(1305.6308, nodeDatum.nTotalLength > 100 * 1024);

                        if (listNodes.Contains(treeNode.Parent) == false)
                        {
                            listKeep.Add(treeNode);
                        }
                    }

                    if (listKeep.Count > 1)
                    {
                        foreach (TreeNode treeNode in listKeep)
                        {
                            ((NodeDatum)treeNode.Tag).m_listClones = listKeep;
                        }
                    }
                    else
                    {
                        listNodes = listKeep;   // kick off "else" logic below after deleting child clones
                    }
                }

                if (listNodes.Count == 1)       // "else"
                {
                    TreeNode treeNode = listNodes[0];

                    if (dictIgnoreNodes.ContainsKey(treeNode))
                    {
                        IgnoreNode_Mark(treeNode);
                    }
                    else if (((NodeDatum)treeNode.Tag).nImmediateFiles > 0)
                    {
                        dictUnique.Add(pair.Key, treeNode);
                    }
                }
            }

            SortedDictionary<HashKey, List<TreeNode>> dictClones = new SortedDictionary<HashKey, List<TreeNode>>();

            foreach (TreeNode treeNode in m_listRootNodes)
            {
                FixClones(dictClones, treeNode);
            }

            m_listRootNodes.Sort((x, y) => String.Compare(x.Text, y.Text));

            foreach (KeyValuePair<HashKey, List<TreeNode>> listNodes in dictClones)
            {
                if (m_bThreadAbort || Form1.AppExit)
                {
                    return;
                }

                int nClones = listNodes.Value.Count;

                if (nClones <= 0)
                {
                    Utilities.Assert(1305.6309, false);
                    continue;
                }

                String str_nClones = null;

                if (nClones > 2)        // includes the subject node: only note three clones or more
                {
                    str_nClones = nClones.ToString("###,###");
                }

                ListViewItem lvItem = new ListViewItem(new String[] { String.Empty, str_nClones });

                lvItem.Tag = listNodes.Value;
                lvItem.ForeColor = listNodes.Value[0].ForeColor;

                TreeNode nameNode = null;
                int nLevel = int.MaxValue;

                foreach (TreeNode treeNode in listNodes.Value)
                {
                    if (m_bThreadAbort || Form1.AppExit)
                    {
                        return;
                    }

                    TreeNode parentNode = treeNode.Parent;

                    while (parentNode != null)
                    {
                        UnsetIgnoredParent(parentNode);
                        parentNode = parentNode.Parent;
                    }

                    if (treeNode.Level < nLevel)
                    {
                        nLevel = treeNode.Level;
                        nameNode = treeNode;
                    }

                    ((NodeDatum)treeNode.Tag).m_lvItem = lvItem;
                }

                lvItem.Text = nameNode.Text;
                Utilities.Assert(1305.63101, Utilities.StrValid(lvItem.Text));
                listLVitems.Add(lvItem);
            }

            dictClones = null;
            InsertSizeMarkers(listLVitems);

            foreach (KeyValuePair<HashKey, TreeNode> listNodes in dictUnique)
            {
                if (m_bThreadAbort || Form1.AppExit)
                {
                    return;
                }

                TreeNode treeNode = listNodes.Value;

                Utilities.Assert(1305.6311, Utilities.StrValid(treeNode.Text));

                ListViewItem lvItem = new ListViewItem(treeNode.Text);

                lvItem.Tag = treeNode;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                Utilities.Assert(1305.6312, nodeDatum.nImmediateFiles > 0);

                TreeNode parentNode = treeNode.Parent;

                while (parentNode != null)
                {
                    NodeDatum nodeDatum_A = UnsetIgnoredParent(parentNode);

                    if (parentNode.ForeColor == Color.Empty)
                    {
                        parentNode.ForeColor = Color.DarkRed;

                        if (nodeDatum_A.m_lvItem != null)
                        {
                            nodeDatum_A.m_lvItem.ForeColor = parentNode.ForeColor;
                        }
                    }

                    parentNode = parentNode.Parent;
                }

                treeNode.ForeColor = lvItem.ForeColor = Color.Red;
                listLVunique.Add(lvItem);
                Utilities.Assert(1305.6313, nodeDatum.m_lvItem == null);
                nodeDatum.m_lvItem = lvItem;
            }

            dictUnique = null;
            InsertSizeMarkers(listLVunique);

            List<TreeNode> listSameVolDescLength = new List<TreeNode>();

            if (m_listRootNodes.Count > 0)
            {
                new AddTreeToList(m_listTreeNodes, listSameVolDescLength).Go(m_listRootNodes[0]);
            }

            listSameVolDescLength.Sort((x, y) => ((NodeDatum)y.Tag).nTotalLength.CompareTo(((NodeDatum)x.Tag).nTotalLength));

            foreach (TreeNode treeNode in listSameVolDescLength)
            {
                if (m_bThreadAbort || Form1.AppExit)
                {
                    return;
                }

                TreeNode parentNode = treeNode.Parent;

                while (parentNode != null)
                {
                    if (parentNode.Tag != null)
                    {
                        UnsetIgnoredParent(parentNode);
                    }

                    parentNode = parentNode.Parent;
                }

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
                int nClones = nodeDatum.m_listClones.Count;

                if (nClones <= 0)
                {
                    Utilities.Assert(1305.6315, false);
                }

                String str_nClones = null;

                if (nClones > 2)
                {
                    str_nClones = nClones.ToString("###,###");
                }

                Utilities.Assert(1305.6314, Utilities.StrValid(treeNode.Text));

                ListViewItem lvItem = new ListViewItem(new String[] { treeNode.Text, str_nClones });

                lvItem.Tag = nodeDatum.m_listClones;
                lvItem.ForeColor = Color.Firebrick;
                lvItem.BackColor = treeNode.BackColor;
                listLVsameVol.Add(lvItem);
                nodeDatum.m_lvItem = lvItem;
            }

            listSameVolDescLength = null;
            InsertSizeMarkers(listLVsameVol);
            treeView.Nodes.Clear();                         // prevents destroy nodes
        }

        public void Step2_OnForm()
        {
            if (m_bThreadAbort || Form1.AppExit)
            {
                return;
            }

            if (form_treeView_Browse.Enabled == false)      // stays enabled when Correlate() is called directly
            {
                form_treeView_Browse.Nodes.Clear();
            }

            if (m_listRootNodes.Count > 0)
            {
                if (form_treeView_Browse.Enabled == false)
                {
                    form_treeView_Browse.Enabled = true;
                    form_treeView_Browse.CheckBoxes = m_bCheckboxes;
                    form_treeView_Browse.Nodes.AddRange(m_listRootNodes.ToArray());
                }

                if (m_bThreadAbort || Form1.AppExit)
                {
                    return;
                }

                Utilities.Assert(1305.6316, form_lvClones.Items.Count == 0);
                form_lvClones.Items.AddRange(listLVitems.ToArray());

                if (m_bThreadAbort || Form1.AppExit)
                {
                    return;
                }

                Utilities.Assert(1305.6317, form_lvUnique.Items.Count == 0);
                form_lvUnique.Items.AddRange(listLVunique.ToArray());

                if (m_bThreadAbort || Form1.AppExit)
                {
                    return;
                }

                Utilities.Assert(1305.6318, form_lvSameVol.Items.Count == 0);
                form_lvSameVol.Items.AddRange(listLVsameVol.ToArray());

                if (form_treeView_Browse.SelectedNode != null)      // m_bPutPathInFindEditBox is set in TreeDoneCallback()
                {
                    TreeNode treeNode = form_treeView_Browse.SelectedNode;

                    form_treeView_Browse.SelectedNode = null;
                    form_treeView_Browse.SelectedNode = treeNode;   // reselect in new correlation listviewers
                }
                else
                {
                    form_treeView_Browse.SelectedNode = m_listRootNodes[0];
                }
            }

            static_this = null;
        }
    }
}
