
namespace DoubleFile
{
    partial class LV_VolumeDetailVM : ListViewVM_GenericBase<LVitem_VolumeDetailVM>
    {
        public string WidthHeader { get { return SCW; } }                   // franken all NaN
        public string WidthDetail { get { return SCW; } }

        internal override int NumCols { get { return LVitem_VolumeDetailVM.NumCols_; } }
    }
}