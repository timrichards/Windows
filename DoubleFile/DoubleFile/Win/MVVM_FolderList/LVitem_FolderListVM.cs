using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FolderListVM : ListViewItemVM_Base, IListUpdater
    {
        public bool Alternate { get; }

        public string Folder => LocalTreeNode.Text;

        public string
            In { get; private set; } = " in ";    // interned

        public string Parent
        {
            get
            {
                _nicknameUpdater.LastGet(this);

                var strRet = LocalTreeNode.Parent?.FullPathGet(_nicknameUpdater.Value);

                if (null == strRet)
                {
                    In = null;
                    RaisePropertyChanged("In");
                }

                return strRet;
            }
        }

        void IListUpdater.RaiseListUpdate() => RaisePropertyChanged("Parent");

        internal LVitem_FolderListVM(LocalTreeNode folder, bool bAlternate, ListUpdater<bool> nicknameUpdater)
        {
            LocalTreeNode = folder;
            Alternate = bAlternate;
            _nicknameUpdater = nicknameUpdater;
        }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal readonly LocalTreeNode
            LocalTreeNode;

        readonly ListUpdater<bool>
            _nicknameUpdater;
    }
}
