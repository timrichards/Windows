using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FileDuplicatesVM : ListViewItemVM_Base
    {
        internal string[] FileLine { get; set; }
        internal LVitem_ProjectVM LVitem_ProjectVM { get; set; }

        public string Filename { get { return FileLine[0]; } }
        public string Path { get { return marr[0]; } private set { SetProperty(0, value); } }

        protected override string[] PropertyNames { get { return new[] { "Filename", "Path" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal LVitem_FileDuplicatesVM(IEnumerable<string> ieString = null)
            : base(null, ieString)
        {
        }

        internal LVitem_FileDuplicatesVM(LVitem_FileDuplicatesVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
