using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class UC_FolderListVM : IDisposable
    {
        internal UC_FolderListVM()
        {
            Util.ThreadMake(() =>
            {
                var stopwatch = Stopwatch.StartNew();

                _dictClones = new Dictionary<int, IReadOnlyList<int>>();
                Setup_AllFileHashes_Scratch(LocalTV.RootNodes);
                _dictClones = null;
                stopwatch.Stop();
                Util.WriteLine("Setup_AllFileHashes_Scratch " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
                _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99701, TreeSelect_FolderDetailUpdated));
                NoResultsVisibility = Visibility.Collapsed;
                RaisePropertyChanged("NoResultsVisibility");

                var folderDetail = LocalTV.TreeSelect_FolderDetail;

                if (null != folderDetail)
                    TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, 0));
            });
        }

        internal UC_FolderListVM Init()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            Icmd_Nicknames = new RelayCommand(() => _nicknameUpdater.UpdateViewport(UseNicknames));
            _nicknameUpdater.Clear();
            _nicknameUpdater.UpdateViewport(UseNicknames);
            return this;
        }

        public void Dispose()
        {
            Util.ThreadMake(() =>
            {
                _bDisposed = true;
                _cts.Cancel();
                Util.LocalDispose(_lsDisposable);
                Cleanup_AllFileHashes_Scratch(LocalTV.RootNodes);
                GC.Collect();
            });
        }

        void TreeSelect_FolderDetailUpdated(Tuple<TreeSelect.FolderDetailUpdated, int> initiatorTuple)
        {
            ClearItems();
            _cts.Cancel();
            NoResultsVisibility = Visibility.Collapsed;
            RaisePropertyChanged("NoResultsVisibility");

            while (0 < _nRefCount)
                Util.Block(20);

            if (_bDisposed)
                return;

            _cts = new CancellationTokenSource();
            ++_nRefCount;

            Util.ThreadMake(() =>
            {
                var bNoResults = true;
                var tuple = initiatorTuple.Item1;
                var searchFolder = tuple.treeNode;

                Util.Closure(() =>
                {
                    if (null == searchFolder.NodeDatum.AllFileHashes_Scratch)
                        return;     // from lambda

                    _lsMatchingFolders = new ConcurrentBag<Tuple<int, LVitem_FolderListVM>>();

                    var searchSet =
                        searchFolder.NodeDatum.AllFileHashes_Scratch
                        .Union(searchFolder.NodeDatum.FileHashes)
                        .OrderBy(n => n)
                        .ToList();

                    if (1 << 11 < searchSet.Count)
                    {
                        searchSet =
                            searchFolder.NodeDatum.FileHashes
                            .ToList();

                        if (1 << 11 < searchSet.Count)
                            return;     // from lambda
                    }

                    if (0 == searchSet.Count)
                        return;     // from lambda

                    FindMatchingFolders(searchFolder, searchSet, LocalTV.RootNodes);

                    if (_cts.IsCancellationRequested)
                        return;     // from lambda

                    if (_lsMatchingFolders?.Any() ?? false)
                    {
                        var lsFolders =
                            _lsMatchingFolders
                            .OrderByDescending(tupleA => tupleA.Item1);

                        _lsMatchingFolders = null;

                        int nMatch = 0;
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
                        bNoResults = false;
                    }

                    Util.WriteLine("FindMatchingFolders " + searchFolder.NodeDatum.AllFileHashes_Scratch.GetHashCode());
                });

                if (bNoResults)
                {
                    NoResultsFolder = searchFolder.Text;
                    NoResultsVisibility = Visibility.Visible;
                    RaisePropertyChanged("NoResultsFolder");
                    RaisePropertyChanged("NoResultsVisibility");
                }

                --_nRefCount;
                GC.Collect();
            });
        }

        void FindMatchingFolders(LocalTreeNode searchFolder, List<int> searchSet, IReadOnlyList<LocalTreeNode> nodes)
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

                if ((searchFolder.NodeDatum.AllFilesHash == testFolder.NodeDatum.AllFilesHash) ||
                    testFolder.IsChildOf(searchFolder))
                {
                    return;     // from lambda: continue
                }

                var nTestChildrenCount =
                    testFolder.NodeDatum.AllFileHashes_Scratch
                    .Intersect(searchSet)
                    .Count();

                var nTestHereCount =
                    testFolder.NodeDatum.FileHashes
                    .Intersect(searchSet)
                    .Count();

                if (0 < nTestHereCount)
                    _lsMatchingFolders.Add(Tuple.Create(nTestChildrenCount + nTestHereCount, new LVitem_FolderListVM(testFolder, _nicknameUpdater)));

                if (0 < nTestChildrenCount)
                    FindMatchingFolders(searchFolder, searchSet, testFolder.Nodes);         // recurse
            });
        }

        void GoTo()
        {
            if (null == _selectedItem)
            {
                Util.Assert(99897, false);    // binding should dim the button
                return;
            }

            _selectedItem.LocalTreeNode.GoToFile(null);
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

                if (_dictClones.TryGetValue(treeNode.NodeDatum.AllFilesHash, out lsClone))
                {
                    treeNode.NodeDatum.AllFileHashes_Scratch = lsClone;
                }
                else
                {
                    treeNode.NodeDatum.AllFileHashes_Scratch = lsAllFileHashes_childNodes.OrderBy(n => n).Distinct().ToList();
                    _dictClones.Add(treeNode.NodeDatum.AllFilesHash, treeNode.NodeDatum.AllFileHashes_Scratch);
                }

                if (false == bStart)
                {
                    lsAllFilesHashes.AddRange(treeNode.NodeDatum.FileHashes);
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
                treeNode.NodeDatum.AllFileHashes_Scratch = null;
                Cleanup_AllFileHashes_Scratch(treeNode.Nodes);                 // recurse
            }
        }

        ConcurrentBag<Tuple<int, LVitem_FolderListVM>>
            _lsMatchingFolders = null;
        CancellationTokenSource
            _cts = new CancellationTokenSource();
        int
            _nRefCount = 0;
        Dictionary<int, IReadOnlyList<int>>
            _dictClones = null;
        readonly ListUpdater<bool>
            _nicknameUpdater = new ListUpdater<bool>(99667);
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
        bool
            _bDisposed = false;
    }
}
