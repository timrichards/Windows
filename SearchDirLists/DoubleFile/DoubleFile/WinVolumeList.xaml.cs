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

        internal WinVolumeList(UList<LVitem_VolumeVM> list_lvVolStrings) : this()
        {
            m_list_lvVolStrings = list_lvVolStrings;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = new LV_VolumeVM();
            var win = new WinVolumeListVM();

            form_lvVolumeList.DataContext = lv;
            form_WinVolumeList.DataContext = win;
            lv.SetPartner(win);
            win.SetPartner(lv);

            lv.SelectedOne = () => { return form_lvVolumeList.SelectedItems.Count == 1; };
            lv.SelectedAny = () => { return form_lvVolumeList.SelectedItems.Count > 0; };
            lv.Refresh = () => { }; // form_lvVolumeList.Items.Refresh(); });
            lv.Selected = () => { return form_lvVolumeList.SelectedItems.Cast<LVitem_VolumeVM>(); };

            lv.GetWindow = () => { return this; };
            win.GetWindow = () => { return this; };

            if (m_list_lvVolStrings != null)
            {
                foreach (var lvItem in m_list_lvVolStrings)
                {
                    lv.Add(lvItem);
                }
            }
        }

        internal UList<LVitem_VolumeVM> m_list_lvVolStrings = null;

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            m_list_lvVolStrings = new UList<LVitem_VolumeVM>();

            foreach (var lvItem in (form_lvVolumeList.DataContext as LV_VolumeVM).ItemsCast)
            {
                m_list_lvVolStrings.Add(lvItem);
            }

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
        }
    }
}
