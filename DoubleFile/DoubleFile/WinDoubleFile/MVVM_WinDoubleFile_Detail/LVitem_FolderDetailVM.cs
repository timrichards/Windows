using System.Collections.Generic;
using System.Windows.Media;

namespace DoubleFile
{
    class LVitem_FolderDetailVM : ListViewItemVM_Base
    {
        public Brush Foreground { get; internal set; }
        public Brush Background { get; internal set; }

        public string Header { get { return marr[0]; } private set { SetProperty(0, value); } }
        public string Detail { get { return marr[1]; } private set { SetProperty(1, value); } }

        protected override string[] PropertyNames { get { return new[] { "Header", "Detail" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal LVitem_FolderDetailVM(IEnumerable<string> ieString = null)
            : base(null, ieString)
        {
            Foreground = Brushes.Black;
            Background = Brushes.Transparent;
        }

        internal LVitem_FolderDetailVM(LVitem_FolderDetailVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
