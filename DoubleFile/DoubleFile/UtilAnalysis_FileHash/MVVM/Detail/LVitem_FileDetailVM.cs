
namespace DoubleFile
{
    class LVitem_FileDetailVM : ListViewItemVM_Base
    {
        public string Header { get { return marr[0]; } set { SetProperty(0, value); } }
        public string Detail { get { return marr[1]; } set { SetProperty(1, value); } }

        protected override string[] PropertyNames { get { return new[] { "Header", "Detail" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal LVitem_FileDetailVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_FileDetailVM(LVitem_FileDetailVM lvItemTemp)
            : base(null, lvItemTemp.StringValues)
        {
        }
    }
}
