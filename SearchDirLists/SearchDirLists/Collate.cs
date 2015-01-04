#if WPF
using System.Windows.Controls;
using System.Windows.Media; using Media = System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using System.Windows;
#else
using System.Windows.Forms;
using System.Drawing;
#endif

using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        readonly SortedDictionary<Correlate, UList<SDL_TreeNode>> m_dictNodes = null;
        readonly SDL_ListView form_lvClones = null;
        readonly SDL_ListView form_lvSameVol = null;
        readonly SDL_ListView form_lvUnique = null;
        readonly List<SDL_TreeNode> m_listRootNodes = null;
        readonly UList<SDL_TreeNode> m_listTreeNodes = null;
        readonly bool m_bCheckboxes = false;
        readonly List<SDL_ListViewItem> m_list_lvIgnore = null;

        // the following are "local" to this object, and do not have m_ prefixes because they do not belong to the form.
        readonly List<SDL_ListViewItem> listLVunique = new List<SDL_ListViewItem>();
        readonly List<SDL_ListViewItem> listLVsameVol = new List<SDL_ListViewItem>();
        readonly List<SDL_ListViewItem> listLVdiffVol = new List<SDL_ListViewItem>();
        readonly Dictionary<SDL_TreeNode, SDL_ListViewItem> dictIgnoreNodes = new Dictionary<SDL_TreeNode, SDL_ListViewItem>();
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
            UList<SDL_TreeNode> m_listTreeNodes = null;
            List<SDL_TreeNode> m_listSameVol = null;

            int m_nCount = 0;
            internal int Count { get { return m_nCount; } }

            public AddTreeToList(UList<SDL_TreeNode> listTreeNodes, List<SDL_TreeNode> listSameVol)
            {
                m_listTreeNodes = listTreeNodes;
                m_listSameVol = listSameVol;
            }

            public AddTreeToList Go(List<SDL_TreeNode> listNodes)
            {
                foreach (SDL_TreeNode treeNode in listNodes)
                {
                    Go(treeNode, bNextNode: false);
                }

                return this;
            }

            void Go(SDL_TreeNode treeNode_in, bool bCloneOK = false, bool bNextNode = true)
            {
                if (treeNode_in == null)
                {
                    Utilities.Assert(1305.6302, false);
                }

                SDL_TreeNode treeNode = treeNode_in;

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

                    if ((treeNode.ForeColor == Drawing.Color.Firebrick) && (treeNode == nodeDatum.m_listClones[0]))
                    {
                        Utilities.Assert(1305.6304, (nodeDatum.m_listClones.Count > 0) && (nodeDatum.m_bDifferentVols == false));
                        m_listSameVol.Add(treeNode);
                    }

                    if (bCloneOK)
                    {
                        treeNode.BackColor = Drawing.Color.LightGoldenrodYellow;

                        if ((nodeDatum.m_lvItem != null) && (nodeDatum.m_lvItem.ListView == null))  // ignore LV
                        {
                            nodeDatum.m_lvItem.BackColor = treeNode.BackColor;
                        }
                    }

                    if (treeNode.FirstNode != null)
                    {
                        Go((SDL_TreeNode)treeNode.FirstNode, bCloneOK || (new Drawing.Color[] { Drawing.Color.SteelBlue, Drawing.Color.DarkBlue }.Contains(treeNode.ForeColor)));
                    }
                }
                while (bNextNode && ((treeNode = (SDL_TreeNode)treeNode.NextNode) != null));
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
                    lvMarker.BackColor = Drawing.Color.DarkSlateGray;
                    lvMarker.ForeColor = Drawing.Color.White;
                    lvMarker.Font = new Drawing.Font(lvMarker.Font, Drawing.FontStyle.Bold);
                    lvMarker.Tag = null;
                    bInit = true;
                }
            }

            public static void Go(List<SDL_ListViewItem> listLVitems, int nIx, bool bUnique, bool bAdd = false)
            {
                Init();

                SDL_ListViewItem lvItem = (SDL_ListViewItem)lvMarker.Clone();

                lvItem.Text = ((Utilities.FormatSize(((NodeDatum)((SDL_TreeNode)(bUnique ? listLVitems[nIx].Tag : ((UList<SDL_TreeNode>)listLVitems[nIx].Tag)[0])).Tag).nTotalLength, bNoDecimal: true)));

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

        public Collate(SortedDictionary<Correlate, UList<SDL_TreeNode>> dictNodes,
            SDL_ListView lvClones, SDL_ListView lvSameVol, SDL_ListView lvUnique,
            List<SDL_TreeNode> listRootNodes, UList<SDL_TreeNode> listTreeNodes, bool bCheckboxes,
            List<SDL_ListViewItem> list_lvIgnore, bool bLoose)
        {
            static_this = this;
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
        void DifferentVolsQuery(SortedDictionary<Correlate, UList<SDL_TreeNode>> dictClones, SDL_TreeNode treeNode, SDL_TreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used as a bool).
            // provisional.

            NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
            UList<SDL_TreeNode> listClones = nodeDatum.m_listClones;
            ulong nLength = nodeDatum.nTotalLength;

            if (nLength <= 100 * 1024)
            {
                treeNode.ForeColor = Drawing.Color.LightGray;
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

                    SDL_TreeNode rootNode = treeNode.Root();
                    RootNodeDatum rootNodeDatum = (RootNodeDatum)rootNode.Tag;

                    Utilities.Assert(1305.6308, new Drawing.Color[] { Drawing.Color.Empty, Drawing.Color.DarkBlue }.Contains(treeNode.ForeColor));
                    treeNode.ForeColor = Drawing.Color.Firebrick;

                    bool bDifferentVols = false;

                    foreach (SDL_TreeNode subnode in listClones)
                    {
                        if (m_bThreadAbort || GlobalData.AppExit)
                        {
                            return;
                        }

                        Utilities.Assert(1305.6309, ((NodeDatum)subnode.Tag).Key == nodeDatum.Key);

                        SDL_TreeNode rootNode_A = subnode.Root();

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

                        if (treeNode.ForeColor != Drawing.Color.DarkBlue)
                        {
                            Utilities.Assert(1305.6311, treeNode.ForeColor == Drawing.Color.Firebrick);
                            treeNode.ForeColor = Drawing.Color.SteelBlue;
                        }

                        bDifferentVols = true;
                        break;
                    }

                    foreach (SDL_TreeNode subNode in listClones)
                    {
                        ((NodeDatum)subNode.Tag).m_bDifferentVols = bDifferentVols;
                        subNode.ForeColor = treeNode.ForeColor;
                    }
                }
            }

            foreach (SDL_TreeNode subNode in treeNode.Nodes)
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

            bool bUnique = (listLVitems[0].Tag is SDL_TreeNode);
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

        void IgnoreNodeAndSubnodes(SDL_ListViewItem lvItem, SDL_TreeNode treeNode_in, bool bContinue = false)
        {
            SDL_TreeNode treeNode = treeNode_in;

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
                    IgnoreNodeAndSubnodes(lvItem, (SDL_TreeNode)treeNode.Nodes[0], bContinue: true);
                }
            }
            while (bContinue && ((treeNode = (SDL_TreeNode)treeNode.NextNode) != null));
        }

        void IgnoreNodeQuery(string sbMatch, int nMaxLevel, SDL_TreeNode treeNode_in)
        {
            if (treeNode_in.Level > nMaxLevel)
            {
                return;
            }

            SDL_TreeNode treeNode = treeNode_in;

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
                    IgnoreNodeQuery(sbMatch, nMaxLevel, (SDL_TreeNode)treeNode.Nodes[0]);
                }
            }
            while ((treeNode = (SDL_TreeNode)treeNode.NextNode) != null);
        }

        void SnowUniqueParents(SDL_TreeNode treeNode)
        {
            SDL_TreeNode parentNode = (SDL_TreeNode)treeNode.Parent;

            while (parentNode != null)
            {
                parentNode.BackColor = Drawing.Color.Snow;

                NodeDatum nodeDatum = (NodeDatum)parentNode.Tag;

                if (nodeDatum.m_lvItem != null)
                {
                    nodeDatum.m_lvItem.BackColor = parentNode.BackColor;
                }

                if (parentNode.ForeColor != Drawing.Color.DarkOrange)
                {
                    Utilities.Assert(1305.6313, (parentNode.ForeColor == Drawing.Color.Empty) == (nodeDatum.m_lvItem == null));
                }

                parentNode = (SDL_TreeNode)parentNode.Parent;
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

            Dictionary<SDL_TreeNode, SDL_ListViewItem> dictIgnoreMark = new Dictionary<SDL_TreeNode, SDL_ListViewItem>();
            SortedDictionary<Correlate, List<SDL_TreeNode>> dictNodes = new SortedDictionary<Correlate, List<SDL_TreeNode>>();

            foreach (KeyValuePair<Correlate, UList<SDL_TreeNode>> pair in m_dictNodes)  // clone to remove ignored
            {                                                                       // m_ vs local check is via List vs UList
                dictNodes.Add(pair.Key, pair.Value.ToList());                       // clone pair.Value to remove ignored, using ToList() 
            }

            foreach (KeyValuePair<SDL_TreeNode, SDL_ListViewItem> pair in dictIgnoreNodes)
            {
                SDL_TreeNode treeNode = pair.Key;
                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                if (dictNodes.ContainsKey(nodeDatum.Key) == false)
                {
                    continue;
                }

                if (m_bLoose)
                {
                    foreach (SDL_TreeNode treeNode_A in dictNodes[nodeDatum.Key])
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

            SortedDictionary<Correlate, SDL_TreeNode> dictUnique = new SortedDictionary<Correlate, SDL_TreeNode>();

            foreach (KeyValuePair<Correlate, List<SDL_TreeNode>> pair in dictNodes)
            {
                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                List<SDL_TreeNode> listNodes = (List<SDL_TreeNode>)pair.Value;

                if (listNodes.Count < 1)
                {
                    Utilities.Assert(1305.6315, false);
                    continue;
                }
                
                if (listNodes.Count > 1)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    UList<SDL_TreeNode> listKeep = new UList<SDL_TreeNode>();

                    foreach (SDL_TreeNode treeNode_A in listNodes)
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
                        foreach (SDL_TreeNode treeNode_A in listKeep)
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
                    SDL_TreeNode treeNode = listNodes[0];

                    if (((NodeDatum)treeNode.Tag).nImmediateFiles > 0)
                    {
                        dictUnique.Add(pair.Key, treeNode);
                    }
                }
            }

            SortedDictionary<Correlate, UList<SDL_TreeNode>> dictClones = new SortedDictionary<Correlate, UList<SDL_TreeNode>>();

            foreach (SDL_TreeNode treeNode in m_listRootNodes)
            {
                DifferentVolsQuery(dictClones, treeNode);
            }

            m_listRootNodes.Sort((x, y) => string.Compare(x.Text, y.Text));

            foreach (KeyValuePair<Correlate, UList<SDL_TreeNode>> listNodes in dictClones)
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

                    foreach (SDL_TreeNode node in listNodes.Value)
                    {
                        node.ForeColor = Drawing.Color.Blue;
                    }
                }

                SDL_ListViewItem lvItem = new SDL_ListViewItem(new string[] { string.Empty, str_nClones });

                lvItem.Tag = listNodes.Value;
                lvItem.ForeColor = listNodes.Value[0].ForeColor;

                SDL_TreeNode nameNode = null;
                int nLevel = int.MaxValue;

                foreach (SDL_TreeNode treeNode in listNodes.Value)
                {
                    if (m_bThreadAbort || GlobalData.AppExit)
                    {
                        return;
                    }

                    SDL_TreeNode parentNode = (SDL_TreeNode)treeNode.Parent;

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

            foreach (KeyValuePair<SDL_TreeNode, SDL_ListViewItem> pair in dictIgnoreMark)
            {
                SDL_TreeNode treeNode = pair.Key;
                SDL_ListViewItem lvIgnoreItem = pair.Value;

                treeNode.ForeColor = Drawing.Color.DarkGray;
                treeNode.BackColor = Drawing.Color.Empty;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                nodeDatum.m_lvItem = lvIgnoreItem;
                Utilities.Assert(1305.6319, nodeDatum.m_lvItem != null);
                nodeDatum.m_listClones.Remove(treeNode);
            }

            dictClones = null;
            InsertSizeMarkers(listLVdiffVol);

            foreach (KeyValuePair<Correlate, SDL_TreeNode> listNodes in dictUnique)
            {
                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return;
                }

                SDL_TreeNode treeNode = listNodes.Value;

                Utilities.Assert(1305.6321, false == string.IsNullOrWhiteSpace(treeNode.Text));

                SDL_ListViewItem lvItem = new SDL_ListViewItem(treeNode.Text);

                lvItem.Tag = treeNode;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                Utilities.Assert(1305.6322, nodeDatum.nImmediateFiles > 0);
                SnowUniqueParents(treeNode);

                if (treeNode.ForeColor != Drawing.Color.DarkOrange)
                {
                    Utilities.Assert(1305.6323, treeNode.ForeColor == Drawing.Color.Empty);
                    treeNode.ForeColor = Drawing.Color.Red;
                }

                lvItem.ForeColor = treeNode.ForeColor;
                listLVunique.Add(lvItem);
                Utilities.Assert(1305.6324, nodeDatum.m_lvItem == null);
                nodeDatum.m_lvItem = lvItem;
            }

            dictUnique = null;
            InsertSizeMarkers(listLVunique);

            List<SDL_TreeNode> listSameVol = new List<SDL_TreeNode>();

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

            foreach (SDL_TreeNode treeNode in listSameVol)
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
                lvItem.ForeColor = Drawing.Color.Firebrick;
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

                if (SDLWPF.treeViewMain.Enabled == false)      // stays enabled when DoCollation() is called directly
                {
                    SDLWPF.treeViewMain.Nodes.Clear();
                }

                if (m_listRootNodes.Count <= 0)
                {
                    return;
                }

                if (SDLWPF.treeViewMain.Enabled == false)
                {
                    SDLWPF.treeViewMain.Enabled = true;
                    SDLWPF.treeViewMain.CheckBoxes = m_bCheckboxes;

                    int nCount = Utilities.CountNodes(m_listRootNodes);

                    Utilities.Write("A");
                    SDLWPF.treeViewMain.Nodes.AddRange(m_listRootNodes.ToArray());
                    Utilities.WriteLine("A");

                    int nCount_A = Utilities.CountNodes(m_listRootNodes);

                    Utilities.Assert(1305.6331, nCount_A == nCount);
                    Utilities.Assert(1305.6332, SDLWPF.treeViewMain.GetNodeCount(includeSubTrees: true) == nCount);
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

                if (SDLWPF.treeViewMain.SelectedNode != null)      // gd.m_bPutPathInFindEditBox is set in TreeDoneCallback()
                {
                    SDL_TreeNode treeNode = (SDL_TreeNode)SDLWPF.treeViewMain.SelectedNode;

                    SDLWPF.treeViewMain.SelectedNode = null;
                    SDLWPF.treeViewMain.SelectedNode = treeNode;   // reselect in repopulated collation listviewers
                }
                else
                {
                    SDLWPF.treeViewMain.SelectedNode = m_listRootNodes[0];
                }
            }));

            static_this = null;
        }
    }
}
