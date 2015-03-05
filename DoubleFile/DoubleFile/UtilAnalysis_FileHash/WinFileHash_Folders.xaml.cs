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
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            MinWidth = Width;
            MinHeight = Height;
            _treeView_FileHashVM = new TreeView_FileHashVM(form_tv);
            _winFileHash_FoldersVM = new WinFileHash_FoldersVM(_gd, _treeView_FileHashVM, _lvProjectVM);
            DataContext = _winFileHash_FoldersVM;
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
            _winFileHash_FoldersVM.Dispose();     // closes the file list (domino/chain) when this tree view closes
            _treeView_FileHashVM.Dispose();
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
        TreeView_FileHashVM
            _treeView_FileHashVM = null;
        WinFileHash_FoldersVM
            _winFileHash_FoldersVM = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
