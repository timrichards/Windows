using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_ProjectSearch : LVitem_ProjectExplorer             // 2. Far fewer (# listing files), inside (3.)
    {
        internal LVitem_ProjectSearch(LVitem_ProjectExplorer lvItemTemp, NicknameUpdater nicknameUpdater)
            : base(lvItemTemp)
        {
            NicknameUpdater = nicknameUpdater;
        }

        internal NicknameUpdater NicknameUpdater;                   // One per search window, inside (2.)
    }

    class LVitem_SearchVM : ListViewItemVM_Base, INicknameUpdater   // 3. Many (N)
    {
        internal LVitem_ProjectSearch
            LVitemProjectSearch;
        internal PathBuilder
            Directory { get { return _datum.As<PathBuilder>(); } set { _datum = value; } }
        internal TabledString<TabledStringType_Files>
            TabledStringFilename;

        internal LocalTreeNode
            LocalTreeNode { get { return _datum.As<LocalTreeNode>(); } set { _datum = value; } }
        object _datum = null;

        public string
            In { get; private set; } = " in ";    // interned

        public int
            Alternate { get; internal set; } = 0;

        void 
            INicknameUpdater.RaiseNicknameChange()
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

                    strRet = LocalTreeNode.FullPathGet(LVitemProjectSearch.NicknameUpdater.UseNickname);
                }
                else
                {
                    if (null != TabledStringFilename)
                        return "" + TabledStringFilename;

                    strRet =
                        (LVitemProjectSearch.NicknameUpdater.UseNickname)
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
                    return LocalTreeNode.Parent?.FullPathGet(LVitemProjectSearch.NicknameUpdater.UseNickname);

                var strDirectory =
                    (LVitemProjectSearch.NicknameUpdater.UseNickname)
                    ? LVitemProjectSearch.InsertNickname(Directory)
                    : "" + Directory;

                var nIx = strDirectory.LastIndexOf('\\');

                return
                    ((null != TabledStringFilename) || (nIx < 0))
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
