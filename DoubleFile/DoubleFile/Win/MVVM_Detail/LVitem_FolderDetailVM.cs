using System.Collections.Generic;
using System.Windows.Media;

namespace DoubleFile
{
    class LVitem_FolderDetailVM : ListViewItemVM_Base
    {
        public Brush Foreground { get; internal set; } = Brushes.White;
        public Brush Background { get; internal set; } = Brushes.Transparent;

        public string Header { get { return SubItems[0]; } private set { SetProperty(0, value); } }
        public string Detail { get { return SubItems[1]; } private set { SetProperty(1, value); } }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 2;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal LVitem_FolderDetailVM(IList<string> lsStr = null)
            : base(null, lsStr)
        {
        }
    }
}
