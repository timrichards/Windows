using System.Windows;
using System.Linq;
using System.Collections.Generic;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinProject.xaml
    /// </summary>
    partial class WinProject : LocalWindow
    {
        internal bool Unsaved { get; set; }
        internal IEnumerable<LVitem_ProjectVM> ListLVvolStrings { get; private set; }

        public WinProject()
        {
            InitializeComponent();
        }

        internal WinProject(IEnumerable<LVitem_ProjectVM> listLVvolStrings, bool bOpenProject = false)
            : this()
        {
            ListLVvolStrings = listLVvolStrings;
            m_bOpenProject = bOpenProject;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = new LV_ProjectVM();
            var win = new WinProjectVM();

            form_lvProject.DataContext = lv;
            DataContext = win;
            win.SetPartner(lv);

            lv.SelectedOne = () => { return form_lvProject.SelectedItems.Count == 1; };
            lv.SelectedAny = () => { return form_lvProject.SelectedItems.Count > 0; };
            lv.Selected = () => { return form_lvProject.SelectedItems.Cast<LVitem_ProjectVM>(); };

            if (ListLVvolStrings != null)
            {
                foreach (var lvItem in ListLVvolStrings)
                {
                    lv.Add(lvItem);
                }
            }

            ListLVvolStrings = null;     // keep just one representation of state at a time.

            if (m_bOpenProject)
            {
                win.OpenProject(Unsaved);
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            var lvProjectVM = form_lvProject.DataContext as LV_ProjectVM;
            var list = new UList<LVitem_ProjectVM>();

            foreach (var lvItem in lvProjectVM.ItemsCast)
            {
                list.Add(lvItem);
            }

            Unsaved = lvProjectVM.Unsaved;
            ListLVvolStrings = list;
            DialogResult = true;

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
        }

        bool m_bOpenProject = false;
    }
}
