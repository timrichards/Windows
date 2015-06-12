using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_VolumeDetailVM : ListViewItemVM_Base
    {
        public string Header { get { return marr[0]; } private set { SetProperty(0, value); } }
        public string Detail { get { return marr[1]; } private set { SetProperty(1, value); } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal LVitem_VolumeDetailVM(IList<string> lsStr = null)
            : base(null, lsStr)
        {
        }

        internal LVitem_VolumeDetailVM(LVitem_VolumeDetailVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
