using System;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFileHash_Folders.xaml
    /// </summary>
    partial class WinFileHash_Folders
    {
        internal WinFileHash_Folders(GlobalData_Base gd, LV_ProjectVM lvProjectVM)
        {
            _gd = gd;
            _lvProjectVM = lvProjectVM;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
            form_btnFilesWindow.Click += Form_ButtonFilesWindowClick;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            MinWidth = Width;
            MinHeight = Height;
            _winFileHashVM = new WinFileHashVM(_gd,
                new TreeView_FileHashVM(form_tv),
                _lvProjectVM);
            DataContext = _winFileHashVM;
        }

        void Form_ButtonFilesWindowClick(object sender, EventArgs e)
        {
            if (null != _winFileHashVM)
                _winFileHashVM.ShowFilesBrowser();
        }

        internal new void Show()
        {
            if (false == IsClosed)
            {
                MBoxStatic.Assert(99903, false, bTraceOnly: true);
                return;
            }

            base.Show();
            
            if (_nWantsLeft > -1)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (null != _winFileHashVM)
                _winFileHashVM.Dispose();     // closes the file list (domino/chain) when this tree view closes

            DataContext = null;
            _lvProjectVM = null;
            _gd = null;
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        GlobalData_Base
            _gd = null;
        LV_ProjectVM
            _lvProjectVM = null;
        WinFileHashVM
            _winFileHashVM = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
