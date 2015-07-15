using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    interface ISaveDirListingsStatus
    {
        void Status(LVitem_ProjectVM lvItemProjectVM, string strError = null, bool bDone = false, double nProgress = double.NaN);
        void Done();
    }

    partial class SaveDirListings
    {
        internal bool
            IsAborted { get; private set; }
        internal int
            FilesWritten = 0;

        static internal bool IsGoodDriveSyntax(string strDrive)
        {
            return ((strDrive.Length > 2) &&
                char.IsLetter(strDrive[0]) &&
                (strDrive.Substring(1, 2) == @":\"));
        }

        internal SaveDirListings(LV_ProjectVM lvProjectVM, ISaveDirListingsStatus saveDirListingsStatus)
        {
            IsAborted = false;
            _lvProjectVM = lvProjectVM;
            _saveDirListingsStatus = saveDirListingsStatus;
        }

        internal void EndThread()
        {
            foreach (var worker in _cbagWorkers)
                worker.Abort();

            _cbagWorkers = new ConcurrentBag<SaveDirListing>();
            IsAborted = true;
            _thread = null;
        }

        internal SaveDirListings DoThreadFactory()
        {
            _thread = Util.ThreadMake(Go);
            return this;
        }

        void Go()
        {
            Util.WriteLine();
            Util.WriteLine("Saving directory listings.");

            var dtStart = DateTime.Now;

            foreach (var lvItemProjectVM
                in _lvProjectVM.ItemsCast
                .Where(lvItemProjectVM => lvItemProjectVM.WouldSave))
            {
                _cbagWorkers
                    .Add(new SaveDirListing(lvItemProjectVM, _saveDirListingsStatus)
                    .DoThreadFactory());
            }

            foreach (var worker in _cbagWorkers)
                worker.Join();

            Util.WriteLine(string.Format("Finished saving directory listings in {0} seconds.",
                ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10d));

            if ((null == Application.Current) || Application.Current.Dispatcher.HasShutdownStarted ||
                IsAborted)
            {
                return;
            }

            if (null == _saveDirListingsStatus)
            {
                MBoxStatic.Assert(99873, false);
                return;
            }

            _saveDirListingsStatus.Done();
        }

        readonly ISaveDirListingsStatus
            _saveDirListingsStatus = null;
        Thread
            _thread = null;
        ConcurrentBag<SaveDirListing>
            _cbagWorkers = new ConcurrentBag<SaveDirListing>();
        LV_ProjectVM
            _lvProjectVM = null;
    }
}
