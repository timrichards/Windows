using System.Windows;
using System.Linq;
using System.Reactive.Linq;
using System;

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

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => Grid_Loaded());

            Observable.FromEventPattern(form_btnOK, "Click")
                .Subscribe(args => BtnOK_Click());

            Observable.FromEventPattern<System.ComponentModel.CancelEventArgs>(this, "Closing")
                .Subscribe(args => WinProject_Closing(args.EventArgs));
        }

        private void Grid_Loaded()
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

        private void BtnOK_Click()
        {
            LVprojectVM = form_lv.DataContext as LV_ProjectVM;
            LocalDialogResult = true;

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
            // Not when simulating modal.
            CloseIfSimulatingModal();
        }

        private void WinProject_Closing(System.ComponentModel.CancelEventArgs e)
        {
            if (LocalDialogResult ?? false)
            {
                return;
            }

            var lvProjectVM = form_lv.DataContext as LV_ProjectVM;
            
            if ((null != lvProjectVM) &&
                lvProjectVM.Unsaved &&
                ((null == LVprojectVM) || (false == LVprojectVM.LocalEquals(lvProjectVM))) &&
                (MessageBoxResult.Cancel ==
                MBoxStatic.ShowDialog(WinProjectVM.UnsavedWarning, "Close Project", MessageBoxButton.OKCancel)))
            {
                e.Cancel = true;
            }
        }

        readonly bool _bOpenProject = false;
    }
}
