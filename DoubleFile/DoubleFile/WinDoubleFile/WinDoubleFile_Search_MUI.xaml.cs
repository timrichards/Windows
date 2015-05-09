using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Duplicates.xaml
    /// </summary>
    partial class WinDoubleFile_Search_MUI : IContent
    {
        public void OnFragmentNavigation(FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(NavigationEventArgs e) { }
        public void OnNavigatedTo(NavigationEventArgs e)
        {
            WinProject_MUI.InitExplorer();
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e) { }

        internal WinDoubleFile_Search_MUI()
        {
            InitializeComponent();
            
            DataContext = new WinDoubleFile_SearchVM
            {
                IsEditBoxNonEmpty = () => false == string.IsNullOrWhiteSpace(form_searchText.Text)
            }.Init();
        }
    }
}
