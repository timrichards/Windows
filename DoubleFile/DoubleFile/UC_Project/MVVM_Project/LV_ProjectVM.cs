using System.Windows;

namespace DoubleFile
{
    partial class LV_ProjectVM : ListViewVM_Base<LVitem_ProjectVM>
    {
        public Visibility Visible
        {
            get
            {
                bool bIsEmpty = 0 == Items.Count;

                MainWindow.WithMainWindowA(mainWindow =>
                    mainWindow.ShowLinks(bIsEmpty));

                return bIsEmpty ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public string WidthNickname => SCW;                     // franken all NaN
        public string WidthSourcePath => SCW;
        public string WidthListingFileNoPath => SCW;
        public string WidthStatus => SCW;
        public string WidthIncludeYN => SCW;
        public string WidthVolumeGroup => SCW;
        public string WidthDriveModel => SCW;
        public string WidthDriveSerial => SCW;
        public string WidthScannedLength => SCW;

        public string WidthListingFile => SCW;                  // HACK
        public LVitem_ProjectVM SelectedItem { get; set; }      // unused

        internal override int NumCols => LVitem_ProjectVM.NumCols_;
    }
}