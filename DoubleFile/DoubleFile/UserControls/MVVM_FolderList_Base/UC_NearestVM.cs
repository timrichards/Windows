using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class UC_NearestVM : UC_FolderListVM_Base
    {
        internal new UC_NearestVM               // new to hide then call base.Init() and return this
            Init()
        {
            base.Init();

            Util.ThreadMake(() =>
            {
                LocalTV.AllFileHashes_AddRef();
                NoResultsText = null;
                RaisePropertyChanged("NoResultsText");
                HideProgressbar();
                StartSearch(LocalTV.TreeSelect_FolderDetail);
            });

            return this;
        }

        public override void Dispose()          // calls base.Dispose() which implements IDisposable
        {
            Util.ThreadMake(() =>
            {
                base.Dispose();                 // sets _cts.IsCancellationRequested
                LocalTV.AllFileHashes_DropRef();
            });
        }

        protected override IEnumerable<LVitem_FolderListVM>
            FillList(LocalTreeNode searchFolder)
        {
            if (null == searchFolder.NodeDatum.Hashes_SubnodeFiles_Scratch)
                return null;

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
                    return null;
            }

            if (0 == searchSet.Count)
                return null;

            FindMatchingFolders(searchFolder, searchSet, LocalTV.RootNodes);

            if (_cts.IsCancellationRequested)
                return null;

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

                Util.WriteLine("FindMatchingFolders " + searchFolder.NodeDatum.Hashes_SubnodeFiles_Scratch.GetHashCode());
                return lsFolders.Select(tupleA => tupleA.Item2);
            }

            return null;
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
                    ?.Intersect(searchSet)
                    .Count() ?? 0;

                var nTestHereCount =
                    testFolder.NodeDatum.Hashes_FilesHere
                    ?.Intersect(searchSet)
                    .Count() ?? 0;

                if (0 < nTestHereCount)
                    _lsMatchingFolders.Add(Tuple.Create(nTestChildrenCount + nTestHereCount, new LVitem_FolderListVM(testFolder, _nicknameUpdater)));

                if (0 < nTestChildrenCount)
                    FindMatchingFolders(searchFolder, searchSet, testFolder.Nodes);         // recurse
            });
        }

        ConcurrentBag<Tuple<int, LVitem_FolderListVM>>
            _lsMatchingFolders = new ConcurrentBag<Tuple<int, LVitem_FolderListVM>>();
    }
}
