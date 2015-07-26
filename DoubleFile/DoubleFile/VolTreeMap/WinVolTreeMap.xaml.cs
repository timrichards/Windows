using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Windows;
using System.Windows.Forms;

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
            var dlg = new FolderBrowserDialog { Description = "Select the path to browse." };

            if (DialogResult.OK !=
                ModalThread.Go(darkWindow => dlg.ShowDialog(((NativeWindow)(Window)darkWindow))))
            {
                Dispatcher.InvokeShutdown();
                return;
            }

            Statics.LVprojectVM = new LV_ProjectVM { new LVitem_ProjectVM { SourcePath = dlg.SelectedPath } };
            SaveDirListings.Hash = false;
            SaveListingsProcess.Go(Statics.LVprojectVM);
            LocalTV.FactoryCreate(Statics.LVprojectVM);
            new BBCodeBlock().LinkNavigator.Navigate(new Uri("/DoubleFile;component/Win/WinTreeMap.xaml", UriKind.Relative), this);
        }
    }
}
