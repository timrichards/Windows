using System.Windows;
using System.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinProject.xaml
    /// </summary>
    partial class WinProject : LocalWindow
    {
        bool m_bOpenProject = false;

        public WinProject()
        {
            InitializeComponent();
        }

        internal WinProject(UList<LVitem_ProjectVM> list_lvVolStrings, bool bOpenProject = false)
            : this()
        {
            m_list_lvVolStrings = list_lvVolStrings;
            m_bOpenProject = bOpenProject;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = new LV_ProjectVM();
            var win = new WinProjectVM();

            form_lvProject.DataContext = lv;
            DataContext = win;
            lv.SetPartner(win);
            win.SetPartner(lv);

            lv.SelectedOne = () => { return form_lvProject.SelectedItems.Count == 1; };
            lv.SelectedAny = () => { return form_lvProject.SelectedItems.Count > 0; };
            lv.Selected = () => { return form_lvProject.SelectedItems.Cast<LVitem_ProjectVM>(); };

            if (m_list_lvVolStrings != null)
            {
                foreach (var lvItem in m_list_lvVolStrings)
                {
                    lv.Add(lvItem);
                }
            }

            m_list_lvVolStrings = null;     // close box / cancel

            if (m_bOpenProject)
            {
                win.OpenProject();
            }
        }

        internal UList<LVitem_ProjectVM> m_list_lvVolStrings = null;

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            m_list_lvVolStrings = new UList<LVitem_ProjectVM>();

            foreach (var lvItem in (form_lvProject.DataContext as LV_ProjectVM).ItemsCast)
            {
                m_list_lvVolStrings.Add(lvItem);
            }

            DialogResult = true;

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
        }
    }
}
