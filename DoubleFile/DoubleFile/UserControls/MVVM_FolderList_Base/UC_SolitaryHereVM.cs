using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_SolitaryHereVM : UC_FolderListVM_Base
    {
        public ICommand Icmd_SolitaryIsAllOneVol { get; private set; }
        public bool AllOneVolIsSolitary { internal get; set; }
        public ICommand Icmd_Export { get; private set; }

        internal new UC_SolitaryHereVM          // new to hide then call base.Init() and return this
            Init()
        {
            base.Init();
            Icmd_SolitaryIsAllOneVol = new RelayCommand(StartSearch);
            Icmd_Export = new RelayCommand(Export);

            Util.ThreadMake(() =>
            {
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
            });
        }

        protected override IEnumerable<LVitem_FolderListVM>
            FillList(LocalTreeNode searchFolder)
        {
            _lsFolders = new ConcurrentBag<LVitem_FolderListVM>();
            _nLengthTotal = 0;
            FindAllSolitary(searchFolder);
            SetFoldersHeader(Util.FormatSize((ulong)_nLengthTotal));
            return _lsFolders.OrderByDescending(lvItem => lvItem.TreeNode.NodeDatum.LengthTotal);
        }

        void FindAllSolitary(LocalTreeNode searchFolder, bool bStart = true)
        {
            if (_cts.IsCancellationRequested)
                return;

            Util.ParallelForEach(99912, searchFolder.Nodes, new ParallelOptions { CancellationToken = _cts.Token },
                treeNode =>
            {
                if ((false == treeNode.IsSolitary)
                    && (false == (AllOneVolIsSolitary && treeNode.IsAllOnOneVolume)))
                {
                    return;     // from lambda
                }

                // Can't descend all copies. Take just element 0, count that one as solitary
                if (false == ReferenceEquals(treeNode, (treeNode.Clones?[0] ?? treeNode)))
                    return;     // from lambda

                var nodeDatum = treeNode.NodeDatum;

                if (0 < nodeDatum.LengthHere)
                    _lsFolders.Add(new LVitem_FolderListVM(treeNode, _nicknameUpdater) { Alternate = treeNode.IsAllOnOneVolume });

                if (bStart)
                    Interlocked.Add(ref _nLengthTotal, (long)nodeDatum.LengthTotal);

                if (null != treeNode.Nodes)
                    FindAllSolitary(treeNode, bStart: false);                 // recurse
            });
        }

        protected override void Clear() => _lsFolders = null;

        long
            _nLengthTotal = 0;
        ConcurrentBag<LVitem_FolderListVM>
            _lsFolders = null;
    }
}
