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
                .LocalSubscribe(99682, x => _ucTreeMap?.LostMouseCapture());
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _ucTreeMap =
                new WinTreeMapVM
            {
                LocalOwner = Application.Current.MainWindow,
            };

            Observable.FromEventPattern<SizeChangedEventArgs>(this, "SizeChanged")
                .LocalSubscribe(99806, x =>
            {

            });

            var treeNode = LocalTV.TopNode;
            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null == folderDetail)
                TreeSelect.DoThreadFactory(LocalTV.TopNode, 0);
            else
                treeNode = folderDetail.treeNode;

            _ucTreeMap.GoTo(treeNode);
        }

        protected override void LocalNavigatedFrom()
        {
            _ucTreeMap.Dispose();

            DataContext = 
                _ucTreeMap = null;
        }

        WinTreeMapVM
            _ucTreeMap = null;
    }
}
