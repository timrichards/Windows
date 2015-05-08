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
    public partial class WinTreeMap_MUI : IContent
    {
        public void OnFragmentNavigation(FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(NavigationEventArgs e) { }
        public void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((null == App.LVprojectVM) ||
                (App.LVprojectVM == _lvProjectVM) ||
                (App.LVprojectVM.LocalEquals(_lvProjectVM)))
            {
                return;
            }

            if (null != LocalTV.Instance)
                LocalTV.LocalDispose();

            App.FileDictionary.Dispose();
            App.FileDictionary = new FileDictionary();

            if (null != _ucTreeMap)
                _ucTreeMap.Dispose();

            _ucTreeMap = new UC_TreeMap();
            _ucTreeMap.LocalOwner = App.LocalMainWindow as Window;

            DataContext =
                _ucTreeMap.TreeMapVM =
                new WinTreeMapVM();

            _host.Child = _ucTreeMap;
            _lvProjectVM = new LV_ProjectVM(App.LVprojectVM);

            if (_lvProjectVM.Items.IsEmpty())
                return;

            LocalTV.FactoryCreate(_lvProjectVM);
            _ucTreeMap.TreeMapVM.TreeNodeCallback(LocalTV.TopNode);
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e) { }

        internal WinTreeMap_MUI()
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
