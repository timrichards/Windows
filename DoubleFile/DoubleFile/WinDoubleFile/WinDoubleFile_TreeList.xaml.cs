using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormDirList.xaml
    /// </summary>
    public partial class WinTreeList
    {
        public WinTreeList()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            var lvChildrenVM = new LV_TreeListChildrenVM();

            formLV_Children.DataContext = lvChildrenVM;

            Tag =
                formLV_Siblings.DataContext =
                _lvTreeListSiblingsVM =
                new LV_TreeListSiblingsVM(lvChildrenVM);
        }

        protected override void CopyTag_NewWindow(WeakReference wr)
        {
            LocalNavigatedTo();
            _lvTreeListSiblingsVM.CopyFrom(wr.Target as LV_TreeListSiblingsVM);
        }

        protected override void LocalDispose_WindowClosed()
        {
            if (null != _lvTreeListSiblingsVM)
                _lvTreeListSiblingsVM.Dispose();
        }

        LV_TreeListSiblingsVM
            _lvTreeListSiblingsVM = null;
    }
}
