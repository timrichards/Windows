using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;

namespace DoubleFile
{
    delegate void TreeStatusDelegate(LVitem_ProjectVM volStrings, TreeNode rootNode = null, bool bError = false);

    partial class Tree : TreeBase
    {
        internal Tree(
            LV_ProjectVM lvProjectVM,
            SortedDictionary<FolderKeyTuple, KeyList<TreeNode>> dictNodes,
            Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback,
            Action doneCallback)
            : base(dictNodes, dictDriveInfo, statusCallback)
        {
            LVprojectVM = lvProjectVM;
            m_doneCallback = doneCallback;
            MBoxStatic.Assert(1301.2301, m_doneCallback != null);
        }

        internal void EndThread(bool bJoin = false)     // bJoin is not used because it induces lag.
        {
            m_bThreadAbort = true;

            if (m_thread != null)
            {
                m_thread.Abort();
                m_thread = null;
            }

            foreach (TreeRootNodeBuilder worker in m_cbagWorkers)
            {
                worker.Abort();
            }

            m_cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
            Collate.Abort();
            m_dictNodes.Clear();
        }

        internal void DoThreadFactory()
        {
            m_thread = new Thread(Go);
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        internal bool IsAborted { get { return m_bThreadAbort; } }

        void Go()
        {
            UtilProject.WriteLine();
            UtilProject.WriteLine("Creating tree.");

            DateTime dtStart = DateTime.Now;

            foreach (var volStrings in LVprojectVM.ItemsCast)
            {
                if (volStrings.CanLoad == false)
                {
                    continue;
                }

                TreeRootNodeBuilder treeRoot = new TreeRootNodeBuilder(volStrings, this);

                m_cbagWorkers.Add(treeRoot.DoThreadFactory());
            }

            foreach (TreeRootNodeBuilder worker in m_cbagWorkers)
            {
                worker.Join();
            }

            UtilProject.WriteLine(string.Format("Completed tree in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100.0));

            if (App.LocalExit || m_bThreadAbort)
                return;

            m_doneCallback();
        }

        LV_ProjectVM LVprojectVM { get; set; }
        readonly Action m_doneCallback = null;
        ConcurrentBag<TreeRootNodeBuilder> m_cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
        Thread m_thread = null;
        bool m_bThreadAbort = false;
    }
}
