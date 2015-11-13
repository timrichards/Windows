using System;
using System.Windows.Media;
using System.Collections.Generic;

namespace DoubleFile
{
    using System.Linq;
    using static Brushes;

    class LVitem_ProgressVM : ListViewItemVM_Base
    {
        public string
            BigLabel { get { return SubItems[0]; } private set { SetProperty(0, value); } }
        public string
            SmallKeyLabel { get { return SubItems[1]; } private set { SetProperty(1, value); } }
        public string
            Status { get { return SubItems[2]; } private set { SetProperty(2, value); } }

        internal enum
            ProgressStates { Indeterminate, Normal, Completed, Error };    // order is int cast as array indices
        internal ProgressStates ProgressState = ProgressStates.Indeterminate;
        internal bool
            IsRunning => new[] { ProgressStates.Indeterminate, ProgressStates.Normal }.Contains(ProgressState);

        public double
            Progress
        {
            get { return new double[] { 0, _nProgress, 1, 1 }[(int)ProgressState]; }
            internal set { _nProgress = value; }    // RaisePropertyChanged is in OnTimerTick()
        }
        double _nProgress = 0;

        public bool
            Indeterminate => ProgressStates.Indeterminate == ProgressState;
        public Brush
            Foreground => new Brush[] { Navy, Navy, LimeGreen, Red }[(int)ProgressState];

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

        internal LVitem_ProgressVM
            SetCompleted()
        {
            ProgressState = ProgressStates.Completed;
            Status = "Completed.";
            Progress_RaisePropertyChanged();
            return this;
        }

        internal LVitem_ProgressVM
            SetError(string strError)
        {
            ProgressState = ProgressStates.Error;
            Status = strError;
            Progress_RaisePropertyChanged();
            return this;
        }

        internal LVitem_ProgressVM
            ResetEstimate()
        {
            _dtRollingProgress = DateTime.MinValue;
            _nRollingProgress = 0;
            _nLastProgress = 0;
            Status = null;
            return this;
        }

        internal LVitem_ProgressVM
            TimerTick()
        {
            var nProgress = _nProgress;     // do not use the Progress property in TimerTick()

            if (_nLastProgress.Equals(nProgress))
                return this;

            if (double.IsNaN(nProgress))
                return this;

            if (ProgressStates.Indeterminate == ProgressState)
                ProgressState = ProgressStates.Normal;

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
                    var nRemaining = (int)TimeSpan.FromTicks((long)(numerator / denominator))
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
            return this;
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
