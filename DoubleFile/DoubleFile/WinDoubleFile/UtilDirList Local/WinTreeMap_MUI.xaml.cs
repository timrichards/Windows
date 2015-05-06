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
        internal LV_ProjectVM LVprojectVM { get; set; }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(NavigationEventArgs e) { }
        public void OnNavigatedTo(NavigationEventArgs e)
        {
            Observable.FromEventPattern(this, "SizeChanged")
                .Subscribe(args => form_ucTreeMap.ClearSelection());

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .Subscribe(args => form_ucTreeMap.TreeMapVM.LostMouseCapture());

            //Observable.FromEventPattern(this, "Unloaded")
            //    .Subscribe(args => LocalDispose());

            ResizeMode = ResizeMode.CanResize;
    //        form_ucTreeMap.LocalOwner = this;

            DataContext =
                form_ucTreeMap.TreeMapVM =
                new WinTreeMapVM();

            LocalTV.FactoryCreate(App.LVprojectVM);
            form_ucTreeMap.TreeMapVM.TreeNode = LocalTV.TopNode;
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e) { }

        internal WinTreeMap_MUI()
        {
            InitializeComponent();
        }

        private void LocalDispose()
        {
            _host.Dispose();
            form_ucTreeMap.Dispose();
        }
    }
}
