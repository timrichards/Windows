
namespace DoubleFile
{
    class LVitem_FileHashVM : ListViewItemVM_Base
    {
        internal System.Collections.Generic.IEnumerable<FileDictionary.DuplicateStruct>
            LSduplicates = null;

        public string Filename { get { return marr[0]; } set { SetProperty(0, value); } }
        public string Duplicates { get { return marr[1]; } set { SetProperty(1, value); } }

        protected override string[] PropertyNames { get { return new[] { "Filename", "Duplicates" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal LVitem_FileHashVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_FileHashVM(LVitem_FileHashVM lvItemTemp)
            : base(null, lvItemTemp.StringValues)
        {
        }
    }
}
