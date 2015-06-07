using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace DoubleFile
{
    class LV_ProgressVM : ListViewVM_Base<LVitem_ProgressVM>
    {
        // queried by ObservableObject but not used for progress bar
        public string WidthNickname { get { return SCW; } }                         // franken all NaN
        public string WidthSourcePath { get { return SCW; } }
        public string WidthProgress { get { return SCW; } }
        public string WidthIndeterminate { get { return SCW; } }
        public string WidthProgressState { get { return SCW; } }
        public string WidthRemaining { get { return SCW; } }

        internal override bool Add(IEnumerable<string[]> ieStr, bool bQuiet)
        {
            MBoxStatic.Assert(99945, false);
            return false;
        }

        internal bool Add(IEnumerable<Tuple<string, string>> ieStr)
        {
            foreach (var tuple in ieStr)
            {
                if (false == this[tuple.Item2].IsEmpty())
                {
                    MBoxStatic.Assert(99955, false);
                    return false;
                }

                base.Add(new LVitem_ProgressVM(this, new[] { tuple.Item1, tuple.Item2 }), bQuiet: true);
            }

            return true;
        }

        internal override int NumCols { get { return LVitem_ProgressVM.NumCols_; } }

        internal LV_ProgressVM()
        {
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .Subscribe(x =>
            {
                foreach (var lvItem in ItemsCast)
                    lvItem.TimerTick();
            });
        }
    }
}
