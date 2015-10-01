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
                NoResultsFolder = null;
                RaisePropertyChanged("NoResultsFolder");
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
            SetFoldersHeader(Util.FormatSize((ulong)_lsFolders.Sum(lvItem => (long)lvItem.Folder.NodeDatum.LengthTotal)));
            return _lsFolders.OrderByDescending(lvItem => lvItem.Folder.NodeDatum.LengthTotal);
        }

        void FindAllClones(LocalTreeNode searchFolder)
        {
            if (_cts.IsCancellationRequested)
                return;

            Util.ParallelForEach(99620, searchFolder.Nodes, new ParallelOptions { CancellationToken = _cts.Token },
                folder =>
            {
                var nodeDatum = folder.NodeDatum;

                if (nodeDatum.IsSolitary)
                {
                    if (null != folder.Nodes)
                        FindAllClones(folder);                 // recurse

                    return;     // from lambda
                }

                if (false ==
                    (AllOneVolIsSolitary && nodeDatum.IsAllOnOneVolume && (false == ReferenceEquals(folder, nodeDatum.Clones[0]))))
                {
                    _lsFolders.Add(new LVitem_FolderListVM(folder, _nicknameUpdater) { Alternate = nodeDatum.IsAllOnOneVolume });
                }
            });
        }

        protected override void Clear() => _lsFolders = null;

        ConcurrentBag<LVitem_FolderListVM>
            _lsFolders = null;
    }
}
