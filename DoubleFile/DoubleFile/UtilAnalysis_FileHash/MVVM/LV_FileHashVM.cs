
namespace DoubleFile
{
    partial class LV_FileHashVM : ListViewVM_GenericBase<LVitem_FileHashVM>
    {
        public string WidthNickname { get { return SCW; } }                   // franken all NaN
        public string WidthSourcePath { get { return SCW; } }
        public string WidthListingFileNoPath { get { return SCW; } }
        public string WidthStatus { get { return SCW; } }
        public string WidthIncludeYN { get { return SCW; } }
        public string WidthVolumeGroup { get { return SCW; } }
        public string WidthDriveModel { get { return SCW; } }
        public string WidthDriveSerial { get { return SCW; } }
        public string WidthScannedLength { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FileHashVM.NumCols_; } }
    }
}