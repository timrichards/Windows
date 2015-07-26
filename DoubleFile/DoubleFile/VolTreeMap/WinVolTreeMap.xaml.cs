using FirstFloor.ModernUI.Windows.Controls;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTreeMap.xaml
    /// </summary>
    public partial class WinVolTreeMap
    {
        public WinVolTreeMap()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            string strSourcePath = null;

            new UC_VolumeEditVM
            {
                FromSourcePathDlg = str => strSourcePath = str,

                DriveModel_CurrentText = () => null,
                DriveSerial_CurrentText = () => null,
                FromListingFileDlg = str => { },
                FromProbe = (x, y) => { },
                ListingFile_CurrentText = () => null,
                SourcePath_CurrentText = () => null,
            }
                .EditSourcePath();

            if (null == strSourcePath)
            {
                Dispatcher.InvokeShutdown();
                return;
            }

            Statics.LVprojectVM = new LV_ProjectVM { new LVitem_ProjectVM { SourcePath = strSourcePath } };
            SaveDirListings.Hash = false;
            SaveListingsProcess.Go(Statics.LVprojectVM);
            LocalTV.FactoryCreate(Statics.LVprojectVM);
            new BBCodeBlock().LinkNavigator.Navigate(new Uri("/DoubleFile;component/Win/WinTreeMap.xaml", UriKind.Relative), this);
        }
    }
}
