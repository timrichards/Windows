using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace DoubleFile
{
    public abstract class LocalUserControlBase : UserControl, IContent
    {
        public string LocalTitle { get; set; }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(NavigationEventArgs e) { LocalNavigatedFrom(); }
        virtual protected void LocalNavigatedFrom() { }
        public void OnNavigatedTo(NavigationEventArgs e) { MainWindow.CurrentPage = this; LocalNavigatedTo(); }
        virtual protected void LocalNavigatedTo() { }
        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            var bSaveListings = false;

            if (MainWindow.SaveListingsFakeKey ==
                "" + e.Source)
            {
                bSaveListings = true;
            }

            if ((false == WinProject.OKtoNavigate_BuildExplorer(bSaveListings)) ||
                bSaveListings)
            {
                e.Cancel = true;
                return;
            }

            if (MainWindow.ExtraWindowFakeKey !=
                "" + e.Source)
            {
                return;
            }

            var page = MainWindow.CurrentPage;
            var content = Activator.CreateInstance(page.GetType()) as LocalUserControlBase;

            var window = new ExtraWindow
            {
                Content = content,
                Title = page.LocalTitle,
            };

            Observable.FromEventPattern(window, "Loaded")
                .Subscribe(x =>
            {
                content.LocalNavigatedTo();
                //content.CopyTag_NewWindow(new WeakReference(page.Tag));
            });

            Observable.FromEventPattern(window, "Closed")
                .Subscribe(x =>
            {
                content.LocalNavigatedFrom();
                content.LocalWindowClosed();
            });

            window.Show();
            e.Cancel = true;
        }

        //virtual protected void CopyTag_NewWindow(WeakReference weakReference)
        //{
        //}

        virtual protected void LocalWindowClosed()
        {
        }
    }
}
