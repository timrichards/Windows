
namespace DoubleFile
{
    class LV_ProgressVM : ListViewVM_GenericBase<LVitem_ProgressVM>
    {
        // queried by ObservableObject but not used for progress bar
        public string WidthVolumeName { get { return SCW; } }                   // franken all NaN
        public string WidthPath { get { return SCW; } }
        public string WidthProgress { get { return SCW; } }
        public string WidthIndeterminate { get { return SCW; } }
        public string WidthProgressState { get { return SCW; } }
        public string WidthRemaining { get { return SCW; } }

        internal override bool NewItem(string[] arrStr, bool bQuiet = false) { Add(new LVitem_ProgressVM(this, arrStr), bQuiet); return true; }
        internal override int NumCols { get { return LVitem_ProgressVM.NumCols_; } }
    }
}
