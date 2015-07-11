namespace DoubleFile
{
    partial class LV_ProjectVM : ListViewVM_Base<LVitem_ProjectVM>
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

        public string WidthListingFile { get { return SCW; } }                  // HACK
        public LVitem_ProjectVM SelectedItem { get; set; }                      // unused

        internal override int NumCols { get { return LVitem_ProjectVM.NumCols_; } }
    }
}