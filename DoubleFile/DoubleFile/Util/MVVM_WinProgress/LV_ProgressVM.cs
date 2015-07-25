using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;

namespace DoubleFile
{
    class LV_ProgressVM : ListViewVM_Base<LVitem_ProgressVM>, IDisposable
    {
        // queried by ObservableObject but not used for progress bar
        public string WidthBigLabel => SCW;                        // franken all NaN
        public string WidthSmallKeyLabel => SCW;
        public string WidthProgress => SCW;
        public string WidthIndeterminate => SCW;
        public string WidthProgressState => SCW;
        public string WidthRemaining => SCW;

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

        internal LV_ProgressVM()
        {
            _lsDisposables.Add(Observable.Timer(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500)).Timestamp()
                .LocalSubscribe(x =>
            {
                foreach (var lvItem in ItemsCast.ToArray())
                    lvItem.TimerTick();
            }));
        }

        public void Dispose()
        {
            Util.LocalDispose(_lsDisposables);
        }

        List<IDisposable>
            _lsDisposables = new List<IDisposable>();
    }
}
