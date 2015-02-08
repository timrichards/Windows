using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DoubleFile;

namespace Local
{
    partial class Collate
    {
        internal Collate(GlobalData_Base gd_in,
            SortedDictionary<FolderKeyStruct, UList<LocalTreeNode>> dictNodes,
            LocalTV tvBrowseWPF,
            LocalLV lvClones,
            LocalLV lvSameVol,
            LocalLV lvUnique,
            List<LocalTreeNode> listRootNodes,
            UList<LocalTreeNode> listTreeNodes,
            bool bCheckboxes,
            List<LocalLVitem> list_lvIgnore,
            bool bLoose)
        {
            gd = gd_in;
            static_this = this;
            m_dictNodes = dictNodes;
            m_tvBrowseWPF = tvBrowseWPF;
            wpf_lvClones = lvClones;
            wpf_lvSameVol = lvSameVol;
            wpf_lvUnique = lvUnique;
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

        internal static void InsertSizeMarkers(List<LocalLVitem> listLVitems)
        {
            if (listLVitems.Count <= 0)
            {
                return;
            }

            bool bUnique = (listLVitems[0].Tag is LocalTreeNode);
            int nCount = listLVitems.Count;
            int nInterval = (nCount < 100) ? 10 : (nCount < 1000) ? 25 : 50;

            InsertSizeMarkerStatic.Go(listLVitems, nCount - 1, bUnique, bAdd: true);

            int nInitial = nCount % nInterval;

            if (nInitial <= 0)
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
            LocalTV treeView = new LocalTV();     // sets Level and NextNode

            if (m_listRootNodes.Count <= 0)
            {
                MBoxStatic.Assert(1305.6314, false);
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

                foreach (LocalLVitem lvItem in m_list_lvIgnore)
                {
                    sbMatch.AppendLine(lvItem.Text);
                }

                IgnoreNodeQuery(sbMatch.ToString().ToLower(), nMaxLevel, m_listRootNodes[0]);
                UtilProject.WriteLine("IgnoreNode " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            }

            Dictionary<LocalTreeNode, LocalLVitem> dictIgnoreMark = new Dictionary<LocalTreeNode, LocalLVitem>();
            SortedDictionary<FolderKeyStruct, List<LocalTreeNode>> dictNodes = new SortedDictionary<FolderKeyStruct, List<LocalTreeNode>>();

            foreach (KeyValuePair<FolderKeyStruct, UList<LocalTreeNode>> pair in m_dictNodes)  // clone to remove ignored
            {                                                                       // m_ vs local check is via List vs UList
                dictNodes.Add(pair.Key, pair.Value.ToList());                       // clone pair.Value to remove ignored, using ToList() 
            }

            foreach (KeyValuePair<LocalTreeNode, LocalLVitem> pair in dictIgnoreNodes)
            {
                LocalTreeNode treeNode = pair.Key;
                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                if (dictNodes.ContainsKey(nodeDatum.Key) == false)
                {
                    continue;
                }

                if (m_bLoose)
                {
                    foreach (LocalTreeNode treeNode_A in dictNodes[nodeDatum.Key])
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

            SortedDictionary<FolderKeyStruct, LocalTreeNode> dictUnique = new SortedDictionary<FolderKeyStruct, LocalTreeNode>();

            foreach (KeyValuePair<FolderKeyStruct, List<LocalTreeNode>> pair in dictNodes)
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                List<LocalTreeNode> listNodes = (List<LocalTreeNode>)pair.Value;

                if (listNodes.Count < 1)
                {
                    MBoxStatic.Assert(1305.6315, false);
                    continue;
                }
                
                if (listNodes.Count > 1)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    UList<LocalTreeNode> listKeep = new UList<LocalTreeNode>();

                    foreach (LocalTreeNode treeNode_A in listNodes)
                    {
                        if (m_bThreadAbort || gd.WindowClosed)
                        {
                            return;
                        }

                        NodeDatum nodeDatum = ((NodeDatum)treeNode_A.Tag);

                        MBoxStatic.Assert(1305.6316, nodeDatum.nTotalLength > 100 * 1024);

                        if (listNodes.Contains(treeNode_A.Parent) == false)
                        {
                            listKeep.Add(treeNode_A);
                        }
                    }

                    if (listKeep.Count > 1)
                    {
                        foreach (LocalTreeNode treeNode_A in listKeep)
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
                    LocalTreeNode treeNode = listNodes[0];

                    if (((NodeDatum)treeNode.Tag).nImmediateFiles > 0)
                    {
                        dictUnique.Add(pair.Key, treeNode);
                    }
                }
            }

            SortedDictionary<FolderKeyStruct, UList<LocalTreeNode>> dictClones = new SortedDictionary<FolderKeyStruct, UList<LocalTreeNode>>();

            foreach (LocalTreeNode treeNode in m_listRootNodes)
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                DifferentVolsQuery(dictClones, treeNode);
            }

            m_listRootNodes.Sort((x, y) => string.Compare(x.Text, y.Text));

            foreach (KeyValuePair<FolderKeyStruct, UList<LocalTreeNode>> listNodes in dictClones)
            {
                // load up listLVdiffVol

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                int nClones = listNodes.Value.Count;

                if (nClones <= 0)
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

                    foreach (LocalTreeNode node in listNodes.Value)
                    {
                        node.ForeColor = Color.Blue;
                    }
                }

                LocalLVitem lvItem = new LocalLVitem(new string[] { string.Empty, str_nClones });

                lvItem.Tag = listNodes.Value;
                lvItem.ForeColor = listNodes.Value[0].ForeColor;

                LocalTreeNode nameNode = null;
                int nLevel = int.MaxValue;

                foreach (LocalTreeNode treeNode in listNodes.Value)
                {
                    if (m_bThreadAbort || gd.WindowClosed)
                    {
                        return;
                    }

                    LocalTreeNode parentNode = (LocalTreeNode)treeNode.Parent;

                    if (treeNode.Level < nLevel)
                    {
                        nLevel = treeNode.Level;
                        nameNode = treeNode;
                    }

                    ((NodeDatum)treeNode.Tag).m_lvItem = lvItem;
                }

                lvItem.Text = nameNode.Text;
                MBoxStatic.Assert(1305.6318, false == string.IsNullOrWhiteSpace(lvItem.Text));
                listLVdiffVol.Add(lvItem);
            }

            foreach (KeyValuePair<LocalTreeNode, LocalLVitem> pair in dictIgnoreMark)
            {
                LocalTreeNode treeNode = pair.Key;
                LocalLVitem lvIgnoreItem = pair.Value;

                treeNode.ForeColor = Color.DarkGray;
                treeNode.BackColor = Color.Empty;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                nodeDatum.m_lvItem = lvIgnoreItem;
                MBoxStatic.Assert(1305.6319, nodeDatum.m_lvItem != null);
                nodeDatum.m_listClones.Remove(treeNode);
            }

            dictClones = null;
            InsertSizeMarkers(listLVdiffVol);

            foreach (KeyValuePair<FolderKeyStruct, LocalTreeNode> listNodes in dictUnique)
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                LocalTreeNode treeNode = listNodes.Value;

                MBoxStatic.Assert(1305.6321, false == string.IsNullOrWhiteSpace(treeNode.Text));

                LocalLVitem lvItem = new LocalLVitem(treeNode.Text);

                lvItem.Tag = treeNode;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                MBoxStatic.Assert(1305.6322, nodeDatum.nImmediateFiles > 0);
                SnowUniqueParents(treeNode);

                if (treeNode.ForeColor != Color.DarkOrange)
                {
                    MBoxStatic.Assert(1305.6323, treeNode.ForeColor == Color.Empty);
                    treeNode.ForeColor = Color.Red;
                }

                lvItem.ForeColor = treeNode.ForeColor;
                listLVunique.Add(lvItem);
                MBoxStatic.Assert(1305.6324, nodeDatum.m_lvItem == null);
                nodeDatum.m_lvItem = lvItem;
            }

            dictUnique = null;
            InsertSizeMarkers(listLVunique);

            List<LocalTreeNode> listSameVol = new List<LocalTreeNode>();

            if (m_listRootNodes.Count > 0)
            {
                int nCount = CountNodes.Go(m_listRootNodes);
                int nCount_A = new AddTreeToList(m_listTreeNodes, listSameVol).Go(m_listRootNodes).Count;

                MBoxStatic.Assert(1305.6325, nCount_A == nCount);
                MBoxStatic.Assert(1305.6326, m_listTreeNodes.Count == nCount);
                MBoxStatic.Assert(1305.6327, CountNodes.Go(m_listRootNodes) == nCount);
                UtilProject.WriteLine("Step1_OnThread " + nCount);
            }

            listSameVol.Sort((y, x) => ((NodeDatum)x.Tag).nTotalLength.CompareTo(((NodeDatum)y.Tag).nTotalLength));

            foreach (LocalTreeNode treeNode in listSameVol)
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                SnowUniqueParents(treeNode);

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
                int nClones = nodeDatum.m_listClones.Count;

                if (nClones <= 0)
                {
                    MBoxStatic.Assert(1305.6328, false);
                }

                string str_nClones = null;

                if (nClones > 2)
                {
                    str_nClones = nClones.ToString("###,###");
                }

                MBoxStatic.Assert(1305.6329, false == string.IsNullOrWhiteSpace(treeNode.Text));

                LocalLVitem lvItem = new LocalLVitem(new string[] { treeNode.Text, str_nClones });

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

        internal void Step2_OnForm()
        {
            UtilAnalysis_DirList.Closure(() =>
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                if (m_tvBrowseWPF.Enabled == false)      // stays enabled when DoCollation() is called directly
                {
                    m_tvBrowseWPF.Nodes.Clear();
                }

                if (m_listRootNodes.Count <= 0)
                {
                    return;
                }

                if (m_tvBrowseWPF.Enabled == false)
                {
                    m_tvBrowseWPF.Enabled = true;
                    m_tvBrowseWPF.CheckBoxes = m_bCheckboxes;

                    int nCount = CountNodes.Go(m_listRootNodes);

                    UtilAnalysis_DirList.Write("A");
                    m_tvBrowseWPF.Nodes.AddRange(m_listRootNodes.ToArray());
                    UtilProject.WriteLine("A");

                    int nCount_A = CountNodes.Go(m_listRootNodes);

                    MBoxStatic.Assert(1305.6331, nCount_A == nCount);
                    MBoxStatic.Assert(1305.6332, m_tvBrowseWPF.GetNodeCount(includeSubTrees: true) == nCount);
                    UtilProject.WriteLine("Step2_OnForm_A " + nCount);
                }

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                MBoxStatic.Assert(1305.6333, wpf_lvClones.Items.Count <= 0);
                UtilAnalysis_DirList.Write("B");
                wpf_lvClones.Items.AddRange(listLVdiffVol.ToArray());
                wpf_lvClones.Invalidate();
                UtilProject.WriteLine("B");

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                MBoxStatic.Assert(1305.6334, wpf_lvUnique.Items.Count <= 0);
                UtilAnalysis_DirList.Write("C");
                wpf_lvUnique.Items.AddRange(listLVunique.ToArray());
                wpf_lvUnique.Invalidate();
                UtilProject.WriteLine("C");

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                MBoxStatic.Assert(1305.6335, wpf_lvSameVol.Items.Count <= 0);
                UtilAnalysis_DirList.Write("D");
                wpf_lvSameVol.Items.AddRange(listLVsameVol.ToArray());
                wpf_lvSameVol.Invalidate();
                UtilProject.WriteLine("D");

                if (m_tvBrowseWPF.SelectedNode != null)      // gd.m_bPutPathInFindEditBox is set in TreeDoneCallback()
                {
                    LocalTreeNode treeNode = m_tvBrowseWPF.SelectedNode;

                    m_tvBrowseWPF.SelectedNode = null;
                    m_tvBrowseWPF.SelectedNode = treeNode;   // reselect in repopulated collation listviewers
                }
                else
                {
                    m_tvBrowseWPF.SelectedNode = m_listRootNodes[0];
                }
            });

            static_this = null;
        }

        // If an outer directory is cloned then all the inner ones are part of the outer clone and their clone status is redundant.
        // Breadth-first.
        void DifferentVolsQuery(SortedDictionary<FolderKeyStruct, UList<LocalTreeNode>> dictClones, LocalTreeNode treeNode, LocalTreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used as a bool).
            // provisional.

            NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
            UList<LocalTreeNode> listClones = nodeDatum.m_listClones;
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
                    MBoxStatic.Assert(1305.6305, dictClones[nodeDatum.Key] == listClones);
                    MBoxStatic.Assert(1305.6306, ((NodeDatum)dictClones[nodeDatum.Key][0].Tag).m_bDifferentVols == nodeDatum.m_bDifferentVols);
                    MBoxStatic.Assert(1305.6307, dictClones[nodeDatum.Key][0].ForeColor == treeNode.ForeColor);
                }
                else
                {
                    dictClones.Add(nodeDatum.Key, listClones);

                    // Test to see if clones are on separate volumes.

                    LocalTreeNode rootNode = treeNode.Root();
                    RootNodeDatum rootNodeDatum = (RootNodeDatum)rootNode.Tag;

                    MBoxStatic.Assert(1305.6308, new Color[] { Color.Empty, Color.DarkBlue }.Contains(treeNode.ForeColor));
                    treeNode.ForeColor = Color.Firebrick;

                    bool bDifferentVols = false;

                    foreach (LocalTreeNode subnode in listClones)
                    {
                        if (m_bThreadAbort || gd.WindowClosed)
                        {
                            return;
                        }

                        MBoxStatic.Assert(1305.6309, ((NodeDatum)subnode.Tag).Key == nodeDatum.Key);

                        LocalTreeNode rootNode_A = subnode.Root();

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
                            MBoxStatic.Assert(1305.6311, treeNode.ForeColor == Color.Firebrick);
                            treeNode.ForeColor = Color.SteelBlue;
                        }

                        bDifferentVols = true;
                        break;
                    }

                    foreach (LocalTreeNode subNode in listClones)
                    {
                        ((NodeDatum)subNode.Tag).m_bDifferentVols = bDifferentVols;
                        subNode.ForeColor = treeNode.ForeColor;
                    }
                }
            }

            foreach (LocalTreeNode subNode in treeNode.Nodes)
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                DifferentVolsQuery(dictClones, subNode, rootClone);
            }
        }

        void IgnoreNodeAndSubnodes(LocalLVitem lvItem, LocalTreeNode treeNode_in, bool bContinue = false)
        {
            LocalTreeNode treeNode = treeNode_in;

            do
            {
                if (dictIgnoreNodes.ContainsKey(treeNode))
                {
                    continue;
                }

                MBoxStatic.Assert(1305.6312, lvItem != null);
                dictIgnoreNodes.Add(treeNode, lvItem);

                if (treeNode.Nodes.Count > 0)
                {
                    IgnoreNodeAndSubnodes(lvItem, (LocalTreeNode)treeNode.Nodes[0], bContinue: true);
                }
            }
            while (bContinue && ((treeNode = (LocalTreeNode)treeNode.NextNode) != null));
        }

        void IgnoreNodeQuery(string sbMatch, int nMaxLevel, LocalTreeNode treeNode_in)
        {
            if (treeNode_in.Level > nMaxLevel)
            {
                return;
            }

            LocalTreeNode treeNode = treeNode_in;

            do
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                if (sbMatch.Contains(treeNode.Text.ToLower()))
                {
                    foreach (LocalLVitem lvItem in m_list_lvIgnore)
                    {
                        if (treeNode.Level != (int.Parse(lvItem.SubItems[1].Text) - 1))
                        {
                            continue;
                        }

                        if (lvItem.Text.ToLower() == treeNode.Text.ToLower())
                        {
                            IgnoreNodeAndSubnodes((LocalLVitem)lvItem.Tag, treeNode);
                            break;
                        }
                    }
                }

                if (treeNode.Nodes.Count > 0)
                {
                    IgnoreNodeQuery(sbMatch, nMaxLevel, (LocalTreeNode)treeNode.Nodes[0]);
                }
            }
            while ((treeNode = (LocalTreeNode)treeNode.NextNode) != null);
        }

        void SnowUniqueParents(LocalTreeNode treeNode)
        {
            LocalTreeNode parentNode = (LocalTreeNode)treeNode.Parent;

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
                    MBoxStatic.Assert(1305.6313, (parentNode.ForeColor == Color.Empty) == (nodeDatum.m_lvItem == null));
                }

                parentNode = (LocalTreeNode)parentNode.Parent;
            }
        }

        // the following are form vars referenced internally, thus keeping their form_ and m_ prefixes
        readonly SortedDictionary<FolderKeyStruct, UList<LocalTreeNode>> m_dictNodes = null;
        readonly LocalTV m_tvBrowseWPF = null;
        readonly LocalLV wpf_lvClones = null;
        readonly LocalLV wpf_lvSameVol = null;
        readonly LocalLV wpf_lvUnique = null;
        readonly List<LocalTreeNode> m_listRootNodes = null;
        readonly UList<LocalTreeNode> m_listTreeNodes = null;
        readonly bool m_bCheckboxes = false;
        readonly List<LocalLVitem> m_list_lvIgnore = null;

        // the following are "local" to this object, and do not have m_ prefixes because they do not belong to the form.
        readonly List<LocalLVitem> listLVunique = new List<LocalLVitem>();
        readonly List<LocalLVitem> listLVsameVol = new List<LocalLVitem>();
        readonly List<LocalLVitem> listLVdiffVol = new List<LocalLVitem>();
        readonly Dictionary<LocalTreeNode, LocalLVitem> dictIgnoreNodes = new Dictionary<LocalTreeNode, LocalLVitem>();
        readonly bool m_bLoose = false;

        bool m_bThreadAbort = false;
        static Collate static_this = null;
        readonly GlobalData_Base gd = null;
    }
}
