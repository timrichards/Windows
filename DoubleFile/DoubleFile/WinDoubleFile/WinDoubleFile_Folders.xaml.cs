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
        internal WinDoubleFile_Folders(LV_ProjectVM lvProjectVM)
        {
            _lvProjectVM = lvProjectVM;
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => Grid_Loaded());

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => Window_Closed());

            ResizeMode = ResizeMode.CanResize;
        }

        private void Grid_Loaded()
        {
            MinWidth = Width;
            MinHeight = Height;
            LocalTV.FactoryCreate(_lvProjectVM);
            _treeView_DoubleFileVM = new TreeView_DoubleFileVM(form_tv, LocalTV.RootNodes);
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinTreeMap();
        }

        private void Window_Closed()
        {
            LocalTV.LocalDispose();
            DataContext = null;
            _lvProjectVM = null;
        }

        LV_ProjectVM
            _lvProjectVM = null;
        TreeView_DoubleFileVM
            _treeView_DoubleFileVM = null;

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
