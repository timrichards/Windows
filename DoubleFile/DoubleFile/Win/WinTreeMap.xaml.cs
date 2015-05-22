using System.Reactive.Linq;
using System.Windows;
using System;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System.Windows.Controls;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormDirList.xaml
    /// </summary>
    public partial class WinTreeMap
    {
        public WinTreeMap()
        {
            InitializeComponent();

            _host.Child =
                _ucTreeMap =
                new UC_TreeMap()
            {
                LocalOwner = App.LocalMainWindow as Window,
                TreeMapVM = new WinTreeMapVM()
            };

            DataContext = _ucTreeMap.TreeMapVM;

            Observable.FromEventPattern(this, "SizeChanged")
                .Subscribe(x => { if (null != _ucTreeMap) _ucTreeMap.ClearSelection(); });

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .Subscribe(x => { if (null != _ucTreeMap) _ucTreeMap.TreeMapVM.LostMouseCapture(); });
        }

        protected override void LocalNavigatedTo()
        {
            var treeNode = LocalTV.TopNode;
            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null == folderDetail)
                TreeSelect.DoThreadFactory(LocalTV.TopNode, 0);
            else
                treeNode = folderDetail.Item2;

            _ucTreeMap.TreeMapVM.GoTo(treeNode);
        }

        protected override void LocalWindowClosed()
        {
            if (null != _ucTreeMap)
                _ucTreeMap.Dispose();
        }

        UC_TreeMap
            _ucTreeMap = null;
    }
}
