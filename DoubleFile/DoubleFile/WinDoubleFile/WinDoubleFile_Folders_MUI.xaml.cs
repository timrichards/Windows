using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Folders.xaml
    /// </summary>
    partial class WinDoubleFile_Folders_MUI : IContent
    {
        public void OnFragmentNavigation(FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(NavigationEventArgs e) { }
        public void OnNavigatedTo(NavigationEventArgs e)
        {
            if (false == WinProject_MUI.InitExplorer())
            {
                if ((null == App.LVprojectVM) ||
                    (App.LVprojectVM == _lvProjectVM) ||
                    (App.LVprojectVM.LocalEquals(_lvProjectVM)))
                {
                    return;
                }
            }

            _lvProjectVM = new LV_ProjectVM(App.LVprojectVM);
            form_tv.DataContext = null;

            if (_lvProjectVM.Items.IsEmpty())
                return;

            new TreeView_DoubleFileVM(form_tv, LocalTV.RootNodes);
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e) { }

        internal WinDoubleFile_Folders_MUI()
        {
            InitializeComponent();
        }

        LV_ProjectVM
            _lvProjectVM = null;
    }
}
