using System;
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


            nProgressDenominator += DictNodes.Count;
            ++nProgressItem;

            foreach (var kvp in DictNodes)
            {
                reportProgress();
                Step1_CreateDictSolitary_NodeDatum_DictClones(kvp);         //Step1_CreateDictSolitary_NodeDatum_DictClones
            }


            nProgressDenominator += RootNodes.Count;
            ++nProgressItem;

            foreach (var rootNode in RootNodes)
            {
                reportProgress();
                Step2_SolitaryHasClones(rootNode);                                              //Step2_SolitaryHasClones
            }


            foreach (var kvp in _dictSolitary.ToList())
                Step3_DictSolitaryRemove_ChildrenOfClones(kvp);                                 //Step3_DictSolitaryRemove_ChildrenOfClones


            #region _lsLVclones                                                                 //#region _lsLVclones
            nProgressDenominator += _dictClones.Count;
            ++nProgressItem;

            var nicknameUpdater = new ListUpdater<bool>(99676);

            foreach (var kvp in _dictClones)
            {
                reportProgress();

                var lvItem = new LVitem_ClonesVM(kvp.Value, nicknameUpdater);

                foreach (var treeNode in kvp.Value)
                    treeNode.LVitem = lvItem;

                _lsLVclones.Add(lvItem);
            }

            InsertSizeMarkers(_lsLVclones);
            #endregion _lsLVclones


            #region _lsLVsolitary                                                               //#region _lsLVsolitary
            nProgressDenominator += _dictSolitary.Count;
            ++nProgressItem;

            foreach (var kvp in _dictSolitary)
            {
                reportProgress();

                var treeNode = kvp.Value.First();
                var lvItem = new LVitem_ClonesVM(new[] { treeNode }, nicknameUpdater);

                _lsLVsolitary.Add(lvItem);
                Util.Assert(99973, null == treeNode.LVitem);
                treeNode.LVitem = lvItem;
            }

            InsertSizeMarkers(_lsLVsolitary);
            #endregion _lsLVsolitary


            foreach (var kvp in _dictSolitary)
                Step4_SolitOrAllOneVolAllDupes(kvp.Value);                                      //Step4_SolitOrAllOneVolAllDupes


            #region AllNodes _lsLVsameVol                                                       //#region AllNodes _lsLVsameVol
            {
                var lsSameVol = new List<LocalTreeNode> { };
                var nCount = CountNodes(RootNodes);

                AddTreeToList.Go(AllNodes, lsSameVol, RootNodes);
                Util.Assert(99975, AllNodes.Count == nCount);

                lsSameVol.Sort((y, x) => x.NodeDatum.LengthTotal.CompareTo(y.NodeDatum.LengthTotal));

                foreach (var treeNode in lsSameVol)
                    Step4_SolitOrAllOneVolAllDupes(treeNode.Clones);                            //Step4_SolitOrAllOneVolAllDupes

                nProgressDenominator += lsSameVol.Count;
                ++nProgressItem;

                foreach (var treeNode in lsSameVol)
                {
                    reportProgress();

                    if (null == treeNode.Clones)
                        Util.Assert(99974, false);

                    _lsLVsameVol.Add(treeNode.LVitem = new LVitem_ClonesVM(treeNode.Clones, nicknameUpdater));
                }
            }

            InsertSizeMarkers(_lsLVsameVol);
            #endregion _lsLVsameVol


            #region Transparent ZeroLengthFolder FolderHasNoHashes                              //#region Transparent ZeroLengthFolder FolderHasNoHashes
            foreach (var treeNode in AllNodes)
            {
                if (Transparent != treeNode.ColorcodeFG)
                    continue;

                Util.Assert(99978, null == treeNode.Clones);

                var nodeDatum = treeNode.NodeDatum;

                if (0 == nodeDatum.LengthTotal)
                    treeNode.ColorcodeFG = ZeroLengthFolder;
                else if (0 == nodeDatum.Hash_AllFiles)
                    treeNode.ColorcodeFG = FolderHasNoHashes;
            }
            #endregion Transparent ZeroLengthFolder FolderHasNoHashes


            Step5_SolitAllDupes();                                                              //Step5_SolitAllDupes
            Step6_SolitAllDupesSepVol();                                                        //Step6_SolitAllDupesSepVol


            #region SolitSomeFilesDuped SolitNoFilesDuped                                       //#region SolitSomeFilesDuped SolitNoFilesDuped
            foreach (var kvp in _dictSolitary)
            {
                var testNode = kvp.Value.Last();

                if (Transparent != testNode.ColorcodeFG)
                    continue;

                var color =
                    testNode.NodeDatum.Hashes_FilesHere.Concat(testNode.NodeDatum.Hashes_SubnodeFiles_Scratch)
                    .Any(nFileID => Statics.DupeFileDictionary.IsDupeSepVolume(nFileID) ?? false)
                    ? SolitSomeFilesDuped
                    : SolitNoFilesDuped;

                foreach (var treeNode in kvp.Value)
                    treeNode.ColorcodeFG = color;
            }
            #endregion SolitSomeFilesDuped SolitNoFilesDuped


#if (DEBUG)
            foreach (var treeNode in AllNodes)
                Step7_FauxUnitTest(treeNode);                                                   //Step7_FauxUnitTest
#endif


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
                // Parent folder may contain only its clone subfolder, in which case unmark the parent folder

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

                        lsKeep[0].ColorcodeFG = 
                            (2 < lsKeep.Count)
                            ? ManyClonesSepVolume
                            : DupeFileDictionary.IsDeletedVolumeView
                            ? OneCloneSepVolOnly
                            : OneCloneSepVolume;

                        break;
                    }

                    foreach (var treeNode in lsKeep.Skip(1))
                        treeNode.ColorcodeFG = lsKeep[0].ColorcodeFG;

                    var nColorParent =
                        (new[] { AllOnOneVolume, OneCloneSepVolOnly }.Contains(lsKeep[0].ColorcodeFG))
                        ? ChildAllOnOneVolume
                        : ChildClonedSepVolume;

                    foreach (var treeNode in lsNodes.Except(lsKeep))
                        treeNode.ColorcodeFG = nColorParent;

                    var lsCheck = _dictClones.TryGetValue(kvp.Key);

                    if (null == lsCheck)
                        _dictClones[kvp.Key] = lsKeep;
                    else
                        Util.Assert(99971, ReferenceEquals(lsCheck, lsKeep));

                    return;
                }
            }

            lsNodes.Sort((x, y) => x.Level.CompareTo(y.Level));      // matrushka
            _dictSolitary.Add(kvp.Key, lsNodes);
        }

        void Step2_SolitaryHasClones(LocalTreeNode treeNode)
        {
            if (null != treeNode.Clones)
            {
                var bCheck = true;

                for (var parent = treeNode.Parent; null != parent; parent = parent.Parent, bCheck = false)
                {
                    if (new[]
                    {
                        SolitaryHasClones, SolitAllClonesOneVol, SolitAllClonesSepVol,
                        SolitAllDupesOneVol, SolitAllDupesSepVol
                    }
                        .Contains(parent.ColorcodeFG))
                    {
                        continue;
                    }

                    Util.Assert(99977, Transparent == parent.ColorcodeFG, bIfDefDebug: true);

                    if (false == (parent.NodeDatum.Hashes_FilesHere_IsComplete))
                        return;

                    var isAllDupSepVol = parent.NodeDatum.Hashes_FilesHere.AsParallel().Aggregate<int, bool?>(true, IsDupeSepVolume);

                    if (true != (isAllDupSepVol ?? false))
                        return;

                    var bSet = SolitaryHasClones;

                    if (bCheck && 
                        parent.Nodes.All(treeNodeA => false == treeNodeA.IsSolitary))
                    {
                        bSet =
                            parent.Nodes.Any(treeNodeA => treeNodeA.IsAllOnOneVolume)
                            ? SolitAllClonesOneVol
                            : SolitAllClonesSepVol;
                    }

                    parent.ColorcodeFG = bSet;
                }

                return;
            }

            if (null == treeNode.Nodes)
                return;

            foreach (var subNode in treeNode.Nodes)
                Step2_SolitaryHasClones(subNode);             // recurse
        }

        void Step3_DictSolitaryRemove_ChildrenOfClones(KeyValuePair<int, List<LocalTreeNode>> kvp)
        {
            // A parent may be cloned and its child may be solitary: files may be distributed differently than in the clone.
            var nParentCloneColor = 0;
            var testNode = kvp.Value.First();

            for (var parent = testNode.Parent; null != parent; parent = parent.Parent)
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

            foreach (var treeNode in kvp.Value)
                treeNode.ColorcodeFG = nParentCloneColor;

            for (var treeNode = testNode.Parent; null != treeNode; treeNode = treeNode.Parent)
            {
                if (null != treeNode.Clones)
                    break;

                treeNode.ColorcodeFG = nParentCloneColor;
            }

            _dictSolitary.Remove(kvp.Key);
        }

        void Step4_SolitOrAllOneVolAllDupes(List<LocalTreeNode> lsTreeNodes)
        {
            var testNode = lsTreeNodes.Last();      // solitary: innermost matrushka
            var nodeDatum = testNode.NodeDatum;

            if (false == (nodeDatum.Hashes_SubnodeFiles_IsComplete && nodeDatum.Hashes_FilesHere_IsComplete))
                return;

            var isAllDupSepVol = nodeDatum.Hashes_FilesHere.AsParallel().Aggregate<int, bool?>(true, IsDupeSepVolume);

            if (null == isAllDupSepVol)
                return;

            isAllDupSepVol = nodeDatum.Hashes_SubnodeFiles_Scratch.AsParallel().Aggregate<int, bool?>(isAllDupSepVol.Value, IsDupeSepVolume);

            if (null == isAllDupSepVol)
                return;

            if (testNode.IsSolitary)
            {
                var color =
                    isAllDupSepVol.Value &&
                    (true !=
                    (testNode.Nodes?.Any(subNode => (subNode.IsSolitary && (subNode.NodeDatum.Hash_AllFiles != nodeDatum.Hash_AllFiles))) ?? false))
                    ? SolitAllDupesSepVol
                    : SolitAllDupesOneVol;

                foreach (var treeNode in lsTreeNodes)
                    treeNode.ColorcodeFG = color;
            }
            else if (isAllDupSepVol.Value)
            {
                foreach (var treeNode in lsTreeNodes)
                    treeNode.ColorcodeFG = OneVolumeDupesSepVol;
            }
        }

        void Step5_SolitAllDupes()
        {
            var bSolitAllDupes = true;

            while (bSolitAllDupes)
            {
                bSolitAllDupes = false;

                foreach (var kvp in _dictSolitary)
                {
                    var testNode = kvp.Value.Last();

                    if (SolitaryHasClones != testNode.ColorcodeFG)
                        continue;

                    if (null == testNode.Nodes)
                        continue;

                    var nodeDatum = testNode.NodeDatum;

                    if (0 < nodeDatum.LengthHere)
                        continue;   // any files here'd be unique else this folder'd bin above

                    if (testNode.Nodes.All(treeNodeA => new[]
                    {
                        ZeroLengthFolder, AllOnOneVolume, SolitAllDupesOneVol, SolitAllClonesOneVol,
                        OneCloneSepVolume, OneCloneSepVolOnly, ManyClonesSepVolume, SolitAllDupesSepVol, SolitAllClonesSepVol
                    }
                        .Contains(treeNodeA.ColorcodeFG)))
                    {
                        var color =
                            (testNode.Nodes.Any(treeNodeA => new[] { AllOnOneVolume, SolitAllDupesOneVol, SolitAllClonesOneVol }
                            .Contains(treeNodeA.ColorcodeFG)))
                            ? SolitAllDupesOneVol
                            : SolitAllDupesSepVol;

                        foreach (var treeNode in kvp.Value)
                            treeNode.ColorcodeFG = color;

                        bSolitAllDupes = true;
                    }
                }
            }
        }

        void Step6_SolitAllDupesSepVol()
        {
            var bSolitAllDupes = true;

            while (bSolitAllDupes)
            {
                bSolitAllDupes = false;

                foreach (var kvp in _dictSolitary)
                {
                    var testNode = kvp.Value.Last();

                    if (false == new[] { SolitaryHasClones, SolitAllDupesOneVol }.Contains(testNode.ColorcodeFG))
                        continue;

                    if (null == testNode.Nodes)
                        continue;

                    var nodeDatum = testNode.NodeDatum;

                    if (false == nodeDatum.Hashes_FilesHere_IsComplete)
                        continue;

                    if (testNode.Nodes.All(treeNodeA => new[]
                    {
                        ZeroLengthFolder, OneCloneSepVolume, OneCloneSepVolOnly, ManyClonesSepVolume,
                        SolitAllDupesSepVol, SolitAllClonesSepVol, OneVolumeDupesSepVol
                    }
                        .Contains(treeNodeA.ColorcodeFG)))
                    {
                        if (nodeDatum.Hashes_FilesHere.AsParallel().Aggregate<int, bool?>(true, IsDupeSepVolume) ?? false)
                        {
                            foreach (var treeNode in kvp.Value)
                                treeNode.ColorcodeFG = SolitAllDupesSepVol;

                            bSolitAllDupes = true;
                        }
                    }
                }
            }
        }

        void Step7_FauxUnitTest(LocalTreeNode treeNode)
        {
            var nodeDatum = treeNode.NodeDatum;

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

            var nInterval = (nCount < 100) ? 10 : (nCount < 1000) ? 25 : 50;

            InsertSizeMarkerStatic.Go(listLVitems, nCount - 1, bAdd: true);

            var nInitial = nCount % nInterval;

            if (0 == nInitial)
                nInitial = nInterval;

            var nHalf = (nInterval >> 1);

            if (nCount - nInitial > nHalf)
            {
                for (var i = nCount - nInitial; i > nHalf; i -= nInterval)
                    InsertSizeMarkerStatic.Go(listLVitems, i);
            }

            InsertSizeMarkerStatic.Go(listLVitems, 0);            // Enter the Zeroth
        }

        bool?
            IsDupeSepVolume(bool? current, int nFileID)
        {
            if (null == current)
                return null;

            var retVal = Statics.DupeFileDictionary.IsDupeSepVolume(nFileID);

            return
                (null == retVal)
                ? null
                : (false == current)
                ? false
                : retVal;
        }

        IDictionary<int, List<LocalTreeNode>>
            _dictSolitary = new SortedDictionary<int, List<LocalTreeNode>>();
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
