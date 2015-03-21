using System;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Folders.xaml
    /// </summary>
    partial class WinDoubleFile_Folders
    {
        internal WinDoubleFile_Folders(LV_ProjectVM lvProjectVM)
        {
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
            _winDoubleFile_FoldersVM = new WinDoubleFile_FoldersVM(_treeView_DoubleFileVM, _lvProjectVM);
            DataContext = _winDoubleFile_FoldersVM;
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Files();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _winDoubleFile_FoldersVM.Dispose();
            _treeView_DoubleFileVM.Dispose();
            WinDoubleFile_FoldersVM.ShowWindows -= ShowWindows;
            DataContext = null;
            _lvProjectVM = null;
        }

        LV_ProjectVM
            _lvProjectVM = null;
        TreeView_DoubleFileVM
            _treeView_DoubleFileVM = null;
        WinDoubleFile_FoldersVM
            _winDoubleFile_FoldersVM = null;

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
