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
            new CreateFileDictProcess(gd, lvProjectVM_in);
            gd.FileDictionary.Deserialize();
            gd.FileDictionary.Serialize();
            if (gd.FileDictionary == null)
            {
                return;
            }

            foreach (var lvItem in m_lvProjectVM.ItemsCast)
            {

            }
        }

        GlobalData_Base gd = null;
        LV_ProjectVM m_lvProjectVM = null;
    }
}
