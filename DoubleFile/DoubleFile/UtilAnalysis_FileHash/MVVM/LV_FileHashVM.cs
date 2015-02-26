
namespace DoubleFile
{
    partial class LV_FileHashVM : ListViewVM_GenericBase<LVitem_FileHashVM>
    {
        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthDuplicates { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FileHashVM.NumCols_; } }
    }
}