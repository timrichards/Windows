using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    using static UtilColorcode;

    partial class Collate
    {
        internal Collate
            Abort() { _bAborted = true; return this; }
        bool _bAborted = false;

        internal Collate(
            IDictionary<int, List<LocalTreeNode>> dictNodes,
            UC_ClonesVM lvClones,
            UC_ClonesVM lvSameVol,
            UC_ClonesVM lvSolitary,
            IReadOnlyList<LocalTreeNode> lsRootNodes,
            List<LocalTreeNode> lsAllNodes,
            List<LVitem_ClonesVM> lsLVignore,
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

        static internal void InsertSizeMarkers(IList<LVitem_ClonesVM> listLVitems)
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

        IDictionary<int, LocalTreeNode>
            Step1_CreateDictSolitary_NodeDatumClones(Action reportProgress)
        {
            var dictSolitary = new SortedDictionary<int, LocalTreeNode>();

            foreach (var kvp in _dictNodes)
            {
                reportProgress();

                var lsNodes = kvp.Value;

                if (1 < lsNodes.Count)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    var lsKeep = new List<LocalTreeNode> { };

                    foreach (var treeNode_A in lsNodes)
                    {
                        if (false == lsNodes.Contains(treeNode_A.Parent))
                            lsKeep.Add(treeNode_A);
                    }

                    if (1 < lsKeep.Count)
                    {
                        foreach (var treeNode_A in lsKeep)
                            treeNode_A.NodeDatum.Clones = lsKeep;
                    }
                    else
                    {
                        lsNodes = lsKeep;  // kick off "else" logic below after deleting child clones
                    }
                }

                if (1 == lsNodes.Count)      // "else"
                {
                    var treeNode = lsNodes[0];

                    if (0 < treeNode.NodeDatum.FileCountHere)
                        dictSolitary.Add(kvp.Key, treeNode);
                }
            }

            return dictSolitary;
        }

        // If an outer directory is cloned then all the inner ones are part of the outer clone and their clone status is redundant.
        // Breadth-first.
        void Step2_DictClonesAdd_MarkIfAllOneVol(
            IDictionary<int, List<LocalTreeNode>> dictClones,
            LocalTreeNode treeNode,
            LocalTreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used like a bool).
            // provisional.

            var nodeDatum = treeNode.NodeDatum;
            var listClones = nodeDatum.Clones;
            var nLength = nodeDatum.LengthTotal;

            if (0 == nLength)
            {
                treeNode.ColorcodeFG = ZeroLengthFolder;
                nodeDatum.Clones = null;
            }

            if ((0 < (listClones?.Count ?? 0)) &&
                (null == rootClone))
            {
                rootClone = treeNode;

                var lsTreeNodes = dictClones.TryGetValue(nodeDatum.Hash_AllFiles);

                if (null != lsTreeNodes)
                {
                    Util.Assert(99971, lsTreeNodes == listClones);
                    Util.Assert(99913, lsTreeNodes[0].ColorcodeFG == treeNode.ColorcodeFG);
                }
                else
                {
                    dictClones.Add(nodeDatum.Hash_AllFiles, listClones);

                    // Test to see if clones are on separate volumes.

                    var rootNode = treeNode.Root;
                    var rootNodeDatum = rootNode.RootNodeDatum;

                    Util.Assert(99970, Transparent == treeNode.ColorcodeFG);
                    treeNode.ColorcodeFG = AllOnOneVolume;

                    foreach (var subnode in listClones)
                    {
                        if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                            _bAborted)
                        {
                            return;
                        }

                        Util.Assert(99999, subnode.NodeDatum.Hash_AllFiles.Equals(nodeDatum.Hash_AllFiles));

                        var rootNodeA = subnode.Root;

                        if (rootNode == rootNodeA)
                            continue;

                        var rootNodeDatumA = rootNodeA.RootNodeDatum;

                        if (false == string.IsNullOrWhiteSpace(rootNodeDatum.LVitemProjectVM.VolumeGroup) &&
                            (rootNodeDatum.LVitemProjectVM.VolumeGroup == rootNodeDatumA.LVitemProjectVM.VolumeGroup))
                        {
                            continue;
                        }

                        Util.Assert(1305.6311m, treeNode.ColorcodeFG == AllOnOneVolume);
                        treeNode.ColorcodeFG = OneCloneSepVolume;
                        break;
                    }

                    foreach (var subNode in listClones)
                        subNode.ColorcodeFG = treeNode.ColorcodeFG;
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

                Step2_DictClonesAdd_MarkIfAllOneVol(dictClones, subNode, rootClone);             // recurse
            }
        }

        void Step3_UnmarkSolitaryChildrenOfClones(IDictionary<int, LocalTreeNode> dictSolitary)
        {
            // A parent may be cloned and its child may be solitary: files may be distributed differently than in the clone.
            foreach (var kvp in dictSolitary.ToList())
            {
                var parent = kvp.Value.Parent;
                var nParentCloneColor = 0;

                while (null != parent)
                {
                    if (null != parent.NodeDatum.Clones)
                    {
                        nParentCloneColor =
                            (parent.ColorcodeFG == AllOnOneVolume)
                            ? SolitaryOneVolParent
                            : SolitaryClonedParent;

                        break;
                    }

                    parent = parent.Parent;
                }

                if (0 != nParentCloneColor)
                {
                    while (null != parent)
                    {
                        if (null != parent.NodeDatum.Clones)
                            break;

                        parent.ColorcodeFG = nParentCloneColor;
                        parent = parent.Parent;
                    }

                    dictSolitary.Remove(kvp.Key);
                }
            }
        }

        void Step4_Create_lsLVdiffVol_LVitem_ClonesVM(
            IDictionary<int, List<LocalTreeNode>> dictClones, ListUpdater<bool> nicknameUpdater, Action reportProgress)
        {
            foreach (var kvp in dictClones)
            {
                reportProgress();

                var nClones = kvp.Value.Count;

                if (0 == nClones)
                {
                    Util.Assert(1305.6317m, false);
                    continue;
                }

                Util.Assert(99591, new[] { OneCloneSepVolume, AllOnOneVolume }.Contains(kvp.Value[0].ColorcodeFG));

                if (1 == nClones)
                    continue;

                if ((3 <= nClones) &&
                    (OneCloneSepVolume == kvp.Value[0].ColorcodeFG))
                {
                    foreach (var node in kvp.Value)
                        node.ColorcodeFG = ManyClonesSepVolume;
                }

                var lvItem = new LVitem_ClonesVM(kvp.Value, nicknameUpdater);

                foreach (var treeNode in kvp.Value)
                    treeNode.NodeDatum.LVitem = lvItem;

                _lsLVdiffVol.Add(lvItem);
            }

            InsertSizeMarkers(_lsLVdiffVol);
        }

        internal void Go(Action<double> reportProgress_)
        {
            double nProgressNumerator = 0;
            double nProgressDenominator = 0;
            double nProgressItem = 0;
            const double knTotalProgressItems = 6;

            Action reportProgress = () => reportProgress_(++nProgressNumerator / nProgressDenominator * nProgressItem / knTotalProgressItems);


            // Step1_CreateDictSolitary_NodeDatumClones
            nProgressDenominator += _dictNodes.Count;
            ++nProgressItem;

            var dictSolitary = Step1_CreateDictSolitary_NodeDatumClones(reportProgress);


            // Step2_DictClonesAdd_MarkIfAllOneVol
            nProgressDenominator += _lsRootNodes.Count;
            ++nProgressItem;

            var dictClones = new SortedDictionary<int, List<LocalTreeNode>>();

            foreach (var rootNode in _lsRootNodes)
            {
                reportProgress();
                Step2_DictClonesAdd_MarkIfAllOneVol(dictClones, rootNode);
            }


            // Step3_UnmarkSolitaryChildrenOfClones
            Step3_UnmarkSolitaryChildrenOfClones(dictSolitary);


            // Step4_Create_lsLVdiffVol_LVitem_ClonesVM ManyClonesSepVolume OneCloneSepVolume AllOnOneVolume
            nProgressDenominator += dictClones.Count;
            ++nProgressItem;

            var nicknameUpdater = new ListUpdater<bool>(99676);
            Step4_Create_lsLVdiffVol_LVitem_ClonesVM(dictClones, nicknameUpdater, reportProgress);


            // Create_lsLVsolitary_MarkSolitaryParentsAsSolitary
            nProgressDenominator += dictSolitary.Count;
            ++nProgressItem;

            foreach (var kvp in dictSolitary)
            {
                reportProgress();

                var treeNode = kvp.Value;
                var nodeDatum = treeNode.NodeDatum;

                Util.Assert(99975, 0 < nodeDatum.FileCountHere);
                MarkSolitaryParentsAsSolitary(treeNode);

                var lvItem = new LVitem_ClonesVM(new[] { treeNode }, nicknameUpdater);

                _lsLVsolitary.Add(lvItem);
                Util.Assert(99973, null == nodeDatum.LVitem);
                nodeDatum.LVitem = lvItem;
            }

            InsertSizeMarkers(_lsLVsolitary);



            // Create_lsLVsameVol
            var lsSameVol = new List<LocalTreeNode>();

            if (0 < _lsRootNodes.Count)
            {
                var nCount = CountNodes(_lsRootNodes);
                
                AddTreeToList.Go(_lsAllNodes, lsSameVol, _lsRootNodes);
                Util.Assert(1305.6326m, _lsAllNodes.Count == nCount);
                Util.WriteLine("Step1_OnThread " + nCount);
            }

            lsSameVol.Sort((y, x) => x.NodeDatum.LengthTotal.CompareTo(y.NodeDatum.LengthTotal));
            nProgressDenominator += lsSameVol.Count;
            ++nProgressItem;

            foreach (var treeNode in lsSameVol)
            {
                reportProgress();

                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    _bAborted)
                {
                    return;
                }

                MarkSolitaryParentsAsSolitary(treeNode);

                var nodeDatum = treeNode.NodeDatum;

                if (0 == (nodeDatum.Clones?.Count ?? 0))
                    Util.Assert(99972, false);

                _lsLVsameVol.Add(nodeDatum.LVitem = new LVitem_ClonesVM(nodeDatum.Clones, nicknameUpdater));
            }

            InsertSizeMarkers(_lsLVsameVol);


            Util.Assert(1305.6333m, 0 == _lvClones.Items.Count);
            Util.Assert(1305.6334m, 0 == _lvSolitary.Items.Count);
            Util.Assert(1305.6335m, 0 == _lvSameVol.Items.Count);

            if (0 < _lsLVdiffVol.Count)
                _lvClones.Add(_lsLVdiffVol);

            if (0 < _lsLVsolitary.Count)
                _lvSolitary.Add(_lsLVsolitary);

            if (0 < _lsLVsameVol.Count)
                _lvSameVol.Add(_lsLVsameVol);
        }

        static int CountNodes(IEnumerable<LocalTreeNode> ieNodes)
        {
            var nCount = 0;

            foreach (var treeNode in ieNodes)
            {
                if (null != treeNode.Nodes)
                    nCount += CountNodes(treeNode.Nodes);

                ++nCount;
            }

            return nCount;
        }

        void IgnoreNodeAndSubnodes(LVitem_ClonesVM lvItem, IEnumerable<LocalTreeNode> ieTreeNodes)
        {
            foreach (var treeNode in ieTreeNodes)
            {
                if (null != _dictIgnoreNodes.TryGetValue(treeNode))
                    continue;

                Util.AssertNotNull(1305.6312m, lvItem);
                _dictIgnoreNodes.Add(treeNode, lvItem);

                if (null != treeNode.Nodes)
                    IgnoreNodeAndSubnodes(lvItem, treeNode.Nodes);
            }
        }

        void IgnoreNodeQuery(string sbMatch, int nMaxLevel, IEnumerable<LocalTreeNode> ieTreeNodes)
        {
            foreach (var treeNode in ieTreeNodes)
            {
                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    _bAborted)
                {
                    return;
                }

                if (sbMatch.Contains(treeNode.PathShort.ToLower()))
                {
                    if (_lsLVignore
                        .Where(lvItem => treeNode.Level == (("" + lvItem.SubItems[1]).ToInt() - 1))
                        .Any(lvItem => ("" + lvItem.PathShort).Equals(treeNode.PathShort, StringComparison.Ordinal)))
                    {
                        Util.Assert(99898, false);    // replace the Tag field with an LVitem
                                                      //      IgnoreNodeAndSubnodes((LocalLVitem)lvItem.Tag, treeNode);
                    }
                }

                if ((null != treeNode.Nodes) &&
                    (treeNode.Level <= nMaxLevel))
                {
                    IgnoreNodeQuery(sbMatch, nMaxLevel, treeNode.Nodes);
                }
            }
        }

        static void MarkSolitaryParentsAsSolitary(LocalTreeNode treeNode)
        {
    //        Util.Assert(99974, Transparent == treeNode.ForeColor);
            treeNode.ColorcodeFG = Solitary;

            LocalTreeNode parentNode = treeNode.Parent;

            while (null != parentNode)
            {
                parentNode.ColorcodeBG = ContainsSolitaryBG;


                //Util.Assert(99900,
                //    (parentNode.ForeColor == Transparent) ==
                //    (null == parentNode.NodeDatum.LVitem));

                //var a = parentNode.ForeColor == Transparent;
                //var b = null == parentNode.NodeDatum.LVitem;

     //           Util.Assert(99900, a == b);


                if (Transparent == parentNode.ColorcodeFG)
                {
                    Util.Assert(99900, null == parentNode.NodeDatum.LVitem);
                    parentNode.ColorcodeFG = Solitary;
                }
                else
                {
      //              Util.Assert(99593, null != parentNode.NodeDatum.LVitem);
    //                Util.Assert(99594, Solitary == parentNode.ForeColor);
                }

                parentNode.ColorcodeFG = Solitary;
//                Util.Assert(99594, new[] { Solitary, Transparent }.Contains(parentNode.ForeColor));

                parentNode = parentNode.Parent;
            }
        }

        // the following are form vars referenced internally, thus keeping their form_ and m_ prefixes
        readonly IDictionary<int, List<LocalTreeNode>>
            _dictNodes = null;
        readonly UC_ClonesVM _lvClones = null;
        readonly UC_ClonesVM _lvSameVol = null;
        readonly UC_ClonesVM _lvSolitary = null;
        readonly IReadOnlyList<LocalTreeNode> _lsRootNodes = null;
        readonly IList<LocalTreeNode> _lsAllNodes = null;
        readonly IList<LVitem_ClonesVM> _lsLVignore = null;

        // the following are "local" to this object, and do not have m_ prefixes because they do not belong to the form.
        readonly IList<LVitem_ClonesVM> _lsLVsolitary = new List<LVitem_ClonesVM>();
        readonly IList<LVitem_ClonesVM> _lsLVsameVol = new List<LVitem_ClonesVM>();
        readonly IList<LVitem_ClonesVM> _lsLVdiffVol = new List<LVitem_ClonesVM>();
        readonly IDictionary<LocalTreeNode, LVitem_ClonesVM> _dictIgnoreNodes = new Dictionary<LocalTreeNode, LVitem_ClonesVM>();
        readonly bool _bLoose = false;
    }
}
