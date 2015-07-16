using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DoubleFile
{
    class SaveListingsProcess : IWinProgressClosing, ISaveDirListingsStatus
    {
        static internal void Go(LV_ProjectVM lvProjectVM)
        {
            new SaveListingsProcess(lvProjectVM);
        }

        SaveListingsProcess(LV_ProjectVM lvProjectVM)
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

            if (0 == listSourcePaths.Count)
                return;

            if ((null != App.SaveDirListings) &&
                (false == App.SaveDirListings.IsAborted))
            {
                MBoxStatic.Assert(99940, false);
                App.SaveDirListings.EndThread();
            }

            (new WinProgress(listNicknames, listSourcePaths, x =>
                App.SaveDirListings =
                    new SaveDirListings(lvProjectVM, this)
                    .DoThreadFactory())
            {
                Title = "Saving Directory Listings",
                WindowClosingCallback = new WeakReference<IWinProgressClosing>(this),
            })
                .AllowSubsequentProcess()
                .ShowDialog();
        }

        void ISaveDirListingsStatus.Status(LVitem_ProjectVM lvItemProjectVM,
            string strError, bool bDone, double nProgress)
        {
            var sdl = App.SaveDirListings;
            var winProgress = WinProgress.WithWinProgress(w => w);

            if (winProgress.LocalIsClosed)
            {
                MBoxStatic.Assert(99804,
                    (null == Application.Current) || Application.Current.Dispatcher.HasShutdownStarted ||
                    (null == sdl) ||
                    sdl.IsAborted);

                return;
            }

            if ((null == Application.Current) || Application.Current.Dispatcher.HasShutdownStarted ||
                sdl.IsAborted)
            {
                winProgress.SetAborted();

                if (false == _bKeepShowingError)
                    winProgress.Close();
                    
                return;
            }

            if (null != strError)
            {
                lvItemProjectVM.Status = FileParse.ksError;
                _bKeepShowingError = true;
                winProgress.SetError(lvItemProjectVM.SourcePath, strError);
            }
            else if (bDone)
            {
                lvItemProjectVM.SetSaved();
                Interlocked.Increment(ref sdl.FilesWritten);
                winProgress.SetCompleted(lvItemProjectVM.SourcePath);
            }
            else if (0 <= nProgress)
            {
                winProgress.SetProgress(lvItemProjectVM.SourcePath, nProgress);
            }
        }

        void ISaveDirListingsStatus.Done()
        {
            App.SaveDirListings = null;
        }

        bool IWinProgressClosing.ConfirmClose()
        {
            if ((null == App.SaveDirListings) ||
                App.SaveDirListings.IsAborted)
            {
                return true;
            }

            if (MessageBoxResult.Yes != MBoxStatic.ShowDialog("Do you want to cancel?", "Saving Directory Listings", MessageBoxButton.YesNo,
                WinProgress.WithWinProgress(w => w)))
                return false;

            if (null != App.SaveDirListings)
                App.SaveDirListings.EndThread();

            return true;
        }

        bool
            _bKeepShowingError = false;
    }
}
