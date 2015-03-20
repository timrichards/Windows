﻿using System.Windows.Media;

namespace DoubleFile
{
    class LVitem_TreeListVM : ListViewItemVM_Base
    {
        public Brush Foreground { get { return (UtilColor.Empty == LocalTreeNode.ForeColor) ? Brushes.Black : UtilColor.ARGBtoBrush(LocalTreeNode.ForeColor); } }
        public Brush Background { get { return UtilColor.ARGBtoBrush(LocalTreeNode.BackColor); } }
        internal LocalTreeNode LocalTreeNode { get; set; }

        public string Folder { get { return marr[0]; } internal set { SetProperty(0, value); } }

        protected override string[] PropertyNames { get { return new[] { "Folder" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 1;

        internal LVitem_TreeListVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_TreeListVM(LVitem_TreeListVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
