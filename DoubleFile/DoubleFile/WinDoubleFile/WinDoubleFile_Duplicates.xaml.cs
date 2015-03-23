using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Duplicates.xaml
    /// </summary>
    partial class WinDoubleFile_Duplicates
    {
        internal WinDoubleFile_Duplicates()
        {
            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
            ContentRendered += WinDoubleFile_Duplicates_ContentRendered;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _winDoubleFile_DuplicatesVM = new WinDoubleFile_DuplicatesVM();
        }

        void WinDoubleFile_Duplicates_ContentRendered(object sender, System.EventArgs e)
        {
            if (Rect.Empty == _rcPosAtClose)
                Top += 50;
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Detail();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _winDoubleFile_DuplicatesVM.Dispose();
        }

        WinDoubleFile_DuplicatesVM
            _winDoubleFile_DuplicatesVM = null;

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
