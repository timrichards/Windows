using System.Windows;
using System.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for VolumeList.xaml
    /// </summary>
    public partial class VolumeList : Window
    {
        public VolumeList()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = new VolumeListViewVM();
            var win = new VolumeListVM();

            form_lvVolumeList.DataContext = lv;
            form_VolumeList.DataContext = win;
            lv.SetPartner(win);
            win.SetPartner(lv);

            lv.SelectedOne = new ListViewVM.BoolQuery(() => { return form_lvVolumeList.SelectedItems.Count == 1; });
            lv.SelectedAny = new ListViewVM.BoolQuery(() => { return form_lvVolumeList.SelectedItems.Count > 0; });
            lv.Refresh = new System.Action(() => {});// form_lvVolumeList.Items.Refresh(); });
            lv.Selected = new VolumeListViewVM.IEnumerableQuery(() => { return form_lvVolumeList.SelectedItems.Cast<VolumeLVitemVM>(); });
        }
    }
}
