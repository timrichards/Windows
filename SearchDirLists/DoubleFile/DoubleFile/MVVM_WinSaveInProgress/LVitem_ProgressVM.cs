
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

        DateTime m_dtStart = DateTime.MinValue;
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

                if (m_dtStart == DateTime.MinValue)
                {
                    m_dtStart = DateTime.Now;
                    m_dtRollingProgress = m_dtStart;
                }

                var tmSpan = DateTime.Now - m_dtStart;

                if (tmSpan > TimeSpan.FromMinutes(2))
                {
                    var tmRolling = DateTime.Now - m_dtRollingProgress;

                    if (tmRolling > TimeSpan.FromMinutes(nRollingMinutes))
                    {
                        var tmRemaining = TimeSpan.FromTicks((long)((1 - value) * tmRolling.Ticks /
                            (value - m_nRollingProgress) /
                            nRollingMinutes));

                        Remaining = tmRemaining.TotalMinutes.ToString("0") + " minutes remaining";

                        m_nRollingProgress = value;
                        m_dtRollingProgress = DateTime.Now;
                    }
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
