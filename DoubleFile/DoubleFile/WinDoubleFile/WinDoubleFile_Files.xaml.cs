using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Files.xaml
    /// </summary>
    partial class WinDoubleFile_Files
    {
        internal WinDoubleFile_Files()
        {
            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
            ResizeMode = ResizeMode.CanResize;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _lvDoubleFile_FilesVM = new LV_DoubleFile_FilesVM();
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Duplicates();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _lvDoubleFile_FilesVM.Dispose();
        }

        LV_DoubleFile_FilesVM
            _lvDoubleFile_FilesVM = null;

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
