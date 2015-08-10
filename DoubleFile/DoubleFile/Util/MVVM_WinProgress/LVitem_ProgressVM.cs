using System;
using System.Windows.Media;
using System.Collections.Generic;

namespace DoubleFile
{
    using static Brushes;

    class LVitem_ProgressVM : ListViewItemVM_Base
    {
        public string
            BigLabel { get { return SubItems[0]; } private set { SetProperty(0, value); } }
        public string
            SmallKeyLabel { get { return SubItems[1]; } private set { SetProperty(1, value); } }
        public string
            Status { get { return SubItems[2]; } private set { SetProperty(2, value); } }

        enum
            ProgressStates { Indeterminate, Determinate, Completed, Error };    // order is int cast as array indices
        ProgressStates _progressState = ProgressStates.Indeterminate;
        internal bool
            IsCompleted => ProgressStates.Completed == _progressState;

        public double
            Progress
        {
            get { return new double[] { 0, _nProgress, 1, 1 }[(int)_progressState]; }
            internal set { _nProgress = value; }    // RaisePropertyChanged is in OnTimerTick()
        }
        double _nProgress = 0;

        public bool
            Indeterminate => ProgressStates.Indeterminate == _progressState;
        public Brush
            Foreground => new Brush[] { Navy, Navy, LimeGreen, Red }[(int)_progressState];

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 3;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        protected override int SearchCol => 1;

        internal LVitem_ProgressVM(LV_ProgressVM LV, string[] arrStr)
            : base(LV, arrStr)
        {
        }

        void Progress_RaisePropertyChanged()
        {
            RaisePropertyChanged("Progress");
            RaisePropertyChanged("Indeterminate");
            RaisePropertyChanged("Foreground");
        }

        internal void SetCompleted()
        {
            _progressState = ProgressStates.Completed;
            Status = "Completed.";
            Progress_RaisePropertyChanged();
        }

        internal void SetError(string strError)
        {
            _progressState = ProgressStates.Error;
            Status = "Error. " + strError;
            Progress_RaisePropertyChanged();
        }

        internal void TimerTick()
        {
            var nProgress = _nProgress;     // do not use the Progress property in TimerTick()

            if (_nLastProgress.Equals(nProgress))
                return;

            if (double.IsNaN(nProgress))
                return;

            if (ProgressStates.Indeterminate == _progressState)
                _progressState = ProgressStates.Determinate;

            if (_dtRollingProgress == DateTime.MinValue)
                _dtRollingProgress = DateTime.Now;

            var tmRolling = DateTime.Now - _dtRollingProgress;

            if (_nRollingProgress.Equals(0) &&
                (tmRolling > TimeSpan.FromSeconds(15)))
            {
                // The operating system caches reads so restarting the drive read sweeps
                // through the already-read data unreasonably fast.
                _nRollingProgress = nProgress;

                if (_nRollingProgress.Equals(0))
                    _nRollingProgress = double.Epsilon;

                _dtRollingProgress = DateTime.Now;
            }
            else if (tmRolling > TimeSpan.FromMinutes(_knRollingMinutes))
            {
                var v = Math.Min(1, nProgress + double.Epsilon);
                var numerator = Math.Max(0, (1 - v) * tmRolling.Ticks);
                var denominator = (v - _nRollingProgress) / _knRollingMinutes;

                if (0 < denominator)
                {
                    var nRemaining = (int) TimeSpan.FromTicks((long)(numerator / denominator))
                        .Add(TimeSpan.FromMinutes(1))
                        .TotalMinutes;

                    Status = "About " +
                        nRemaining.ToString("0") +
                        " Minute" +
                        (nRemaining != 1 ? "s" : "") +
                        " remaining";
                }

                _nRollingProgress = v;
                _dtRollingProgress = DateTime.Now;
            }

            Progress_RaisePropertyChanged();
            _nLastProgress = nProgress;
        }

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
