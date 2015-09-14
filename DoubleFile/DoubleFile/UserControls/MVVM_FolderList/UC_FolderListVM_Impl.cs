using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace DoubleFile
{
    partial class UC_FolderListVM : IDisposable
    {
        internal UC_FolderListVM()
        {
            _dictClones = new Dictionary<int, IEnumerable<int>>();
            Setup_AllFileHashes_Scratch(LocalTV.RootNodes);
            _dictClones = null;

            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99701, TreeSelect_FolderDetailUpdated));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, 0));
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
            Util.LocalDispose(_lsDisposable);
            Cleanup_AllFileHashes_Scratch(LocalTV.RootNodes);
            GC.Collect();
        }

        void TreeSelect_FolderDetailUpdated(Tuple<TreeSelect.FolderDetailUpdated, int> initiatorTuple)
        {
            ClearItems();
            _cts.Cancel();

            while (0 < _nRefCount)
                Util.Block(20);

            var tuple = initiatorTuple.Item1;
            var thisNode = tuple.treeNode;

            if (null == thisNode.NodeDatum.AllFileHashes_Scratch)
                return;

            _lsMatchingFolders = new ConcurrentBag<Tuple<double, LVitem_FolderListVM>>();
            ++_nRefCount;
            _cts = new CancellationTokenSource();
            FindMatchingFolders(thisNode, LocalTV.RootNodes);
            --_nRefCount;

            if (_cts.IsCancellationRequested)
                return;

            if (_lsMatchingFolders?.Any() ?? false)
            {
                var lsItems = _lsMatchingFolders.OrderByDescending(tupleA => tupleA.Item1);

                _lsMatchingFolders = null;
                Util.UIthread(99912, () => Add(lsItems.Select(tupleA => tupleA.Item2)));
                RaiseItems();
            }

            Util.WriteLine("AllFileHashes_Scratch " + thisNode.NodeDatum.AllFileHashes_Scratch.GetHashCode());
        }
        ConcurrentBag<Tuple<double,LVitem_FolderListVM>>
            _lsMatchingFolders = null;
        CancellationTokenSource
            _cts = new CancellationTokenSource();
        int
            _nRefCount = 0;

        bool FindMatchingFolders(LocalTreeNode thisNode, IReadOnlyList<LocalTreeNode> nodes)
        {
            if (_cts.IsCancellationRequested)
                return false;

            var thisSet = thisNode.NodeDatum.AllFileHashes_Scratch;
            var bFound = false;

            Util.ParallelForEach(0, nodes,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = _cts.Token },
                treeNode =>
            {
                if (treeNode.IsChildOf(thisNode))
                    return;     // from lambda: continue

                if (thisNode.IsChildOf(treeNode))
                    return;     // from lambda: continue

                if (null == treeNode.NodeDatum.AllFileHashes_Scratch)
                    return;     // from lambda: continue

                if (thisNode.NodeDatum.AllFilesHash == treeNode.NodeDatum.AllFilesHash)
                    return;     // from lambda: continue

                var set = treeNode.NodeDatum.AllFileHashes_Scratch;

                if (false == set.Where(n => thisSet.Contains(n)).Any())
                    return;     // from lambda: continue

                var bRecurse2 = true;

                if (false == set.Except(thisSet).Any())
                {
                    // treeNode has all elements of thisNode. Find deepest subnode like this.
                    var bFoundA = false;

                    if (null != treeNode.Nodes)
                        bFoundA = FindMatchingFolders(thisNode, treeNode.Nodes);    // recurse

                    if (false == bFoundA)
                        _lsMatchingFolders.Add(Tuple.Create(1d, new LVitem_FolderListVM(treeNode, false, _nicknameUpdater)));

                    bRecurse2 = false;
                    bFound |= bFoundA;
                }
                else
                {
                    var jaccard = Util.Jaccard(thisSet, set);

                    if (.8 < jaccard)
                        _lsMatchingFolders.Add(Tuple.Create(jaccard, new LVitem_FolderListVM(treeNode, false, _nicknameUpdater)));
                    else if (.2 > jaccard)
                        bRecurse2 = false;
                }

                if (bRecurse2)
                {
                    if (null != treeNode.Nodes)
                        FindMatchingFolders(thisNode, treeNode.Nodes);              // recurse
                }
            });

            return bFound;
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
                var lsAllFileHashes_treeNode = treeNode.NodeDatum.FileHashes.ToList();

                if (null != treeNode.Nodes)
                    lsAllFileHashes_treeNode.AddRange(Setup_AllFileHashes_Scratch(treeNode.Nodes, bStart: false));    // recurse

                IEnumerable<int> lsClone = null;

                if (_dictClones.TryGetValue(treeNode.NodeDatum.AllFilesHash, out lsClone))
                {
                    treeNode.NodeDatum.AllFileHashes_Scratch = lsClone;
                }
                else
                {
                    treeNode.NodeDatum.AllFileHashes_Scratch = lsAllFileHashes_treeNode.Distinct().ToArray();
                    _dictClones.Add(treeNode.NodeDatum.AllFilesHash, treeNode.NodeDatum.AllFileHashes_Scratch);
                }

                if (false == bStart)
                    lsAllFilesHashes.AddRange(lsAllFileHashes_treeNode);
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

        Dictionary<int, IEnumerable<int>>
            _dictClones = null;
        readonly ListUpdater<bool>
            _nicknameUpdater = new ListUpdater<bool>(99667);
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
