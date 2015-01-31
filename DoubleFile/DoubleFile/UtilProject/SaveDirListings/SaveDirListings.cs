using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;

namespace DoubleFile
{
    partial class SaveDirListings : FileParse
    {
        internal int FilesWritten { get; set; }

        internal static bool IsGoodDriveSyntax(string strDrive)
        {
            return ((strDrive.Length > 2) && char.IsLetter(strDrive[0]) && (strDrive.Substring(1, 2) == @":\"));
        }

        internal SaveDirListings(GlobalData_Base gd_in,
            LV_ProjectVM lvProjectVM,
            SaveDirListingsStatusDelegate statusCallback,
            Action doneCallback)
        {
            gd = gd_in;
            m_lvProjectVM = lvProjectVM;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
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

        void Go()
        {
            UtilProject.WriteLine();
            UtilProject.WriteLine("Saving directory listings.");

            DateTime dtStart = DateTime.Now;

            foreach (var lvItemProjectVM in m_lvProjectVM.ItemsCast)
            {
                if (false == lvItemProjectVM.WouldSave)
                {
                    continue;
                }

                m_cbagWorkers.Add(new SaveDirListing(gd, lvItemProjectVM, m_statusCallback).DoThreadFactory());
            }

            foreach (SaveDirListing worker in m_cbagWorkers)
            {
                worker.Join();
            }

            UtilProject.WriteLine(string.Format("Finished saving directory listings in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (m_bThreadAbort || gd.WindowClosed)
            {
                return;
            }

            m_doneCallback();
        }

        readonly GlobalData_Base gd = null;
        readonly SaveDirListingsStatusDelegate m_statusCallback = null;
        readonly Action m_doneCallback = null;
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SaveDirListing> m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
        LV_ProjectVM m_lvProjectVM = null;
    }
}
