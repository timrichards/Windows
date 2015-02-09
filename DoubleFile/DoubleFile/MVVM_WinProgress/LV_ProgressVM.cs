
using System;
namespace DoubleFile
{
    class LV_ProgressVM : ListViewVM_GenericBase<LVitem_ProgressVM>
    {
        // queried by ObservableObject but not used for progress bar
        public string WidthNickname { get { return SCW; } }                   // franken all NaN
        public string WidthSourcePath { get { return SCW; } }
        public string WidthProgress { get { return SCW; } }
        public string WidthIndeterminate { get { return SCW; } }
        public string WidthProgressState { get { return SCW; } }
        public string WidthRemaining { get { return SCW; } }

        internal override bool NewItem(string[] arrStr, bool bQuiet = false) { Add(new LVitem_ProgressVM(this, arrStr), bQuiet); return true; }
        internal override int NumCols { get { return LVitem_ProgressVM.NumCols_; } }

        internal LV_ProgressVM()
        {
            m_tmrUpdate = new SDL_Timer(() =>
            {
                foreach (var lvItem in ItemsCast)
                {
                    lvItem.TimerTick();
                }
            }).Start();
        }

        readonly SDL_Timer m_tmrUpdate = null;
    }
}
