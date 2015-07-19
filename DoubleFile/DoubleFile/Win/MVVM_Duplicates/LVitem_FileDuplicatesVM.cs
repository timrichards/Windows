using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FileDuplicatesVM : ListViewItemVM_Base
    {
        internal string[] FileLine { get; set; }
        internal LVitem_ProjectVM LVitem_ProjectVM { get; set; }

        public string Filename { get { return FileLine[0]; } }
        public string Path { get { return SubItems[0]; } private set { SetProperty(0, value); } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal LVitem_FileDuplicatesVM(IList<string> lsStr)
            : base(null, lsStr)
        {
        }
    }
}
