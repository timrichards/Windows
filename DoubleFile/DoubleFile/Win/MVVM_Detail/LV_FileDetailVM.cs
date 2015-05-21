using System.Windows.Input;
namespace DoubleFile
{
    partial class LV_FileDetailVM : ListViewVM_Base<LVitem_FileDetailVM>
    {
        public ICommand Icmd_Copy { get; private set; }

        public string LocalPath { get; private set; }
        void LocalPath_Set(LocalTreeNode treeNode = null, string strFile = null)
        {
            var strLocalPath =
                (null != treeNode)
                ? treeNode.FullPath +
                    ((false == string.IsNullOrEmpty(strFile))
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
                RaisePropertyChanged("Title");
            }
        }
        string _Title = null;

        public string WidthHeader { get { return SCW; } }                   // franken all NaN
        public string WidthDetail { get { return SCW; } }   // not used

        internal override int NumCols { get { return LVitem_FileDetailVM.NumCols_; } }
    }
}
