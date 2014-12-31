using System.Windows;
using System.Windows.Input;

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

        private void form_VolumeList_Initialized(object sender, System.EventArgs e)
        {
            var lv = new VolumeListViewVM();
            var win = new VolumeListVM();
            form_lvVolumeList.DataContext = lv;
            form_VolumeList.DataContext = win;
            lv.SetPartner(win);
            win.SetPartner(lv);
        }
    }
}
