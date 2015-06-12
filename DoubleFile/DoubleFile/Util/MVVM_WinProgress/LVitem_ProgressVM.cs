using System;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_ProgressVM : ListViewItemVM_Base
    {
        public string
            Nickname { get { return marr[0]; } private set { SetProperty(0, value); } }
        public string
            SourcePath { get { return marr[1]; } private set { SetProperty(1, value); } }
        public double
            Progress { get; internal set; }

        public bool
            Indeterminate
        {
            get { return _bIndeterminate; }
            internal set
            {
                _bIndeterminate = value;

                ProgressState = _bIndeterminate ? Brushes.Navy : Brushes.Navy;
                RaisePropertyChanged();
            }
        }
        bool _bIndeterminate = true;

        public Brush
            ProgressState
        {
            get { return _brushProgressState; }
            private set { _brushProgressState = value; RaisePropertyChanged(); }
        }
        Brush _brushProgressState = Brushes.Navy;
        
        public string
            Remaining { get { return marr[5]; } private set { SetProperty(5, value); } }

        internal override int
            NumCols { get { return NumCols_; } }
        internal const int
            NumCols_ = 6;

        protected override IEnumerable<string> PropNames { get { return _propNames; } }
        static IEnumerable<string> _propNames = GetProps(typeof(LVitem_ProgressVM));

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
                return;

            if (double.IsNaN(Progress))
                return;

            if (_bIndeterminate)
                Indeterminate = false;

            if (_dtRollingProgress == DateTime.MinValue)
                _dtRollingProgress = DateTime.Now;

            var tmRolling = DateTime.Now - _dtRollingProgress;

            if (_nRollingProgress.Equals(0) &&
                (tmRolling > TimeSpan.FromSeconds(15)))
            {
                // The operating system caches reads so restarting the drive read sweeps
                // through the already-read data unreasonably fast.
                _nRollingProgress = Progress;

                if (_nRollingProgress.Equals(0))
                    _nRollingProgress = double.Epsilon;

                _dtRollingProgress = DateTime.Now;
            }
            else if (tmRolling > TimeSpan.FromMinutes(_knRollingMinutes))
            {
                var v = Math.Min(1, Progress + double.Epsilon);
                var numerator = Math.Max(0, (1 - v) * tmRolling.Ticks);
                var denominator = (v - _nRollingProgress) / _knRollingMinutes;

                if (0 < denominator)
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

        const string
            _ksProgress = "Progress";
        const int
            _knRollingMinutes = 2;

        DateTime
            _dtRollingProgress = DateTime.MinValue;
        double
            _nRollingProgress = 0;
        double
            _nLastProgress = 0;
    }
}
