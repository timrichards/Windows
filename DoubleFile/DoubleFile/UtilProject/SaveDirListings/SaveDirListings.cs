﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;

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
            IsAborted = false;
            gd = gd_in;
            m_lvProjectVM = lvProjectVM;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        internal void EndThread()
        {
            foreach (var worker in m_cbagWorkers)
            {
                worker.Abort();
            }

            m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
            IsAborted = true;
            m_thread = null;
        }

        internal void DoThreadFactory()
        {
            m_thread = new Thread(Go) {IsBackground = true};
            m_thread.Start();
        }

        internal bool IsAborted { get; private set; }

        void Go()
        {
            UtilProject.WriteLine();
            UtilProject.WriteLine("Saving directory listings.");

            var dtStart = DateTime.Now;

            foreach (var lvItemProjectVM
                in m_lvProjectVM.ItemsCast
                .Where(lvItemProjectVM => lvItemProjectVM.WouldSave))
            {
                m_cbagWorkers.Add(new SaveDirListing(gd, lvItemProjectVM, m_statusCallback).DoThreadFactory());
            }

            foreach (var worker in m_cbagWorkers)
            {
                worker.Join();
            }

            UtilProject.WriteLine(string.Format("Finished saving directory listings in {0} seconds.",
                ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (IsAborted || gd.WindowClosed)
            {
                return;
            }

            m_doneCallback();
        }

        readonly GlobalData_Base gd = null;
        readonly SaveDirListingsStatusDelegate m_statusCallback = null;
        readonly Action m_doneCallback = null;
        Thread m_thread = null;
        ConcurrentBag<SaveDirListing> m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
        LV_ProjectVM m_lvProjectVM = null;
    }
}
