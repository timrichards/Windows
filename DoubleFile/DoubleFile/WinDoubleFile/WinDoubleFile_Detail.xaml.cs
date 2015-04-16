using System.Reactive.Linq;
using System.Windows;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Detail.xaml
    /// </summary>
    partial class WinDoubleFile_Detail
    {
        public WinDoubleFile_Detail()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => Grid_Loaded());

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => Window_Closed());
        }

        private void Grid_Loaded()
        {
            form_localPath.DataContext = form_lvFile.DataContext = _lvFileDetailVM = new LV_FileDetailVM();          
            form_lvFolder.DataContext = _lvFolderDetailVM = new LV_FolderDetailVM();
            form_lvVolume.DataContext = _lvVolumeDetailVM = new LV_VolumeDetailVM();
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Search();
        }

        private void Window_Closed()
        {
            _lvFileDetailVM.Dispose();
            _lvFolderDetailVM.Dispose();
            _lvVolumeDetailVM.Dispose();
        }

        LV_FileDetailVM
            _lvFileDetailVM = null;
        LV_FolderDetailVM
            _lvFolderDetailVM = null;
        LV_VolumeDetailVM
            _lvVolumeDetailVM = null;

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
