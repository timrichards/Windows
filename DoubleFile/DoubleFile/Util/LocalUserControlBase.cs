using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DoubleFile
{
    public abstract class LocalUserControlBase : UserControl, IContent
    {
        public string LocalTitle { get; set; }

        internal bool CantDupeThisUsercontrol = false;

        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            _strFragment = e.Fragment;
            LocalFragmentNavigation(_strFragment);
        }
        virtual protected void LocalFragmentNavigation(string strFragment) { }
        string _strFragment = null;

        public void OnNavigatedFrom(NavigationEventArgs e) { if (false == _bNavigatedFromAlready) LocalNavigatedFrom(); }
        virtual protected void LocalNavigatedFrom() { }
        bool _bNavigatedFromAlready = false;

        public void OnNavigatedTo(NavigationEventArgs e)
        {
            Statics.CurrentPage = this;

            if (false == _bNavigatedFromAlready)
                LocalNavigatedTo();
        }
        virtual protected void LocalNavigatedTo() { }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            var strSource = "" + e.Source;

            if ("/Introduction.xaml" == strSource)
                return;

            if ("/UC_Project/UC_Project.xaml" == strSource)
                return;

            // use-case: VolTreeMap project shares assembly
            if (null != MainWindow.WithMainWindow(w => w))
            {
                var bSaveListings = false;

                if (MainWindow.SaveListingsFakeKey == strSource)
                    bSaveListings = true;

                if ((false == UC_Project.OKtoNavigate_BuildExplorer(bSaveListings)) ||
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

            var content = (LocalUserControlBase)Activator.CreateInstance(GetType());

            var window = new ExtraWindow
            {
                Content = content,
                Title = LocalTitle,
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

                if (false == CantDupeThisUsercontrol)
                    return;

                Content = ((LocalUserControlBase)Activator.CreateInstance(GetType())).Content;

                if (this != Statics.CurrentPage)
                    return;     // from lambda

                _bNavigatedFromAlready = false;
                LocalNavigatedTo();
                LocalFragmentNavigation(_strFragment);
                MainWindow.UpdateTitleLinks();
            });

            if (CantDupeThisUsercontrol)
            {
                LocalNavigatedFrom();
                _bNavigatedFromAlready = true;
                Content = new UC_ExtraWindow { WindowExtra = window };
                MainWindow.UpdateTitleLinks();
            }

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
                Util.ParallelForEach(99654,
                    Statics.LVprojectVM_Copy.ItemsCast
                    .Where(lvItem => lvItem.CanLoad), lvItem =>
                {
                    var lsFileLengths_ = new List<decimal>();

                    foreach (var nLength in
                        lvItem.ListingFile
                        .ReadLines(99644)
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
