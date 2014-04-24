using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Drawing;

namespace SearchDirLists
{
    class TreeDone : Utilities
    {
        // the following are form vars referenced internally, thus keeping their form_ and m_ prefixes
        TreeView form_treeView_Browse = null;
        Hashtable m_hashCache = null;
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
                        Debug.Assert((nodeDatum.m_listClones != null) && (nodeDatum.m_bDifferentVols == false));
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

        public TreeDone(TreeView treeView_Browse, Hashtable hashCache,
            ListView lvClones, ListView lvSameVol, ListView lvUnique,
            List<TreeNode> listRootNodes, List<TreeNode> listTreeNodes, bool bCheckboxes,
            List<ListViewItem> list_lvIgnore, bool bLoose)
        {
            form_treeView_Browse = treeView_Browse;
            m_hashCache = hashCache;
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
                    Debug.Assert(dictClones[nodeDatum.Key] == listClones);
                    Debug.Assert(((NodeDatum)dictClones[nodeDatum.Key][0].Tag).m_bDifferentVols == nodeDatum.m_bDifferentVols);
                    Debug.Assert(dictClones[nodeDatum.Key][0].ForeColor == treeNode.ForeColor);
                }
                else
                {
                    dictClones.Add(nodeDatum.Key, listClones);

                    // Test to see if clones are on separate volumes.

                    TreeNode rootNode = TreeSelect.GetParentRoot(treeNode);
                    RootNodeDatum rootNodeDatum = (RootNodeDatum)rootNode.Tag;

                    Debug.Assert(treeNode.ForeColor == Color.Empty);
                    treeNode.ForeColor = Color.Firebrick;

                    bool bDifferentVols = false;

                    foreach (TreeNode subnode in listClones)
                    {
                        Debug.Assert(((NodeDatum)subnode.Tag).Key == nodeDatum.Key);

                        TreeNode rootNode_A = TreeSelect.GetParentRoot(subnode);

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
                Debug.Assert(parentNode.ForeColor == Color.DarkGray);
                nodeDatum_A.m_lvItem = null;
                parentNode.ForeColor = Color.Empty;
            }

            return nodeDatum_A;
        }

        public void Step1_OnThread()
        {
            TreeView treeView = new TreeView();     // sets Level and Next.
            treeView.Nodes.AddRange(m_listRootNodes.ToArray());

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

            Hashtable hashTable = new Hashtable();
            SortedDictionary<HashKey, TreeNode> dictUnique = new SortedDictionary<HashKey, TreeNode>();

            foreach (DictionaryEntry pair in m_hashCache)
            {
                if ((pair.Value is List<TreeNode>) == false)
                {
                    hashTable.Add(pair.Key, pair.Value);
                    continue;
                }

                List<TreeNode> listNodes = (List<TreeNode>)pair.Value;

                if (listNodes.Count < 1)
                {
                    Debug.Assert(false);
                    continue;
                }

                if (listNodes.Count > 1)
                {
                    bool bIgnore = true;
                    bool bAny = false;
                    ListViewItem lvIgnoreItem = null;

                    foreach (TreeNode treeNode in listNodes)
                    {
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
                        NodeDatum nodeDatum = ((NodeDatum)treeNode.Tag);

                        Debug.Assert(nodeDatum.nTotalLength > 100 * 1024);

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
                        continue;
                    }

                    if (((NodeDatum)treeNode.Tag).nImmediateFiles > 0)
                    {
                        dictUnique.Add((HashKey)pair.Key, treeNode);
                    }

                    continue;
                }
            }

            m_hashCache = hashTable;

            SortedDictionary<HashKey, List<TreeNode>> dictClones = new SortedDictionary<HashKey, List<TreeNode>>();

            foreach (TreeNode treeNode in m_listRootNodes)
            {
                FixClones(dictClones, treeNode);
            }

            m_listRootNodes.Sort((x, y) => String.Compare(x.Text, y.Text));

            IEnumerable<KeyValuePair<HashKey, List<TreeNode>>> dictReverse = dictClones.Reverse();

            dictClones = null;

            foreach (KeyValuePair<HashKey, List<TreeNode>> listNodes in dictReverse)
            {
                int nClones = listNodes.Value.Count;

                if (nClones <= 0)
                {
                    Debug.Assert(false);
                    continue;
                }

                String str_nClones = null;

                if (nClones > 2)        // includes the subject node: this line says don't put 2's all over the listviewer
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
                Debug.Assert(Utilities.StrValid(lvItem.Text));
                listLVitems.Add(lvItem);
            }

            dictReverse = null;
            InsertSizeMarkers(listLVitems);

            IEnumerable<KeyValuePair<HashKey, TreeNode>> dictUniqueReverse = dictUnique.Reverse();

            foreach (KeyValuePair<HashKey, TreeNode> listNodes in dictUniqueReverse)
            {
                TreeNode treeNode = listNodes.Value;

                Debug.Assert(Utilities.StrValid(treeNode.Text));

                ListViewItem lvItem = new ListViewItem(treeNode.Text);

                lvItem.Tag = treeNode;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                Debug.Assert(nodeDatum.nImmediateFiles > 0);

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
                Debug.Assert(nodeDatum.m_lvItem == null);
                nodeDatum.m_lvItem = lvItem;
            }

            dictUniqueReverse = null;
            InsertSizeMarkers(listLVunique);

            List<TreeNode> listSameVolDescLength = new List<TreeNode>();

            if (m_listRootNodes.Count > 0)
            {
                new AddTreeToList(m_listTreeNodes, listSameVolDescLength).Go(m_listRootNodes[0]);
            }

            listSameVolDescLength.Sort((x, y) => ((NodeDatum)y.Tag).nTotalLength.CompareTo(((NodeDatum)x.Tag).nTotalLength));

            foreach (TreeNode treeNode in listSameVolDescLength)
            {
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

                Debug.Assert(Utilities.StrValid(treeNode.Text));

                int nClones = nodeDatum.m_listClones.Count;

                if (nClones <= 0)
                {
                    Debug.Assert(false);
                }

                String str_nClones = null;

                if (nClones > 2)
                {
                    str_nClones = nClones.ToString("###,###");
                }

                ListViewItem lvItem = new ListViewItem(new String[] { treeNode.Text, str_nClones });

                lvItem.Tag = nodeDatum.m_listClones;
                lvItem.ForeColor = Color.Firebrick;
                lvItem.BackColor = treeNode.BackColor;
                listLVsameVol.Add(lvItem);
                nodeDatum.m_lvItem = lvItem;
            }

            listSameVolDescLength = null;
            InsertSizeMarkers(listLVsameVol);
            treeView.Nodes.Clear();                 // prevents destroy nodes
        }

        public bool Step2_OnForm()
        {
            form_treeView_Browse.Nodes.Clear();

            if (m_listRootNodes.Count > 0)
            {
                form_treeView_Browse.Enabled = true;
                form_treeView_Browse.CheckBoxes = m_bCheckboxes;
                form_treeView_Browse.Nodes.AddRange(m_listRootNodes.ToArray());
                Debug.Assert(form_lvClones.Items.Count == 0);
                form_lvClones.Items.AddRange(listLVitems.ToArray());
                Debug.Assert(form_lvUnique.Items.Count == 0);
                form_lvUnique.Items.AddRange(listLVunique.ToArray());
                Debug.Assert(form_lvSameVol.Items.Count == 0);
                form_lvSameVol.Items.AddRange(listLVsameVol.ToArray());
                form_treeView_Browse.SelectedNode = m_listRootNodes[0];
            }

            return false;           // unused bool return
        }
    }
}