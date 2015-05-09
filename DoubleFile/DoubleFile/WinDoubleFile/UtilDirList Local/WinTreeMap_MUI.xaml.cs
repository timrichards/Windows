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
            if (false == WinProject_MUI.InitExplorer())
            {
                if ((null == App.LVprojectVM) ||
                    (App.LVprojectVM == _lvProjectVM) ||
                    (App.LVprojectVM.LocalEquals(_lvProjectVM)))
                {
                    return;
                }
            }

            _lvProjectVM = new LV_ProjectVM(App.LVprojectVM);

            if (null != _ucTreeMap)
                _ucTreeMap.Dispose();

            _ucTreeMap = new UC_TreeMap();
            _ucTreeMap.LocalOwner = App.LocalMainWindow as Window;

            DataContext =
                _ucTreeMap.TreeMapVM =
                new WinTreeMapVM();

            _host.Child = _ucTreeMap;

            if (_lvProjectVM.Items.IsEmpty())
                return;

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

        LV_ProjectVM
            _lvProjectVM = null;
        UC_TreeMap
            _ucTreeMap = null;
    }
}
