using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FileDuplicatesVM : ListViewItemVM_Base
    {
        internal IReadOnlyList<string> FileLine;
        internal LVitem_ProjectVM LVitem_ProjectVM;

        public string Filename => FileLine[0];
        public string Path { get { return SubItems[0]; } private set { SetProperty(0, value); } }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 1;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal LVitem_FileDuplicatesVM(IList<string> lsStr)
            : base(null, lsStr)
        {
        }
    }
}
