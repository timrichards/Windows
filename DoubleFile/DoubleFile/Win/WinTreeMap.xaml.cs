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

            Observable.FromEventPattern(this, "SizeChanged")
                .Subscribe(x => { if (null != _ucTreeMap) _ucTreeMap.ClearSelection(); });

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .Subscribe(x => { if (null != _ucTreeMap) _ucTreeMap.TreeMapVM.LostMouseCapture(); });
        }

        protected override void LocalNavigatedTo()
        {
            _host.Child =
                _ucTreeMap =
                new UC_TreeMap()
            {
                LocalOwner = App.LocalMainWindow as Window,
                TreeMapVM = new WinTreeMapVM()
            };

            DataContext = _ucTreeMap.TreeMapVM;

            var treeNode = LocalTV.TopNode;
            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                treeNode = folderDetail.Item2;

            _ucTreeMap.TreeMapVM.GoTo(treeNode);
        }

        protected override void CopyTag_NewWindow(WeakReference wr)
        {
            LocalNavigatedTo();
        }

        protected override void LocalDispose_WindowClosed()
        {
            if (null != _ucTreeMap)
                _ucTreeMap.Dispose();
        }

        UC_TreeMap
            _ucTreeMap = null;
    }
}
