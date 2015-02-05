using System;
using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    delegate void SaveDirListingsStatusDelegate(LVitem_ProjectVM lvItemProjectVM, string strError = null, bool bDone = false, double nProgress = double.NaN);

    partial class GlobalData
    {
        internal SaveDirListings m_saveDirListings = null;
    }

    class SaveListingsProcess
    {
        internal SaveListingsProcess(GlobalData_Base gd_in,
            LV_ProjectVM lvProjectVM)
        {
            gd = gd_in;
            gd_old = GlobalData.Instance;

            var listNicknames = new List<string>();
            var listSourcePaths = new List<string>();

            foreach (var volStrings in lvProjectVM.ItemsCast)
            {
                if (volStrings.WouldSave)
                {
                    listNicknames.Add(volStrings.Nickname);
                    listSourcePaths.Add(volStrings.SourcePath);
                }
            }

            if (listSourcePaths.Count > 0)
            {
                m_winProgress = new WinProgress();
                m_winProgress.InitProgress(listNicknames, listSourcePaths);
                m_winProgress.WindowTitle = "Saving Directory Listings";
                m_winProgress.WindowClosingCallback = (() =>
                {
                    if (gd_old.m_saveDirListings == null)
                    {
                        return true;
                    }

                    if (gd_old.m_saveDirListings.IsAborted)
                    {
                        return true;
                    }

                    if (MBoxStatic.ShowDialog("Do you want to cancel?", "Saving Directory Listings",
                        MessageBoxButton.YesNo) ==
                        MessageBoxResult.Yes)
                    {
                        gd_old.m_saveDirListings.EndThread();
                        return true;
                    }

                    return false;
                });

                if ((gd_old.m_saveDirListings != null) && (gd_old.m_saveDirListings.IsAborted == false))
                {
                    MBoxStatic.Assert(0, false);
                    gd_old.m_saveDirListings.EndThread();
                }

                (gd_old.m_saveDirListings = new SaveDirListings(gd,
                    lvProjectVM,
                    SaveDirListingsStatusCallback,
                    SaveDirListingsDoneCallback)).DoThreadFactory();
                m_winProgress.ShowDialog();
            }
        }

        internal void SaveDirListingsStatusCallback(LVitem_ProjectVM lvItemProjectVM, string strError = null, bool bDone = false, double nProgress = double.NaN)
        {
            UtilProject.CheckAndInvoke(new Action(() =>
            {
                if (gd.WindowClosed || (gd_old.m_saveDirListings == null) || gd_old.m_saveDirListings.IsAborted)
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

                    lock (gd_old.m_saveDirListings)
                    {
                        ++gd_old.m_saveDirListings.FilesWritten;
                    }
                }
                else if (nProgress >= 0)
                {
                    m_winProgress.SetProgress(lvItemProjectVM.SourcePath, nProgress);
                }
            }));
        }

        internal void SaveDirListingsDoneCallback()
        {
            if (gd.WindowClosed || (gd_old.m_saveDirListings == null) || gd_old.m_saveDirListings.IsAborted)
            {
                return;
            }

            UtilProject.CheckAndInvoke(new Action(() =>
            {
                if (gd_old.m_saveDirListings.FilesWritten > 0)
                {
                    gd_old.RestartTreeTimer();
                }

                int nFilesWritten = gd_old.m_saveDirListings.FilesWritten;

                gd_old.m_saveDirListings = null;   // has to precede messagebox
                MBoxStatic.ShowDialog("Completed. " + nFilesWritten + " file" + (nFilesWritten != 1 ? "s" : "") + " written.", "Save Directory Listings");
            }));
        }

        readonly GlobalData_Base gd = null;
        GlobalData gd_old = null;
        WinProgress m_winProgress = null;
        bool m_bKeepShowingError = false;
    }
}
