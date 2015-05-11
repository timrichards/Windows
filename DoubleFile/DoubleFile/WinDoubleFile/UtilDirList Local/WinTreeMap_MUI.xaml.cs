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
    public partial class WinTreeMap_MUI
    {
        protected override void LocalNavigatedTo()
        {
            if (null != _ucTreeMap)
                _ucTreeMap.Dispose();

            _ucTreeMap = new UC_TreeMap();
            _ucTreeMap.LocalOwner = App.LocalMainWindow as Window;

            DataContext =
                _ucTreeMap.TreeMapVM =
                new WinTreeMapVM();

            _host.Child = _ucTreeMap;
            _ucTreeMap.TreeMapVM.GoTo(LocalTV.TopNode);
        }

        public WinTreeMap_MUI()
        {
            InitializeComponent();

            Observable.FromEventPattern(this, "SizeChanged")
                .Subscribe(args => { if (null != _ucTreeMap) _ucTreeMap.ClearSelection(); });

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .Subscribe(args => { if (null != _ucTreeMap) _ucTreeMap.TreeMapVM.LostMouseCapture(); });
        }

        UC_TreeMap
            _ucTreeMap = null;
    }
}
