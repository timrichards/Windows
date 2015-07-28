using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTreeMap.xaml
    /// </summary>
    public partial class WinTreeMap
    {
        public WinTreeMap()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .LocalSubscribe(99682, x => _ucTreeMap?.TreeMapVM.LostMouseCapture());
        }

        protected override void LocalNavigatedTo()
        {
            _host.Child =
                _ucTreeMap =
                new UC_TreeMap()
            {
                LocalOwner = Application.Current.MainWindow,
                TreeMapVM = new WinTreeMapVM()
            };

            DataContext = _ucTreeMap.TreeMapVM;

            var treeNode = LocalTV.TopNode;
            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null == folderDetail)
                TreeSelect.DoThreadFactory(LocalTV.TopNode, 0);
            else
                treeNode = folderDetail.treeNode;

            _ucTreeMap.TreeMapVM.GoTo(treeNode);
        }

        protected override void LocalNavigatedFrom()
        {
            _ucTreeMap.Dispose();

            _host.Child =
                _ucTreeMap =
                null;

            DataContext = null;
        }

        UC_TreeMap
            _ucTreeMap = null;
    }
}
