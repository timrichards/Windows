using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFileHash_Files.xaml
    /// </summary>
    partial class WinFileHash_Duplicates
    {
        internal WinFileHash_Duplicates(GlobalData_Base gd)
        {
            _gd = gd;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += (o, e) => _lvFileDuplicatesVM.Dispose();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            _lvFileDuplicatesVM = new LV_FileDuplicatesVM(_gd);
            DataContext = _lvFileDuplicatesVM;
        }

        internal void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates)
        {
            _lvFileDuplicatesVM.TreeFileSelChanged(lsDuplicates);
        }

        LV_FileDuplicatesVM _lvFileDuplicatesVM = null;
        GlobalData_Base _gd = null;
    }
}
