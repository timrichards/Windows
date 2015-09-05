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
                .LocalSubscribe(99684, x => _lvChildrenVM?.LostMouseCapture());
        }

        protected override void LocalNavigatedTo()
        {
            formLV_Children.DataContext = 
                form_gridChildrenCtls.DataContext =
                _lvChildrenVM =
                new LV_TreeListChildrenVM();

            Tag =
                formLV_Siblings.DataContext =
                _lvSiblingsVM =
                new LV_TreeListSiblingsVM(_lvChildrenVM);
        }

        protected override void LocalNavigatedFrom()
        {
            _lvSiblingsVM?.Dispose();

            formLV_Children.DataContext =
                form_gridChildrenCtls.DataContext =
                _lvChildrenVM =
                null;

            Tag =
                formLV_Siblings.DataContext =
                _lvSiblingsVM =
                null;
        }

        LV_TreeListSiblingsVM
            _lvSiblingsVM = null;
        LV_TreeListChildrenVM
            _lvChildrenVM = null;
    }
}
