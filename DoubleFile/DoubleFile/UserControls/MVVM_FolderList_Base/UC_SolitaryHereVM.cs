namespace DoubleFile
{
    partial class UC_SolitaryHereVM : UC_FolderListVM
    {
        internal new UC_SolitaryHereVM          // new to hide then call base.Init() and return this
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
                    TreeSelect_FolderDetailUpdated(folderDetail.treeNode);
            });

            return this;
        }

        public override void Dispose()          // calls base.Dispose() which implements IDisposable
        {
            Util.ThreadMake(() =>
            {
                base.Dispose();                 // sets _bDisposed and _cts.Cancel()
            });
        }

        protected override void TreeSelect_FolderDetailUpdated(LocalTreeNode searchFolder)
        {
            if (_cts.IsCancellationRequested)
                return;
        }
    }
}
