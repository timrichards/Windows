using System;
using System.Reactive.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTreeList.xaml
    /// </summary>
    public partial class WinTreeList
    {
        public WinTreeList()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .Subscribe(x => { _lvTreeListChildrenVM?.LostMouseCapture(); });
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
            _lvTreeListSiblingsVM.Dispose();

            formLV_Children.DataContext =
                form_gridChildrenCtls.DataContext =
                _lvTreeListChildrenVM =
                null;

            Tag =
                formLV_Siblings.DataContext =
                _lvTreeListSiblingsVM =
                null;
        }

        LV_TreeListSiblingsVM
            _lvTreeListSiblingsVM = null;
        LV_TreeListChildrenVM
            _lvTreeListChildrenVM = null;
    }
}
