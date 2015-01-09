using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        WinSaveInProgress m_winProgress = null;
        System.Windows.Window m_app = null;

        internal SaveListingsProcess(IEnumerable<LVitem_VolumeVM> list_lvVolStrings)
        {
            gd = GlobalData.GetInstance();
            m_app = GlobalData.static_wpfWin;

            var listNicknames = new List<string>();
            var listPaths = new List<string>();

            foreach (LVitem_VolumeVM volStrings in list_lvVolStrings)
            {
                if (SaveDirListings.WontSave(volStrings))
                {
                    continue;
                }

                listNicknames.Add(volStrings.VolumeName);
                listPaths.Add(volStrings.Path);
            }

            if (listPaths.Count > 0)
            {
                m_winProgress = new WinSaveInProgress();
                m_winProgress.InitProgress(listNicknames, listPaths);

                if ((gd.m_saveDirListings != null) && (gd.m_saveDirListings.IsAborted == false))
                {
                    MBox.Assert(0, false);
                    gd.m_saveDirListings.EndThread();
                }

                (gd.m_saveDirListings = new SaveDirListings(list_lvVolStrings,
                    SaveDirListingsStatusCallback, SaveDirListingsDoneCallback)).DoThreadFactory();
                m_winProgress.ShowDialog(m_app);
            }
        }

        internal void SaveDirListingsStatusCallback(string strPath, string strText = null, bool bDone = false, double nProgress = double.NaN)
        {
            if (GlobalData.AppExit || (gd.m_saveDirListings == null) || gd.m_saveDirListings.IsAborted)
            {
                return;
            }

            if (m_app.Dispatcher.CheckAccess() == false) { m_app.Dispatcher.Invoke(new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback), new object[] { strPath, strText, bDone, nProgress }); return; }

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

            if (m_app.Dispatcher.CheckAccess() == false) { m_app.Dispatcher.Invoke(new Action(SaveDirListingsDoneCallback)); return; }

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
