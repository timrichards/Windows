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

        internal new void Show()
        {
            if (false == IsClosed)
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
            if ((null != _winFileHash_Detail) &&
                (false == _winFileHash_Detail.IsClosed))
            {
                return;
            }

            (_winFileHash_Detail = new WinFileHash_Detail(_gd)).Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if ((null != _winFileHash_Detail) &&
                (false == _winFileHash_Detail.IsClosed))
            {
                _winFileHash_Detail.Close();
            }

            _lvFileDuplicatesVM.Dispose();
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        LV_FileDuplicatesVM
            _lvFileDuplicatesVM = null;
        GlobalData_Base
            _gd = null;
        WinFileHash_Detail
            _winFileHash_Detail = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
