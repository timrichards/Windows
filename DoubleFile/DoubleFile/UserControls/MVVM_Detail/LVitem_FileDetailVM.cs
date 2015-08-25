using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FileDetailVM : ListViewItemVM_Base
    {
        public string Header { get { return SubItems[0]; } private set { SetProperty(0, value); } }
        public string Detail { get { return SubItems[1]; } private set { SetProperty(1, value); } }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 2;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal LVitem_FileDetailVM(IList<string> lsStr)
            : base(null, lsStr)
        {
        }
    }
}
