using System.Threading;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

namespace DoubleFile
{
    partial class Tree : TreeBase
    {
        internal bool IsAborted => _cts.IsCancellationRequested;

        internal Tree(
            LV_ProjectVM lvProjectVM,
            TreeBase treeBase)
            : base(treeBase)
        {
            LVprojectVM = lvProjectVM;
        }

        internal void
            EndThread() => _cts.Cancel();

        internal void Go()
        {
            var stopwatch = Stopwatch.StartNew();

            GC.Collect();
            stopwatch.Stop();
            Util.WriteLine("Tree.Go GC.Collect " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
            stopwatch.Reset();
            stopwatch.Start();

            Util.ParallelForEach(99940,
                from lvItemProjectVM
                in LVprojectVM.Items.Cast<LVitemProject_Explorer>()
                where lvItemProjectVM.CanLoad
                select new TreeRootNodeBuilder(lvItemProjectVM, this),
             new ParallelOptions
             {
                 CancellationToken = _cts.Token,
                 MaxDegreeOfParallelism = Environment.ProcessorCount
             }, treeRoot => treeRoot.Go(_cts));

            stopwatch.Stop();

            Util.WriteLine(string.Format("Completed tree in {0} seconds.",
                ((int)stopwatch.ElapsedMilliseconds / 10) / 100d));

            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                IsAborted)
            {
                return;
            }

            var treeStatus = _callbackWR?.Get(w => w);

            if (null == treeStatus)
            {
                Util.Assert(99864, false);
                return;
            }

            treeStatus.Done();
        }

        LV_ProjectVM
            LVprojectVM { get; set; }
        CancellationTokenSource
            _cts = new CancellationTokenSource();
    }
}
