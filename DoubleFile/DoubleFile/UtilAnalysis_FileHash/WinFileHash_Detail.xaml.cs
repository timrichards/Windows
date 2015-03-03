using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFileHash_Detail.xaml
    /// </summary>
    partial class WinFileHash_Detail
    {
        internal WinFileHash_Detail(GlobalData_Base gd)
        {
            _gd = gd;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            form_lvFile.DataContext = _lvFileDetailVM = new LV_FileDetailVM(_gd);          
            form_lvFolder.DataContext = _lvFolderDetailVM = new LV_FolderDetailVM(_gd);
            form_lvVolume.DataContext = _lvVolumeDetailVM = new LV_VolumeDetailVM(_gd);
        }

        internal void UpdateFileDetail(string strFileLine = null)
        {
            _lvFileDetailVM.Update(strFileLine);
        }

        internal new void Show()
        {
            if (false == IsClosed)
            {
                MBoxStatic.Assert(99906, false, bTraceOnly: true);
                return;
            }

            base.Show();
            
            if (_nWantsLeft > -1)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _lvFolderDetailVM.Dispose();
            _lvVolumeDetailVM.Dispose();
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        LV_FileDetailVM
            _lvFileDetailVM = null;
        LV_FolderDetailVM
            _lvFolderDetailVM = null;
        LV_VolumeDetailVM
            _lvVolumeDetailVM = null;

        GlobalData_Base
            _gd = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
