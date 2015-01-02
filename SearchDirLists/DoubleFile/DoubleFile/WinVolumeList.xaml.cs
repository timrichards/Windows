using System.Windows;
using System.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeList.xaml
    /// </summary>
    public partial class WinVolumeList : Window
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

            lv.SelectedOne = new ListViewVM.BoolQuery(() => { return form_lvVolumeList.SelectedItems.Count == 1; });
            lv.SelectedAny = new ListViewVM.BoolQuery(() => { return form_lvVolumeList.SelectedItems.Count > 0; });
            lv.Refresh = new System.Action(() => {});// form_lvVolumeList.Items.Refresh(); });
            lv.Selected = new LV_VolumeVM.IEnumerableQuery(() => { return form_lvVolumeList.SelectedItems.Cast<LVitem_VolumeVM>(); });
        }
    }
}
