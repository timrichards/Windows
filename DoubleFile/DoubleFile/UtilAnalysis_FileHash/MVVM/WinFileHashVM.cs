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
            DoTree();
        }

        GlobalData_Base gd = null;
        TreeView_FileHashVM m_tvVM = null;
        LV_ProjectVM m_lvProjectVM = null;
    }
}
