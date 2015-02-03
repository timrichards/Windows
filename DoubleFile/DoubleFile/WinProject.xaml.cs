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
        internal LV_ProjectVM LVprojectVM { get; private set; }

        internal WinProject(GlobalData_Base gd_in, LV_ProjectVM lvProjectVM = null)
        {
            InitializeComponent();
            gd = gd_in;
            LVprojectVM = lvProjectVM;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var bOpenProject = (null == LVprojectVM);
            var lvProjectVM = new LV_ProjectVM(gd, LVprojectVM);

            LVprojectVM = null;         // only one representation of state at a time

            var win = new WinProjectVM(gd);

            form_lvProject.DataContext = lvProjectVM;
            DataContext = win;
            win.SetPartner(lvProjectVM);

            lvProjectVM.SelectedOne = () => { return form_lvProject.SelectedItems.Count == 1; };
            lvProjectVM.SelectedAny = () => { return form_lvProject.SelectedItems.Count > 0; };
            lvProjectVM.Selected = () => { return form_lvProject.SelectedItems.Cast<LVitem_ProjectVM>(); };

            if (bOpenProject)
            {
                win.OpenProject();
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            LVprojectVM = form_lvProject.DataContext as LV_ProjectVM;
            DialogResult = true;

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
        }

        private void WinProject_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult ?? false)
            {
                return;
            }

            var lvProjectVM = form_lvProject.DataContext as LV_ProjectVM;
            
            if ((lvProjectVM != null) && lvProjectVM.Unsaved
                && (MessageBoxResult.Cancel ==
                MBox.ShowDialog(WinProjectVM.ksUnsavedWarning, "Close Project", MessageBoxButton.OKCancel)))
            {
                e.Cancel = true;
                return;
            }
        }

        GlobalData_Base gd = null;
    }
}
