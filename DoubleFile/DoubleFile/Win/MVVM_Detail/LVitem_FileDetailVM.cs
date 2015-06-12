using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FileDetailVM : ListViewItemVM_Base
    {
        public string Header { get { return marr[0]; } private set { SetProperty(0, value); } }
        public string Detail { get { return marr[1]; } private set { SetProperty(1, value); } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        protected override IEnumerable<string> PropNames { get { return _propNames; } }
        static IEnumerable<string> _propNames = GetProps(typeof(LVitem_FileDetailVM));

        internal LVitem_FileDetailVM(IList<string> lsStr = null)
            : base(null, lsStr)
        {
        }

        internal LVitem_FileDetailVM(LVitem_FileDetailVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
