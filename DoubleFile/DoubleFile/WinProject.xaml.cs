using System.Windows;
using System.Linq;
using System.Collections.Generic;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinProject.xaml
    /// </summary>
    partial class WinProject
    {
        internal LV_ProjectVM LVprojectVM { get; private set; }

        internal WinProject(GlobalData_Base gd_in, LV_ProjectVM lvProjectVM = null)
        {
            InitializeComponent();
            _gd = gd_in;
            LVprojectVM = lvProjectVM;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var bOpenProject = (null == LVprojectVM);
            var lvProjectVM = new LV_ProjectVM(_gd, LVprojectVM);

            var win = new WinProjectVM(_gd, lvProjectVM);

            form_lvProject.DataContext = lvProjectVM;
            DataContext = win;

            lvProjectVM.SelectedOne = () => form_lvProject.SelectedItems.HasOnlyOne();
            lvProjectVM.SelectedAny = () => (false == form_lvProject.SelectedItems.IsEmptyA());
            lvProjectVM.Selected = () => form_lvProject.SelectedItems.Cast<LVitem_ProjectVM>();

            if (bOpenProject)
            {
                win.OpenProject();
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            LVprojectVM = form_lvProject.DataContext as LV_ProjectVM;
            LocalDialogResult = true;

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
            // Not when simulating modal.
            CloseIfSimulatingModal();
        }

        private void WinProject_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (LocalDialogResult ?? false)
            {
                return;
            }

            var lvProjectVM = form_lvProject.DataContext as LV_ProjectVM;
            
            if ((null != lvProjectVM) &&
                lvProjectVM.Unsaved &&
                ((null == LVprojectVM ) || (false == LVprojectVM.Equals(lvProjectVM))) &&
                (MessageBoxResult.Cancel ==
                MBoxStatic.ShowDialog(WinProjectVM.UnsavedWarning, "Close Project", MessageBoxButton.OKCancel)))
            {
                e.Cancel = true;
            }
        }

        readonly GlobalData_Base _gd = null;
    }
}
