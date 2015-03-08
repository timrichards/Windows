using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFileHash_Duplicates.xaml
    /// </summary>
    partial class WinFileHash_Duplicates
    {
        internal WinFileHash_Duplicates(GlobalData_Base gd)
        {
            _gd = gd;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
            ResizeMode = ResizeMode.CanResize;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _winFileHash_DuplicatesVM = new WinFileHash_DuplicatesVM(_gd);
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

            ShowDetailsWindow();
        }

        internal void ShowDetailsWindow()
        {
            _winFileHash_DuplicatesVM.ShowDetailsWindow();
        }

        internal void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates, string strFileLine)
        {
            _winFileHash_DuplicatesVM.TreeFileSelChanged(lsDuplicates, strFileLine);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _winFileHash_DuplicatesVM.Dispose();
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        WinFileHash_DuplicatesVM
            _winFileHash_DuplicatesVM = null;
        GlobalData_Base
            _gd = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
