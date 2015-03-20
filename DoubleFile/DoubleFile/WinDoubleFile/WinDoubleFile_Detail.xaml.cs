using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Detail.xaml
    /// </summary>
    partial class WinDoubleFile_Detail
    {
        internal WinDoubleFile_Detail(GlobalData_Base gd)
        {
            _gd = gd;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
            WinDoubleFile_DuplicatesVM.UpdateFileDetail += UpdateFileDetail;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            form_lvFile.DataContext = _lvFileDetailVM = new LV_FileDetailVM(_gd);          
            form_lvFolder.DataContext = _lvFolderDetailVM = new LV_FolderDetailVM(_gd);
            form_lvVolume.DataContext = _lvVolumeDetailVM = new LV_VolumeDetailVM(_gd);
        }

        internal void UpdateFileDetail(string strFileLine)
        {
            _lvFileDetailVM.Update(strFileLine);
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinTreeMap();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _lvFolderDetailVM.Dispose();
            _lvVolumeDetailVM.Dispose();
            WinDoubleFile_DuplicatesVM.UpdateFileDetail -= UpdateFileDetail;
        }

        LV_FileDetailVM
            _lvFileDetailVM = null;
        LV_FolderDetailVM
            _lvFolderDetailVM = null;
        LV_VolumeDetailVM
            _lvVolumeDetailVM = null;
        GlobalData_Base
            _gd = null;

        override protected double WantsLeft { get { return _nWantsLeft; } set { _nWantsLeft = value; } }
        override protected double WantsTop { get { return _nWantsTop; } set { _nWantsTop = value; } }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
