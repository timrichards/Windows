using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_ClonesHereVM : UC_FolderListVM_Base
    {
        public ICommand Icmd_SolitaryIsAllOneVol { get; private set; }
        public bool AllOneVolIsSolitary { internal get; set; }
        public ICommand Icmd_Export { get; private set; }

        internal new UC_ClonesHereVM          // new to hide then call base.Init() and return this
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
            FindAllClones(searchFolder);
            SetFoldersHeader(Util.FormatSize((ulong)_lsFolders.Sum(lvItem => (long)lvItem.TreeNode.NodeDatum.LengthTotal)));
            return _lsFolders.OrderByDescending(lvItem => lvItem.TreeNode.NodeDatum.LengthTotal);
        }

        void FindAllClones(LocalTreeNode searchFolder)
        {
            if (_cts.IsCancellationRequested)
                return;

            Util.ParallelForEach(99620, searchFolder.Nodes, new ParallelOptions { CancellationToken = _cts.Token },
                treeNode =>
            {
                if (treeNode.IsSolitary)
                {
                    if (null != treeNode.Nodes)
                        FindAllClones(treeNode);                 // recurse

                    return;     // from lambda
                }

                if (false ==
                    (AllOneVolIsSolitary && treeNode.IsAllOnOneVolume && (false == ReferenceEquals(treeNode, treeNode.Clones[0]))))
                {
                    _lsFolders.Add(new LVitem_FolderListVM(treeNode, _nicknameUpdater) { Alternate = treeNode.IsAllOnOneVolume });
                }
            });
        }

        protected override void Clear() => _lsFolders = null;

        ConcurrentBag<LVitem_FolderListVM>
            _lsFolders = null;
    }
}
