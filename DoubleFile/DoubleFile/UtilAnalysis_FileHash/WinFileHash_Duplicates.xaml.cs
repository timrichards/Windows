using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFileHash_Duplicates.xaml
    /// </summary>
    partial class WinFileHash_Duplicates
    {
        internal WinFileHash_Duplicates(GlobalData_Base gd)
        {
            _gd = gd;

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Local.TreeSelect.FileListUpdated += TreeSelect_FileList;
            form_lv.SelectionChanged += SelectionChanged;
            Closed += Window_Closed;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _lvFileDuplicatesVM = new LV_FileDuplicatesVM(_gd);

            _lvFileDuplicatesVM.SelectedOne = () => form_lv.SelectedItems.HasOnlyOne();
            _lvFileDuplicatesVM.SelectedAny = () => (false == form_lv.SelectedItems.IsEmptyA());
            _lvFileDuplicatesVM.Selected = () => form_lv.SelectedItems.Cast<LVitem_FileDuplicatesVM>();
        }

        void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 == e.AddedItems.Count)
                return;

            var lvItem = e.AddedItems[0] as LVitem_FileDuplicatesVM;

            if (null == lvItem)
            {
                MBoxStatic.Assert(99902, false);
                return;
            }

            _winFileHash_Detail.UpdateFileDetail(lvItem.FileLine);
        }

        internal void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates, string strFileLine)
        {
            _lvFileDuplicatesVM.TreeFileSelChanged(lsDuplicates);
            _winFileHash_Detail.UpdateFileDetail(strFileLine);
        }

        void TreeSelect_FileList(IEnumerable<string> lsFileLines, string strListingFile)
        {
            _lvFileDuplicatesVM.ClearItems();
            _winFileHash_Detail.UpdateFileDetail(/*clear items*/);
        }

        internal new void Show()
        {
            if (false == IsClosed)
            {
                MBoxStatic.Assert(99905, false, bTraceOnly: true);
                return;
            }

            base.Show();
            
            if (_nWantsLeft > -1)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
            }

            ShowDetailsWindow();
        }

        internal void ShowDetailsWindow()
        {
            if ((null != _winFileHash_Detail) &&
                (false == _winFileHash_Detail.IsClosed))
            {
                return;
            }

            (_winFileHash_Detail = new WinFileHash_Detail(_gd)).Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if ((null != _winFileHash_Detail) &&
                (false == _winFileHash_Detail.IsClosed))
            {
                _winFileHash_Detail.Close();
            }

            Local.TreeSelect.FileListUpdated -= TreeSelect_FileList;
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        LV_FileDuplicatesVM
            _lvFileDuplicatesVM = null;
        GlobalData_Base
            _gd = null;
        WinFileHash_Detail
            _winFileHash_Detail = null;

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
