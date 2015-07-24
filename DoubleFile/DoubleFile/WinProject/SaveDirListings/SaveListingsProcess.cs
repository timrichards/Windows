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

            if (false == (Statics.SaveDirListings?.IsAborted ?? true))
            {
                MBoxStatic.Assert(99940, false);
                Statics.SaveDirListings.EndThread();
            }

            (new WinProgress(listNicknames, listSourcePaths, x =>
                Statics.SaveDirListings =
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
            var sdl = Statics.SaveDirListings;
            var winProgress = WinProgress.WithWinProgress(w => w);

            if (winProgress.LocalIsClosed)
            {
                MBoxStatic.Assert(99804,
                    (Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    (sdl?.IsAborted ?? true));

                return;
            }

            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                sdl.IsAborted)
            {
                winProgress.Abort();

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
            Statics.SaveDirListings = null;
        }

        bool IWinProgressClosing.ConfirmClose()
        {
            if (Statics.SaveDirListings?.IsAborted ?? true)
                return true;

            if (MessageBoxResult.Yes !=
                MBoxStatic.ShowDialog("Do you want to cancel?", "Saving Directory Listings", MessageBoxButton.YesNo,
                WinProgress.WithWinProgress(w => w)))
                return false;

            Statics.SaveDirListings?.EndThread();
            return true;
        }

        bool
            _bKeepShowingError = false;
    }
}
