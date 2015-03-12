using System;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Folders.xaml
    /// </summary>
    partial class WinDoubleFile_Folders
    {
        internal WinDoubleFile_Folders(GlobalData_Base gd, LV_ProjectVM lvProjectVM)
        {
            _gd = gd;
            _lvProjectVM = lvProjectVM;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
            ResizeMode = ResizeMode.CanResize;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            MinWidth = Width;
            MinHeight = Height;
            _treeView_DoubleFileVM = new TreeView_DoubleFileVM(form_tv);
            _winDoubleFile_FoldersVM = new WinDoubleFile_FoldersVM(_gd, _treeView_DoubleFileVM, _lvProjectVM);
            DataContext = _winDoubleFile_FoldersVM;
        }

        internal new void Show()
        {
            if (false == LocalIsClosed)
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

        internal bool ShowWindows()
        {
            return _winDoubleFile_FoldersVM.ShowWindows();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _winDoubleFile_FoldersVM.Dispose();     // closes the file list (domino/chain) when this tree view closes
            _treeView_DoubleFileVM.Dispose();
            DataContext = null;
            _lvProjectVM = null;
            _gd = null;
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        LV_ProjectVM
            _lvProjectVM = null;
        TreeView_DoubleFileVM
            _treeView_DoubleFileVM = null;
        WinDoubleFile_FoldersVM
            _winDoubleFile_FoldersVM = null;
        GlobalData_Base
            _gd = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
