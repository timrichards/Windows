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
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _winDoubleFile_DuplicatesVM = new WinDoubleFile_DuplicatesVM(_gd);
        }

        internal new void Show()
        {
            if (false == LocalIsClosed)
            {
                MBoxStatic.Assert(99905, false, bTraceOnly: true);
                return;
            }

            base.Show();
            
            if (_nWantsLeft > -1)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
            }

            ShowWindows();
        }

        internal bool ShowWindows()
        {
            return _winDoubleFile_DuplicatesVM.ShowWindows();
        }

        internal void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates, string strFileLine)
        {
            _winDoubleFile_DuplicatesVM.TreeFileSelChanged(lsDuplicates, strFileLine);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _winDoubleFile_DuplicatesVM.Dispose();
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        WinDoubleFile_DuplicatesVM
            _winDoubleFile_DuplicatesVM = null;
        GlobalData_Base
            _gd = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
