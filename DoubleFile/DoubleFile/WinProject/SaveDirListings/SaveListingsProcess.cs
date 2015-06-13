﻿using System;
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

            if (listSourcePaths.IsEmpty())
                return;

            if ((null != App.SaveDirListings) &&
                (false == App.SaveDirListings.IsAborted))
            {
                MBoxStatic.Assert(99940, false);
                App.SaveDirListings.EndThread();
            }

            App.SaveDirListings =
                new SaveDirListings(lvProjectVM, this)
                .DoThreadFactory();

            (_winProgress = new WinProgress(listNicknames, listSourcePaths)
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

            if (App.LocalExit ||
                (null == sdl) ||
                sdl.IsAborted)
            {
                _winProgress.SetAborted();

                if (false == _bKeepShowingError)
                    _winProgress.Close();
                    
                return;
            }

            if (null != strError)
            {
                lvItemProjectVM.Status = FileParse.ksError;
                _bKeepShowingError = true;
                _winProgress.SetError(lvItemProjectVM.SourcePath, strError);
            }
            else if (bDone)
            {
                lvItemProjectVM.SetSaved();
                Interlocked.Increment(ref sdl.FilesWritten);
                _winProgress.SetCompleted(lvItemProjectVM.SourcePath);
            }
            else if (0 <= nProgress)
            {
                _winProgress.SetProgress(lvItemProjectVM.SourcePath, nProgress);
            }
        }

        void ISaveDirListingsStatus.Done()
        {
            App.SaveDirListings = null;
        }

        bool IWinProgressClosing.ConfirmClose()
        {
            if (null == App.SaveDirListings)
                return true;

            if (App.SaveDirListings.IsAborted)
                return true;

            if (MBoxStatic.ShowDialog("Do you want to cancel?", "Saving Directory Listings",
                MessageBoxButton.YesNo,
                _winProgress) ==
                MessageBoxResult.Yes)
            {
                if (null != App.SaveDirListings)
                    App.SaveDirListings.EndThread();

                return true;
            }

            return false;
        }

        WinProgress
            _winProgress = null;
        bool
            _bKeepShowingError = false;
    }
}
