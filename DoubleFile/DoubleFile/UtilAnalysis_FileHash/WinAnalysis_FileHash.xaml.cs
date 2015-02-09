using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinAnalysis_FileHash.xaml
    /// </summary>
    public partial class WinAnalysis_FileHash : LocalWindow
    {
        internal WinAnalysis_FileHash(GlobalData_Base gd_in, LV_ProjectVM lvProjectVM_in)
        {
            InitializeComponent();
            gd = gd_in;
            m_lvProjectVM = lvProjectVM_in;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new WinFileHashVM(gd,
                new TreeView_FileHashVM(form_tv, Dispatcher),
                m_lvProjectVM);
        }

        GlobalData_Base gd = null;
        LV_ProjectVM m_lvProjectVM = null;
    }
}
