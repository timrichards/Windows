using System.Reactive.Linq;
using System.Windows;
using System;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System.Windows.Controls;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormDirList.xaml
    /// </summary>
    public partial class WinDoubleFile_TreeList_MUI : IContent
    {
        public void OnFragmentNavigation(FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(NavigationEventArgs e) { }
        public void OnNavigatedTo(NavigationEventArgs e)
        {
            WinProject_MUI.InitExplorer();
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e) { }

        internal WinDoubleFile_TreeList_MUI()
        {
            InitializeComponent();

            var lvChildrenVM = new LV_TreeListChildrenVM();

            form_lvChildren.DataContext = lvChildrenVM;
            form_lvSiblings.DataContext = new LV_TreeListSiblingsVM(lvChildrenVM);
        }
    }
}
