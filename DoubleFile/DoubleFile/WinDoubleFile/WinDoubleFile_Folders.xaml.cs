using System;
using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Folders.xaml
    /// </summary>
    partial class WinDoubleFile_Folders
    {
        public WinDoubleFile_Folders()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => Grid_Loaded());

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => Window_Closed());

            ResizeMode = ResizeMode.CanResize;
        }

        internal WinDoubleFile_Folders(LV_ProjectVM lvProjectVM)
            : this()
        {
            _lvProjectVM = lvProjectVM;
        }

        private void Grid_Loaded()
        {
            MinWidth = Width;
            MinHeight = Height;
            _treeView_DoubleFileVM = new TreeView_DoubleFileVM(form_tv);
            _winDoubleFile_FoldersVM = new WinDoubleFile_FoldersVM(_treeView_DoubleFileVM, _lvProjectVM);
            DataContext = _winDoubleFile_FoldersVM;
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinTreeMap();
        }

        private void Window_Closed()
        {
            _winDoubleFile_FoldersVM.Dispose();
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
