using System.Windows.Input;

namespace DoubleFile
{
    partial class LV_FileDetailVM : ListViewVM_Base<LVitem_FileDetailVM>
    {
        public ICommand Icmd_Copy { get; }

        public string LocalPath { get; private set; }
        void LocalPath_Set(LocalTreeNode treeNode = null, string strFile = null)
        {
            var strLocalPath =
                (null != treeNode)
                ? treeNode.PathFull +
                    ((false == string.IsNullOrWhiteSpace(strFile))
                    ? '\\' + strFile
                    : "")
                : null;

            if (strLocalPath == LocalPath)
                return;

            LocalPath = strLocalPath;
            RaisePropertyChanged("LocalPath");
        }

        public string Title
        {
            get { return ("" + _Title).Replace("_", "__"); }
            private set
            {
                _Title = value;
                RaisePropertyChanged();
            }
        }
        string _Title = null;

        public string WidthHeader => SCW;       // franken all NaN
        public string WidthDetail => SCW;       // not used

        internal override int NumCols => LVitem_FileDetailVM.NumCols_;
    }
}
