using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;

namespace DoubleFile
{
    class LV_ProgressVM : ListViewVM_Base<LVitem_ProgressVM>
    {
        // queried by ObservableObject but not used for progress bar
        public string WidthBigLabel { get { return SCW; } }                         // franken all NaN
        public string WidthSmallKeyLabel { get { return SCW; } }
        public string WidthProgress { get { return SCW; } }
        public string WidthIndeterminate { get { return SCW; } }
        public string WidthProgressState { get { return SCW; } }
        public string WidthRemaining { get { return SCW; } }

        internal override int NumCols { get { return LVitem_ProgressVM.NumCols_; } }

        internal void Add(IEnumerable<Tuple<string, string>> ieStr)
        {
            base.Add(
                ieStr
                .Select(tuple => 
            {
                if (this[tuple.Item2].LocalAny())
                    MBoxStatic.Assert(99955, false);

                return new LVitem_ProgressVM(this, new[] { tuple.Item1, tuple.Item2 });
            }));
        }

        internal LV_ProgressVM()
        {
            Observable.Timer(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500)).Timestamp()
                .Subscribe(x =>
            {
                foreach (var lvItem in ItemsCast.ToArray())
                    lvItem.TimerTick();
            });
        }
    }
}
