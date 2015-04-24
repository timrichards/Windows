using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DoubleFile
{
    delegate void SaveDirListingsStatusDelegate(
        LVitem_ProjectVM lvItemProjectVM,
        string strError = null,
        bool bDone = false,
        double nProgress = double.NaN);

    class SaveListingsProcess
    {
        internal SaveListingsProcess(LV_ProjectVM lvProjectVM)
        {
            var listNicknames = new List<string>();
            var listSourcePaths = new List<string>();

            foreach (var volStrings
                in lvProjectVM.ItemsCast
                .Where(volStrings => volStrings.WouldSave))
            {
                listNicknames.Add(volStrings.Nickname);
                listSourcePaths.Add(volStrings.SourcePath);
            }

            if (listSourcePaths.IsEmpty())
                return;

            _winProgress = new WinProgress(listNicknames, listSourcePaths)
            {
                Title = "Saving Directory Listings",
                WindowClosingCallback = () =>
                {
                    if (null == MainWindow.SaveDirListings)
                        return true;

                    if (MainWindow.SaveDirListings.IsAborted)
                        return true;

                    if (MBoxStatic.ShowDialog("Do you want to cancel?", "Saving Directory Listings",
                        MessageBoxButton.YesNo,
                        _winProgress) ==
                        MessageBoxResult.Yes)
                    {
                        MainWindow.SaveDirListings.EndThread();
                        return true;
                    }

                    return false;
                }
            };

            if ((null != MainWindow.SaveDirListings) &&
                (false == MainWindow.SaveDirListings.IsAborted))
            {
                MBoxStatic.Assert(99940, false);
                MainWindow.SaveDirListings.EndThread();
            }

            (MainWindow.SaveDirListings = (new SaveDirListings(
                lvProjectVM,
                SaveDirListingsStatusCallback,
                SaveDirListingsDoneCallback))
            ).DoThreadFactory();

            _winProgress.ShowDialog();
        }

        internal void SaveDirListingsStatusCallback(LVitem_ProjectVM lvItemProjectVM,
            string strError = null, bool bDone = false, double nProgress = double.NaN)
        {
            UtilProject.UIthread(() =>
            {
                var sdl = MainWindow.SaveDirListings;

                if (App.LocalExit ||
                    (null == sdl) ||
                    sdl.IsAborted)
                {
                    _winProgress.Aborted = true;

                    if (false == _bKeepShowingError)
                        _winProgress.Close();
                    
                    return;
                }

                if (null != strError)
                {
                    _winProgress.SetError(lvItemProjectVM.SourcePath, strError);
                    lvItemProjectVM.Status = FileParse.ksError;
                    _bKeepShowingError = true;
                }
                else if (bDone)
                {
                    _winProgress.SetCompleted(lvItemProjectVM.SourcePath);
                    lvItemProjectVM.SetSaved();
                    Interlocked.Increment(ref sdl.FilesWritten);
                }
                else if (nProgress >= 0)
                {
                    _winProgress.SetProgress(lvItemProjectVM.SourcePath, nProgress);
                }
            });
        }

        internal void SaveDirListingsDoneCallback()
        {
            MainWindow.SaveDirListings = null;
        }

        WinProgress
            _winProgress = null;
        bool
            _bKeepShowingError = false;
    }
}
