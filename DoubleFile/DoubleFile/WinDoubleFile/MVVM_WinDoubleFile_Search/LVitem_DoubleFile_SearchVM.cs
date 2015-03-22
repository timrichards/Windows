namespace DoubleFile
{
    class LVitem_DoubleFile_SearchVM : ListViewItemVM_Base
    {
        public string Results { get { return marr[0]; } internal set { SetProperty(0, value); } }

        protected override string[] PropertyNames { get { return new[] { "Results" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 1;

        internal LVitem_DoubleFile_SearchVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_DoubleFile_SearchVM(LVitem_DoubleFile_SearchVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
