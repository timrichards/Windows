
namespace DoubleFile
{
    partial class LV_FileDuplicatesVM : ListViewVM_GenericBase<LVitem_FileDuplicatesVM>
    {
        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthPath { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FileDuplicatesVM.NumCols_; } }
    }
}