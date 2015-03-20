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
            WinDoubleFile_FoldersVM.ShowWindows += ShowWindows;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            MinWidth = Width;
            MinHeight = Height;
            _treeView_DoubleFileVM = new TreeView_DoubleFileVM(form_tv);
            _winDoubleFile_FoldersVM = new WinDoubleFile_FoldersVM(_gd, _treeView_DoubleFileVM, _lvProjectVM);
            DataContext = _winDoubleFile_FoldersVM;
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Files(_gd);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _winDoubleFile_FoldersVM.Dispose();
            _treeView_DoubleFileVM.Dispose();
            WinDoubleFile_FoldersVM.ShowWindows -= ShowWindows;
            DataContext = null;
            _lvProjectVM = null;
            _gd = null;
        }

        LV_ProjectVM
            _lvProjectVM = null;
        TreeView_DoubleFileVM
            _treeView_DoubleFileVM = null;
        WinDoubleFile_FoldersVM
            _winDoubleFile_FoldersVM = null;
        GlobalData_Base
            _gd = null;

        override protected double WantsLeft { get { return _nWantsLeft; } set { _nWantsLeft = value; } }
        override protected double WantsTop { get { return _nWantsTop; } set { _nWantsTop = value; } }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
