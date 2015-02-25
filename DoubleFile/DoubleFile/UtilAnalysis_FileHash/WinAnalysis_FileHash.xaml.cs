using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinAnalysis_FileHash.xaml
    /// </summary>
    public partial class WinAnalysis_FileHash
    {
        internal WinAnalysis_FileHash(GlobalData_Base gd, LV_ProjectVM lvProjectVM)
        {
            InitializeComponent();
            _gd = gd;
            _lvProjectVM = lvProjectVM;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            UString.AddRef();
            Closed += (o, a) =>
            {
                UString.DropRef();
                DataContext = null;
                _lvProjectVM = null;
                _gd = null;
            };

            DataContext = new WinFileHashVM(_gd,
                new TreeView_FileHashVM(form_tv),
                _lvProjectVM);
        }

        GlobalData_Base
            _gd = null;
        LV_ProjectVM
            _lvProjectVM = null;
    }
}
