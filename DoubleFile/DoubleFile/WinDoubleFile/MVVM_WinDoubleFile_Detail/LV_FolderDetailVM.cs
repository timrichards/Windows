namespace DoubleFile
{
    partial class LV_FolderDetailVM : ListViewVM_GenericBase<LVitem_FolderDetailVM>
    {
        public string Title
        {
            get { return (_Title ?? "").Replace("_", "__"); }
            internal set
            {
                _Title = value;
                RaisePropertyChanged("Title");
            }
        }
        string _Title = null;

        public string WidthHeader { get { return SCW; } }                   // franken all NaN
        public string WidthDetail { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FolderDetailVM.NumCols_; } }
    }
}