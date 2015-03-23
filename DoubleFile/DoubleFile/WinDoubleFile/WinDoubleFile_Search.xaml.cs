using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Duplicates.xaml
    /// </summary>
    partial class WinDoubleFile_Search
    {
        internal WinDoubleFile_Search()
        {
            InitializeComponent();
            
            var vm = new WinDoubleFile_SearchVM();

            vm.IsSearchEnabled = () => false == string.IsNullOrWhiteSpace(form_searchText.Text);
            DataContext = vm;
        }

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
