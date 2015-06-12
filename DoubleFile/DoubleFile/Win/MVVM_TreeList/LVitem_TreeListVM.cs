using System.Collections.Generic;
using System.Windows.Media;

namespace DoubleFile
{
    class LVitem_TreeListVM : ListViewItemVM_Base
    {
        public Brush Foreground { get { return LocalTreeNode.Foreground; } }
        public Brush Background { get { return LocalTreeNode.Background; } }
        internal LocalTreeNode LocalTreeNode { get; set; }

        public string Folder { get { return marr[0]; } private set { SetProperty(0, value); } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 1;

        protected override IEnumerable<string> PropNames { get { return _propNames; } }
        static IEnumerable<string> _propNames = GetProps(typeof(LVitem_TreeListVM));

        internal LVitem_TreeListVM(IList<string> lsStr = null)
            : base(null, lsStr)
        {
        }

        internal LVitem_TreeListVM(LVitem_TreeListVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
