using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Duplicates.xaml
    /// </summary>
    partial class WinDoubleFile_Duplicates
    {
        internal WinDoubleFile_Duplicates(GlobalData_Base gd)
        {
            _gd = gd;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
            ResizeMode = ResizeMode.CanResize;
            LV_DoubleFile_FilesVM.TreeFileSelChanged += TreeFileSelChanged;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _winDoubleFile_DuplicatesVM = new WinDoubleFile_DuplicatesVM(_gd);
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Detail(_gd);
        }

        internal void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates, string strFileLine)
        {
            _winDoubleFile_DuplicatesVM.TreeFileSelChanged(lsDuplicates, strFileLine);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _winDoubleFile_DuplicatesVM.Dispose();
            LV_DoubleFile_FilesVM.TreeFileSelChanged -= TreeFileSelChanged;
        }

        WinDoubleFile_DuplicatesVM
            _winDoubleFile_DuplicatesVM = null;
        GlobalData_Base
            _gd = null;

        override protected double WantsLeft { get { return _nWantsLeft; } set { _nWantsLeft = value; } }
        override protected double WantsTop { get { return _nWantsTop; } set { _nWantsTop = value; } }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
