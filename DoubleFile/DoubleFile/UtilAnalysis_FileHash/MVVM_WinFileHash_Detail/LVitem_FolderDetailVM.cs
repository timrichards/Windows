
namespace DoubleFile
{
    class LVitem_FolderDetailVM : ListViewItemVM_Base
    {
        public string Header { get { return marr[0]; } internal set { SetProperty(0, value); } }
        public string Detail { get { return marr[1]; } internal set { SetProperty(1, value); } }

        protected override string[] PropertyNames { get { return new[] { "Header", "Detail" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal LVitem_FolderDetailVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_FolderDetailVM(LVitem_FolderDetailVM lvItemTemp)
            : base(null, lvItemTemp.StringValues)
        {
        }
    }
}
