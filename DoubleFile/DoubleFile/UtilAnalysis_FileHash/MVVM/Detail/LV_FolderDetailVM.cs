
namespace DoubleFile
{
    partial class LV_FolderDetailVM : ListViewVM_GenericBase<LVitem_FolderDetailVM>
    {
        public string WidthHeader { get { return SCW; } }                   // franken all NaN
        public string WidthDetail { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FolderDetailVM.NumCols_; } }
    }
}