using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_SearchVM : ListViewItemVM_Base, IListUpdater
    {
        internal readonly LVitemProject_Updater<bool>
            LVitemProject_Updater;

        internal PathBuilder
            Directory => _datum.As<PathBuilder>();
        internal readonly TabledString<TabledStringType_Files>
            TabledStringFilename;

        internal LocalTreeNode
            LocalTreeNode => _datum.As<LocalTreeNode>();
        readonly object _datum = null;

        public string
            In { get; private set; } = " in ";    // interned

        public int
            Alternate { get; } = 0;

        internal LVitem_SearchVM(LVitemProject_Updater<bool> lvItemProjectUpdater, PathBuilder directory)
        {
            LVitemProject_Updater = lvItemProjectUpdater;
            _datum = directory;
        }

        internal LVitem_SearchVM(LVitemProject_Updater<bool> lvItemProjectSearch, PathBuilder directory,
            TabledString<TabledStringType_Files> tabledFilename, int nAlternate)
            : this(lvItemProjectSearch, directory)
        {
            TabledStringFilename = tabledFilename;
            Alternate = nAlternate;
        }

        internal LVitem_SearchVM(LVitemProject_Updater<bool> lvItemProjectSearch, LocalTreeNode localTreeNode)
        {
            LVitemProject_Updater = lvItemProjectSearch;
            _datum = localTreeNode;
        }

        void 
            IListUpdater.RaiseListUpdate()
        {
            RaisePropertyChanged("FolderOrFile");
            RaisePropertyChanged("Parent");
        }

        public string
            FolderOrFile
        {
            get
            {
                LVitemProject_Updater.ListUpdater.LastGet(this);

                string strRet = null;

                if (null != LocalTreeNode)
                {
                    var parent = LocalTreeNode.Parent;

                    if (null != parent)
                        return LocalTreeNode.Text;

                    strRet = LocalTreeNode.FullPathGet(LVitemProject_Updater.ListUpdater.Value);
                }
                else
                {
                    if (null != TabledStringFilename)
                        return "" + TabledStringFilename;

                    strRet =
                        (LVitemProject_Updater.ListUpdater.Value)
                        ? LVitemProject_Updater.InsertNickname("" + Directory)
                        : "" + Directory;
                }

                In = null;
                RaisePropertyChanged("In");
                RaisePropertyChanged("Parent");
                return strRet;
            }
        }

        public string
            Parent
        {
            get
            {
                if (null == In)
                    return null;

                if (null != LocalTreeNode)
                    return LocalTreeNode.Parent?.FullPathGet(LVitemProject_Updater.ListUpdater.Value);

                var strDirectory =
                    (LVitemProject_Updater.ListUpdater.Value)
                    ? LVitemProject_Updater.InsertNickname("" + Directory)
                    : "" + Directory;

                var nIx = strDirectory.LastIndexOf('\\');

                return
                    ((null != TabledStringFilename) || (0 > nIx))
                    ? strDirectory
                    : strDirectory.Substring(0, nIx);
            }
        }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;
    }
}
