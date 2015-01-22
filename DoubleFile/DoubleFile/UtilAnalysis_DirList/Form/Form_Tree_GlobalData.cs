﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace DoubleFile
{
    partial class GlobalData
    {
        internal readonly SortedDictionary<Correlate, UList<TreeNode>> m_dictNodes = new SortedDictionary<Correlate, UList<TreeNode>>();
        internal readonly Dictionary<string, string> m_dictDriveInfo = new Dictionary<string, string>();
        internal Tree m_tree = null;
        internal Thread m_threadCollate = null;
        internal Thread m_threadSelect = null;
        internal Thread m_threadSelectCompare = null;

        internal readonly List<ListViewItem> m_listLVignore = new List<ListViewItem>();
        internal readonly UList<TreeNode> m_listTreeNodes = new UList<TreeNode>();
        internal readonly List<TreeNode> m_listRootNodes = new List<TreeNode>();

        internal void ClearMem_TreeForm()
        {
            MBox.Assert(1304.5301, m_listLVignore.Count == 0);

            m_listLVignore.Clear();
            m_listTreeNodes.Clear();
            m_listRootNodes.Clear();

            MBox.Assert(1304.5302, m_tree == null);
            m_tree = null;

            m_dictDriveInfo.Clear();

            // m_dictNodes is tested to recreate tree.
            MBox.Assert(1304.5303, m_dictNodes.Count == 0);
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

            var formSDL = static_MainWindow.Analysis_DirListForm;

            formSDL.Invoke(new Action(() =>
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

        internal void DoTreeSelect(TreeNode treeNode, TreeSelectStatusDelegate statusCallback, TreeSelectDoneDelegate doneCallback)
        {
            TreeNode rootNode = treeNode.Root();
            string strFile = ((RootNodeDatum)rootNode.Tag).StrFile;
            bool bSecondComparePane = (m_bCompareMode && rootNode.Checked);
            Thread threadKill = bSecondComparePane ? m_threadSelectCompare : m_threadSelect;

            if ((threadKill != null) && threadKill.IsAlive)
            {
                threadKill.Abort();
            }

            m_threadSelectCompare = null;
            m_threadSelect = null;

            TreeSelect treeSelect = new TreeSelect(treeNode, m_dictNodes, m_dictDriveInfo,
                strFile, m_bCompareMode, bSecondComparePane,
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
