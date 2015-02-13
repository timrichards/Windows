using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DoubleFile;
using System.Collections.Concurrent;

namespace Local
{
    partial class Collate
    {
        internal Collate(GlobalData_Base gd_in,
            ConcurrentDictionary<FolderKeyStruct, UList<LocalTreeNode>> dictNodes,
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
            if (listLVitems.IsEmpty())
            {
                return;
            }

            var bUnique = (listLVitems[0].Tag is LocalTreeNode);
            var nCount = listLVitems.Count;
            var nInterval = (nCount < 100) ? 10 : (nCount < 1000) ? 25 : 50;

            InsertSizeMarkerStatic.Go(listLVitems, nCount - 1, bUnique, bAdd: true);

            var nInitial = nCount % nInterval;

            if (nInitial == 0)
            {
                nInitial = nInterval;
            }

            if (nCount - nInitial > nInterval / 2)
            {
                for (var i = nCount - nInitial; i > nInterval / 2; i -= nInterval)
                {
                    InsertSizeMarkerStatic.Go(listLVitems, i, bUnique);
                }
            }

            InsertSizeMarkerStatic.Go(listLVitems, 0, bUnique);            // Enter the Zeroth
        }

        internal void Step1_OnThread(Action<double> reportProgress)
        {
            double nProgressNumerator = 0;
            double nProgressDenominator = 0;
            double nProgressItem = 0;
            const double nTotalProgressItems = 6;

            var treeView = new LocalTV();     // sets Level and NextNode

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
                var dtStart = DateTime.Now;
                var nMaxLevel = m_list_lvIgnore.Max(i => int.Parse(i.SubItems[1].Text) - 1);
                var sbMatch = new StringBuilder();

                foreach (var lvItem in m_list_lvIgnore)
                {
                    sbMatch.AppendLine(lvItem.Text);
                }

                IgnoreNodeQuery(sbMatch.ToString().ToLower(), nMaxLevel, m_listRootNodes[0]);
                UtilProject.WriteLine("IgnoreNode " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            }

            var dictIgnoreMark = new Dictionary<LocalTreeNode, LocalLVitem>();
            var dictNodes = new SortedDictionary<FolderKeyStruct, List<LocalTreeNode>>();

            foreach (var kvp in m_dictNodes)                    // clone to remove ignored
            {                                                   // m_ vs local check is via List vs UList
                dictNodes.Add(kvp.Key, kvp.Value.ToList());     // clone pair.Value to remove ignored, using ToList() 
            }

            nProgressDenominator += dictIgnoreNodes.Count;
            ++nProgressItem;

            foreach (var kvp in dictIgnoreNodes)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem/nTotalProgressItems);
                var treeNode = kvp.Key;
                var nodeDatum = (NodeDatum)treeNode.Tag;

                if (dictNodes.ContainsKey(nodeDatum.Key) == false)
                {
                    continue;
                }

                if (m_bLoose)
                {
                    foreach (var treeNode_A in dictNodes[nodeDatum.Key])
                    {
                        dictIgnoreMark.Add(treeNode_A, kvp.Value);
                    }

                    dictNodes.Remove(nodeDatum.Key);
                }
                else if (dictNodes[nodeDatum.Key].Contains(treeNode))
                {
                    dictIgnoreMark.Add(treeNode, kvp.Value);
                    dictNodes[nodeDatum.Key].Remove(treeNode);

                    if (dictNodes[nodeDatum.Key].IsEmpty())
                    {
                        dictNodes.Remove(nodeDatum.Key);
                    }
                }
            }

            var dictUnique = new SortedDictionary<FolderKeyStruct, LocalTreeNode>();
            nProgressDenominator += dictNodes.Count;
            ++nProgressItem;

            foreach (var kvp in dictNodes)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                var listNodes = kvp.Value;

                if (listNodes.Count < 1)
                {
                    MBoxStatic.Assert(1305.6315, false);
                    continue;
                }
                
                if (listNodes.Count > 1)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    var listKeep = new UList<LocalTreeNode>();

                    foreach (var treeNode_A in listNodes)
                    {
                        if (m_bThreadAbort || gd.WindowClosed)
                        {
                            return;
                        }

                        var nodeDatum = (treeNode_A.Tag as NodeDatum);

                        if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
                        {
                            MBoxStatic.Assert(0, false);
                            return;
                        }

                        MBoxStatic.Assert(1305.6316, nodeDatum.nTotalLength > 100 * 1024);

                        if (listNodes.Contains(treeNode_A.Parent) == false)
                        {
                            listKeep.Add(treeNode_A);
                        }
                    }

                    if (listKeep.Count > 1)
                    {
                        foreach (var treeNode_A in listKeep)
                        {
                            var nodeDatum = (treeNode_A.Tag as NodeDatum);

                            if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
                            {
                                MBoxStatic.Assert(0, false);
                                return;
                            }

                            nodeDatum.m_listClones = listKeep;
                        }
                    }
                    else
                    {
                        listNodes = listKeep.ToList();  // kick off "else" logic below after deleting child clones
                    }
                }

                if (listNodes.Count == 1)               // "else"
                {
                    var treeNode = listNodes[0];

                    var nodeDatum = (treeNode.Tag as NodeDatum);

                    if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
                    {
                        MBoxStatic.Assert(0, false);
                        return;
                    }

                    if (nodeDatum.nImmediateFiles > 0)
                    {
                        dictUnique.Add(kvp.Key, treeNode);
                    }
                }
            }

            var dictClones = new SortedDictionary<FolderKeyStruct, UList<LocalTreeNode>>();

            nProgressDenominator += m_listRootNodes.Count;
            ++nProgressItem;

            foreach (var treeNode in m_listRootNodes)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);
                
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                DifferentVolsQuery(dictClones, treeNode);
            }

            m_listRootNodes.Sort((x, y) => string.Compare(x.Text, y.Text));
            nProgressDenominator += dictClones.Count;
            ++nProgressItem;

            foreach (var listNodes in dictClones)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                // load up listLVdiffVol

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                var nClones = listNodes.Value.Count;

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

                    foreach (var node in listNodes.Value)
                    {
                        node.ForeColor = UtilColor.Blue;
                    }
                }

                var lvItem = new LocalLVitem(new[] {string.Empty, str_nClones})
                {
                    Tag = listNodes.Value,
                    ForeColor = listNodes.Value[0].ForeColor
                };

                LocalTreeNode nameNode = null;
                var nLevel = int.MaxValue;

                foreach (var treeNode in listNodes.Value)
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

                    var nodeDatum = (treeNode.Tag as NodeDatum);

                    if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
                    {
                        MBoxStatic.Assert(0, false);
                        return;
                    }

                    nodeDatum.m_lvItem = lvItem;
                }

                lvItem.Text = nameNode.Text;
                MBoxStatic.Assert(1305.6318, false == string.IsNullOrWhiteSpace(lvItem.Text));
                listLVdiffVol.Add(lvItem);
            }

            nProgressDenominator += dictIgnoreMark.Count;
            ++nProgressItem;

            foreach (var kvp in dictIgnoreMark)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                var treeNode = kvp.Key;
                var lvIgnoreItem = kvp.Value;

                treeNode.ForeColor = UtilColor.DarkGray;
                treeNode.BackColor = UtilColor.Empty;

                var nodeDatum = (treeNode.Tag as NodeDatum);

                if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
                {
                    MBoxStatic.Assert(0, false);
                    return;
                }

                nodeDatum.m_lvItem = lvIgnoreItem;
                MBoxStatic.Assert(1305.6319, nodeDatum.m_lvItem != null);
                nodeDatum.m_listClones.Remove(treeNode);
            }

            InsertSizeMarkers(listLVdiffVol);
            nProgressDenominator += dictUnique.Count;

            foreach (var listNodes in dictUnique)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator);

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                var treeNode = listNodes.Value;

                MBoxStatic.Assert(1305.6321, false == string.IsNullOrWhiteSpace(treeNode.Text));

                var lvItem = new LocalLVitem(treeNode.Text) {Tag = treeNode};
                var nodeDatum = (treeNode.Tag as NodeDatum);

                if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
                {
                    MBoxStatic.Assert(0, false);
                    return;
                }

                MBoxStatic.Assert(1305.6322, nodeDatum.nImmediateFiles > 0);
                SnowUniqueParents(treeNode);

                if (treeNode.ForeColor != UtilColor.DarkOrange)
                {
                    MBoxStatic.Assert(1305.6323, treeNode.ForeColor == UtilColor.Empty);
                    treeNode.ForeColor = UtilColor.Red;
                }

                lvItem.ForeColor = treeNode.ForeColor;
                listLVunique.Add(lvItem);
                MBoxStatic.Assert(1305.6324, nodeDatum.m_lvItem == null);
                nodeDatum.m_lvItem = lvItem;
            }

            InsertSizeMarkers(listLVunique);

            var listSameVol = new List<LocalTreeNode>();

            if (false == m_listRootNodes.IsEmpty())
            {
                var nCount = CountNodes.Go(m_listRootNodes);
                var nCount_A = new AddTreeToList(m_listTreeNodes, listSameVol).Go(m_listRootNodes).Count;

                MBoxStatic.Assert(1305.6325, nCount_A == nCount);
                MBoxStatic.Assert(1305.6326, m_listTreeNodes.Count == nCount);
                MBoxStatic.Assert(1305.6327, CountNodes.Go(m_listRootNodes) == nCount);
                UtilProject.WriteLine("Step1_OnThread " + nCount);
            }

            listSameVol.Sort((y, x) => ((NodeDatum)x.Tag).nTotalLength.CompareTo(((NodeDatum)y.Tag).nTotalLength));
            nProgressDenominator += listSameVol.Count;
            ++nProgressItem;

            foreach (var treeNode in listSameVol)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                SnowUniqueParents(treeNode);

                var nodeDatum = (treeNode.Tag as NodeDatum);

                if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
                {
                    MBoxStatic.Assert(0, false);
                    return;
                }

                var nClones = nodeDatum.m_listClones.Count;

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

                var lvItem = new LocalLVitem(new[] {treeNode.Text, str_nClones})
                {
                    Tag = nodeDatum.m_listClones,
                    ForeColor = UtilColor.Firebrick,
                    BackColor = treeNode.BackColor
                };

                listLVsameVol.Add(lvItem);
                nodeDatum.m_lvItem = lvItem;
            }

            InsertSizeMarkers(listLVsameVol);
            treeView.Nodes.Clear();                             // prevents destroy nodes
        }

        internal void Step2_OnForm()
        {
            UtilProject.UIthread(() =>
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                if (m_tvBrowseWPF.Enabled == false)      // stays enabled when DoCollation() is called directly
                {
                    m_tvBrowseWPF.Nodes.Clear();
                }

                if (m_listRootNodes.IsEmpty())
                {
                    return;
                }

                if (m_tvBrowseWPF.Enabled == false)
                {
                    m_tvBrowseWPF.Enabled = true;
                    m_tvBrowseWPF.CheckBoxes = m_bCheckboxes;

                    var nCount = CountNodes.Go(m_listRootNodes);

                    UtilAnalysis_DirList.Write("A");
                    m_tvBrowseWPF.Nodes.AddRange(m_listRootNodes.ToArray());
                    UtilProject.WriteLine("A");

                    var nCount_A = CountNodes.Go(m_listRootNodes);

                    MBoxStatic.Assert(1305.6331, nCount_A == nCount);
                    MBoxStatic.Assert(1305.6332, m_tvBrowseWPF.GetNodeCount(includeSubTrees: true) == nCount);
                    UtilProject.WriteLine("Step2_OnForm_A " + nCount);
                }

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                MBoxStatic.Assert(1305.6333, wpf_lvClones.Items.IsEmpty());
                UtilAnalysis_DirList.Write("B");
                wpf_lvClones.Items.AddRange(listLVdiffVol.ToArray());
                wpf_lvClones.Invalidate();
                UtilProject.WriteLine("B");

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                MBoxStatic.Assert(1305.6334, wpf_lvUnique.Items.IsEmpty());
                UtilAnalysis_DirList.Write("C");
                wpf_lvUnique.Items.AddRange(listLVunique.ToArray());
                wpf_lvUnique.Invalidate();
                UtilProject.WriteLine("C");

                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                MBoxStatic.Assert(1305.6335, wpf_lvSameVol.Items.IsEmpty());
                UtilAnalysis_DirList.Write("D");
                wpf_lvSameVol.Items.AddRange(listLVsameVol.ToArray());
                wpf_lvSameVol.Invalidate();
                UtilProject.WriteLine("D");

                if (m_tvBrowseWPF.SelectedNode != null)      // gd.m_bPutPathInFindEditBox is set in TreeDoneCallback()
                {
                    var treeNode = m_tvBrowseWPF.SelectedNode;

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
        void DifferentVolsQuery(
            IDictionary<FolderKeyStruct, UList<LocalTreeNode>> dictClones,
            LocalTreeNode treeNode,
            LocalTreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used as a bool).
            // provisional.

            var nodeDatum = (NodeDatum)treeNode.Tag;
            var listClones = nodeDatum.m_listClones;
            var nLength = nodeDatum.nTotalLength;

            if (nLength <= 100 * 1024)
            {
                treeNode.ForeColor = UtilColor.LightGray;
                nodeDatum.m_listClones.Clear();
            }

            if ((false == listClones.IsEmpty()) &&
                (null == rootClone))
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

                    var rootNode = treeNode.Root();
                    var rootNodeDatum = (rootNode.Tag as RootNodeDatum);

                    if (null == rootNodeDatum)      // this check is new 2/13/15 and has never been hit
                    {
                        MBoxStatic.Assert(0, false);
                        return;
                    }


                    MBoxStatic.Assert(1305.6308, new int[] { UtilColor.Empty, UtilColor.DarkBlue }.Contains(treeNode.ForeColor));
                    treeNode.ForeColor = UtilColor.Firebrick;

                    var bDifferentVols = false;

                    foreach (var subnode in listClones)
                    {
                        if (m_bThreadAbort || gd.WindowClosed)
                        {
                            return;
                        }

                        MBoxStatic.Assert(1305.6309, ((NodeDatum)subnode.Tag).Key == nodeDatum.Key);

                        var rootNode_A = subnode.Root();

                        if (rootNode == rootNode_A)
                        {
                            continue;
                        }

                        var rootNodeDatum_A = (treeNode.Tag as RootNodeDatum);

                        if (null == rootNodeDatum_A)      // this check is new 2/13/15 and has never been hit
                        {
                            MBoxStatic.Assert(0, false);
                            return;
                        }

                        if (false == string.IsNullOrWhiteSpace(rootNodeDatum.StrVolumeGroup) &&
                            (rootNodeDatum.StrVolumeGroup == rootNodeDatum_A.StrVolumeGroup))
                        {
                            continue;
                        }

                        if (treeNode.ForeColor != UtilColor.DarkBlue)
                        {
                            MBoxStatic.Assert(1305.6311, treeNode.ForeColor == UtilColor.Firebrick);
                            treeNode.ForeColor = UtilColor.SteelBlue;
                        }

                        bDifferentVols = true;
                        break;
                    }

                    foreach (var subNode in listClones)
                    {
                        var nodeDatum_A = (subNode.Tag as NodeDatum);

                        if (null == nodeDatum_A)      // this check is new 2/13/15 and has never been hit
                        {
                            MBoxStatic.Assert(0, false);
                            return;
                        }

                        nodeDatum_A.m_bDifferentVols = bDifferentVols;
                        subNode.ForeColor = treeNode.ForeColor;
                    }
                }
            }

            foreach (var subNode in treeNode.Nodes)
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
            var treeNode = treeNode_in;

            do
            {
                if (dictIgnoreNodes.ContainsKey(treeNode))
                {
                    continue;
                }

                MBoxStatic.Assert(1305.6312, lvItem != null);
                dictIgnoreNodes.Add(treeNode, lvItem);

                if (false == treeNode.Nodes.IsEmpty())
                {
                    IgnoreNodeAndSubnodes(lvItem, treeNode.Nodes[0], bContinue: true);
                }
            }
            while (bContinue && ((treeNode = treeNode.NextNode) != null));
        }

        void IgnoreNodeQuery(string sbMatch, int nMaxLevel, LocalTreeNode treeNode_in)
        {
            if (treeNode_in.Level > nMaxLevel)
            {
                return;
            }

            var treeNode = treeNode_in;

            do
            {
                if (m_bThreadAbort || gd.WindowClosed)
                {
                    return;
                }

                if (sbMatch.Contains(treeNode.Text.ToLower()))
                {
                    foreach (var lvItem
                        in m_list_lvIgnore
                        .Where(lvItem => treeNode.Level == (int.Parse(lvItem.SubItems[1].Text) - 1))
                        .Where(lvItem => lvItem.Text.Equals(treeNode.Text,
                            StringComparison.InvariantCultureIgnoreCase)))
                    {
                        IgnoreNodeAndSubnodes((LocalLVitem)lvItem.Tag, treeNode);
                        break;
                    }
                }

                if (false == treeNode.Nodes.IsEmpty())
                {
                    IgnoreNodeQuery(sbMatch, nMaxLevel, treeNode.Nodes[0]);
                }
            }
            while ((treeNode = treeNode.NextNode) != null);
        }

        static void SnowUniqueParents(LocalTreeNode treeNode)
        {
            LocalTreeNode parentNode = treeNode.Parent;

            while (parentNode != null)
            {
                parentNode.BackColor = UtilColor.Snow;

                var nodeDatum = (NodeDatum)parentNode.Tag;

                if (nodeDatum.m_lvItem != null)
                {
                    nodeDatum.m_lvItem.BackColor = parentNode.BackColor;
                }

                if (parentNode.ForeColor != UtilColor.DarkOrange)
                {
                    MBoxStatic.Assert(1305.6313,
                        (parentNode.ForeColor == UtilColor.Empty) ==
                        (nodeDatum.m_lvItem == null));
                }

                parentNode = parentNode.Parent;
            }
        }

        // the following are form vars referenced internally, thus keeping their form_ and m_ prefixes
        readonly ConcurrentDictionary<FolderKeyStruct, UList<LocalTreeNode>> m_dictNodes = null;
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
