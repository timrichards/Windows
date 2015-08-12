using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    // can't be struct because of pass-by-reference
    class WinSearch_Instance                                   // One per search window, inside (2.)
    {
        internal bool UseNickname;

        internal void LastGet(LVitem_SearchVM lvItemSearchVM)
        {
            if (_bUpdating)
                return;

            _lastGets.Add(lvItemSearchVM);
        }

        internal void UpdateNicknames(bool bUseNickname)
        {
            UseNickname = bUseNickname;

            _lastGets = _lastGets.Take(1024).ToList();
            _bUpdating = true;

            foreach (var lvitemSearchVM in _lastGets)
                lvitemSearchVM.RaiseNicknameChange();

            _bUpdating = false;
        }

        internal void Clear() =>
            _lastGets = new List<LVitem_SearchVM> { };

        List<LVitem_SearchVM>
            _lastGets = new List<LVitem_SearchVM> { };
        bool
            _bUpdating = false;
    }

    class LVitem_ProjectSearch : LVitem_ProjectExplorer        // 2. Far fewer (# listing files), inside (3.)
    {
        internal LVitem_ProjectSearch(LVitem_ProjectExplorer lvItemTemp, WinSearch_Instance winSearchInstance)
            : base(lvItemTemp)
        {
            WinSearchInstance = winSearchInstance;
        }

        internal WinSearch_Instance WinSearchInstance;
    }

    class LVitem_SearchVM : ListViewItemVM_Base                 // 3. Many (N)
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
                LVitemProjectSearch.WinSearchInstance.LastGet(this);

                string strRet = null;

                if (null != LocalTreeNode)
                {
                    var parent = LocalTreeNode.Parent;

                    if (null != parent)
                        return LocalTreeNode.Text;

                    strRet = LocalTreeNode.FullPathGet(LVitemProjectSearch.WinSearchInstance.UseNickname);
                }
                else
                {
                    if (null != TabledStringFilename)
                        return "" + TabledStringFilename;

                    strRet =
                        (LVitemProjectSearch.WinSearchInstance.UseNickname)
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
                    return LocalTreeNode.Parent?.FullPathGet(LVitemProjectSearch.WinSearchInstance.UseNickname);

                var strDirectory =
                    (LVitemProjectSearch.WinSearchInstance.UseNickname)
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
