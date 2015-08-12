using System.Collections.Generic;

namespace DoubleFile
{
    // can't be struct because of pass-by-reference
    class WinSearch_Instance                                   // One per search window, inside (2.)
    {
        internal bool UseNickname;
    }

    class LVitem_SearchExplorer : LVitem_ProjectExplorer        // 2. Far fewer (# listing files), inside (3.)
    {
        internal LVitem_SearchExplorer(LVitem_ProjectExplorer lvItemTemp, WinSearch_Instance winSearchInstance)
            : base(lvItemTemp)
        {
            WinSearchInstance = winSearchInstance;
        }

        internal WinSearch_Instance WinSearchInstance;
    }

    class LVitem_SearchVM : ListViewItemVM_Base                 // 3. Many (N)
    {
        internal LVitem_SearchExplorer
            LVitemSearchExplorer;
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

        internal void 
            RaiseNicknameChange()
        {
            RaisePropertyChanged("FolderOrFile");
            RaisePropertyChanged("Parent");
        }

        public string
            FolderOrFile
        {
            get
            {
                string strRet = null;

                if (null != LocalTreeNode)
                {
                    var parent = LocalTreeNode.Parent;

                    if (null != parent)
                        return LocalTreeNode.Text;

                    strRet = LocalTreeNode.FullPathGet(LVitemSearchExplorer.WinSearchInstance.UseNickname);
                }
                else
                {
                    if (null != TabledStringFilename)
                        return "" + TabledStringFilename;

                    strRet =
                        (LVitemSearchExplorer.WinSearchInstance.UseNickname)
                        ? LVitemSearchExplorer.InsertNickname(Directory)
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
                    return LocalTreeNode.Parent?.FullPathGet(LVitemSearchExplorer.WinSearchInstance.UseNickname);

                var strDirectory =
                    (LVitemSearchExplorer.WinSearchInstance.UseNickname)
                    ? LVitemSearchExplorer.InsertNickname(Directory)
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
