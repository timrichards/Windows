using System.Collections.Generic;
using System.Windows.Media;

namespace DoubleFile
{
    class LVitem_TreeListVM : ListViewItemVM_Base
    {
        public Brush Foreground => LocalTreeNode.Foreground;
        public Brush Background => LocalTreeNode.Background;

        public string Folder { get { return SubItems[0]; } private set { SetProperty(0, value); } }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 1;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal LVitem_TreeListVM(IList<string> lsStr)
            : base(null, lsStr)
        {
        }

        internal LocalTreeNode LocalTreeNode;
    }
}
