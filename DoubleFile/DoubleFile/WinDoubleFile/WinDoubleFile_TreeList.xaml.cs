using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormDirList.xaml
    /// </summary>
    public partial class WinDoubleFile_TreeList
    {
        internal WinDoubleFile_TreeList()
        {
            InitializeComponent();
            Closed += Window_Closed;
            ResizeMode = ResizeMode.CanResize;

            var lvChildrenVM = new LV_TreeListChildrenVM();

            LV_Children.DataContext = lvChildrenVM;
            LV_Siblings.DataContext = _lvTreeListSiblingsVM = new LV_TreeListSiblingsVM(lvChildrenVM);
        }

        internal new void Show()
        {
            if (false == LocalIsClosed)
            {
                MBoxStatic.Assert(99897, false, bTraceOnly: true);
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
            _nWantsLeft = Left;
            _nWantsTop = Top;

            _lvTreeListSiblingsVM.Dispose();
        }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;

        LV_TreeListSiblingsVM
            _lvTreeListSiblingsVM = null;
    }
}
