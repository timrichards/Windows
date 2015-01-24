using System;
using System.Collections.Generic;

namespace DoubleFile
{
    delegate void SaveDirListingsStatusDelegate(string strPath, string strText = null, bool bDone = false, double nProgress = double.NaN);

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
            IEnumerable<LVitem_ProjectVM> listLVvolStrings)
        {
            gd = gd_in;
            gd_old = GlobalData.Instance;

            var listNicknames = new List<string>();
            var listSourcePaths = new List<string>();

            foreach (LVitem_ProjectVM volStrings in listLVvolStrings)
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
                    listLVvolStrings,
                    SaveDirListingsStatusCallback,
                    SaveDirListingsDoneCallback)).DoThreadFactory();
                m_winProgress.ShowDialog();
            }
        }

        internal void SaveDirListingsStatusCallback(string strPath, string strText = null, bool bDone = false, double nProgress = double.NaN)
        {
            if (gd.WindowClosed || (gd_old.m_saveDirListings == null) || gd_old.m_saveDirListings.IsAborted)
            {
                UtilProject.CheckAndInvoke(new Action(() =>
                {
                    m_winProgress.Aborted = true;
                    m_winProgress.Close();
                }));
                
                return;
            }

            if (GlobalData.static_TopWindow.Dispatcher.CheckAccess() == false) { GlobalData.static_TopWindow.Dispatcher.Invoke(new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback), new object[] { strPath, strText, bDone, nProgress }); return; }

            if (nProgress >= 0)
            {
                MBox.Assert(1306.7305, strText == null);
                MBox.Assert(1306.7306, bDone == false);
                m_winProgress.SetProgress(strPath, nProgress);
            }

            if (bDone)
            {
                m_winProgress.SetCompleted(strPath);

                lock (gd_old.m_saveDirListings)
                {
                    ++gd_old.m_saveDirListings.FilesWritten;
                }
            }
        }

        internal void SaveDirListingsDoneCallback()
        {
            if (gd.WindowClosed || (gd_old.m_saveDirListings == null) || gd_old.m_saveDirListings.IsAborted)
            {
                return;
            }

            if (GlobalData.static_TopWindow.Dispatcher.CheckAccess() == false) { GlobalData.static_TopWindow.Dispatcher.Invoke(SaveDirListingsDoneCallback); return; }

            if (gd_old.m_saveDirListings.FilesWritten > 0)
            {
                gd_old.RestartTreeTimer();
            }

            int nFilesWritten = gd_old.m_saveDirListings.FilesWritten;

            gd_old.m_saveDirListings = null;   // has to precede messagebox
            MBox.ShowDialog("Completed. " + nFilesWritten + " file" + (nFilesWritten != 1 ? "s" : "") + " written.", "Save Directory Listings");
        }
    }
}
