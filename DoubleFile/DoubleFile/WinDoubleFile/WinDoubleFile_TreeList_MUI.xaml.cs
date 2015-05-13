using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormDirList.xaml
    /// </summary>
    public partial class WinDoubleFile_TreeList_MUI
    {
        public WinDoubleFile_TreeList_MUI()
        {
            InitializeComponent();

            var lvChildrenVM = new LV_TreeListChildrenVM();

            formLV_Children.DataContext = lvChildrenVM;

            Tag =
                formLV_Siblings.DataContext =
                _lvTreeListSiblingsVM =
                new LV_TreeListSiblingsVM(lvChildrenVM);
        }

        protected override void CopyTag_NewWindow(WeakReference wr)
        {
            _lvTreeListSiblingsVM.CopyFrom(wr.Target as LV_TreeListSiblingsVM);
        }

        LV_TreeListSiblingsVM
            _lvTreeListSiblingsVM = null;
    }
}
