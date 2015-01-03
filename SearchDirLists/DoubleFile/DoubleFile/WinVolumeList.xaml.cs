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
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = new LV_VolumeVM();
            var win = new WinVolumeListVM();

            form_lvVolumeList.DataContext = lv;
            form_VolumeList.DataContext = win;
            lv.SetPartner(win);
            win.SetPartner(lv);

            lv.SelectedOne = () => { return form_lvVolumeList.SelectedItems.Count == 1; };
            lv.SelectedAny = () => { return form_lvVolumeList.SelectedItems.Count > 0; };
            lv.Refresh = () => {};// form_lvVolumeList.Items.Refresh(); });
            lv.Selected = () => { return form_lvVolumeList.SelectedItems.Cast<LVitem_VolumeVM>(); };

            lv.GetWindow = () => { return this; };
            win.GetWindow = () => { return this; };
        }
    }
}
