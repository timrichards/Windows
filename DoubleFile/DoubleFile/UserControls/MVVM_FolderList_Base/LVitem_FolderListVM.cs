using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FolderListVM : ListViewItemVM_Base, IListUpdater
    {
        internal override string
            ExportLine => Folder.PathFullGet(_nicknameUpdater.Value);

        public bool
            Alternate { get; internal set; }
        public string
            PathShort => Folder.PathShort;
        public string
            In { get; private set; } = " in ";    // interned

        public string
            Parent
        {
            get
            {
                var parent = Folder.Parent;

                if (null != parent)
                {
                    _nicknameUpdater.LastGet(this);
                    return parent.PathFullGet(_nicknameUpdater.Value);
                }

                In = null;
                RaisePropertyChanged("In");
                return null;
            }
        }

        void IListUpdater.RaiseListUpdate() => RaisePropertyChanged("Parent");

        internal LVitem_FolderListVM(LocalTreeNode folder, ListUpdater<bool> nicknameUpdater)
        {
            Folder = folder;
            _nicknameUpdater = nicknameUpdater;
        }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal readonly LocalTreeNode
            Folder;

        readonly ListUpdater<bool>
            _nicknameUpdater;
    }
}
