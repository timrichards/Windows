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
        GlobalData gd = null;
        WinProgress m_winProgress = null;

        internal SaveListingsProcess(IEnumerable<LVitem_ProjectVM> list_lvVolStrings)
        {
            gd = GlobalData.GetInstance();

            var listNicknames = new List<string>();
            var listSourcePaths = new List<string>();

            foreach (LVitem_ProjectVM volStrings in list_lvVolStrings)
            {
                if (SaveDirListings.WontSave(volStrings))
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

                if ((gd.m_saveDirListings != null) && (gd.m_saveDirListings.IsAborted == false))
                {
                    MBox.Assert(0, false);
                    gd.m_saveDirListings.EndThread();
                }

                (gd.m_saveDirListings = new SaveDirListings(list_lvVolStrings,
                    SaveDirListingsStatusCallback, SaveDirListingsDoneCallback)).DoThreadFactory();
                m_winProgress.ShowDialog();
            }
        }

        internal void SaveDirListingsStatusCallback(string strPath, string strText = null, bool bDone = false, double nProgress = double.NaN)
        {
            if (GlobalData.AppExit || (gd.m_saveDirListings == null) || gd.m_saveDirListings.IsAborted)
            {
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

                lock (gd.m_saveDirListings)
                {
                    ++gd.m_saveDirListings.FilesWritten;
                }
            }
        }

        internal void SaveDirListingsDoneCallback()
        {
            if (GlobalData.AppExit || (gd.m_saveDirListings == null) || gd.m_saveDirListings.IsAborted)
            {
                return;
            }

            if (GlobalData.static_TopWindow.Dispatcher.CheckAccess() == false) { GlobalData.static_TopWindow.Dispatcher.Invoke(SaveDirListingsDoneCallback); return; }

            if (gd.m_saveDirListings.FilesWritten > 0)
            {
                gd.RestartTreeTimer();
            }

            int nFilesWritten = gd.m_saveDirListings.FilesWritten;

            gd.m_saveDirListings = null;   // has to precede messagebox
            MBox.ShowDialog("Completed. " + nFilesWritten + " file" + (nFilesWritten != 1 ? "s" : "") + " written.", "Save Directory Listings");
        }
    }
}
