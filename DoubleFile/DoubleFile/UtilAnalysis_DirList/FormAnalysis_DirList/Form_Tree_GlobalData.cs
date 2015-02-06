using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace DoubleFile
{
    class GlobalData_Tree
    {
        GlobalData gd
        {
            get { return _gd; }
            set
            {
                MBoxStatic.Assert(0, _gd == null);
                _gd = value;
            }
        }
        GlobalData _gd = null;

        internal readonly SortedDictionary<FolderKeyStruct, UList<BothNodes>> m_dictNodes = new SortedDictionary<FolderKeyStruct, UList<BothNodes>>();
        internal readonly Dictionary<string, string> m_dictDriveInfo = new Dictionary<string, string>();
        internal Tree m_tree = null;
        internal Thread m_threadCollate = null;
        internal Thread m_threadSelect = null;
        internal Thread m_threadSelectCompare = null;

        internal readonly List<ListViewItem> m_listLVignore = new List<ListViewItem>();
        internal readonly UList<BothNodes> m_listTreeNodes = new UList<BothNodes>();
        internal readonly List<BothNodes> m_listRootNodes = new List<BothNodes>();

        internal GlobalData_Tree(GlobalData gd_in)
        {
            gd = gd_in;
        }

        internal void ClearMem_TreeForm()
        {
            MBoxStatic.Assert(1304.5301, m_listLVignore.Count == 0);

            m_listLVignore.Clear();
            m_listTreeNodes.Clear();
            m_listRootNodes.Clear();

            MBoxStatic.Assert(1304.5302, m_tree == null);
            m_tree = null;

            m_dictDriveInfo.Clear();

            // m_dictNodes is tested to recreate tree.
            MBoxStatic.Assert(1304.5303, m_dictNodes.Count == 0);
            m_dictNodes.Clear();
        }

        internal void KillTreeBuilder(bool bJoin = false)
        {
            if (m_tree != null)
            {
                m_tree.EndThread(bJoin: bJoin);
                ((SDL_TreeView)GlobalData.static_MainWindow.Analysis_DirListForm.form_treeViewBrowse).Nodes.Clear();
                m_listRootNodes.Clear();
                TreeCleanup();
            }
        }

        internal void TreeCleanup()
        {
            m_tree = null;
            m_threadCollate = null;
            m_listLVignore.Clear();
            Collate.ClearMem();

            var formSDL = GlobalData.static_MainWindow.Analysis_DirListForm;

            UtilAnalysis_DirList.CheckAndInvoke(formSDL, new Action(() =>
            {
                // Display the treemap after the tree is built
                var treeViewBrowse = formSDL.form_treeViewBrowse;

                treeViewBrowse.SelectedNode = null;

                if (treeViewBrowse.Nodes.Count > 0)
                {
                    treeViewBrowse.SelectedNode = treeViewBrowse.Nodes[0];
                }
            }));
        }

        internal void DoTreeSelect(BothNodes treeNode, TreeSelectStatusDelegate statusCallback, TreeSelectDoneDelegate doneCallback)
        {
            BothNodes rootNode = treeNode.Root;
            string strFile = ((RootNodeDatum)rootNode.Tag).StrFile;
            bool bSecondComparePane = (gd.m_bCompareMode && rootNode.Checked);
            Thread threadKill = bSecondComparePane ? m_threadSelectCompare : m_threadSelect;

            if ((threadKill != null) && threadKill.IsAlive)
            {
                threadKill.Abort();
            }

            m_threadSelectCompare = null;
            m_threadSelect = null;

            TreeSelect treeSelect = new TreeSelect(treeNode, m_dictNodes, m_dictDriveInfo,
                strFile, gd.m_bCompareMode, bSecondComparePane,
                statusCallback, doneCallback);

            Thread thread = treeSelect.DoThreadFactory();

            if (bSecondComparePane)
            {
                m_threadSelectCompare = thread;
            }
            else
            {
                m_threadSelect = thread;
            }
        }
    }
}
