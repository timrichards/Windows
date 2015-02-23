using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using DoubleFile;

namespace Local
{
    delegate void TreeStatusDelegate(
        LVitem_ProjectVM volStrings,
        LocalTreeNode rootNode = null,
        bool bError = false);

    partial class Tree : TreeBase
    {
        internal Tree(GlobalData_Base gd_in,
            LV_ProjectVM lvProjectVM,
            ConcurrentDictionary<FolderKeyStruct, UList<LocalTreeNode>> dictNodes,
            Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback,
            Action doneCallback)
            : base(gd_in, dictNodes, dictDriveInfo, statusCallback)
        {
            IsAborted = false;
            LVprojectVM = lvProjectVM;
            m_doneCallback = doneCallback;
            MBoxStatic.Assert(1301.2301, m_doneCallback != null);
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
            m_dictNodes.Clear();
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
            UtilProject.WriteLine("Creating tree.");

            var dtStart = DateTime.Now;

            UString.GenerationStarting();

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

            UtilProject.WriteLine(string.Format("Completed tree in {0} seconds.",
                ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100.0));

            if (IsAborted || gd.WindowClosed)
            {
                return;
            }

            m_doneCallback();
        }

        LV_ProjectVM LVprojectVM { get; set; }
        readonly Action m_doneCallback = null;
        ConcurrentBag<TreeRootNodeBuilder> m_cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
        Thread m_thread = null;
    }
}
