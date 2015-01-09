using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;

namespace DoubleFile
{
    delegate void SaveDirListingsStatusDelegate(string strPath, string strText = null, bool bDone = false, double nProgress = double.NaN);

    partial class SaveDirListings : FileParse
    {
        readonly SaveDirListingsStatusDelegate m_statusCallback = null;
        readonly Action m_doneCallback = null;
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SaveDirListing> m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
        IEnumerable<LVitem_VolumeVM> m_list_LVitem_VolumeVM = null;

        internal int FilesWritten { get; set; }

        internal static bool IsGoodDriveSyntax(string strDrive)
        {
            return ((strDrive.Length > 2) && char.IsLetter(strDrive[0]) && (strDrive.Substring(1, 2) == @":\"));
        }

        internal SaveDirListings(IEnumerable<LVitem_VolumeVM> list_LVitem_VolumeVM,
            SaveDirListingsStatusDelegate statusCallback,
            Action doneCallback)
        {
            m_list_LVitem_VolumeVM = list_LVitem_VolumeVM;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        internal static bool WontSave(LVitem_VolumeVM volStrings)
        {
            return ((mSTRusingFile + mSTRsaved + mSTRcantSave).Contains(volStrings.Status));
        }

        void Go()
        {
            Utilities.WriteLine();
            Utilities.WriteLine("Saving directory listings.");

            DateTime dtStart = DateTime.Now;

            foreach (LVitem_VolumeVM volStrings in m_list_LVitem_VolumeVM)
            {
                if (WontSave(volStrings))
                {
                    continue;
                }

                m_statusCallback(volStrings.Path, "Saving...");
                m_cbagWorkers.Add(new SaveDirListing(volStrings, m_statusCallback).DoThreadFactory());
            }

            foreach (SaveDirListing worker in m_cbagWorkers)
            {
                worker.Join();
            }

            Utilities.WriteLine(string.Format("Finished saving directory listings in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (m_bThreadAbort || GlobalData.AppExit)
            {
                return;
            }

            m_doneCallback();
        }

        internal void EndThread()
        {
            foreach (SaveDirListing worker in m_cbagWorkers)
            {
                worker.Abort();
            }

            m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
            m_bThreadAbort = true;
            m_thread = null;
        }

        internal void DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        internal bool IsAborted { get { return m_bThreadAbort; } }
    }

    partial class GlobalData
    {
        internal SaveDirListings m_saveDirListings = null;

        internal void ClearMem_SaveDirListings()
        {
            MBox.Assert(1306.73045, m_saveDirListings == null);

            m_saveDirListings = null;
        }

        internal bool DoSaveDirListings(IEnumerable<LVitem_VolumeVM> lvItems, SaveDirListingsStatusDelegate statusCallback, Action doneCallback)
        {
            if (m_saveDirListings != null)
            {
                MBox.ShowDialog("Already in progress.", "Save Directory Listings");
                return false;
            }

            UList<LVitem_VolumeVM> list_LVitem_VolumeVM = new UList<LVitem_VolumeVM>();

            foreach (LVitem_VolumeVM lvItem in lvItems)
            {
                list_LVitem_VolumeVM.Add(lvItem);
            }

            (m_saveDirListings = new SaveDirListings(list_LVitem_VolumeVM, statusCallback, doneCallback)).DoThreadFactory();
            return true;
        }
    }
}
