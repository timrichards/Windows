namespace DoubleFile
{
    partial class LV_VolumeDetailVM : ListViewVM_Base<LVitem_VolumeDetailVM>
    {
        public string Title
        {
            get { return ("" + _Title).Replace("_", "__"); }
            private set
            {
                _Title = value;
                RaisePropertyChanged();
            }
        }
        string _Title = null;

        public string WidthHeader => SCW;       // franken all NaN
        public string WidthDetail => SCW;       // not used

        internal override int NumCols => LVitem_VolumeDetailVM.NumCols_;
    }
}