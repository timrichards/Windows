
namespace DoubleFile
{
    class LVitem_ProgressVM : ListViewItemVM
    {
        public string VolumeName { get { return marr[0]; } set { SetProperty(0, value); } }
        public string Path { get { return marr[1]; } set { SetProperty(1, value); } }
        public decimal Progress { get { return m_nProgress; } set { m_nProgress = value; RaisePropertyChanged(PropertyNames[2]); } }
        readonly static string[] marrPropName = new string[] { "VolumeName", "Path", "Progress" };
        internal const int NumCols_ = 3;

        internal LVitem_ProgressVM(LV_ProgressVM LV, string[] arrStr)
            : base(LV, arrStr)
        {
        }

        internal override int NumCols { get { return NumCols_; } }
        protected override string[] PropertyNames { get { return marrPropName; } }

        decimal m_nProgress = 0;
    }
}
