using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;

namespace DoubleFile
{
    class LV_ProgressVM : ListViewVM_Base<LVitem_ProgressVM>, IDisposable
    {
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

        internal LV_ProgressVM()
        {
            _lsDisposable.Add(Observable.Timer(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500)).Timestamp()
                .LocalSubscribe(99733, x =>
            {
                foreach (var lvItem in ItemsCast.ToArray())
                    lvItem.TimerTick();
            }));
        }

        public void Dispose() => Util.LocalDispose(_lsDisposable);

        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
