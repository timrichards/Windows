
namespace DoubleFile
{
    class LV_ProgressVM : ListViewVM_Generic<LVitem_ProgressVM>
    {
        // queried by ObservableObject but not used for progress bar
        public string WidthVolumeName { get { return SCW; } }                   // franken all NaN
        public string WidthPath { get { return SCW; } }
        public string WidthProgress { get { return SCW; } }
        public string WidthIndeterminate { get { return SCW; } }
        public string WidthProgressState { get { return SCW; } }

        internal override void NewItem(string[] arrStr) { Add(new LVitem_ProgressVM(this, arrStr)); }
        internal override int NumCols { get { return LVitem_ProgressVM.NumCols_; } }
    }
}
