using System.Windows;
using System.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinProject.xaml
    /// </summary>
    partial class WinProject
    {
        internal LV_ProjectVM LVprojectVM { get; private set; }

        internal WinProject(LV_ProjectVM lvProjectVM = null, bool bOpenProject = false)
        {
            _bOpenProject = bOpenProject;
            LVprojectVM = lvProjectVM;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            form_btnOK.Click += BtnOK_Click;
            Closing += WinProject_Closing;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var lvProjectVM = new LV_ProjectVM(LVprojectVM);
            var win = new WinProjectVM(lvProjectVM);

            form_lv.DataContext = lvProjectVM;
            DataContext = win;

            lvProjectVM.SelectedOne = () => form_lv.SelectedItems.HasOnlyOne();
            lvProjectVM.SelectedAny = () => (false == form_lv.SelectedItems.IsEmptyA());
            lvProjectVM.Selected = () => form_lv.SelectedItems.Cast<LVitem_ProjectVM>();

            if (_bOpenProject)
            {
                win.OpenProject();
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            LVprojectVM = form_lv.DataContext as LV_ProjectVM;
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

            var lvProjectVM = form_lv.DataContext as LV_ProjectVM;
            
            if ((null != lvProjectVM) &&
                lvProjectVM.Unsaved &&
                ((null == LVprojectVM) || (false == LVprojectVM.Equals(lvProjectVM))) &&
                (MessageBoxResult.Cancel ==
                MBoxStatic.ShowDialog(WinProjectVM.UnsavedWarning, "Close Project", MessageBoxButton.OKCancel)))
            {
                e.Cancel = true;
            }
        }

        readonly bool _bOpenProject = false;
    }
}
