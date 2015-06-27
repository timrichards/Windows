using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Linq;
using System.IO;
using System.Collections.Concurrent;

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
            if ("/Introduction.xaml" ==
                "" + e.Source)
            {
                return;
            }

            if ("/WinProject/WinProject.xaml" ==
                "" + e.Source)
            {
                return;
            }

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

            if (MainWindow.AdvancedFakeKey ==
                "" + e.Source)
            {
                CalculateAverageFileLength();
                e.Cancel = true;
                return;
            }

            if (MainWindow.ExtraWindowFakeKey !=
                "" + e.Source)
            {
                return;
            }

            var page = MainWindow.CurrentPage;
            var content = (LocalUserControlBase)Activator.CreateInstance(page.GetType());

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

        private void CalculateAverageFileLength()
        {
            var lsFileLengths = new ConcurrentBag<decimal>();

            Util.ThreadMake(() =>
            {
                Parallel.ForEach(
                    App.LVprojectVM.ItemsCast
                    .Where(lvItem => lvItem.CanLoad), lvItem =>
                {
                    foreach (var nLength in
                        File
                        .ReadLines(lvItem.ListingFile)
                        .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                        .Select(strLine => strLine.Split('\t'))
                        .Where(asLine => FileParse.knColLength < asLine.Length)
                        .Select(asLine => (decimal)("" + asLine[FileParse.knColLength]).ToUlong()))
                    {
                        lsFileLengths.Add(nLength);
                    }
                });

                Util.WriteLine("Average file length = " + Util.FormatSize((ulong)lsFileLengths.Average(), true));
            });
        }
    }
}
