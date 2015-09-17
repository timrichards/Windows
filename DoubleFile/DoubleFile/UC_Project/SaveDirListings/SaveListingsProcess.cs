using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DoubleFile
{
    class SaveListingsProcess : IProgressOverlayClosing, ISaveDirListingsStatus
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
                Util.Assert(99940, false);
                Statics.SaveDirListings.EndThread();
            }

            (new ProgressOverlay(listNicknames, listSourcePaths, x =>
                Statics.SaveDirListings =
                    new SaveDirListings(lvProjectVM, this)
                    .DoThreadFactory())
            {
                Title = "Saving Directory Listings",
                WindowClosingCallback = new WeakReference<IProgressOverlayClosing>(this),
            })
                .AllowSubsequentProcess()
                .ShowOverlay();
        }

        void ISaveDirListingsStatus.Status(LVitem_ProjectVM lvItemProjectVM,
            string strError, bool bDone, double nProgress)
        {
            var sdl = Statics.SaveDirListings;
            var winProgress = ProgressOverlay.WithProgressOverlay(w => w);

            if (winProgress.LocalIsClosed)
            {
                Util.Assert(99804,
                    (Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                    (sdl?.IsAborted ?? true));

                return;
            }

            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                sdl.IsAborted)
            {
                if (false == _bKeepShowingError)
                    ProgressOverlay.CloseForced();
                    
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

        bool IProgressOverlayClosing.ConfirmClose()
        {
            if (Statics.SaveDirListings?.IsAborted ?? true)
                return true;

            if (MessageBoxResult.Yes !=
                MBoxStatic.ShowOverlay("Do you want to cancel?", "Saving Directory Listings", MessageBoxButton.YesNo,
                ProgressOverlay.WithProgressOverlay(w => w)))
                return false;

            Statics.SaveDirListings?.EndThread();
            return true;
        }

        bool
            _bKeepShowingError = false;
    }
}
