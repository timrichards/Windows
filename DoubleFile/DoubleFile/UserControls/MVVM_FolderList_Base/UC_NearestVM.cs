using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class UC_NearestVM : UC_FolderListVM
    {
        internal new UC_NearestVM               // new to hide then call base.Init() and return this
            Init()
        {
            base.Init();

            Util.ThreadMake(() =>
            {
                var stopwatch = Stopwatch.StartNew();

                _dictClones = new Dictionary<int, IReadOnlyList<int>>();
                Setup_AllFileHashes_Scratch(LocalTV.RootNodes);
                _dictClones = null;
                stopwatch.Stop();
                Util.WriteLine("Setup_AllFileHashes_Scratch " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
                NoResultsFolder = null;
                RaisePropertyChanged("NoResultsFolder");
                HideProgressbar();

                var folderDetail = LocalTV.TreeSelect_FolderDetail;

                if (null != folderDetail)
                    TreeSelect_FolderDetailUpdated(folderDetail.treeNode);
            });

            return this;
        }

        public override void Dispose()          // calls base.Dispose() which implements IDisposable
        {
            Util.ThreadMake(() =>
            {
                base.Dispose();                 // sets _bDisposed and _cts.Cancel()
                Cleanup_AllFileHashes_Scratch(LocalTV.RootNodes);
                GC.Collect();
            });
        }

        protected override void TreeSelect_FolderDetailUpdated(LocalTreeNode searchFolder)
        {
            if (null == searchFolder.NodeDatum.Hashes_SubnodeFiles_Scratch)
                return;

            _lsMatchingFolders = new ConcurrentBag<Tuple<int, LVitem_FolderListVM>>();

            var searchSet =
                searchFolder.NodeDatum.Hashes_SubnodeFiles_Scratch
                .Union(searchFolder.NodeDatum.Hashes_FilesHere)
                .ToList();

            if (1 << 11 < searchSet.Count)
            {
                searchSet =
                    searchFolder.NodeDatum.Hashes_FilesHere
                    .ToList();

                if (1 << 11 < searchSet.Count)
                    return;
            }

            if (0 == searchSet.Count)
                return;

            FindMatchingFolders(searchFolder, searchSet, LocalTV.RootNodes);

            if (_cts.IsCancellationRequested)
                return;

            if (_lsMatchingFolders?.Any() ?? false)
            {
                var lsFolders =
                    _lsMatchingFolders
                    .OrderByDescending(tupleA => tupleA.Item1);

                _lsMatchingFolders = new ConcurrentBag<Tuple<int, LVitem_FolderListVM>>();

                var nMatch = 0;
                var bAlternate = false;

                foreach (var folder in lsFolders)
                {
                    if (folder.Item1 != nMatch)
                    {
                        bAlternate = (false == bAlternate);
                        nMatch = folder.Item1;
                    }

                    folder.Item2.Alternate = bAlternate;
                }

                Util.UIthread(99912, () => Add(lsFolders.Select(tupleA => tupleA.Item2)));
            }

            Util.WriteLine("FindMatchingFolders " + searchFolder.NodeDatum.Hashes_SubnodeFiles_Scratch.GetHashCode());
        }

        void FindMatchingFolders(LocalTreeNode searchFolder, IReadOnlyList<int> searchSet, IReadOnlyList<LocalTreeNode> nodes)
        {
            if (_cts.IsCancellationRequested)
                return;

            if (null == nodes)
                return;

            Util.ParallelForEach(99634, nodes,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = _cts.Token },
                testFolder =>
            {
                if (_cts.IsCancellationRequested)
                    return;     // from lambda: continue

                if ((searchFolder.NodeDatum.Hash_AllFiles == testFolder.NodeDatum.Hash_AllFiles) ||
                    testFolder.IsChildOf(searchFolder))
                {
                    return;     // from lambda: continue
                }

                var nTestChildrenCount =
                    testFolder.NodeDatum.Hashes_SubnodeFiles_Scratch
                    .Intersect(searchSet)
                    .Count();

                var nTestHereCount =
                    testFolder.NodeDatum.Hashes_FilesHere
                    .Intersect(searchSet)
                    .Count();

                if (0 < nTestHereCount)
                    _lsMatchingFolders.Add(Tuple.Create(nTestChildrenCount + nTestHereCount, new LVitem_FolderListVM(testFolder, _nicknameUpdater)));

                if (0 < nTestChildrenCount)
                    FindMatchingFolders(searchFolder, searchSet, testFolder.Nodes);         // recurse
            });
        }

        IReadOnlyList<int>
            Setup_AllFileHashes_Scratch(IReadOnlyList<LocalTreeNode> nodes, bool bStart = true)
        {
            if (null == nodes)
                return new int[0];

            var lsAllFilesHashes = new List<int> { };

            foreach (var treeNode in nodes)
            {
                if (_bDisposed)
                    return new int[0];

                var lsAllFileHashes_childNodes = new List<int> { };

                if (null != treeNode.Nodes)
                    lsAllFileHashes_childNodes.AddRange(Setup_AllFileHashes_Scratch(treeNode.Nodes, bStart: false));    // recurse

                IReadOnlyList<int> lsClone = null;

                if (_dictClones.TryGetValue(treeNode.NodeDatum.Hash_AllFiles, out lsClone))
                {
                    treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch = lsClone;
                }
                else
                {
                    treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch = lsAllFileHashes_childNodes.OrderBy(n => n).Distinct().ToList();
                    _dictClones.Add(treeNode.NodeDatum.Hash_AllFiles, treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch);
                }

                if (false == bStart)
                {
                    lsAllFilesHashes.AddRange(treeNode.NodeDatum.Hashes_FilesHere);
                    lsAllFilesHashes.AddRange(lsAllFileHashes_childNodes);
                }
            }

            return lsAllFilesHashes;
        }

        void
            Cleanup_AllFileHashes_Scratch(IReadOnlyList<LocalTreeNode> nodes)
        {
            if (null == nodes)
                return;

            foreach (var treeNode in nodes)
            {
                treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch = null;
                Cleanup_AllFileHashes_Scratch(treeNode.Nodes);                 // recurse
            }
        }

        ConcurrentBag<Tuple<int, LVitem_FolderListVM>>
            _lsMatchingFolders = new ConcurrentBag<Tuple<int, LVitem_FolderListVM>>();
        Dictionary<int, IReadOnlyList<int>>
            _dictClones = null;
    }
}
