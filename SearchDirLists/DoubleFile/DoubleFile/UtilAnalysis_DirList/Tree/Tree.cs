using System.Windows.Controls;
using System.Windows.Media;
using Media = System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using System.Windows;

using Forms = System.Windows.Forms;
using Drawing = System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace DoubleFile
{
    delegate void TreeStatusDelegate(LVitem_ProjectVM volStrings, TreeNode rootNode = null, bool bError = false);
    delegate void TreeSelectStatusDelegate(SDL_ListViewItem[] lvItemDetails = null, SDL_ListViewItem[] itemArray = null, SDL_ListViewItem[] lvVolDetails = null, bool bSecondComparePane = false, LVitemFileTag lvFileItem = null);
    delegate void TreeSelectDoneDelegate(bool bSecondComparePane);

    partial class Tree : TreeBase
    {
        readonly UList<LVitem_ProjectVM> m_list_lvVolStrings = null;
        readonly Action m_doneCallback = null;
        ConcurrentBag<TreeRootNodeBuilder> m_cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
        Thread m_thread = null;
        bool m_bThreadAbort = false;

        internal Tree(UList<LVitem_ProjectVM> listLVvolStrings,
            SortedDictionary<Correlate, UList<TreeNode>> dictNodes, Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback, Action doneCallback)
            : base(dictNodes, dictDriveInfo, statusCallback)
        {
            m_list_lvVolStrings = listLVvolStrings;
            m_doneCallback = doneCallback;
            MBox.Assert(1301.2301, m_statusCallback != null);
        }

        void Go()
        {
            UtilProject.WriteLine();
            UtilProject.WriteLine("Creating tree.");

            DateTime dtStart = DateTime.Now;

            foreach (LVitem_ProjectVM volStrings in m_list_lvVolStrings)
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

            if (m_bThreadAbort || GlobalData.AppExit)
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
