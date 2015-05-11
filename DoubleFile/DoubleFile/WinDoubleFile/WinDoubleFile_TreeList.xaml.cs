using System.Reactive.Linq;
using System.Windows;
using System;

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

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => _lvTreeListSiblingsVM.Dispose());

            ResizeMode = ResizeMode.CanResize;

            var lvChildrenVM = new LV_TreeListChildrenVM();

            formLV_Children.DataContext = lvChildrenVM;
            formLV_Siblings.DataContext = _lvTreeListSiblingsVM = new LV_TreeListSiblingsVM(lvChildrenVM);
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Files();
        }

        LV_TreeListSiblingsVM
            _lvTreeListSiblingsVM = null;

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
