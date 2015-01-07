
using System;
using System.Windows.Media;
namespace DoubleFile
{
    class LVitem_ProgressVM : ListViewItemVM
    {
        public string VolumeName { get { return marr[0]; } set { SetProperty(0, value); } }
        public string Path { get { return marr[1]; } set { SetProperty(1, value); } }

        const string ksProgress = "Progress";
        const string ksIndeterminate = "Indeterminate";
        const string ksProgressState = "ProgressState";

        double m_nProgress = 0;
        DateTime m_dtRollingProgress = DateTime.MinValue;
        double m_nRollingProgress = 0;
        const int nRollingMinutes = 2;
        public double Progress
        {
            get { return m_nProgress; }
            set
            {
                if (value == double.NaN)
                {
                    return;
                }

                if (m_bIndeterminate)
                {
                    Indeterminate = false;
                }

                if (m_dtRollingProgress == DateTime.MinValue)
                {
                    m_dtRollingProgress = DateTime.Now;
                }

                var tmRolling = DateTime.Now - m_dtRollingProgress;

                if ((m_nRollingProgress == 0) && (tmRolling > TimeSpan.FromSeconds(15)))
                {
                    // The operating system caches reads so restarting the drive read sweeps
                    // through the already-read data unreasonably fast.
                    m_nRollingProgress = value;

                    if (m_nRollingProgress == 0)
                    {
                        m_nRollingProgress = double.Epsilon;    // just in case
                    }

                    m_dtRollingProgress = DateTime.Now;
                }
                else if (tmRolling > TimeSpan.FromMinutes(nRollingMinutes))
                {
                    Remaining = TimeSpan.FromTicks((long)((1 - value) * tmRolling.Ticks /
                        (value - m_nRollingProgress) /
                        nRollingMinutes))
                        .Add(TimeSpan.FromMinutes(1))           // fudge factor 1
                        .TotalMinutes.ToString("0") + " minutes remaining";
                    m_nRollingProgress = value;
                    m_dtRollingProgress = DateTime.Now;
                }

                m_nProgress = value;
                RaisePropertyChanged(ksProgress);
            }
        }

        bool m_bIndeterminate = true;
        Brush m_brushProgressState = Brushes.Yellow;
        public bool Indeterminate
        {
            get { return m_bIndeterminate; }
            set
            {
                m_bIndeterminate = value;
                ProgressState = m_bIndeterminate ? Brushes.Yellow : Brushes.Blue;
                RaisePropertyChanged(ksIndeterminate);
            }
        }

        public Brush ProgressState { get { return m_brushProgressState; } set { m_brushProgressState = value; RaisePropertyChanged(ksProgressState); } }
        public string Remaining { get { return marr[5]; } set { SetProperty(5, value); } }

        readonly static string[] marrPropName = new string[] { "VolumeName", "Path", ksProgress, ksIndeterminate, ksProgressState, "Remaining" };
        internal const int NumCols_ = 6;

        internal void SetCompleted()
        {
            Progress = 1;
            ProgressState = Brushes.LimeGreen;
            Remaining = "Completed.";
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
