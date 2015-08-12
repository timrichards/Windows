using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FolderListVM : ListViewItemVM_Base, INicknameUpdater
    {
        public bool Alternate { get; }

        public string Folder => LocalTreeNode.Text;
        public string Parent
        {
            get
            {
                NicknameUpdater.LastGet(this);
                return LocalTreeNode.Parent?.FullPathGet(NicknameUpdater.UseNickname);
            }
        }

        void INicknameUpdater.RaiseNicknameChange() => RaisePropertyChanged("Parent");

        internal LVitem_FolderListVM(LocalTreeNode folder, bool bAlternate, NicknameUpdater nicknameUpdater)
        {
            LocalTreeNode = folder;
            Alternate = bAlternate;
            NicknameUpdater = nicknameUpdater;
        }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal readonly LocalTreeNode LocalTreeNode;
        readonly NicknameUpdater NicknameUpdater;
    }
}
