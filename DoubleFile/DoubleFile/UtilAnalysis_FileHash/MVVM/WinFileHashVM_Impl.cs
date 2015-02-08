using System.Windows.Forms;
using System.Drawing;
using System;
using System.Threading;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class WinFileHashVM
    {
        internal readonly SortedDictionary<FolderKeyStruct, UList<LocalTreeNode>> m_dictNodes = new SortedDictionary<FolderKeyStruct, UList<LocalTreeNode>>();
        internal readonly Dictionary<string, string> m_dictDriveInfo = new Dictionary<string, string>();
        internal Local.Tree m_tree = null;

        internal readonly UList<LocalTreeNode> m_listTreeNodes = new UList<LocalTreeNode>();
        internal readonly List<LocalTreeNode> m_listRootNodes = new List<LocalTreeNode>();

        internal void ClearMem_TreeForm()
        {
            MBoxStatic.Assert(1304.5302, m_tree == null);
            m_tree = null;

            m_dictDriveInfo.Clear();

            // m_dictNodes is tested to recreate tree.
            MBoxStatic.Assert(1304.5303, m_dictNodes.Count == 0);
            m_dictNodes.Clear();
        }

        internal void TreeCleanup()
        {
            m_tree = null;
            Local.Collate.ClearMem();
        }

        void TreeStatusCallback(LVitem_ProjectVM volStrings, LocalTreeNode rootNode = null, bool bError = false)
        {
            //if (GlobalData.AppExit || (m_tree == null) || (m_tree.IsAborted))
            //{
            //    TreeCleanup();
            //    return;
            //}

            UtilProject.CheckAndInvoke(() =>
            {
                if (bError)
                {
                    //           volStrings.SetStatus_BadFile(LV);
                }
                else if (rootNode != null)
                {
                    lock (m_listRootNodes)
                    {
                        m_listRootNodes.Add(rootNode);
                    }
                }
                else
                {
                    MBoxStatic.Assert(1304.5309, false);
                }
            });
        }

        void TreeDoneCallback()
        {
            TreeCleanup();

            UtilProject.CheckAndInvoke(() =>
            {
                m_tvVM.SetData(m_listRootNodes);
            });
        }

        void DoTree(bool bKill = false)
        {
            //m_tree = new Local.Tree(LVvolumesVM, m_dictNodes, m_dictDriveInfo,
            //    TreeStatusCallback, TreeDoneCallback);
            m_tree.DoThreadFactory();
        }
    }
}
