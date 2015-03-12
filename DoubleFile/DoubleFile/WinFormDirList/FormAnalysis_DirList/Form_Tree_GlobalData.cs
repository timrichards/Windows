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
                MBoxStatic.Assert(99981, _gd == null);
                _gd = value;
            }
        }
        GlobalData _gd = null;

        internal readonly SortedDictionary<FolderKeyTuple, KeyList<TreeNode>> m_dictNodes = new SortedDictionary<FolderKeyTuple, KeyList<TreeNode>>();
        internal readonly Dictionary<string, string> m_dictDriveInfo = new Dictionary<string, string>();
        internal Tree m_tree = null;
        internal Thread m_threadCollate = null;
        internal Thread m_threadSelect = null;
        internal Thread m_threadSelectCompare = null;

        internal readonly List<ListViewItem> m_listLVignore = new List<ListViewItem>();
        internal readonly KeyList<TreeNode> m_listTreeNodes = new KeyList<TreeNode>();
        internal readonly List<TreeNode> m_listRootNodes = new List<TreeNode>();

        internal GlobalData_Tree(GlobalData gd_in)
        {
            gd = gd_in;
        }

        internal void ClearMem_TreeForm()
        {
            MBoxStatic.Assert(1304.5301, m_listLVignore.IsEmpty());

            m_listLVignore.Clear();
            m_listTreeNodes.Clear();
            m_listRootNodes.Clear();

            MBoxStatic.Assert(1304.5302, m_tree == null);
            m_tree = null;

            m_dictDriveInfo.Clear();

            // m_dictNodes is tested to recreate tree.
            MBoxStatic.Assert(1304.5303, m_dictNodes.IsEmpty());
            m_dictNodes.Clear();
        }

        internal void KillTreeBuilder(bool bJoin = false)
        {
            if (m_tree != null)
            {
                m_tree.EndThread(bJoin: bJoin);
                ((SDL_TreeView)GlobalData.static_MainWindow.DirListForm._form.form_treeViewBrowse).Nodes.Clear();
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

            var formSDL = GlobalData.static_MainWindow.DirListForm;

            UtilProject.UIthread(() =>
            {
                // Display the treemap after the tree is built
                var treeViewBrowse = formSDL._form.form_treeViewBrowse;

                treeViewBrowse.SelectedNode = null;

                if (false == treeViewBrowse.Nodes.IsEmpty())
                {
                    treeViewBrowse.SelectedNode = treeViewBrowse.Nodes[0];
                }
            });
        }

        internal void DoTreeSelect(TreeNode treeNode, TreeSelectStatusDelegate statusCallback, TreeSelectDoneDelegate doneCallback)
        {
            TreeNode rootNode = treeNode.Root();
            string strFile = ((RootNodeDatum)rootNode.Tag).ListingFile;
            bool bSecondComparePane = (gd.m_bCompareMode && rootNode.Checked);
            Thread threadKill = bSecondComparePane ? m_threadSelectCompare : m_threadSelect;

            if ((threadKill != null) && threadKill.IsAlive)
            {
                threadKill.Abort();
            }

            m_threadSelectCompare = null;
            m_threadSelect = null;

            TreeSelect treeSelect = new TreeSelect(treeNode, m_dictDriveInfo,
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
