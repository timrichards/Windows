using System.Threading;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class SaveDirListings : IProgressOverlayClosing
    {
        static internal bool
            IsGoodDriveSyntax(string strDrive) =>
            ((strDrive.Length > 2) &&
            char.IsLetter(strDrive[0]) &&
            (strDrive.Substring(1, 2) == @":\"));

        static internal void
            Go(LV_ProjectVM lvProjectVM) => new SaveDirListings(lvProjectVM);

        SaveDirListings(LV_ProjectVM lvProjectVM)
        {
            _lvProjectVM = lvProjectVM;

            var listNicknames = new List<string> { };
            var listSourcePaths = new List<string> { };

            foreach (var volStrings
                in lvProjectVM.ItemsCast
                .Where(volStrings => volStrings.WouldSave))
            {
                listNicknames.Add(volStrings.Nickname);
                listSourcePaths.Add(volStrings.SourcePath);
            }

            if (0 == listSourcePaths.Count)
                return;

            new ProgressOverlay(listNicknames, listSourcePaths, x => Util.ThreadMake(() =>
            {
                Util.WriteLine("\nSaving directory listings.");

                var stopwatch = Stopwatch.StartNew();

                Util.ParallelForEach(99857,
                    _lvProjectVM.ItemsCast.Where(lvItemProjectVM => lvItemProjectVM.WouldSave),
                    new ParallelOptions
                {
                    CancellationToken = _cts.Token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                },
                    lvItemProjectVM =>{ using (new SaveDirListing(lvItemProjectVM, _cts).Go()) { }; });

                stopwatch.Stop();

                Util.WriteLine(string.Format("Finished saving directory listings in {0} seconds.",
                    ((int)stopwatch.ElapsedMilliseconds / 100) / 10d));

                Util.Block(1000);
                stopwatch.Reset();
                stopwatch.Start();
                GC.Collect();
                stopwatch.Stop();
                Util.WriteLine("SaveDirListings GC.Collect " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
            }))
            {
                Title = "Saving Directory Listings",
                WindowClosingCallback = new WeakReference<IProgressOverlayClosing>(this),
            }
                .AllowSubsequentProcess()
                .ShowOverlay();
        }

        bool IProgressOverlayClosing.ConfirmClose()
        {
            if (_cts.IsCancellationRequested)
                return true;

            if (MessageBoxResult.Yes !=
                MBoxStatic.AskToCancel("Saving Directory Listings"))
            {
                return false;
            }

            _cts.Cancel();
            return false;       // it will close on its own
        }

        CancellationTokenSource
            _cts = new CancellationTokenSource();
        LV_ProjectVM
            _lvProjectVM = null;
    }
}
