﻿using System;
using System.Collections.Generic;

namespace DoubleFile
{
    delegate void SaveDirListingsStatusDelegate(LVitem_ProjectVM lvItemProjectVM, string strText = null, bool bDone = false, double nProgress = double.NaN);

    partial class GlobalData
    {
        internal SaveDirListings m_saveDirListings = null;
    }

    class SaveListingsProcess
    {
        readonly GlobalData_Base gd = null;
        GlobalData gd_old = null;
        WinProgress m_winProgress = null;

        internal SaveListingsProcess(GlobalData_Base gd_in,
            LV_ProjectVM lvProjectVM)
        {
            gd = gd_in;
            gd_old = GlobalData.Instance;

            var listNicknames = new List<string>();
            var listSourcePaths = new List<string>();

            foreach (var volStrings in lvProjectVM.ItemsCast)
            {
                if (false == volStrings.WouldSave)
                {
                    continue;
                }

                listNicknames.Add(volStrings.Nickname);
                listSourcePaths.Add(volStrings.SourcePath);
            }

            if (listSourcePaths.Count > 0)
            {
                m_winProgress = new WinProgress();
                m_winProgress.InitProgress(listNicknames, listSourcePaths);
                m_winProgress.WindowTitle = "Saving Directory Listings";

                if ((gd_old.m_saveDirListings != null) && (gd_old.m_saveDirListings.IsAborted == false))
                {
                    MBox.Assert(0, false);
                    gd_old.m_saveDirListings.EndThread();
                }

                (gd_old.m_saveDirListings = new SaveDirListings(gd,
                    lvProjectVM,
                    SaveDirListingsStatusCallback,
                    SaveDirListingsDoneCallback)).DoThreadFactory();
                m_winProgress.ShowDialog();
            }
        }

        internal void SaveDirListingsStatusCallback(LVitem_ProjectVM lvItemProjectVM, string strText = null, bool bDone = false, double nProgress = double.NaN)
        {
            UtilProject.CheckAndInvoke(new Action(() =>
            {
                if (gd.WindowClosed || (gd_old.m_saveDirListings == null) || gd_old.m_saveDirListings.IsAborted)
                {
                    m_winProgress.Aborted = true;
                    m_winProgress.Close();
                    return;
                }

                if (nProgress >= 0)
                {
                    MBox.Assert(1306.7305, strText == null);
                    MBox.Assert(1306.7306, bDone == false);
                    m_winProgress.SetProgress(lvItemProjectVM.SourcePath, nProgress);
                }

                if (bDone)
                {
                    m_winProgress.SetCompleted(lvItemProjectVM.SourcePath);
                    lvItemProjectVM.Status = FileParse.ksSaved;

                    lock (gd_old.m_saveDirListings)
                    {
                        ++gd_old.m_saveDirListings.FilesWritten;
                    }
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
                MBox.ShowDialog("Completed. " + nFilesWritten + " file" + (nFilesWritten != 1 ? "s" : "") + " written.", "Save Directory Listings");
            }));
        }
    }
}
