using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFileHash_Files.xaml
    /// </summary>
    partial class WinFileHash_Files
    {
        internal WinFileHash_Files(GlobalData_Base gd)
        {
            _gd = gd;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            _lvFileHashVM = new LV_FileHashVM(_gd);
            DataContext = _lvFileHashVM;
            form_lv.SelectionChanged += SelectionChanged;
            Closed += Window_Closed;
        }

        void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 == e.AddedItems.Count)
                return;

            var lvItem = e.AddedItems[0] as LVitem_FileHashVM;

            if (null == lvItem)
            {
                MBoxStatic.Assert(99909, false);
                return;
            }

            _winFileHash_Duplicates.TreeFileSelChanged(lvItem.LSduplicates);
        }

        internal void ShowFilesBrowser()
        {
            if ((null != _winFileHash_Duplicates) &&
                (false == _winFileHash_Duplicates.IsClosed))
            {
                _winFileHash_Duplicates.ShowDetailsWindow();
                Activate();
                return;
            }

            (_winFileHash_Duplicates = new WinFileHash_Duplicates(_gd)).Show();
        }

        internal new void Show()
        {
            if (false == IsClosed)
            {
                MBoxStatic.Assert(99904, false, bTraceOnly: true);
                return;
            }

            base.Show();
            
            if (_nWantsLeft > -1)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _lvFileHashVM.Dispose();
            _winFileHash_Duplicates.Close();
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        GlobalData_Base
            _gd = null;
        LV_FileHashVM
            _lvFileHashVM = null;
        WinFileHash_Duplicates
            _winFileHash_Duplicates = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
