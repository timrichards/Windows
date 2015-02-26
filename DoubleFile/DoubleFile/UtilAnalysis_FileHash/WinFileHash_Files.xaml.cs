using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFileHash_Files.xaml
    /// </summary>
    public partial class WinFileHash_Files
    {
        public WinFileHash_Files()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new LV_FileHashVM();
        }
    }
}
