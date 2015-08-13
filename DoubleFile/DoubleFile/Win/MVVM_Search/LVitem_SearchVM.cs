using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_ProjectSearch : LVitem_ProjectExplorer         // 2. Far fewer (# listing files), inside (3.)
    {
        internal LVitem_ProjectSearch(LVitem_ProjectExplorer lvItemTemp, ListUpdater<bool> nicknameUpdater)
            : base(lvItemTemp)
        {
            NicknameUpdater = nicknameUpdater;
        }

        internal readonly ListUpdater<bool> NicknameUpdater;    // One per search window, inside (2.)
    }

    class LVitem_SearchVM : ListViewItemVM_Base, IListUpdater   // 3. Many (N)
    {
        internal readonly LVitem_ProjectSearch
            LVitemProjectSearch;

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

        internal LVitem_SearchVM(LVitem_ProjectSearch lvItemProjectSearch, PathBuilder directory)
        {
            LVitemProjectSearch = lvItemProjectSearch;
            _datum = directory;
        }

        internal LVitem_SearchVM(LVitem_ProjectSearch lvItemProjectSearch, PathBuilder directory,
            TabledString<TabledStringType_Files> tabledFilename, int nAlternate)
            : this(lvItemProjectSearch, directory)
        {
            TabledStringFilename = tabledFilename;
            Alternate = nAlternate;
        }

        internal LVitem_SearchVM(LVitem_ProjectSearch lvItemProjectSearch, LocalTreeNode localTreeNode)
        {
            LVitemProjectSearch = lvItemProjectSearch;
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
                LVitemProjectSearch.NicknameUpdater.LastGet(this);

                string strRet = null;

                if (null != LocalTreeNode)
                {
                    var parent = LocalTreeNode.Parent;

                    if (null != parent)
                        return LocalTreeNode.Text;

                    strRet = LocalTreeNode.FullPathGet(LVitemProjectSearch.NicknameUpdater.Value);
                }
                else
                {
                    if (null != TabledStringFilename)
                        return "" + TabledStringFilename;

                    strRet =
                        (LVitemProjectSearch.NicknameUpdater.Value)
                        ? LVitemProjectSearch.InsertNickname(Directory)
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
                    return LocalTreeNode.Parent?.FullPathGet(LVitemProjectSearch.NicknameUpdater.Value);

                var strDirectory =
                    (LVitemProjectSearch.NicknameUpdater.Value)
                    ? LVitemProjectSearch.InsertNickname(Directory)
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
