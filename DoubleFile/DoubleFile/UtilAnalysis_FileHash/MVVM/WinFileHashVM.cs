using System.Windows.Input;

namespace DoubleFile
{
    partial class WinFileHashVM : ObservableObject_OwnerWindow
    {
        internal WinFileHashVM(GlobalData_Base gd_in, TreeView_FileHashVM tvVM, LV_ProjectVM lvProjectVM_in)
        {
            gd = gd_in;
            m_tvVM = tvVM;
            m_lvProjectVM = lvProjectVM_in;

            if (m_lvProjectVM == null)
            {
                return;
            }

            m_winProgress = new WinProgress(); 
            m_nCorrelateProgressDenominator = m_lvProjectVM.Count;
            DoTree();
        }

        readonly GlobalData_Base gd = null;
        readonly TreeView_FileHashVM m_tvVM = null;
        readonly LV_ProjectVM m_lvProjectVM = null;
        readonly WinProgress m_winProgress = null;
        readonly double m_nCorrelateProgressDenominator = 0;
    }
}
