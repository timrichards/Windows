
using System.Windows.Media;
namespace DoubleFile
{
    class LVitem_ProgressVM : ListViewItemVM
    {
        public string VolumeName { get { return marr[0]; } set { SetProperty(0, value); } }
        public string Path { get { return marr[1]; } set { SetProperty(1, value); } }

        double m_nProgress = 0;
        public double Progress { get { return m_nProgress; } set { m_nProgress = value; RaisePropertyChanged(PropertyNames[2]); } }

        bool m_bIndeterminate = true;
        Brush m_brushProgressState = Brushes.Yellow;
        public bool Indeterminate { get { return m_bIndeterminate; } set { m_bIndeterminate = value; RaisePropertyChanged(PropertyNames[3]); } }
        public Brush ProgressState { get { return m_brushProgressState; } set { m_brushProgressState = value; RaisePropertyChanged(PropertyNames[4]); } }

        const string ksProgress = "Progress";
        const string ksIndeterminate = "Indeterminate";
        const string ksProgressState = "ProgressState";

        readonly static string[] marrPropName = new string[] { "VolumeName", "Path", ksProgress, ksIndeterminate, ksProgressState };
        internal const int NumCols_ = 5;

        void SetProgressState(Brush brushProgressState)
        {
            m_brushProgressState = brushProgressState;
            RaisePropertyChanged(ksProgressState);
        }
        
        internal void SetIndeterminate(bool bIndeterminate)
        {
            m_bIndeterminate = bIndeterminate;
            SetProgressState(m_bIndeterminate ? Brushes.Yellow : Brushes.Blue);
            RaisePropertyChanged(ksIndeterminate);
        }

        internal void SetProgress(double d)
        {
            if (d == double.NaN)
            {
                return;
            }

            if (m_bIndeterminate)
            {
                SetIndeterminate(false);
            }

            m_nProgress = d;
            RaisePropertyChanged(ksProgress);
        }

        internal void SetCompleted()
        {
            SetProgress(1);
            SetProgressState(Brushes.LimeGreen);
        }

        internal LVitem_ProgressVM(LV_ProgressVM LV, string[] arrStr)
            : base(LV, arrStr)
        {
        }

        internal override int NumCols { get { return NumCols_; } }
        protected override string[] PropertyNames { get { return marrPropName; } }

        protected override int SearchCol { get { return 1; } }
    }
}
