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

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Duplicates(_gd);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _lvDoubleFile_FilesVM.Dispose();
        }

        LV_DoubleFile_FilesVM
            _lvDoubleFile_FilesVM = null;
        GlobalData_Base
            _gd = null;

        override protected double WantsLeft { get { return _nWantsLeft; } set { _nWantsLeft = value; } }
        override protected double WantsTop { get { return _nWantsTop; } set { _nWantsTop = value; } }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
