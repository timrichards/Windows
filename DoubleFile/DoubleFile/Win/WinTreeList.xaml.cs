using System;
using System.Reactive.Linq;

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

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .Subscribe(x => { if (null != _lvTreeListChildrenVM) _lvTreeListChildrenVM.LostMouseCapture(); });
        }

        protected override void LocalNavigatedTo()
        {
            formLV_Children.DataContext = 
                form_gridChildrenCtls.DataContext =
                _lvTreeListChildrenVM =
                new LV_TreeListChildrenVM();

            Tag =
                formLV_Siblings.DataContext =
                _lvTreeListSiblingsVM =
                new LV_TreeListSiblingsVM(_lvTreeListChildrenVM);
        }

        protected override void LocalNavigatedFrom()
        {
            if (null != _lvTreeListSiblingsVM)
                _lvTreeListSiblingsVM.Dispose();
        }

        LV_TreeListSiblingsVM
            _lvTreeListSiblingsVM = null;
        LV_TreeListChildrenVM
            _lvTreeListChildrenVM = null;
    }
}
