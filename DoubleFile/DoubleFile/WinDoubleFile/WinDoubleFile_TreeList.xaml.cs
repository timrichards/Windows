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

            form_lvChildren.DataContext = lvChildrenVM;
            form_lvSiblings.DataContext = _lvTreeListSiblingsVM = new LV_TreeListSiblingsVM(lvChildrenVM);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _lvTreeListSiblingsVM.Dispose();
        }

        LV_TreeListSiblingsVM
            _lvTreeListSiblingsVM = null;

        override protected double WantsLeft { get { return _nWantsLeft; } set { _nWantsLeft = value; } }
        override protected double WantsTop { get { return _nWantsTop; } set { _nWantsTop = value; } }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
