using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFileHash_Files.xaml
    /// </summary>
    public partial class WinFileHash_Files
    {
        internal WinFileHash_Files(GlobalData_Base gd)
        {
            _gd = gd;

            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var lvFileHashVM = new LV_FileHashVM(_gd);

            DataContext = lvFileHashVM;
            Closed += (o, a) => lvFileHashVM.Dispose();
        }

        GlobalData_Base _gd = null;
    }
}
