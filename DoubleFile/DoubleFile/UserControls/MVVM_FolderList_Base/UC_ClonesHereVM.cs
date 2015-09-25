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
        public bool SolitaryIsAllOneVol { internal get; set; }

        internal new UC_ClonesHereVM          // new to hide then call base.Init() and return this
            Init()
        {
            base.Init();
            Icmd_SolitaryIsAllOneVol = new RelayCommand(StartSearch);

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
            _lsFolders = new ConcurrentBag<LocalTreeNode>();
            FindAllClones(searchFolder);

            return _lsFolders.OrderByDescending(folder => folder.NodeDatum.LengthTotal)
                .Select(folder => new LVitem_FolderListVM(folder, _nicknameUpdater));
        }

        void FindAllClones(LocalTreeNode searchFolder)
        {
            if (_cts.IsCancellationRequested)
                return;

            Util.ParallelForEach(99620, searchFolder.Nodes, new ParallelOptions { CancellationToken = _cts.Token },
                folder =>
            {
                if (folder.IsSolitary)
                {
                    if (null != folder.Nodes)
                        FindAllClones(folder);

                    return;     // from lambda
                }

                if (false == (SolitaryIsAllOneVol && folder.IsAllOnOneVolume))
                    _lsFolders.Add(folder);
            });
        }

        protected override void Clear() => _lsFolders = null;

        ConcurrentBag<LocalTreeNode>
            _lsFolders = null;
    }
}
