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
            IEnumerable<LocalTreeNode> ieRootNodes,
            List<LocalTreeNode> lsTreeNodes,
            List<LocalLVitem> lsLVignore,
            bool bLoose)
        {
            _static_this = this;
            _dictNodes = dictNodes;
            _lvClones = lvClones;
            _lvSameVol = lvSameVol;
            _lvUnique = lvUnique;
            _ieRootNodes = ieRootNodes;
            _lsTreeNodes = lsTreeNodes;
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

        static internal void InsertSizeMarkers(List<LocalLVitem> listLVitems)
        {
            if (listLVitems.IsEmpty())
                return;

            var bUnique = (null != listLVitems[0].LocalTreeNode);
            var nCount = listLVitems.Count;
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

            if (_ieRootNodes.IsEmpty())
            {
                MBoxStatic.Assert(1305.6314, false);
                return;
            }

            if (false == _lsLVignore.IsEmpty())
            {
                var dtStart = DateTime.Now;
                var nMaxLevel = _lsLVignore.Max(i => int.Parse(i.SubItems[1].Text) - 1);
                var sbMatch = new StringBuilder();

                foreach (var lvItem in _lsLVignore)
                    sbMatch.AppendLine(lvItem.Text);

                IgnoreNodeQuery(("" + sbMatch).ToLower(), nMaxLevel, _ieRootNodes.ElementAt(0));
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

                    if (lsTreeNodes.IsEmpty())
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

                var listNodes = kvp.Value;

                if (1 > listNodes.Count)
                {
                    MBoxStatic.Assert(1305.6315, false);
                    continue;
                }
                
                if (1 < listNodes.Count)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    var listKeep = new List<LocalTreeNode>();

                    foreach (var treeNode_A in listNodes)
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

                        MBoxStatic.Assert(1305.6316, 0 < nodeDatum.TotalLength);

                        if (listNodes.Contains(treeNode_A.Parent) == false)
                            listKeep.Add(treeNode_A);
                    }

                    if (listKeep.Count > 1)
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
                        listNodes = listKeep;  // kick off "else" logic below after deleting child clones
                    }
                }

                if (1 == listNodes.Count)               // "else"
                {
                    var treeNode = listNodes[0];
                    var nodeDatum = treeNode.NodeDatum;

                    if (null == nodeDatum)      // added 2/13/15
                    {
                        MBoxStatic.Assert(99975, false);
                        return;
                    }

                    if (0 < nodeDatum.FilesHere)
                        dictUnique.Add(kvp.Key, treeNode);
                }
            }

            var dictClones = new SortedDictionary<FolderKeyTuple, List<LocalTreeNode>>();

            nProgressDenominator += _ieRootNodes.Count();
            ++nProgressItem;

            foreach (var treeNode in _ieRootNodes)
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
                    MBoxStatic.Assert(1305.6317, false);
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
                MBoxStatic.Assert(1305.6318, false == string.IsNullOrWhiteSpace(lvItem.Text));
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
                MBoxStatic.Assert(1305.6319, nodeDatum.LVitem != null);

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

                MBoxStatic.Assert(1305.6321, false == string.IsNullOrWhiteSpace(treeNode.Text));

                var lvItem = new LocalLVitem(treeNode.Text) { LocalTreeNode = treeNode };
                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    MBoxStatic.Assert(99972, false);
                    return;
                }

                MBoxStatic.Assert(1305.6322, nodeDatum.FilesHere > 0);
                SnowUniqueParents(treeNode);

                MBoxStatic.Assert(1305.6323, treeNode.ForeColor == UtilColor.Empty);
                treeNode.ForeColor = UtilColor.Red;

                lvItem.ForeColor = treeNode.ForeColor;
                _lsLVunique.Add(lvItem);
                MBoxStatic.Assert(1305.6324, nodeDatum.LVitem == null);
                nodeDatum.LVitem = lvItem;
            }

            InsertSizeMarkers(_lsLVunique);

            var listSameVol = new List<LocalTreeNode>();

            if (false == _ieRootNodes.IsEmpty())
            {
                var nCount = CountNodes.Go(_ieRootNodes);
                var nCount_A = new AddTreeToList(_lsTreeNodes, listSameVol).Go(_ieRootNodes).Count;

                MBoxStatic.Assert(1305.6325, nCount_A == nCount);
                MBoxStatic.Assert(1305.6326, _lsTreeNodes.Count == nCount);
                Util.WriteLine("Step1_OnThread " + nCount);
            }

            listSameVol.Sort((y, x) => x.NodeDatum.TotalLength.CompareTo(y.NodeDatum.TotalLength));
            nProgressDenominator += listSameVol.Count;
            ++nProgressItem;

            foreach (var treeNode in listSameVol)
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
                    MBoxStatic.Assert(1305.6328, false);

                string str_nClones = null;

                if (2 < nClones)
                    str_nClones = nClones.ToString("###,###");

                MBoxStatic.Assert(1305.6329, false == string.IsNullOrWhiteSpace(treeNode.Text));

                var lvItem = new LocalLVitem(new[] {"" + treeNode.Text, str_nClones})
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

            MBoxStatic.Assert(1305.6333, null == _lvClones.Items);
            _lvClones.Items = _lsLVdiffVol.ToArray();
            _lvClones.Invalidate();

            if (App.LocalExit || _bThreadAbort)
                return;

            MBoxStatic.Assert(1305.6334, null == _lvUnique.Items);
            _lvUnique.Items =_lsLVunique.ToArray();
            _lvUnique.Invalidate();

            if (App.LocalExit || _bThreadAbort)
                return;

            MBoxStatic.Assert(1305.6335, null == _lvSameVol.Items);
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
                (false == listClones.IsEmpty()) &&
                (null == rootClone))
            {
                rootClone = treeNode;

                var lsTreeNodes = dictClones.TryGetValue(nodeDatum.Key);

                if (null != lsTreeNodes)
                {
                    MBoxStatic.Assert(1305.6305, lsTreeNodes == listClones);
                    MBoxStatic.Assert(1305.6307, lsTreeNodes[0].ForeColor == treeNode.ForeColor);
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

                    MBoxStatic.Assert(1305.6308, UtilColor.Empty == treeNode.ForeColor);
                    treeNode.ForeColor = UtilColor.Firebrick;

                    foreach (var subnode in listClones)
                    {
                        if (App.LocalExit || _bThreadAbort)
                            return;

                        MBoxStatic.Assert(1305.6309, subnode.NodeDatum.Key.Equals(nodeDatum.Key));

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

                        MBoxStatic.Assert(1305.6311, treeNode.ForeColor == UtilColor.Firebrick);
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

                MBoxStatic.Assert(1305.6312, null != lvItem);
                _dictIgnoreNodes.Add(treeNode, lvItem);

                if ((null != treeNode.Nodes) &&
                    (false == treeNode.Nodes.IsEmpty()))
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
                        .Where(lvItem => treeNode.Level == (int.Parse(lvItem.SubItems[1].Text) - 1))
                        .Any(lvItem => ((string)lvItem.Text).Equals(treeNode.Text,
                            StringComparison.InvariantCultureIgnoreCase)))
                    {
                        MBoxStatic.Assert(99898, false);    // replace the Tag field with an LVitem
                  //      IgnoreNodeAndSubnodes((LocalLVitem)lvItem.Tag, treeNode);
                    }
                }

                if ((null != treeNode.Nodes) &&
                    (false == treeNode.Nodes.IsEmpty()))
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

                MBoxStatic.Assert(1305.6313,
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
        readonly IEnumerable<LocalTreeNode> _ieRootNodes = null;
        readonly List<LocalTreeNode> _lsTreeNodes = null;
        readonly List<LocalLVitem> _lsLVignore = null;

        // the following are "local" to this object, and do not have m_ prefixes because they do not belong to the form.
        readonly List<LocalLVitem> _lsLVunique = new List<LocalLVitem>();
        readonly List<LocalLVitem> _lsLVsameVol = new List<LocalLVitem>();
        readonly List<LocalLVitem> _lsLVdiffVol = new List<LocalLVitem>();
        readonly IDictionary<LocalTreeNode, LocalLVitem> _dictIgnoreNodes = new Dictionary<LocalTreeNode, LocalLVitem>();
        readonly bool _bLoose = false;

        bool _bThreadAbort = false;
        static Collate _static_this = null;
    }
}
