﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Windows;

namespace DoubleFile
{
    partial class Collate
    {
        internal Collate
            Abort() { _bAborted = true; return this; }
        bool _bAborted = false;

        internal Collate(
            ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>> dictNodes,
            LocalLVVM lvClones,
            LocalLVVM lvSameVol,
            LocalLVVM lvSolitary,
            IReadOnlyList<LocalTreeNode> lsRootNodes,
            List<LocalTreeNode> lsAllNodes,
            List<LocalLVitemVM> lsLVignore,
            bool bLoose)
        {
            _dictNodes = dictNodes;
            _lvClones = lvClones;
            _lvSameVol = lvSameVol;
            _lvSolitary = lvSolitary;
            _lsRootNodes = lsRootNodes;
            _lsAllNodes = lsAllNodes;
            _lsLVignore = lsLVignore;
            _bLoose = bLoose;
        }

        static internal void InsertSizeMarkers(IList<LocalLVitemVM> listLVitems)
        {
            var nCount = listLVitems.Count;

            if (0 == nCount)
                return;

            var bUnique = (1 == listLVitems.Count);
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
                Util.Assert(1305.6314m, false);
                return;
            }

            if (0 < _lsLVignore.Count)
            {
                var dtStart = DateTime.Now;
                var nMaxLevel = _lsLVignore.Max(i => ("" + i.SubItems[1]).ToInt() - 1);
                var sbMatch = new StringBuilder();

                foreach (var lvItem in _lsLVignore)
                    sbMatch.AppendLine(lvItem.Folder);

                IgnoreNodeQuery(("" + sbMatch).ToLower(), nMaxLevel, _lsRootNodes[0]);
                Util.WriteLine("IgnoreNode " + (DateTime.Now - dtStart).TotalMilliseconds / 1000d + " seconds."); dtStart = DateTime.Now;
            }

            var dictIgnoreMark = new Dictionary<LocalTreeNode, LocalLVitemVM>();
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
                    Util.Assert(99900, false);    // This check is new 3/7/15

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

            foreach (var kvp in dictNodes.Reverse())
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    _bAborted)
                {
                    return;
                }

                var lsNodes = kvp.Value;

                if (0 == lsNodes.Count)
                {
                    Util.Assert(1305.6315m, false);
                    continue;
                }
                
                if (1 < lsNodes.Count)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    var listKeep = new List<LocalTreeNode>();

                    foreach (var treeNode_A in lsNodes)
                    {
                        if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                            _bAborted)
                        {
                            return;
                        }

                        if (null == treeNode_A)
                        {
                            Util.Assert(99978, false);
                            continue;
                        }

                        var nodeDatum = treeNode_A.NodeDatum;

                        if (null == nodeDatum)      // added 2/13/15
                        {
                            Util.Assert(99977, false);
                            return;
                        }

                        Util.Assert(1305.6316m, 0 < nodeDatum.TotalLength);

                        if (false == lsNodes.Contains(treeNode_A.Parent))
                            listKeep.Add(treeNode_A);
                    }

                    if (1 < listKeep.Count)
                    {
                        foreach (var treeNode_A in listKeep)
                        {
                            var nodeDatum = treeNode_A.NodeDatum;

                            if (null == nodeDatum)      // added 2/13/15
                            {
                                Util.Assert(99976, false);
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
                        Util.Assert(99975, false);
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
                
                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    _bAborted)
                {
                    return;
                }

                DifferentVolsQuery(dictClones, treeNode);
            }

            nProgressDenominator += dictClones.Count;
            ++nProgressItem;

            foreach (var listNodes in dictClones.Reverse())
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                // load up listLVdiffVol

                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    _bAborted)
                {
                    return;
                }

                var nClones = listNodes.Value.Count;

                if (0 == nClones)
                {
                    Util.Assert(1305.6317m, false);
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

                var lvItem = new LocalLVitemVM(new[] { "", str_nClones })
                {
                    TreeNodes = listNodes.Value,
                    ForeColor = listNodes.Value[0].ForeColor
                };

                LocalTreeNode nameNode = null;
                var nLevel = int.MaxValue;

                foreach (var treeNode in listNodes.Value)
                {
                    if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                        _bAborted)
                    {
                        return;
                    }

                    if (treeNode.Level < nLevel)
                    {
                        nLevel = treeNode.Level;
                        nameNode = treeNode;
                    }

                    var nodeDatum = treeNode.NodeDatum;

                    if (null == nodeDatum)      // added 2/13/15
                    {
                        Util.Assert(99974, false);
                        return;
                    }

                    nodeDatum.LVitem = lvItem;
                }

                lvItem.Folder = nameNode.Text;
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
                    Util.Assert(99973, false);
                    return;
                }

                nodeDatum.LVitem = lvIgnoreItem;
                Util.Assert(1305.6319m, nodeDatum.LVitem != null);
                nodeDatum.Clones?.Remove(treeNode);
            }

            InsertSizeMarkers(_lsLVdiffVol);
            nProgressDenominator += dictUnique.Count;

            foreach (var kvp in dictUnique.Reverse())
            {
                reportProgress(++nProgressNumerator / nProgressDenominator);

                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    _bAborted)
                {
                    return;
                }

                var treeNode = kvp.Value;

                Util.Assert(1305.6321m, false == string.IsNullOrWhiteSpace(treeNode.Text));

                var lvItem = new LocalLVitemVM(new[] { treeNode.Text }) { TreeNodes = new[] { treeNode } };
                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    Util.Assert(99972, false);
                    return;
                }

                Util.Assert(1305.6322m, 0 < nodeDatum.FileCountHere);
                SnowUniqueParents(treeNode);
                Util.Assert(1305.6323m, UtilColor.Empty == treeNode.ForeColor);
                treeNode.ForeColor = UtilColor.Red;
                lvItem.ForeColor = treeNode.ForeColor;
                _lsLVsolitary.Add(lvItem);
                Util.Assert(1305.6324m, null == nodeDatum.LVitem);
                nodeDatum.LVitem = lvItem;
            }

            InsertSizeMarkers(_lsLVsolitary);

            var lsSameVol = new List<LocalTreeNode>();

            if (0 < _lsRootNodes.Count)
            {
                var nCount = CountNodes.Go(_lsRootNodes);
                
                AddTreeToList.Go(_lsAllNodes, lsSameVol, _lsRootNodes);
                Util.Assert(1305.6326m, _lsAllNodes.Count == nCount);
                Util.WriteLine("Step1_OnThread " + nCount);
            }

            lsSameVol.Sort((y, x) => x.NodeDatum.TotalLength.CompareTo(y.NodeDatum.TotalLength));
            nProgressDenominator += lsSameVol.Count;
            ++nProgressItem;

            foreach (var treeNode in lsSameVol)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    _bAborted)
                {
                    return;
                }

                SnowUniqueParents(treeNode);

                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    Util.Assert(99971, false);
                    return;
                }

                var nClones = nodeDatum.Clones?.Count ?? 0;

                if (0 == nClones)
                    Util.Assert(1305.6328m, false);

                string str_nClones = null;

                if (2 < nClones)
                    str_nClones = nClones.ToString("###,###");

                Util.Assert(1305.6329m, false == string.IsNullOrWhiteSpace(treeNode.Text));

                var lvItem = new LocalLVitemVM(new[] { "" + treeNode.Text, str_nClones })
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
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                _bAborted)
            {
                return;
            }

            Util.Assert(1305.6333m, 0 == _lvClones.Items.Count);

            if (0 < _lsLVdiffVol.Count)
                _lvClones.Add(_lsLVdiffVol);

            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                _bAborted)
            {
                return;
            }

            Util.Assert(1305.6334m, 0 == _lvSolitary.Items.Count);

            if (0 < _lsLVsolitary.Count)
                _lvSolitary.Add(_lsLVsolitary);

            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                _bAborted)
            {
                return;
            }

            Util.Assert(1305.6335m, 0 == _lvSameVol.Items.Count);

            if (0 < _lsLVsameVol.Count)
                _lvSameVol.Add(_lsLVsameVol);
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
                Util.Assert(99913, false);    // This check is new 2/23/15
                return;
            }

            var listClones = nodeDatum.Clones;
            var nLength = nodeDatum.TotalLength;

            if (0 == nLength)
            {
                treeNode.ForeColor = UtilColor.LightGray;
                nodeDatum.Clones = null;
            }

            if ((0 < (listClones?.Count ?? 0)) &&
                (null == rootClone))
            {
                rootClone = treeNode;

                var lsTreeNodes = dictClones.TryGetValue(nodeDatum.Key);

                if (null != lsTreeNodes)
                {
                    Util.Assert(1305.6305m, lsTreeNodes == listClones);
                    Util.Assert(1305.6307m, lsTreeNodes[0].ForeColor == treeNode.ForeColor);
                }
                else
                {
                    dictClones.Add(nodeDatum.Key, listClones);

                    // Test to see if clones are on separate volumes.

                    var rootNode = treeNode.Root;
                    var rootNodeDatum = rootNode.NodeDatum.As<RootNodeDatum>();

                    if (null == rootNodeDatum)      // added 2/13/15, got hit on 5/4/15
                    {
                        Util.Assert(99970, false);
                        return;
                    }

                    Util.Assert(1305.6308m, UtilColor.Empty == treeNode.ForeColor);
                    treeNode.ForeColor = UtilColor.Firebrick;

                    foreach (var subnode in listClones)
                    {
                        if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                            _bAborted)
                        {
                            return;
                        }

                        Util.Assert(1305.6309m, subnode.NodeDatum.Key.Equals(nodeDatum.Key));

                        var rootNodeA = subnode.Root;

                        if (rootNode == rootNodeA)
                            continue;

                        var rootNodeDatumA = rootNodeA.NodeDatum.As<RootNodeDatum>();

                        if (null == rootNodeDatumA)      // added 2/13/15, got hit on 5/4/15
                        {
                            Util.Assert(99999, false);
                            return;
                        }

                        if (false == string.IsNullOrWhiteSpace(rootNodeDatum.VolumeGroup) &&
                            (rootNodeDatum.VolumeGroup == rootNodeDatumA.VolumeGroup))
                        {
                            continue;
                        }

                        Util.Assert(1305.6311m, treeNode.ForeColor == UtilColor.Firebrick);
                        treeNode.ForeColor = UtilColor.SteelBlue;
                        break;
                    }

                    foreach (var subNode in listClones)
                    {
                        var nodeDatum_A = subNode.NodeDatum;

                        if (null == nodeDatum_A)      // added 2/13/15
                        {
                            Util.Assert(99994, false);
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
                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    _bAborted)
                {
                    return;
                }

                DifferentVolsQuery(dictClones, subNode, rootClone);
            }
        }

        void IgnoreNodeAndSubnodes(LocalLVitemVM lvItem, LocalTreeNode treeNode_in, bool bContinue = false)
        {
            var treeNode = treeNode_in;

            do
            {
                if (null != _dictIgnoreNodes.TryGetValue(treeNode))
                    continue;

                Util.AssertNutNull(1305.6312m, lvItem);
                _dictIgnoreNodes.Add(treeNode, lvItem);

                if (0 < (treeNode.Nodes?.Count ?? 0))
                    IgnoreNodeAndSubnodes(lvItem, treeNode.Nodes[0], bContinue: true);
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
                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    _bAborted)
                {
                    return;
                }

                if (sbMatch.Contains(treeNode.Text.ToLower()))
                {
                    if (_lsLVignore
                        .Where(lvItem => treeNode.Level == (("" + lvItem.SubItems[1]).ToInt() - 1))
                        .Any(lvItem => ("" + lvItem.Folder).Equals(treeNode.Text,
                            StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Util.Assert(99898, false);    // replace the Tag field with an LVitem
                  //      IgnoreNodeAndSubnodes((LocalLVitem)lvItem.Tag, treeNode);
                    }
                }

                if (0 < (treeNode.Nodes?.Count ?? 0))
                    IgnoreNodeQuery(sbMatch, nMaxLevel, treeNode.Nodes[0]);
            }
            while (null !=
                (treeNode = treeNode.NextNode));
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

                Util.Assert(1305.6313m,
                    (parentNode.ForeColor == UtilColor.Empty) ==
                    (null == nodeDatum.LVitem));

                parentNode = parentNode.Parent;
            }
        }

        // the following are form vars referenced internally, thus keeping their form_ and m_ prefixes
        readonly ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>
            _dictNodes = null;
        readonly LocalLVVM _lvClones = null;
        readonly LocalLVVM _lvSameVol = null;
        readonly LocalLVVM _lvSolitary = null;
        readonly IReadOnlyList<LocalTreeNode> _lsRootNodes = null;
        readonly IList<LocalTreeNode> _lsAllNodes = null;
        readonly IList<LocalLVitemVM> _lsLVignore = null;

        // the following are "local" to this object, and do not have m_ prefixes because they do not belong to the form.
        readonly IList<LocalLVitemVM> _lsLVsolitary = new List<LocalLVitemVM>();
        readonly IList<LocalLVitemVM> _lsLVsameVol = new List<LocalLVitemVM>();
        readonly IList<LocalLVitemVM> _lsLVdiffVol = new List<LocalLVitemVM>();
        readonly IDictionary<LocalTreeNode, LocalLVitemVM> _dictIgnoreNodes = new Dictionary<LocalTreeNode, LocalLVitemVM>();
        readonly bool _bLoose = false;
    }
}
