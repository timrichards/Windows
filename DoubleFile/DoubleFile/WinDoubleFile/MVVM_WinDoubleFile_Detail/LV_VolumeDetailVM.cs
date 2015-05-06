namespace DoubleFile
{
    partial class LV_VolumeDetailVM : ListViewVM_Base<LVitem_VolumeDetailVM>
    {
        public string Title
        {
            get { return (_Title ?? "").Replace("_", "__"); }
            private set
            {
                _Title = value;
                RaisePropertyChanged("Title");
            }
        }
        string _Title = null;

        public string WidthHeader { get { return SCW; } }                   // franken all NaN
        public string WidthDetail { get { return SCW; } }   // not used

        internal override int NumCols { get { return LVitem_VolumeDetailVM.NumCols_; } }
    }
}