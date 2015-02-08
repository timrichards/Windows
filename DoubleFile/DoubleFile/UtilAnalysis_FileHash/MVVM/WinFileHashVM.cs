using System.Windows.Input;

namespace DoubleFile
{
    partial class WinFileHashVM : ObservableObject_OwnerWindow
    {
        internal WinFileHashVM()
        {
     //       m_tvVM = new TreeView_FileHashVM(m_app.xaml_treeViewBrowse, m_app.Dispatcher);
        }

        internal void SetPartner(TreeView_FileHashVM tvVM)
        {
            m_tvVM = tvVM;
        }

        TreeView_FileHashVM m_tvVM = null;
    }
}
