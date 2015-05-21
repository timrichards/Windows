using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace DoubleFile
{
    public abstract class LocalUserControlBase : UserControl, IContent
    {
        public void OnFragmentNavigation(FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(NavigationEventArgs e) { LocalDispose_WindowClosed(); }
        public void OnNavigatedTo(NavigationEventArgs e) { MainWindow.CurrentPage = this; LocalNavigatedTo(); }
        virtual protected void LocalNavigatedTo() { }
        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (false == WinProject.OKtoNavigate())
            {
                e.Cancel = true;
                return;
            }

            if (MainWindow.ExtraWindowFakeKey != "" + e.Source)
                return;

            var page = MainWindow.CurrentPage;
            var content = Activator.CreateInstance(page.GetType()) as LocalUserControlBase;

            content.CopyTag_NewWindow(new WeakReference(page.Tag));

            var window = new ExtraWindow
            {
                Content = content,
                Title = page.LocalTitle,
            }
                .Show();

            Observable.FromEventPattern(window, "Closed")
                .Subscribe(x => content.LocalDispose_WindowClosed());

            e.Cancel = true;
        }

        virtual protected void CopyTag_NewWindow(WeakReference weakReference)
        {
        }

        virtual protected void LocalDispose_WindowClosed()
        {
        }

        public string LocalTitle { get; set; }

        internal bool LocalIsClosed { get; private set; }
        internal bool LocalIsClosing { get; private set; }

        internal bool? LocalDialogResult { get; set; }
        internal void CloseIfSimulatingModal() { }
    }
}
