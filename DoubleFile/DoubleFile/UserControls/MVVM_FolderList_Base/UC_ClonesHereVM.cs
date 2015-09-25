using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class UC_ClonesHereVM : UC_FolderListVM
    {
        internal new UC_ClonesHereVM          // new to hide then call base.Init() and return this
            Init()
        {
            base.Init();

            Util.ThreadMake(() =>
            {
                NoResultsFolder = null;
                RaisePropertyChanged("NoResultsFolder");
                HideProgressbar();

                var folderDetail = LocalTV.TreeSelect_FolderDetail;

                if (null != folderDetail)
                    StartSearch(folderDetail.treeNode);
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
                if (null == folder.NodeDatum.Clones)
                {
                    if (null != folder.Nodes)
                        FindAllClones(folder);

                    return;     // from lambda
                }

                _lsFolders.Add(folder);
            });
        }

        protected override void Clear() => _lsFolders = null;

        ConcurrentBag<LocalTreeNode>
            _lsFolders = null;
    }
}
