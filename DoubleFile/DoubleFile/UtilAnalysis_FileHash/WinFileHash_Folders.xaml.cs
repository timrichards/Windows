using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFileHash_Folders.xaml
    /// </summary>
    public partial class WinFileHash_Folders
    {
        internal WinFileHash_Folders(GlobalData_Base gd, LV_ProjectVM lvProjectVM)
        {
            _gd = gd;
            _lvProjectVM = lvProjectVM;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closed += Window_Closed;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            UString.AddRef();

            DataContext = new WinFileHashVM(_gd,
                new TreeView_FileHashVM(form_tv),
                _lvProjectVM);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            UString.DropRef();
            DataContext = null;
            _lvProjectVM = null;
            _gd = null;
        }

        GlobalData_Base
            _gd = null;
        LV_ProjectVM
            _lvProjectVM = null;
    }
}
