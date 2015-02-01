using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinAnalysis_FileHash.xaml
    /// </summary>
    public partial class WinAnalysis_FileHash : Window
    {
        internal WinAnalysis_FileHash(GlobalData_Base gd_in, LV_ProjectVM lvProjectVM_in)
        {
            InitializeComponent();
            new CreateFileDictProcess(gd_in, lvProjectVM_in);
        }
    }
}
