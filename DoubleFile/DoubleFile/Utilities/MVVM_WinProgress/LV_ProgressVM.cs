
using System;
namespace DoubleFile
{
    class LV_ProgressVM : ListViewVM_GenericBase<LVitem_ProgressVM>, IDisposable    // Timer so IDisposable
    {
        // queried by ObservableObject but not used for progress bar
        public string WidthNickname { get { return SCW; } }                         // franken all NaN
        public string WidthSourcePath { get { return SCW; } }
        public string WidthProgress { get { return SCW; } }
        public string WidthIndeterminate { get { return SCW; } }
        public string WidthProgressState { get { return SCW; } }
        public string WidthRemaining { get { return SCW; } }

        internal override bool Add(string[] arrStr, bool bQuiet = false)
        {
            Add(new LVitem_ProgressVM(this, arrStr), bQuiet);
            return true;
        }

        internal override int NumCols { get { return LVitem_ProgressVM.NumCols_; } }

        internal LV_ProgressVM()
        {
            _tmrUpdate = new LocalTimer(() =>
            {
                foreach (var lvItem in ItemsCast)
                {
                    lvItem.TimerTick();
                }
            }).Start();
        }

        public void Dispose()
        {
            _tmrUpdate.Dispose();
        }

        readonly LocalTimer _tmrUpdate = null;
    }
}
