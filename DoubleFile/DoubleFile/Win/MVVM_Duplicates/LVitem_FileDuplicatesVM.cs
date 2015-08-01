using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FileDuplicatesVM : ListViewItemVM_Base
    {
        internal string[] FileLine;
        internal LVitem_ProjectVM LVitem_ProjectVM;

        public string Filename => FileLine[0];
        public string Path { get { return SubItems[0]; } private set { SetProperty(0, value); } }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 1;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal LVitem_FileDuplicatesVM(IList<string> lsStr)
            : base(null, lsStr)
        {
        }
    }
}
