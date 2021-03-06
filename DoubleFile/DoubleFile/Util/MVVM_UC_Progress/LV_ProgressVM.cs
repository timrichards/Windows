﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using System.Windows.Shell;

namespace DoubleFile
{
    using static LVitem_ProgressVM.ProgressStates;

    class LV_ProgressVM : ListViewVM_Base<LVitem_ProgressVM>, IDisposable
    {
        public TaskbarItemProgressState ProgressState { get; private set; } = TaskbarItemProgressState.Indeterminate;
        public double Progress { get; private set; } = 0;

        public string WidthBigLabel => SCW;             // franken all NaN
        public string WidthSmallKeyLabel => SCW;
        public string WidthStatus => SCW;

        internal override int NumCols => LVitem_ProgressVM.NumCols_;

        internal void Add(IEnumerable<Tuple<string, string>> ieStr)
        {
            base.Add(
                ieStr
                .Select(tuple => 
            {
                if (this[tuple.Item2].Any())
                    Util.Assert(99955, false);

                return new LVitem_ProgressVM(this, new[] { tuple.Item1, tuple.Item2 });
            }));
        }

        internal LV_ProgressVM Init()
        {
            _lsDisposable.Add(Observable.Timer(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500)).Timestamp()
                .LocalSubscribe(99733, x =>
            {
                double nProgress = 0;
                var nDenominator = 0;

                ProgressState = TaskbarItemProgressState.Indeterminate;

                foreach (var lvItem in ItemsCast.ToArray())
                {
                    lvItem.TimerTick();

                    if (Error == lvItem.ProgressState)
                    {
                        ProgressState = TaskbarItemProgressState.Error;
                    }
                    else if (new[] { Normal, Completed }.Contains(lvItem.ProgressState))
                    {
                        if (TaskbarItemProgressState.Indeterminate == ProgressState)
                            ProgressState = TaskbarItemProgressState.Normal;

                        nProgress += lvItem.Progress;
                    }

                    ++nDenominator;
                }

                Progress = nProgress / nDenominator;
                RaisePropertyChanged("ProgressState");
                RaisePropertyChanged("Progress");
            }));

            return this;
        }

        public void Dispose() => Util.LocalDispose(_lsDisposable);

        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
