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
        public void OnNavigatedFrom(NavigationEventArgs e)
        {
       //     _host.Dispose();
       //     form_ucTreeMap.Dispose();
        }

        public void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((null == App.LVprojectVM) ||
                (App.LVprojectVM == _lvProjectVM) ||
                (App.LVprojectVM.LocalEquals(_lvProjectVM)))
            {
                return;
            }

            _lvProjectVM = new LV_ProjectVM(App.LVprojectVM);

            if (null != LocalTV.Instance)
                LocalTV.LocalDispose();

            LocalTV.FactoryCreate(_lvProjectVM);
            form_ucTreeMap.TreeMapVM.TreeNodeCallback(LocalTV.TopNode);
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e) { }

        internal WinTreeMap_MUI()
        {
            InitializeComponent();

            Observable.FromEventPattern(this, "SizeChanged")
                .Subscribe(args => form_ucTreeMap.ClearSelection());

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .Subscribe(args => form_ucTreeMap.TreeMapVM.LostMouseCapture());

            DataContext =
                form_ucTreeMap.TreeMapVM =
                new WinTreeMapVM();

    //        form_ucTreeMap.LocalOwner = this;
        }

        LV_ProjectVM
            _lvProjectVM = null;
    }
}
