using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTreemap.xaml
    /// </summary>
    public partial class UC_VolTreemap
    {
        public UC_VolTreemap()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            var dlg = new FolderBrowserDialog
            {
                Description = "Select the path to browse.",
                ShowNewFolderButton = false
            };

            if (false == (ModalThread.Go(darkWindow => dlg.ShowDialog((Window)darkWindow))))
            {
                Dispatcher.InvokeShutdown();
                return;
            }

            var lvProjectVM = new LV_ProjectVM { new LVitem_ProjectVM { SourcePath = dlg.SelectedPath } };

            SaveDirListings.Hash = false;
            SaveDirListings.Go(lvProjectVM);

            if (0 == lvProjectVM.CanLoadCount)
            {
                Dispatcher.InvokeShutdown();
                return;
            }

            LocalTV.WithLocalTV(localTV => localTV.LocalDispose());     // use-case: back button/backspace key
            LocalTV.FactoryCreate(lvProjectVM);

            if (null == LocalTV.TopNode)
            {
                Dispatcher.InvokeShutdown();
                return;
            }

            Statics.LVprojectVM = lvProjectVM;
            new BBCodeBlock().LinkNavigator.Navigate(new Uri("/DoubleFile;component/UserControls/UC_Treemap.xaml", UriKind.Relative), this);
            LVitem_FilesVM.ShowDuplicatesA = false;
        }
    }
}
