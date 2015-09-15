using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace DoubleFile
{
    partial class UC_FolderListVM : IDisposable
    {
        internal UC_FolderListVM()
        {
            new ProgressOverlay(new[] { "" }, new[] { "Setting up Nearest view" }, x =>
            {
                var stopwatch = Stopwatch.StartNew();

                _dictClones = new Dictionary<int, IReadOnlyList<int>>();
                Setup_AllFileHashes_Scratch(LocalTV.RootNodes);
                _dictClones = null;
                stopwatch.Stop();
                Util.WriteLine("Setup_AllFileHashes_Scratch " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
                _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99701, TreeSelect_FolderDetailUpdated));

                var folderDetail = LocalTV.TreeSelect_FolderDetail;

                if (null != folderDetail)
                    TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, 0));

                ProgressOverlay.CloseForced();
            })
                .ShowDialog();
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
            new ProgressOverlay(new[] { "" }, new[] { "Cleaning up Nearest view" }, x =>
            {
                Util.LocalDispose(_lsDisposable);
                Cleanup_AllFileHashes_Scratch(LocalTV.RootNodes);
                GC.Collect();
                ProgressOverlay.CloseForced();
            })
                .ShowDialog();
        }

        void TreeSelect_FolderDetailUpdated(Tuple<TreeSelect.FolderDetailUpdated, int> initiatorTuple)
        {
            new ProgressOverlay(new[] { "" }, new[] { "Finding Nearest" }, x =>
            {
                ClearItems();
                _cts.Cancel();

                while (0 < _nRefCount)
                    Util.Block(20);

                var tuple = initiatorTuple.Item1;
                var thisNode = tuple.treeNode;

                if (null == thisNode.NodeDatum.AllFileHashes_Scratch)
                    return;

                _lsMatchingFolders = new ConcurrentBag<Tuple<int, LVitem_FolderListVM>>();
                ++_nRefCount;
                _cts = new CancellationTokenSource();
                FindMatchingFolders(thisNode, LocalTV.RootNodes);
                --_nRefCount;

                if (_cts.IsCancellationRequested)
                    return;

                if (_lsMatchingFolders?.Any() ?? false)
                {
                    var lsItems =
                        _lsMatchingFolders
                        .OrderByDescending(tupleA => tupleA.Item1)
                        .Select(tupleA => tupleA.Item2);

                    _lsMatchingFolders = null;
                    Util.UIthread(99912, () => Add(lsItems));
                }

                Util.WriteLine("FindMatchingFolders " + thisNode.NodeDatum.AllFileHashes_Scratch.GetHashCode());
                ProgressOverlay.CloseForced();
            })
                .ShowDialog();
        }

        bool FindMatchingFolders(LocalTreeNode thisNode, IReadOnlyList<LocalTreeNode> nodes)
        {
            if (_cts.IsCancellationRequested)
                return false;

            var allChildrenSet = thisNode.NodeDatum.AllFileHashes_Scratch;
            var bDoNotAddParent = false;

            Util.ParallelForEach(0, nodes,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = _cts.Token },
                treeNode =>
            {
                if ((thisNode.NodeDatum.AllFilesHash == treeNode.NodeDatum.AllFilesHash) ||
                    treeNode.IsChildOf(thisNode) ||
                    thisNode.IsChildOf(treeNode))
                {
                    bDoNotAddParent = true;
                    return;     // from lambda: continue
                }

                if (null == treeNode.NodeDatum.AllFileHashes_Scratch)
                    return;     // from lambda: continue

                var set = treeNode.NodeDatum.AllFileHashes_Scratch;
                var nSame = allChildrenSet.Where(n => set.Contains(n)).Count();

                if (nSame < allChildrenSet.Count/2)
                    return;     // from lambda: continue

                //if (thisNode.NodeDatum.FileHashes == nSame)
                //{
                //    // thisNode has all elements of treeNode. Find deepest subnode like this.
                //    var bFound = true;

                //    if (null != treeNode.Nodes)
                //        bFound = FindMatchingFolders(thisNode, treeNode.Nodes); // recurse 1 of 2

                //    if (false == bFound)
                //        _lsMatchingFolders.Add(Tuple.Create(1, new LVitem_FolderListVM(treeNode, false, _nicknameUpdater)));

                //    bDoNotAddParent = true;
                //    return;     // from lambda: continue
                //}

                //if (.8 < jaccard)
                //{
                //    _lsMatchingFolders.Add(Tuple.Create(jaccard, new LVitem_FolderListVM(treeNode, false, _nicknameUpdater)));
                //    bDoNotAddParent = true;
                //}
                //else if (.2 < jaccard)
                //{
                //    if (null != treeNode.Nodes)
                //        FindMatchingFolders(thisNode, treeNode.Nodes);          // recurse 2 of 2
                //}
            });

            return bDoNotAddParent;
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
                return null;

            var lsAllFilesHashes = new List<int> { };

            foreach (var treeNode in nodes)
            {
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
                    treeNode.NodeDatum.AllFileHashes_Scratch = lsAllFileHashes_childNodes.Distinct().OrderBy(n => n).ToArray();
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
    }
}
