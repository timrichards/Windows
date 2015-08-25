using System.Reactive.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTreeList.xaml
    /// </summary>
    public partial class UC_TreeList
    {
        public UC_TreeList()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .LocalSubscribe(99684, x => _lvTreeListChildrenVM?.LostMouseCapture());
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
