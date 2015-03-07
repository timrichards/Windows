using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFileHash_Files.xaml
    /// </summary>
    partial class WinFileHash_Files
    {
        internal WinFileHash_Files(GlobalData_Base gd)
        {
            _gd = gd;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
            ResizeMode = ResizeMode.CanResize;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _lvFileHash_FilesVM = new LV_FileHash_FilesVM(_gd);
        }

        internal bool ShowWindows()
        {
            return _lvFileHash_FilesVM.ShowWindows();
        }

        internal new void Show()
        {
            if (false == IsClosed)
            {
                MBoxStatic.Assert(99904, false, bTraceOnly: true);
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
            _lvFileHash_FilesVM.Dispose();
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        LV_FileHash_FilesVM
            _lvFileHash_FilesVM = null;
        GlobalData_Base
            _gd = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
