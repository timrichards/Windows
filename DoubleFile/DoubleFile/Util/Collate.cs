﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    using static UtilColorcode;

    partial class Collate
    {
        internal IReadOnlyList<LocalTreeNode>
            RootNodes = null;
        internal IDictionary<int, List<LocalTreeNode>>
            DictNodes = null;
        internal IList<LocalTreeNode>
            AllNodes = null;
        internal UC_ClonesVM
            LVsolitary = null;
        internal UC_ClonesVM
            LVsameVol = null;
        internal UC_ClonesVM 
            LVclones = null;

        internal void Go(Action<double> reportProgress_)
        {
            double nProgressNumerator = 0;
            double nProgressDenominator = 0;
            double nProgressItem = 0;
            const double knTotalProgressItems = 5;
            Action reportProgress = () => reportProgress_(++nProgressNumerator / nProgressDenominator * nProgressItem / knTotalProgressItems);


            // Step1_CreateDictSolitary_NodeDatum_DictClones
            nProgressDenominator += DictNodes.Count;
            ++nProgressItem;

            foreach (var kvp in DictNodes)
            {
                reportProgress();
                Step1_CreateDictSolitary_NodeDatum_DictClones(kvp);
            }


            // Step2_SolitaryHasClones
            nProgressDenominator += RootNodes.Count;
            ++nProgressItem;

            foreach (var rootNode in RootNodes)
            {
                reportProgress();
                Step2_SolitaryHasClones(rootNode);
            }


            // Step3_DictSolitaryRemove_ChildrenOfClones
            foreach (var kvp in _dictSolitary.ToList())
                Step3_DictSolitaryRemove_ChildrenOfClones(kvp);


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
                Step5_MarkSolitaryParentsAsSolitary(treeNode);

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
            var nCount = CountNodes(RootNodes);
                
            AddTreeToList.Go(AllNodes, lsSameVol, RootNodes);
            Util.Assert(1305.6326m, AllNodes.Count == nCount);

            foreach (var treeNode in AllNodes)
            {
                var nodeDatum = treeNode.NodeDatum;

                if (Transparent == treeNode.ColorcodeFG)
                {
                    Util.Assert(99978, null == treeNode.Clones);

                    if (0 == nodeDatum.LengthTotal)
                        treeNode.ColorcodeFG = ZeroLengthFolder;
                    else if (0 == nodeDatum.Hash_AllFiles)
                        treeNode.ColorcodeFG = FolderHasNoHashes;
                }
#if (DEBUG)
                if (null != treeNode.Clones)
                {
                    if (0 == treeNode.Clones.Count)
                        Util.Assert(99972, false);

                    var lsClones = _dictClones.TryGetValue(nodeDatum.Hash_AllFiles);

                    if (null != lsClones)
                        Util.Assert(99583, ReferenceEquals(lsClones, treeNode.Clones));
                    else
                        Util.Assert(99584, false, bIfDefDebug: true);
                }

                if (Transparent == treeNode.ColorcodeFG)
                {
                    var treeNodeA = DictNodes.TryGetValue(nodeDatum.Hash_AllFiles);
                    var treeNodeB = _dictSolitary.TryGetValue(nodeDatum.Hash_AllFiles);
                    var treeNodeC = _dictClones.TryGetValue(nodeDatum.Hash_AllFiles);
                    var nIndex = AllNodes.IndexOf(treeNode);

                    Util.Assert(99585, false);
                }
#endif
            }

            lsSameVol.Sort((y, x) => x.NodeDatum.LengthTotal.CompareTo(y.NodeDatum.LengthTotal));
            nProgressDenominator += lsSameVol.Count;
            ++nProgressItem;

            foreach (var treeNode in lsSameVol)
            {
                reportProgress();

                if (null == treeNode.Clones)
                    Util.Assert(99972, false);

                _lsLVsameVol.Add(treeNode.LVitem = new LVitem_ClonesVM(treeNode.Clones, nicknameUpdater));
            }

            InsertSizeMarkers(_lsLVsameVol);


            Util.Assert(1305.6333m, 0 == LVclones.Items.Count);
            Util.Assert(1305.6334m, 0 == LVsolitary.Items.Count);
            Util.Assert(1305.6335m, 0 == LVsameVol.Items.Count);

            if (0 < _lsLVclones.Count)
                LVclones.Add(_lsLVclones);

            if (0 < _lsLVsolitary.Count)
                LVsolitary.Add(_lsLVsolitary);

            if (0 < _lsLVsameVol.Count)
                LVsameVol.Add(_lsLVsameVol);
        }

        void Step1_CreateDictSolitary_NodeDatum_DictClones(KeyValuePair<int, List<LocalTreeNode>> kvp)
        {
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

                    var lsCheck = _dictClones.TryGetValue(kvp.Key);

                    if (null == lsCheck)
                        _dictClones[kvp.Key] = lsKeep;
                    else
                        Util.Assert(99971, ReferenceEquals(lsCheck, lsKeep));
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

        void Step2_SolitaryHasClones(LocalTreeNode treeNode)
        {
            if (null != treeNode.Clones)
            {
                for (var parent = treeNode.Parent; null != parent; parent = parent.Parent)
                {
                    if (SolitaryHasClones == parent.ColorcodeFG)
                        continue;

                    Util.Assert(99977, new[] { Solitary, Transparent }.Contains(parent.ColorcodeFG), bIfDefDebug: true);
                    parent.ColorcodeFG = SolitaryHasClones;
                }

                return;
            }

            if (null == treeNode.Nodes)
                return;

            foreach (var subNode in treeNode.Nodes)
                Step2_SolitaryHasClones(subNode);             // recurse
        }

        void Step3_DictSolitaryRemove_ChildrenOfClones(KeyValuePair<int, LocalTreeNode> kvp)
        {
            // A parent may be cloned and its child may be solitary: files may be distributed differently than in the clone.
            var nParentCloneColor = 0;

            for (var parent = kvp.Value.Parent; null != parent; parent = parent.Parent)
            {
                if (null == parent.Clones)
                    continue;

                nParentCloneColor =
                    (AllOnOneVolume == parent.ColorcodeFG)
                    ? SolitaryOneVolParent
                    : SolitaryClonedParent;

                break;
            }

            if (0 == nParentCloneColor)
                return;

            for (var treeNode = kvp.Value; null != treeNode; treeNode = treeNode.Parent)
            {
                if (null != treeNode.Clones)
                    break;

                treeNode.ColorcodeFG = nParentCloneColor;
            }

            _dictSolitary.Remove(kvp.Key);
        }

        void Step4_Create_lsLVdiffVol_LVitem_ClonesVM(ListUpdater<bool> nicknameUpdater, Action reportProgress)
        {
            foreach (var kvp in _dictClones)
            {
                reportProgress();

                var lvItem = new LVitem_ClonesVM(kvp.Value, nicknameUpdater);

                foreach (var treeNode in kvp.Value)
                    treeNode.LVitem = lvItem;

                _lsLVclones.Add(lvItem);
            }

            InsertSizeMarkers(_lsLVclones);
        }

        static void Step5_MarkSolitaryParentsAsSolitary(LocalTreeNode treeNode)
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
            _dictSolitary = new SortedDictionary<int, LocalTreeNode>();
        readonly IList<LVitem_ClonesVM>
            _lsLVsolitary = new List<LVitem_ClonesVM>();

        readonly IList<LVitem_ClonesVM>
            _lsLVsameVol = new List<LVitem_ClonesVM>();

        IDictionary<int, List<LocalTreeNode>>
            _dictClones = new SortedDictionary<int, List<LocalTreeNode>>();
        readonly IList<LVitem_ClonesVM>
            _lsLVclones = new List<LVitem_ClonesVM>();
    }
}
