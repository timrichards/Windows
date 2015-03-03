
namespace DoubleFile
{
    partial class LV_FileDetailVM : ListViewVM_GenericBase<LVitem_FileDetailVM>
    {
        public string WidthHeader { get { return SCW; } }                   // franken all NaN
        public string WidthDetail { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FileDetailVM.NumCols_; } }
    }
}