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

        internal void Go(Action<double> reportProgress_)
        {
            double nProgressNumerator = 0;
            double nProgressDenominator = 0;
            double nProgressItem = 0;
            const double knTotalProgressItems = 5;
            Action reportProgress = () => reportProgress_(++nProgressNumerator / nProgressDenominator * nProgressItem / knTotalProgressItems);


            // Step1_CreateDictSolitary_NodeDatumClones
            nProgressDenominator += _dictNodes.Count;
            ++nProgressItem;

            Step1_CreateDictSolitary_NodeDatum_DictClones(reportProgress);


            // Step2_DictClonesAdd_MarkIfAllOneVol
            nProgressDenominator += _lsRootNodes.Count;
            ++nProgressItem;

            foreach (var rootNode in _lsRootNodes)
            {
                reportProgress();
                Step2_MarkIfAllOneVol(rootNode);
            }


            // Step3_UnmarkSolitaryChildrenOfClones
            Step3_UnmarkSolitaryChildrenOfClones();


            // Step4_Create_lsLVdiffVol_LVitem_ClonesVM ManyClonesSepVolume OneCloneSepVolume AllOnOneVolume
            nProgressDenominator += _dictClones.Count;
            ++nProgressItem;

            var nicknameUpdater = new ListUpdater<bool>(99676);
            Step4_Create_lsLVdiffVol_LVitem_ClonesVM(nicknameUpdater, reportProgress);


            // Create lsLVsolitary MarkSolitaryParentsAsSolitary
            nProgressDenominator += _dictSolitary.Count;
            ++nProgressItem;

            foreach (var kvp in _dictSolitary)
            {
                reportProgress();

                var treeNode = kvp.Value;
                var nodeDatum = treeNode.NodeDatum;

                Util.Assert(99975, 0 < nodeDatum.FileCountHere);
                MarkSolitaryParentsAsSolitary(treeNode);

                var lvItem = new LVitem_ClonesVM(new[] { treeNode }, nicknameUpdater);

                _lsLVsolitary.Add(lvItem);
                Util.Assert(99973, null == treeNode.LVitem);
                treeNode.LVitem = lvItem;

                if (nodeDatum.Hashes_SubnodeFiles_Scratch_IsComplete &&
                    nodeDatum.Hashes_FilesHere_IsComplete &&
                    nodeDatum.Hashes_FilesHere.AsParallel().All(nFileID => Statics.DupeFileDictionary.IsDuplicate(nFileID)) &&
                    nodeDatum.Hashes_SubnodeFiles_Scratch.AsParallel().All(nFileID => Statics.DupeFileDictionary.IsDuplicate(nFileID)))
                {
                    treeNode.ColorcodeFG = SolitaryHasAllDupes;
                }
            }

            InsertSizeMarkers(_lsLVsolitary);


            // Create lsLVsameVol MarkSolitaryParentsAsSolitary
            var lsSameVol = new List<LocalTreeNode> { };

            if (0 < _lsRootNodes.Count)
            {
                var nCount = CountNodes(_lsRootNodes);
                
                AddTreeToList.Go(_lsAllNodes, lsSameVol, _lsRootNodes);
                Util.Assert(1305.6326m, _lsAllNodes.Count == nCount);
                Util.WriteLine("Step1_OnThread " + nCount);
            }

            foreach (var treeNode in _lsAllNodes)
            {
                if ((Transparent == treeNode.ColorcodeFG) &&
                    (0 < treeNode.NodeDatum.LengthTotal) &&
                    (0 == treeNode.NodeDatum.Hash_AllFiles))
                {
                    treeNode.ColorcodeFG = FolderHasNoHashes;
                }

                if (null != treeNode.Clones)
                {
                    var lsClones = _dictClones.TryGetValue(treeNode.NodeDatum.Hash_AllFiles);

                    if (null != lsClones)
                        Util.Assert(99583, ReferenceEquals(lsClones, treeNode.Clones));
                    else
                        Util.Assert(99584, false);
                }

                if (Transparent == treeNode.ColorcodeFG)
                {
                    var treeNodeA = _dictNodes.TryGetValue(treeNode.NodeDatum.Hash_AllFiles);
                    var treeNodeB = _dictSolitary.TryGetValue(treeNode.NodeDatum.Hash_AllFiles);
                    var treeNodeC = _dictClones.TryGetValue(treeNode.NodeDatum.Hash_AllFiles);
                    var nIndex = _lsAllNodes.IndexOf(treeNode);

                    Util.Assert(99585, false);
                }
            }

            lsSameVol.Sort((y, x) => x.NodeDatum.LengthTotal.CompareTo(y.NodeDatum.LengthTotal));
            nProgressDenominator += lsSameVol.Count;
            ++nProgressItem;

            foreach (var treeNode in lsSameVol)
            {
                reportProgress();
                MarkSolitaryParentsAsSolitary(treeNode);

                if (0 == (treeNode.Clones?.Count ?? 0))
                    Util.Assert(99972, false);

                _lsLVsameVol.Add(treeNode.LVitem = new LVitem_ClonesVM(treeNode.Clones, nicknameUpdater));
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

        void Step1_CreateDictSolitary_NodeDatum_DictClones(Action reportProgress)
        {
            foreach (var kvp in _dictNodes)
            {
                reportProgress();

                var lsNodes = kvp.Value;

                if (1 < lsNodes.Count)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    var lsKeep = new List<LocalTreeNode> { };

                    foreach (var treeNode in lsNodes)
                    {
                        if (false == lsNodes.Contains(treeNode.Parent))
                            lsKeep.Add(treeNode);
                    }

                    if (1 < lsKeep.Count)
                    {
                        var firstRootNodeDatum = lsKeep[0].RootNodeDatum;

                        foreach (var treeNode in lsKeep)
                            treeNode.Clones = lsKeep;

                        lsKeep[0].ColorcodeFG = AllOnOneVolume;

                        foreach (var treeNode in lsKeep.Skip(1))
                        {
                            // Test to see if clones are on separate volumes.

                            Util.Assert(99999, treeNode.NodeDatum.Hash_AllFiles == lsKeep[0].NodeDatum.Hash_AllFiles);

                            var rootNodeDatum = treeNode.RootNodeDatum;

                            if (ReferenceEquals(firstRootNodeDatum, rootNodeDatum))
                                continue;

                            if (firstRootNodeDatum.LVitemProjectVM.Volume == rootNodeDatum.LVitemProjectVM.Volume)
                                continue;

                            lsKeep[0].ColorcodeFG = (2 < lsKeep.Count) ? ManyClonesSepVolume : OneCloneSepVolume;
                            break;
                        }

                        foreach (var treeNode in lsKeep.Skip(1))
                            treeNode.ColorcodeFG = lsKeep[0].ColorcodeFG;

                        var nColorParent =
                            (AllOnOneVolume == lsKeep[0].ColorcodeFG)
                            ? ChildAllOnOneVolume
                            : ChildClonedSepVolume;

                        foreach (var treeNode in lsNodes.Except(lsKeep))
                            treeNode.ColorcodeFG = nColorParent;

                        if (null == _dictClones.TryGetValue(kvp.Key))
                            _dictClones[kvp.Key] = lsKeep;
                    }
                    else
                    {
                        foreach (var treeNode in lsNodes)
                            treeNode.ColorcodeFG = Solitary;

                        lsNodes = lsKeep;   // kick off "else" logic below after deleting child clones
                    }
                }

                if (1 == lsNodes.Count)     // "else"
                {
                    var treeNode = lsNodes[0];

                    if (0 < treeNode.NodeDatum.FileCountHere)
                        _dictSolitary.Add(kvp.Key, treeNode);

                    treeNode.ColorcodeFG = Solitary;
                }
            }
        }

        void Step2_MarkIfAllOneVol(LocalTreeNode treeNode, bool rootClone = false)
        {
            var nodeDatum = treeNode.NodeDatum;

            if (0 == nodeDatum.LengthTotal)
            {
                treeNode.ColorcodeFG = ZeroLengthFolder;
                Util.Assert(99978, null == treeNode.Clones);
            }

            if ((false == rootClone) &&
                (null != treeNode.Clones))
            {
                rootClone = true;

                // mark all parents as having clones
                for (var parent = treeNode.Parent; null != parent; parent = parent.Parent)
                {
                    if (SolitaryHasClones != parent.ColorcodeFG)
                    {
                        Util.Assert(99977, new[] { Solitary, Transparent }.Contains(parent.ColorcodeFG), bIfDefDebug: true);
                        parent.ColorcodeFG = SolitaryHasClones;
                    }
                }
            }

            if (null == treeNode.Nodes)
                return;

            foreach (var subNode in treeNode.Nodes)
                Step2_MarkIfAllOneVol(subNode, rootClone);             // recurse
        }

        void Step3_UnmarkSolitaryChildrenOfClones()
        {
            // A parent may be cloned and its child may be solitary: files may be distributed differently than in the clone.
            foreach (var kvp in _dictSolitary.ToList())
            {
                var nParentCloneColor = 0;

                for (var parent = kvp.Value.Parent; null != parent; parent = parent.Parent)
                {
                    if (null != parent.Clones)
                    {
                        nParentCloneColor =
                            (parent.ColorcodeFG == AllOnOneVolume)
                            ? SolitaryOneVolParent
                            : SolitaryClonedParent;

                        break;
                    }
                }

                if (0 != nParentCloneColor)
                {
                    for (var treeNode = kvp.Value; null != treeNode; treeNode = treeNode.Parent)
                    {
                        if (null != treeNode.Clones)
                            break;

                        treeNode.ColorcodeFG = nParentCloneColor;
                    }

                    _dictSolitary.Remove(kvp.Key);
                }
            }
        }

        void Step4_Create_lsLVdiffVol_LVitem_ClonesVM(ListUpdater<bool> nicknameUpdater, Action reportProgress)
        {
            foreach (var kvp in _dictClones)
            {
                reportProgress();

                var lvItem = new LVitem_ClonesVM(kvp.Value, nicknameUpdater);

                foreach (var treeNode in kvp.Value)
                    treeNode.LVitem = lvItem;

                _lsLVdiffVol.Add(lvItem);
            }

            InsertSizeMarkers(_lsLVdiffVol);
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

        static void MarkSolitaryParentsAsSolitary(LocalTreeNode treeNode)
        {
            Func<LocalTreeNode, bool> check_and_mark = checkNode =>
            {
                if (new[] { ManyClonesSepVolume, OneCloneSepVolume, AllOnOneVolume }.Contains(checkNode.ColorcodeFG))
                    return true; // from lambda. solitary folder can have cloned parent if files are distributed differently among the parent's clones

                if (false == new[] { SolitaryHasClones, SolitaryHasAllDupes, Solitary }.Contains(checkNode.ColorcodeFG))
                {
                    // ParentCloned is unimportant because it's just a nicety: it's got ParentClonedBG
                    Util.Assert(99974, new[] { Transparent, ParentCloned }.Contains(checkNode.ColorcodeFG), bIfDefDebug: true);
                    checkNode.ColorcodeFG = Solitary;
                }

                return false;   // from lambda
            };

            if (check_and_mark(treeNode))
                return;

            for (var parent = treeNode.Parent; null != parent; parent = parent.Parent)
            {
                parent.ColorcodeBG = ContainsSolitaryBG;

                if (check_and_mark(parent))
                    return;
            }
        }

        IDictionary<int, LocalTreeNode>
            _dictSolitary = new SortedDictionary<int, LocalTreeNode>();
        IDictionary<int, List<LocalTreeNode>>
            _dictClones = new SortedDictionary<int, List<LocalTreeNode>>();

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
