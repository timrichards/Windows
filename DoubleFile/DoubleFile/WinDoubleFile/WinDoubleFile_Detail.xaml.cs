using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Detail.xaml
    /// </summary>
    partial class WinDoubleFile_Detail
    {
        internal WinDoubleFile_Detail()
        {
            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Search();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            form_lvFile.DataContext = _lvFileDetailVM = new LV_FileDetailVM();          
            form_lvFolder.DataContext = _lvFolderDetailVM = new LV_FolderDetailVM();
            form_lvVolume.DataContext = _lvVolumeDetailVM = new LV_VolumeDetailVM();
        }

        private void Window_Closed(object sender, System.EventArgs e)
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
