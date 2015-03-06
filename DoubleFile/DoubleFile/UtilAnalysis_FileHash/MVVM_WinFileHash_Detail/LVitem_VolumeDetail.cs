namespace DoubleFile
{
    class LVitem_VolumeDetailVM : ListViewItemVM_Base
    {
        public string Header { get { return marr[0]; } internal set { SetProperty(0, value); } }
        public string Detail { get { return marr[1]; } internal set { SetProperty(1, value); } }

        protected override string[] PropertyNames { get { return new[] { "Header", "Detail" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal LVitem_VolumeDetailVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_VolumeDetailVM(LVitem_VolumeDetailVM lvItemTemp)
            : base(null, lvItemTemp.StringValues)
        {
        }
    }
}
