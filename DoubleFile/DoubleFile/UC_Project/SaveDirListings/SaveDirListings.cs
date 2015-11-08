using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System;

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
            IsAborted => _cts.IsCancellationRequested;

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
            _lvProjectVM = lvProjectVM;
            _saveDirListingsStatus = saveDirListingsStatus;
        }

        internal void EndThread()
        {
            _cts.Cancel();
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

            var stopwatch = Stopwatch.StartNew();

            Util.ParallelForEach(99857,
                _lvProjectVM.ItemsCast.Where(lvItemProjectVM => lvItemProjectVM.WouldSave),
                new ParallelOptions
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            },
                lvItemProjectVM => new SaveDirListing(lvItemProjectVM, _saveDirListingsStatus, _cts).Go());

            stopwatch.Stop();

            Util.WriteLine(string.Format("Finished saving directory listings in {0} seconds.",
                ((int)stopwatch.ElapsedMilliseconds / 100) / 10d));

            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                IsAborted)
            {
                return;
            }

            if (null == _saveDirListingsStatus)
            {
                Util.Assert(99873, false);
                return;
            }

            _saveDirListingsStatus.Done();
        }

        readonly ISaveDirListingsStatus
            _saveDirListingsStatus = null;
        CancellationTokenSource
            _cts = new CancellationTokenSource();
        Thread
            _thread = null;
        LV_ProjectVM
            _lvProjectVM = null;
    }
}
