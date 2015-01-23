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
        IEnumerable<LVitem_ProjectVM> ListLVvolStrings { get; set; }
        readonly Action m_doneCallback = null;
        ConcurrentBag<TreeRootNodeBuilder> m_cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
        Thread m_thread = null;
        bool m_bThreadAbort = false;

        internal static Tree Factory(GlobalDataBase gd_in,
            IEnumerable<LVitem_ProjectVM> listLVvolStrings,
            SortedDictionary<Correlate, UList<TreeNode>> dictNodes,
            Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback,
            Action doneCallback)
        {
            return new Tree(gd_in, listLVvolStrings, dictNodes, dictDriveInfo, statusCallback, doneCallback);
        }

        private Tree(GlobalDataBase gd_in,
            IEnumerable<LVitem_ProjectVM> listLVvolStrings,
            SortedDictionary<Correlate, UList<TreeNode>> dictNodes,
            Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback,
            Action doneCallback)
            : base(gd_in, dictNodes, dictDriveInfo, statusCallback)
        {
            ListLVvolStrings = listLVvolStrings;
            m_doneCallback = doneCallback;
            MBox.Assert(1301.2301, m_statusCallback != null);
        }

        void Go()
        {
            UtilAnalysis_DirList.WriteLine();
            UtilAnalysis_DirList.WriteLine("Creating tree.");

            DateTime dtStart = DateTime.Now;

            foreach (LVitem_ProjectVM volStrings in ListLVvolStrings)
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

            UtilAnalysis_DirList.WriteLine(string.Format("Completed tree in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100.0));

            if (m_bThreadAbort || GlobalData.Instance.FormAnalysis_DirList_Closing)
            {
                return;
            }

            m_doneCallback();
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
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        internal bool IsAborted { get { return m_bThreadAbort; } }
    }
}
