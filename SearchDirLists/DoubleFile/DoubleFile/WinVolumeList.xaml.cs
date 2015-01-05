using System.Windows;
using System.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeList.xaml
    /// </summary>
    partial class WinVolumeList : Window
    {
        public WinVolumeList()
        {
            InitializeComponent();

            gd = GlobalData.GetInstance(this);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = new LV_VolumeVM();
            var win = new WinVolumeListVM();

            form_lvVolumeList.DataContext = lv;
            form_WinVolumeList.DataContext = win;
            lv.SetPartner(win);
            win.SetPartner(lv);
            m_lvVM = lv;

            lv.SelectedOne = () => { return form_lvVolumeList.SelectedItems.Count == 1; };
            lv.SelectedAny = () => { return form_lvVolumeList.SelectedItems.Count > 0; };
            lv.Refresh = () => { }; // form_lvVolumeList.Items.Refresh(); });
            lv.Selected = () => { return form_lvVolumeList.SelectedItems.Cast<LVitem_VolumeVM>(); };

            lv.GetWindow = () => { return this; };
            win.GetWindow = () => { return this; };

            new WinSaveInProgress().Show(this);
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            var list_lvVolStrings = new UList<LVitem_VolumeVM>();
            var win = form_WinVolumeList.DataContext as WinVolumeListVM;

            foreach (var lvItem in (form_lvVolumeList.DataContext as LV_VolumeVM).ItemsCast)
            {
                list_lvVolStrings.Add(lvItem);
            }

            (new SaveDirListings(list_lvVolStrings, SaveDirListingsStatusCallback, SaveDirListingsDoneCallback)).DoThreadFactory();

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
        }

        LV_VolumeVM m_lvVM = null;
        readonly GlobalData gd = null;
        readonly MainWindow m_app = null;
    }
}
