
namespace DoubleFile
{
    class LV_TreeListVM : ListViewVM_GenericBase<LVitem_TreeListVM>
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

        public string WidthFolder { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_TreeListVM.NumCols_; } }
    }
}
