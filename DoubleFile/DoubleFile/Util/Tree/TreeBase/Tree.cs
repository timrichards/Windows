using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;

namespace DoubleFile
{
    partial class Tree : TreeBase
    {
        internal Tree(
            LV_ProjectVM lvProjectVM,
            ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>> dictNodes,
            Dictionary<string, string> dictDriveInfo,
            WeakReference<ITreeStatus> callbackWR)
            : base(dictNodes, dictDriveInfo, callbackWR)
        {
            IsAborted = false;
            LVprojectVM = lvProjectVM;
            MBoxStatic.Assert(1301.2301, callbackWR != null);
        }

        internal void EndThread()     // bJoin is not used because it induces lag.
        {
            IsAborted = true;

            if (m_thread != null)
            {
                m_thread.Abort();
                m_thread = null;
            }

            foreach (var worker in m_cbagWorkers)
            {
                worker.Abort();
            }

            m_cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
            Collate.Abort();
            _dictNodes.Clear();
        }

        internal Tree DoThreadFactory()
        {
            m_thread = new Thread(Go) {IsBackground = true};
            m_thread.Start();
            return this;
        }

        internal bool IsAborted { get; private set; }

        void Go()
        {
            Util.WriteLine();
            Util.WriteLine("Creating tree.");

            var dtStart = DateTime.Now;

            foreach (var treeRoot
                in from volStrings
                in LVprojectVM.ItemsCast
                where volStrings.CanLoad
                select new TreeRootNodeBuilder(volStrings, this))
            {
                m_cbagWorkers.Add(treeRoot.DoThreadFactory());
            }

            foreach (var worker in m_cbagWorkers)
            {
                worker.Join();
            }

            Util.WriteLine(string.Format("Completed tree in {0} seconds.",
                ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100d));

            if (App.LocalExit || IsAborted)
            {
                return;
            }

            if (null == _callbackWR)
            {
                MBoxStatic.Assert(99865, false);
                return;
            }

            ITreeStatus treeStatus = null;

            _callbackWR.TryGetTarget(out treeStatus);

            if (null == treeStatus)
            {
                MBoxStatic.Assert(99864, false);
                return;
            }

            treeStatus.Done();
        }

        LV_ProjectVM LVprojectVM { get; set; }
        ConcurrentBag<TreeRootNodeBuilder> m_cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
        Thread m_thread = null;
    }
}
