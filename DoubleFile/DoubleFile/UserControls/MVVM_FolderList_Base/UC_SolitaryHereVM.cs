using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_SolitaryHereVM : UC_FolderListVM_Base
    {
        public ICommand Icmd_SolitaryIsAllOneVol { get; private set; }
        public bool SolitaryIsAllOneVol { internal get; set; }
        public ICommand Icmd_Export { get; private set; }

        internal new UC_SolitaryHereVM          // new to hide then call base.Init() and return this
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
            _lsFolders = new ConcurrentBag<LocalTreeNode>();
            FindAllSolitary(searchFolder);

            return _lsFolders.OrderByDescending(folder => folder.NodeDatum.LengthTotal)
                .Select(folder => new LVitem_FolderListVM(folder, _nicknameUpdater));
        }

        void FindAllSolitary(LocalTreeNode searchFolder)
        {
            if (_cts.IsCancellationRequested)
                return;

            Util.ParallelForEach(99912, searchFolder.Nodes, new ParallelOptions { CancellationToken = _cts.Token },
                folder =>
            {
                if ((false == folder.IsSolitary)
                    && (false == (SolitaryIsAllOneVol && folder.IsAllOnOneVolume)))
                {
                    return;     // from lambda
                }

                if (0 < folder.NodeDatum.LengthHere)
                    _lsFolders.Add(folder);

                if (null != folder.Nodes)
                    FindAllSolitary(folder);
            });
        }

        protected override void Clear() => _lsFolders = null;

        ConcurrentBag<LocalTreeNode>
            _lsFolders = null;
    }
}
