
namespace DoubleFile
{
    class LVitem_FileDuplicatesVM : ListViewItemVM_Base
    {
        public string Filename { get { return marr[0]; } set { SetProperty(0, value); } }
        public string Path { get { return marr[1]; } set { SetProperty(1, value); } }

        protected override string[] PropertyNames { get { return new[] { "Filename", "Path" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal LVitem_FileDuplicatesVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_FileDuplicatesVM(LVitem_FileDuplicatesVM lvItemTemp)
            : base(null, lvItemTemp.StringValues)
        {
        }
    }
}
