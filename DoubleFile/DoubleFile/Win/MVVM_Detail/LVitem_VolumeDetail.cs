using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_VolumeDetailVM : ListViewItemVM_Base
    {
        public string Header { get { return SubItems[0]; } private set { SetProperty(0, value); } }
        public string Detail { get { return SubItems[1]; } private set { SetProperty(1, value); } }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 2;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal LVitem_VolumeDetailVM(IList<string> lsStr = null)
            : base(null, lsStr)
        {
        }
    }
}
