using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    partial class Tree : TreeBase
    {
        internal bool IsAborted { get; private set; }

        internal Tree(
            LV_ProjectVM lvProjectVM,
            TreeBase treeBase)
            : base(treeBase)
        {
            LVprojectVM = lvProjectVM;
        }

        internal void EndThread()     // bJoin is not used because it induces lag.
        {
            IsAborted = true;

            if (_thread != null)
            {
                _thread.Abort();
                _thread = null;
            }

            foreach (var worker in _cbagWorkers)
                worker.Abort();

            _cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
            _dictNodes.Clear();
        }

        internal Tree DoThreadFactory()
        {
            _thread = Util.ThreadMake(Go);
            return this;
        }

        void Go()
        {
            Util.WriteLine();
            Util.WriteLine("Creating tree.");

            var dtStart = DateTime.Now;

            foreach (var treeRoot
                in from lvItemProjectVM
                in LVprojectVM.Items.Cast<LVitem_ProjectExplorer>()
                where lvItemProjectVM.CanLoad
                select new TreeRootNodeBuilder(lvItemProjectVM, this))
            {
                _cbagWorkers.Add(treeRoot.DoThreadFactory());
            }

            foreach (var worker in _cbagWorkers)
                worker.Join();

            Util.WriteLine(string.Format("Completed tree in {0} seconds.",
                ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100d));

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
        ConcurrentBag<TreeRootNodeBuilder>
            _cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
        Thread
            _thread = null;
    }
}
