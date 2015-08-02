using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Linq;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DoubleFile
{
    public abstract class LocalUserControlBase : UserControl, IContent
    {
        public string LocalTitle { get; set; }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            _strFragment = e.Fragment;
            LocalFragmentNavigation(_strFragment);
        }
        virtual protected void LocalFragmentNavigation(string strFragment) { }
        string _strFragment = null;

        public void OnNavigatedFrom(NavigationEventArgs e) => LocalNavigatedFrom();
        virtual protected void LocalNavigatedFrom() { }

        public void OnNavigatedTo(NavigationEventArgs e)
        {
            Statics.CurrentPage = this;
            LocalNavigatedTo();
        }
        virtual protected void LocalNavigatedTo() { }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            var strSource = "" + e.Source;

            if ("/Introduction.xaml" == strSource)
                return;

            if ("/WinProject/WinProject.xaml" == strSource)
                return;

            // use-case: VolTreeMap project shares assembly
            if (null != MainWindow.WithMainWindow(w => w))
            {
                var bSaveListings = false;

                if (MainWindow.SaveListingsFakeKey == strSource)
                    bSaveListings = true;

                if ((false == WinProject.OKtoNavigate_BuildExplorer(bSaveListings)) ||
                    bSaveListings)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (MainWindow.AdvancedFakeKey == strSource)
            {
                CalculateAverageFileLength();
                e.Cancel = true;
                return;
            }

            if (MainWindow.ExtraWindowFakeKey != strSource)
                return;

            var page = Statics.CurrentPage;
            var content = (LocalUserControlBase)Activator.CreateInstance(page.GetType());

            var window = new ExtraWindow
            {
                Content = content,
                Title = page.LocalTitle,
            };

            Observable.FromEventPattern(window, "Loaded")
                .LocalSubscribe(99746, x =>
            {
                content.LocalNavigatedTo();
                content.LocalFragmentNavigation(_strFragment);
                //content.CopyTag_NewWindow(new WeakReference(page.Tag));
            });

            Observable.FromEventPattern(window, "Closed")
                .LocalSubscribe(99745, x =>
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
                    Statics.LVprojectVM.ItemsCast
                    .Where(lvItem => lvItem.CanLoad), lvItem =>
                {
                    var lsFileLengths_ = new List<decimal>();

                    foreach (var nLength in
                        lvItem.ListingFile
                        .ReadLines()
                        .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                        .Select(strLine => strLine.Split('\t'))
                        .Where(asLine => FileParse.knColLength < asLine.Length)
                        .Select(asLine => (decimal)("" + asLine[FileParse.knColLength]).ToUlong()))
                    {
                        lsFileLengths_.Add(nLength);
                    }

                    try
                    {
                        lsFileLengths.Add(lsFileLengths_.Average());
                    }
                    catch (OutOfMemoryException) { }
                });

                decimal nAverage = 0;

                try
                {
                    nAverage = lsFileLengths.Average();
                }
                catch (OutOfMemoryException) { }

                Util.WriteLine("Average file length = " + Util.FormatSize((ulong)nAverage, true));
            });
        }
    }
}
