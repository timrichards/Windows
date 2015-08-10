using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FolderListVM : ListViewItemVM_Base
    {
        public bool Alternate { get; internal set; }

        public string Folder => LocalTreeNode.Text;
        public string Parent => LocalTreeNode.Parent?.FullPath;

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal LocalTreeNode LocalTreeNode;
    }
}
