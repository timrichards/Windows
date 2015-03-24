using System;
using System.Collections.Generic;
using System.Linq;
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
            {
                return;
            }

            m_winProgress = new WinProgress();
            m_winProgress.InitProgress(listNicknames, listSourcePaths);
            m_winProgress.Title = "Saving Directory Listings";
            m_winProgress.WindowClosingCallback = () =>
            {
                if (MainWindow._saveDirListings == null)
                {
                    return true;
                }

                if (MainWindow._saveDirListings.IsAborted)
                {
                    return true;
                }

                if (MBoxStatic.ShowDialog("Do you want to cancel?", "Saving Directory Listings",
                    MessageBoxButton.YesNo,
                    m_winProgress) ==
                    MessageBoxResult.Yes)
                {
                    MainWindow._saveDirListings.EndThread();
                    return true;
                }

                return false;
            };

            if ((null != MainWindow._saveDirListings) &&
                (false == MainWindow._saveDirListings.IsAborted))
            {
                MBoxStatic.Assert(99940, false);
                MainWindow._saveDirListings.EndThread();
            }

            (MainWindow._saveDirListings = new SaveDirListings(
                lvProjectVM,
                SaveDirListingsStatusCallback,
                SaveDirListingsDoneCallback)).DoThreadFactory();
            m_winProgress.ShowDialog();
        }

        internal void SaveDirListingsStatusCallback(LVitem_ProjectVM lvItemProjectVM,
            string strError = null, bool bDone = false, double nProgress = double.NaN)
        {
            UtilProject.UIthread(() =>
            {
                if (App.LocalExit || (MainWindow._saveDirListings == null) || MainWindow._saveDirListings.IsAborted)
                {
                    m_winProgress.Aborted = true;

                    if (m_bKeepShowingError == false)
                    {
                        m_winProgress.Close();
                    }
                    
                    return;
                }

                if (strError != null)
                {
                    m_winProgress.SetError(lvItemProjectVM.SourcePath, strError);
                    lvItemProjectVM.Status = FileParse.ksError;
                    m_bKeepShowingError = true;
                }
                else if (bDone)
                {
                    m_winProgress.SetCompleted(lvItemProjectVM.SourcePath);
                    lvItemProjectVM.SetSaved();

                    lock (MainWindow._saveDirListings)
                    {
                        ++MainWindow._saveDirListings.FilesWritten;
                    }
                }
                else if (nProgress >= 0)
                {
                    m_winProgress.SetProgress(lvItemProjectVM.SourcePath, nProgress);
                }
            });
        }

        internal void SaveDirListingsDoneCallback()
        {
            MainWindow._saveDirListings = null;
        }

        WinProgress m_winProgress = null;
        bool m_bKeepShowingError = false;
    }
}
