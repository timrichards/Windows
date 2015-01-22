using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;

namespace DoubleFile
{
    partial class SaveDirListings : FileParse
    {
        readonly SaveDirListingsStatusDelegate m_statusCallback = null;
        readonly Action m_doneCallback = null;
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SaveDirListing> m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
        IEnumerable<LVitem_ProjectVM> m_list_LVitem_VolumeVM = null;

        internal int FilesWritten { get; set; }

        internal static bool IsGoodDriveSyntax(string strDrive)
        {
            return ((strDrive.Length > 2) && char.IsLetter(strDrive[0]) && (strDrive.Substring(1, 2) == @":\"));
        }

        internal SaveDirListings(IEnumerable<LVitem_ProjectVM> list_LVitem_VolumeVM,
            SaveDirListingsStatusDelegate statusCallback,
            Action doneCallback)
        {
            m_list_LVitem_VolumeVM = list_LVitem_VolumeVM;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        void Go()
        {
            UtilProject.WriteLine();
            UtilProject.WriteLine("Saving directory listings.");

            DateTime dtStart = DateTime.Now;

            foreach (LVitem_ProjectVM volStrings in m_list_LVitem_VolumeVM)
            {
                if (false == volStrings.WouldSave)
                {
                    continue;
                }

                m_statusCallback(volStrings.SourcePath, "Saving...");
                m_cbagWorkers.Add(new SaveDirListing(volStrings, m_statusCallback).DoThreadFactory());
            }

            foreach (SaveDirListing worker in m_cbagWorkers)
            {
                worker.Join();
            }

            UtilProject.WriteLine(string.Format("Finished saving directory listings in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (m_bThreadAbort || GlobalData.Instance.FormAnalysis_DirList_Closing)
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
}
