using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace SearchDirLists
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal readonly GlobalData gd = null;
        internal readonly VolumesTabVM VolumesTab = null;
        internal readonly BrowseTabVM BrowseTab = null;

        public MainWindow()
        {
            gd = GlobalData.GetInstance(this);
            gd.m_blinky = new Blinky(xaml_cbFindbox);
            InitializeComponent();
            VolumesTab = new VolumesTabVM(this);
            BrowseTab = new BrowseTabVM(this);
        }

        private void xaml_cbSaveAs_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (new Key[] {Key.Enter, Key.Return}.Contains(e.Key))
            {
#if (WPF)
                xaml_btnAddVolume.Command.Execute(null);
#endif
            }
        }

        private void xaml_ClonesLV_KeyUp(object sender, KeyEventArgs e)
        {
            ((ListViewItemVM)((ListViewItem)sender).DataContext).KeyUp(e);
        }

        private void xaml_ClonesLV_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((ListViewItemVM)((ListViewItem)sender).DataContext).MouseUp();
        }
    }
}
