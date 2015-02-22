using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinAnalysis_FileHash.xaml
    /// </summary>
    public partial class WinAnalysis_FileHash
    {
        internal WinAnalysis_FileHash(GlobalData_Base gd_in, LV_ProjectVM lvProjectVM_in)
        {
            InitializeComponent();
            gd = gd_in;
            m_lvProjectVM = lvProjectVM_in;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            UString.AddRef();
            Closed += (o, a) => UString.DropRef();
            DataContext = new WinFileHashVM(gd,
                new TreeView_FileHashVM(form_tv),
                m_lvProjectVM);
        }

        readonly GlobalData_Base gd = null;
        readonly LV_ProjectVM m_lvProjectVM = null;
    }
}
