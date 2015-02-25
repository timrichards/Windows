using System;
using System.Windows.Media;
using System.Linq;

namespace DoubleFile
{
    class LVitem_ProgressVM : ListViewItemVM_Base
    {
        public string
            Nickname { get { return marr[0]; } set { SetProperty(0, value); } }
        public string
            SourcePath { get { return marr[1]; } set { SetProperty(1, value); } }
        public double
            Progress { get; set; }

        public bool
            Indeterminate
        {
            get { return _bIndeterminate; }
            set
            {
                _bIndeterminate = value;

                ProgressState = _bIndeterminate ? Brushes.Yellow : Brushes.Blue;
                RaisePropertyChanged(_ksIndeterminate);
            }
        }
        bool _bIndeterminate = true;

        public Brush
            ProgressState { get { return _brushProgressState; } set { _brushProgressState = value; RaisePropertyChanged(_ksProgressState); } }
        Brush _brushProgressState = Brushes.Yellow;
        
        public string
            Remaining { get { return marr[5]; } set { SetProperty(5, value); } }

        protected override string[]
            PropertyNames { get { return marrPropName; } }
        readonly static string[]
            marrPropName = { "Nickname", "SourcePath", _ksProgress, _ksIndeterminate, _ksProgressState, "Remaining" };

        internal override int
            NumCols { get { return NumCols_; } }
        internal const int
            NumCols_ = 6;

        protected override int SearchCol { get { return 1; } }

        internal LVitem_ProgressVM(LV_ProgressVM LV, string[] arrStr)
            : base(LV, arrStr)
        {
        }

        internal void SetCompleted()
        {
            Indeterminate = false;
            ProgressState = Brushes.LimeGreen;
            Remaining = "Completed.";
            Progress = 1;
            RaisePropertyChanged(_ksProgress);
        }

        internal void SetError(string strError)
        {
            Indeterminate = false;
            ProgressState = Brushes.Red;
            Remaining = "Error. " + strError;
            Progress = 1;
            RaisePropertyChanged(_ksProgress);
        }

        internal void TimerTick()
        {
            if (_nLastProgress.Equals(Progress))
            {
                return;
            }

            if (double.IsNaN(Progress))
            {
                return;
            }

            if (_bIndeterminate)
            {
                Indeterminate = false;
            }

            if (_dtRollingProgress == DateTime.MinValue)
            {
                _dtRollingProgress = DateTime.Now;
            }

            var tmRolling = DateTime.Now - _dtRollingProgress;

            if ((_nRollingProgress.Equals(0)) && (tmRolling > TimeSpan.FromSeconds(15)))
            {
                // The operating system caches reads so restarting the drive read sweeps
                // through the already-read data unreasonably fast.
                _nRollingProgress = Progress;

                if (_nRollingProgress.Equals(0))
                {
                    _nRollingProgress = double.Epsilon;
                }

                _dtRollingProgress = DateTime.Now;
            }
            else if (tmRolling > TimeSpan.FromMinutes(_knRollingMinutes))
            {
                var v = Math.Min(1, Progress + double.Epsilon);
                var numerator = Math.Max(0, (1 - v) * tmRolling.Ticks);
                var denominator = (v - _nRollingProgress) / _knRollingMinutes;

                if (denominator > 0)
                {
                    var nRemaining = (int) TimeSpan.FromTicks((long)(numerator / denominator))
                        .Add(TimeSpan.FromMinutes(1))
                        .TotalMinutes;

                    Remaining = "About " +
                        nRemaining.ToString("0") +
                        " Minute" +
                        (nRemaining != 1 ? "s" : "") +
                        " remaining";
                }

                _nRollingProgress = v;
                _dtRollingProgress = DateTime.Now;
            }

            RaisePropertyChanged(_ksProgress);
            _nLastProgress = Progress;
        }

        const string _ksProgress = "Progress";
        const string _ksIndeterminate = "Indeterminate";
        const string _ksProgressState = "ProgressState";
        const int _knRollingMinutes = 2;

        DateTime _dtRollingProgress = DateTime.MinValue;
        double _nRollingProgress = 0;
        double _nLastProgress = 0;
    }
}
