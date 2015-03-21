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

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Files();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _lvTreeListSiblingsVM.Dispose();
        }

        LV_TreeListSiblingsVM
            _lvTreeListSiblingsVM = null;

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
