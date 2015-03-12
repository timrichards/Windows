using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Files.xaml
    /// </summary>
    partial class WinDoubleFile_Files
    {
        internal WinDoubleFile_Files(GlobalData_Base gd)
        {
            _gd = gd;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
            ResizeMode = ResizeMode.CanResize;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _lvDoubleFile_FilesVM = new LV_DoubleFile_FilesVM(_gd);
        }

        internal bool ShowWindows()
        {
            return _lvDoubleFile_FilesVM.ShowWindows();
        }

        internal new void Show()
        {
            if (false == LocalIsClosed)
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
            _lvDoubleFile_FilesVM.Dispose();
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        LV_DoubleFile_FilesVM
            _lvDoubleFile_FilesVM = null;
        GlobalData_Base
            _gd = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
