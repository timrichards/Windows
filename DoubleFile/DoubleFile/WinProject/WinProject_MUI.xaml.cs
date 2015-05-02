using System.Windows;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Collections.Generic;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinProject.xaml
    /// </summary>
    partial class WinProject_MUI : IContent
    {
        internal LV_ProjectVM LVprojectVM { get; private set; }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(NavigationEventArgs e) { }
        public void OnNavigatedTo(NavigationEventArgs e) { Init(); }
        public void OnNavigatingFrom(NavigatingCancelEventArgs e) { e.Cancel = WinProject_CancelOrDispose(); }

        public WinProject_MUI()
        {
            InitializeComponent();

            _lsDisposable.Add(Observable.FromEventPattern(form_btnOK, "Click")
                .Subscribe(args => BtnOK_Click()));
        }

        internal WinProject_MUI(LV_ProjectVM lvProjectVM = null, bool bOpenProject = false)
            : this()
        {
            _bOpenProject = bOpenProject;
            LVprojectVM = lvProjectVM;

            _lsDisposable.Add(Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => Init()));

            _lsDisposable.Add(Observable.FromEventPattern<System.ComponentModel.CancelEventArgs>(this, "Closing")
                .Subscribe(args => args.EventArgs.Cancel = WinProject_CancelOrDispose()));
        }

        private void Init()
        {
            var lvProjectVM = new LV_ProjectVM(LVprojectVM)
            {
                SelectedOne = () => form_lv.SelectedItems.HasOnlyOne(),
                SelectedAny = () => false == form_lv.SelectedItems.IsEmptyA(),
                Selected = () => form_lv.SelectedItems.Cast<LVitem_ProjectVM>()
            };

            var win = new WinProjectVM(lvProjectVM);

            form_lv.DataContext = lvProjectVM;
            DataContext = win;
            LocalDialogResult = false;

            if (_bOpenProject)
                win.OpenProject();
        }

        private void BtnOK_Click()
        {
            LVprojectVM = form_lv.DataContext as LV_ProjectVM;
            LocalDialogResult = true;

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
            // Not when simulating modal.
            CloseIfSimulatingModal();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>as in CancelEventArgs: true if cancel close</returns>
        private bool WinProject_CancelOrDispose()
        {
            if (LocalDialogResult ?? false)
                return false;

            var lvProjectVM = form_lv.DataContext as LV_ProjectVM;

            if (false ==
                (((null != lvProjectVM) &&
                lvProjectVM.Unsaved &&
                ((null == LVprojectVM) || (false == LVprojectVM.LocalEquals(lvProjectVM))) &&
                (MessageBoxResult.Cancel ==
                MBoxStatic.ShowDialog(WinProjectVM.UnsavedWarning, "Close Project", MessageBoxButton.OKCancel)))))
            {
                foreach (var d in _lsDisposable)
                    d.Dispose();

                return false;
            }

            return true;
        }

        readonly bool
            _bOpenProject = false;
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
