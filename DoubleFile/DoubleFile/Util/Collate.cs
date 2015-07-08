using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace DoubleFile
{
    partial class Collate
    {
        internal Collate(
            ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>> dictNodes,
            LocalLV lvClones,
            LocalLV lvSameVol,
            LocalLV lvUnique,
            IReadOnlyList<LocalTreeNode> lsRootNodes,
            List<LocalTreeNode> lsAllNodes,
            List<LocalLVitem> lsLVignore,
            bool bLoose)
        {
            _static_this = this;
            _dictNodes = dictNodes;
            _lvClones = lvClones;
            _lvSameVol = lvSameVol;
            _lvUnique = lvUnique;
            _lsRootNodes = lsRootNodes;
            _lsAllNodes = lsAllNodes;
            _lsLVignore = lsLVignore;
            _bLoose = bLoose;
        }

        static internal void ClearMem()
        {
            Abort();
            _static_this = null;
        }

        static internal void Abort()
        {
            if (null != _static_this)
                _static_this._bThreadAbort = true;
        }

        static internal void InsertSizeMarkers(IList<LocalLVitem> listLVitems)
        {
            var nCount = listLVitems.Count;

            if (0 == nCount)
                return;

            var bUnique = (null != listLVitems[0].LocalTreeNode);
            var nInterval = (nCount < 100) ? 10 : (nCount < 1000) ? 25 : 50;

            InsertSizeMarkerStatic.Go(listLVitems, nCount - 1, bUnique, bAdd: true);

            var nInitial = nCount % nInterval;

            if (0 == nInitial)
                nInitial = nInterval;

            var nHalf = (nInterval >> 1);

            if (nCount - nInitial > nHalf)
            {
                for (var i = nCount - nInitial; i > nHalf; i -= nInterval)
                    InsertSizeMarkerStatic.Go(listLVitems, i, bUnique);
            }

            InsertSizeMarkerStatic.Go(listLVitems, 0, bUnique);            // Enter the Zeroth
        }

        internal void Step1(Action<double> reportProgress)
        {
            double nProgressNumerator = 0;
            double nProgressDenominator = 0;
            double nProgressItem = 0;
            const double nTotalProgressItems = 6;

            if (0 == _lsRootNodes.Count)
            {
                MBoxStatic.Assert(1305.6314m, false);
                return;
            }

            if (0 < _lsLVignore.Count)
            {
                var dtStart = DateTime.Now;
                var nMaxLevel = _lsLVignore.Max(i => ("" + i.SubItems[1].Text).ToInt() - 1);
                var sbMatch = new StringBuilder();

                foreach (var lvItem in _lsLVignore)
                    sbMatch.AppendLine(lvItem.Text);

                IgnoreNodeQuery(("" + sbMatch).ToLower(), nMaxLevel, _lsRootNodes[0]);
                Util.WriteLine("IgnoreNode " + (DateTime.Now - dtStart).TotalMilliseconds / 1000d + " seconds."); dtStart = DateTime.Now;
            }

            var dictIgnoreMark = new Dictionary<LocalTreeNode, LocalLVitem>();
            var dictNodes = new SortedDictionary<FolderKeyTuple, List<LocalTreeNode>>();

            foreach (var kvp in _dictNodes)                     // clone to remove ignored
            {                                                   // m_ vs local check is via List vs UList
                dictNodes.Add(kvp.Key, kvp.Value.ToList());     // clone pair.Value to remove ignored, using ToList() 
            }

            nProgressDenominator += _dictIgnoreNodes.Count;
            ++nProgressItem;

            foreach (var kvp in _dictIgnoreNodes)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem/nTotalProgressItems);

                var treeNode = kvp.Key;
                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)
                    MBoxStatic.Assert(99900, false);    // This check is new 3/7/15

                var lsTreeNodes = dictNodes.TryGetValue(nodeDatum.Key);

                if (null == lsTreeNodes)
                    continue;

                if (_bLoose)
                {
                    foreach (var treeNode_A in lsTreeNodes)
                        dictIgnoreMark.Add(treeNode_A, kvp.Value);

                    dictNodes.Remove(nodeDatum.Key);
                }
                else if (lsTreeNodes.Contains(treeNode))
                {
                    dictIgnoreMark.Add(treeNode, kvp.Value);
                    lsTreeNodes.Remove(treeNode);

                    if (0 == lsTreeNodes.Count)
                        dictNodes.Remove(nodeDatum.Key);
                }
            }

            var dictUnique = new SortedDictionary<FolderKeyTuple, LocalTreeNode>();
            nProgressDenominator += dictNodes.Count;
            ++nProgressItem;

            foreach (var kvp in dictNodes)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                if (App.LocalExit || _bThreadAbort)
                    return;

                var lsNodes = kvp.Value;

                if (0 == lsNodes.Count)
                {
                    MBoxStatic.Assert(1305.6315m, false);
                    continue;
                }
                
                if (1 < lsNodes.Count)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    var listKeep = new List<LocalTreeNode>();

                    foreach (var treeNode_A in lsNodes)
                    {
                        if (App.LocalExit || _bThreadAbort)
                            return;

                        if (null == treeNode_A)
                        {
                            MBoxStatic.Assert(99978, false);
                            continue;
                        }

                        var nodeDatum = treeNode_A.NodeDatum;

                        if (null == nodeDatum)      // added 2/13/15
                        {
                            MBoxStatic.Assert(99977, false);
                            return;
                        }

                        MBoxStatic.Assert(1305.6316m, 0 < nodeDatum.TotalLength);

                        if (lsNodes.Contains(treeNode_A.Parent) == false)
                            listKeep.Add(treeNode_A);
                    }

                    if (1 < listKeep.Count)
                    {
                        foreach (var treeNode_A in listKeep)
                        {
                            var nodeDatum = treeNode_A.NodeDatum;

                            if (null == nodeDatum)      // added 2/13/15
                            {
                                MBoxStatic.Assert(99976, false);
                                return;
                            }

                            nodeDatum.Clones = listKeep;
                        }
                    }
                    else
                    {
                        lsNodes = listKeep;  // kick off "else" logic below after deleting child clones
                    }
                }

                if (1 == lsNodes.Count)      // "else"
                {
                    var treeNode = lsNodes[0];
                    var nodeDatum = treeNode.NodeDatum;

                    if (null == nodeDatum)      // added 2/13/15
                    {
                        MBoxStatic.Assert(99975, false);
                        return;
                    }

                    if (0 < nodeDatum.FileCountHere)
                        dictUnique.Add(kvp.Key, treeNode);
                }
            }

            var dictClones = new SortedDictionary<FolderKeyTuple, List<LocalTreeNode>>();

            nProgressDenominator += _lsRootNodes.Count;
            ++nProgressItem;

            foreach (var treeNode in _lsRootNodes)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);
                
                if (App.LocalExit || _bThreadAbort)
                    return;

                DifferentVolsQuery(dictClones, treeNode);
            }

            nProgressDenominator += dictClones.Count;
            ++nProgressItem;

            foreach (var listNodes in dictClones)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                // load up listLVdiffVol

                if (App.LocalExit || _bThreadAbort)
                    return;

                var nClones = listNodes.Value.Count;

                if (0 == nClones)
                {
                    MBoxStatic.Assert(1305.6317m, false);
                    continue;
                }

                if (1 == nClones)
                    continue;           // keep the same-vol

                string str_nClones = null;

                if (2 < nClones)        // includes the subject node: only note three clones or more
                {
                    str_nClones = nClones.ToString("###,###");

                    foreach (var node in listNodes.Value)
                        node.ForeColor = UtilColor.LightBlue;
                }

                var lvItem = new LocalLVitem(new[] { string.Empty, str_nClones })
                {
                    TreeNodes = listNodes.Value,
                    ForeColor = listNodes.Value[0].ForeColor
                };

                LocalTreeNode nameNode = null;
                var nLevel = int.MaxValue;

                foreach (var treeNode in listNodes.Value)
                {
                    if (App.LocalExit || _bThreadAbort)
                        return;

                    if (treeNode.Level < nLevel)
                    {
                        nLevel = treeNode.Level;
                        nameNode = treeNode;
                    }

                    var nodeDatum = treeNode.NodeDatum;

                    if (null == nodeDatum)      // added 2/13/15
                    {
                        MBoxStatic.Assert(99974, false);
                        return;
                    }

                    nodeDatum.LVitem = lvItem;
                }

                lvItem.Text = nameNode.Text;
                MBoxStatic.Assert(1305.6318m, false == string.IsNullOrWhiteSpace(lvItem.Text));
                _lsLVdiffVol.Add(lvItem);
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

                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    MBoxStatic.Assert(99973, false);
                    return;
                }

                nodeDatum.LVitem = lvIgnoreItem;
                MBoxStatic.Assert(1305.6319m, nodeDatum.LVitem != null);

                if (null != nodeDatum.Clones)
                    nodeDatum.Clones.Remove(treeNode);
            }

            InsertSizeMarkers(_lsLVdiffVol);
            nProgressDenominator += dictUnique.Count;

            foreach (var kvp in dictUnique)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator);

                if (App.LocalExit || _bThreadAbort)
                    return;

                var treeNode = kvp.Value;

                MBoxStatic.Assert(1305.6321m, false == string.IsNullOrWhiteSpace(treeNode.Text));

                var lvItem = new LocalLVitem(treeNode.Text) { LocalTreeNode = treeNode };
                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    MBoxStatic.Assert(99972, false);
                    return;
                }

                MBoxStatic.Assert(1305.6322m, nodeDatum.FileCountHere > 0);
                SnowUniqueParents(treeNode);

                MBoxStatic.Assert(1305.6323m, treeNode.ForeColor == UtilColor.Empty);
                treeNode.ForeColor = UtilColor.Red;

                lvItem.ForeColor = treeNode.ForeColor;
                _lsLVunique.Add(lvItem);
                MBoxStatic.Assert(1305.6324m, nodeDatum.LVitem == null);
                nodeDatum.LVitem = lvItem;
            }

            InsertSizeMarkers(_lsLVunique);

            var lsSameVol = new List<LocalTreeNode>();

            if (0 < _lsRootNodes.Count)
            {
                var nCount = CountNodes.Go(_lsRootNodes);
                
                AddTreeToList.Go(_lsAllNodes, lsSameVol, _lsRootNodes);
                MBoxStatic.Assert(1305.6326m, _lsAllNodes.Count == nCount);
                Util.WriteLine("Step1_OnThread " + nCount);
            }

            lsSameVol.Sort((y, x) => x.NodeDatum.TotalLength.CompareTo(y.NodeDatum.TotalLength));
            nProgressDenominator += lsSameVol.Count;
            ++nProgressItem;

            foreach (var treeNode in lsSameVol)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                if (App.LocalExit || _bThreadAbort)
                    return;

                SnowUniqueParents(treeNode);

                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    MBoxStatic.Assert(99971, false);
                    return;
                }

                int nClones = 0;

                if (null != nodeDatum.Clones)
                    nClones = nodeDatum.Clones.Count;

                if (0 == nClones)
                    MBoxStatic.Assert(1305.6328m, false);

                string str_nClones = null;

                if (2 < nClones)
                    str_nClones = nClones.ToString("###,###");

                MBoxStatic.Assert(1305.6329m, false == string.IsNullOrWhiteSpace(treeNode.Text));

                var lvItem = new LocalLVitem(new[] { "" + treeNode.Text, str_nClones })
                {
                    TreeNodes = nodeDatum.Clones,
                    ForeColor = UtilColor.Firebrick,
                    BackColor = treeNode.BackColor
                };

                _lsLVsameVol.Add(lvItem);
                nodeDatum.LVitem = lvItem;
            }

            InsertSizeMarkers(_lsLVsameVol);
        }

        internal void Step2()
        {
            if (App.LocalExit || _bThreadAbort)
                return;

            MBoxStatic.Assert(1305.6333m, null == _lvClones.Items);
            _lvClones.Items = _lsLVdiffVol.ToArray();
            _lvClones.Invalidate();

            if (App.LocalExit || _bThreadAbort)
                return;

            MBoxStatic.Assert(1305.6334m, null == _lvUnique.Items);
            _lvUnique.Items =_lsLVunique.ToArray();
            _lvUnique.Invalidate();

            if (App.LocalExit || _bThreadAbort)
                return;

            MBoxStatic.Assert(1305.6335m, null == _lvSameVol.Items);
            _lvSameVol.Items = _lsLVsameVol.ToArray();
            _lvSameVol.Invalidate();
            _static_this = null;
        }

        // If an outer directory is cloned then all the inner ones are part of the outer clone and their clone status is redundant.
        // Breadth-first.
        void DifferentVolsQuery(
            IDictionary<FolderKeyTuple, List<LocalTreeNode>> dictClones,
            LocalTreeNode treeNode,
            LocalTreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used like a bool).
            // provisional.

            var nodeDatum = treeNode.NodeDatum;

            if (null == nodeDatum)
            {
                MBoxStatic.Assert(99913, false);    // This check is new 2/23/15
                return;
            }

            var listClones = nodeDatum.Clones;
            var nLength = nodeDatum.TotalLength;

            if (0 == nLength)
            {
                treeNode.ForeColor = UtilColor.LightGray;
                nodeDatum.Clones = null;
            }

            if ((null != listClones) &&
                (0 < listClones.Count) &&
                (null == rootClone))
            {
                rootClone = treeNode;

                var lsTreeNodes = dictClones.TryGetValue(nodeDatum.Key);

                if (null != lsTreeNodes)
                {
                    MBoxStatic.Assert(1305.6305m, lsTreeNodes == listClones);
                    MBoxStatic.Assert(1305.6307m, lsTreeNodes[0].ForeColor == treeNode.ForeColor);
                }
                else
                {
                    dictClones.Add(nodeDatum.Key, listClones);

                    // Test to see if clones are on separate volumes.

                    var rootNode = treeNode.Root;

                    if (false == rootNode.NodeDatum is RootNodeDatum)      // added 2/13/15, got hit on 5/4/15
                    {
                        MBoxStatic.Assert(99970, false);
                        return;
                    }

                    var rootNodeDatum = (RootNodeDatum)rootNode.NodeDatum;

                    MBoxStatic.Assert(1305.6308m, UtilColor.Empty == treeNode.ForeColor);
                    treeNode.ForeColor = UtilColor.Firebrick;

                    foreach (var subnode in listClones)
                    {
                        if (App.LocalExit || _bThreadAbort)
                            return;

                        MBoxStatic.Assert(1305.6309m, subnode.NodeDatum.Key.Equals(nodeDatum.Key));

                        var rootNode_A = subnode.Root;

                        if (rootNode == rootNode_A)
                            continue;

                        if (false == rootNode_A.NodeDatum is RootNodeDatum)      // added 2/13/15, got hit on 5/4/15
                        {
                            MBoxStatic.Assert(99999, false);
                            return;
                        }

                        var rootNodeDatum_A = (RootNodeDatum)rootNode_A.NodeDatum;

                        if (false == string.IsNullOrWhiteSpace(rootNodeDatum.VolumeGroup) &&
                            (rootNodeDatum.VolumeGroup == rootNodeDatum_A.VolumeGroup))
                        {
                            continue;
                        }

                        MBoxStatic.Assert(1305.6311m, treeNode.ForeColor == UtilColor.Firebrick);
                        treeNode.ForeColor = UtilColor.SteelBlue;
                        break;
                    }

                    foreach (var subNode in listClones)
                    {
                        var nodeDatum_A = subNode.NodeDatum;

                        if (null == nodeDatum_A)      // added 2/13/15
                        {
                            MBoxStatic.Assert(99994, false);
                            return;
                        }

                        subNode.ForeColor = treeNode.ForeColor;
                    }
                }
            }

            if (null == treeNode.Nodes)
                return;

            foreach (var subNode in treeNode.Nodes)
            {
                if (App.LocalExit || _bThreadAbort)
                    return;

                DifferentVolsQuery(dictClones, subNode, rootClone);
            }
        }

        void IgnoreNodeAndSubnodes(LocalLVitem lvItem, LocalTreeNode treeNode_in, bool bContinue = false)
        {
            var treeNode = treeNode_in;

            do
            {
                if (null != _dictIgnoreNodes.TryGetValue(treeNode))
                    continue;

                MBoxStatic.Assert(1305.6312m, null != lvItem);
                _dictIgnoreNodes.Add(treeNode, lvItem);

                if ((null != treeNode.Nodes) &&
                    (0 < treeNode.Nodes.Count))
                {
                    IgnoreNodeAndSubnodes(lvItem, treeNode.Nodes[0], bContinue: true);
                }
            }
            while (bContinue &&
                (null != (treeNode = treeNode.NextNode)));
        }

        void IgnoreNodeQuery(string sbMatch, int nMaxLevel, LocalTreeNode treeNode_in)
        {
            if (treeNode_in.Level > nMaxLevel)
                return;

            var treeNode = treeNode_in;

            do
            {
                if (App.LocalExit || _bThreadAbort)
                    return;

                if (sbMatch.Contains(treeNode.Text.ToLower()))
                {
                    if (_lsLVignore
                        .Where(lvItem => treeNode.Level == (("" + lvItem.SubItems[1].Text).ToInt() - 1))
                        .Any(lvItem => ("" + lvItem.Text).Equals(treeNode.Text,
                            StringComparison.InvariantCultureIgnoreCase)))
                    {
                        MBoxStatic.Assert(99898, false);    // replace the Tag field with an LVitem
                  //      IgnoreNodeAndSubnodes((LocalLVitem)lvItem.Tag, treeNode);
                    }
                }

                if ((null != treeNode.Nodes) &&
                    (0 < treeNode.Nodes.Count))
                {
                    IgnoreNodeQuery(sbMatch, nMaxLevel, treeNode.Nodes[0]);
                }
            }
            while (null != (treeNode = treeNode.NextNode));
        }

        static void SnowUniqueParents(LocalTreeNode treeNode)
        {
            LocalTreeNode parentNode = treeNode.Parent;

            while (null != parentNode)
            {
                parentNode.BackColor = UtilColor.DarkRedBG;

                var nodeDatum = parentNode.NodeDatum;

                if (null != nodeDatum.LVitem)
                    nodeDatum.LVitem.BackColor = parentNode.BackColor;

                MBoxStatic.Assert(1305.6313m,
                    (parentNode.ForeColor == UtilColor.Empty) ==
                    (null == nodeDatum.LVitem));

                parentNode = parentNode.Parent;
            }
        }

        // the following are form vars referenced internally, thus keeping their form_ and m_ prefixes
        readonly ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>
            _dictNodes = null;
        readonly LocalLV _lvClones = null;
        readonly LocalLV _lvSameVol = null;
        readonly LocalLV _lvUnique = null;
        readonly IReadOnlyList<LocalTreeNode> _lsRootNodes = null;
        readonly IList<LocalTreeNode> _lsAllNodes = null;
        readonly IList<LocalLVitem> _lsLVignore = null;

        // the following are "local" to this object, and do not have m_ prefixes because they do not belong to the form.
        readonly IList<LocalLVitem> _lsLVunique = new List<LocalLVitem>();
        readonly IList<LocalLVitem> _lsLVsameVol = new List<LocalLVitem>();
        readonly IList<LocalLVitem> _lsLVdiffVol = new List<LocalLVitem>();
        readonly IDictionary<LocalTreeNode, LocalLVitem> _dictIgnoreNodes = new Dictionary<LocalTreeNode, LocalLVitem>();
        readonly bool _bLoose = false;

        bool _bThreadAbort = false;
        static Collate _static_this = null;
    }
}
