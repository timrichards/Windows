
namespace DoubleFile
{
    partial class LV_FileHash_FilesVM : ListViewVM_GenericBase<LVitem_FileHash_FilesVM>
    {
        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthDuplicates { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FileHash_FilesVM.NumCols_; } }
    }
}