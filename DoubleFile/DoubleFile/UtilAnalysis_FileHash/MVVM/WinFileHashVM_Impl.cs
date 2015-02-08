using System.Windows.Forms;
using System.Drawing;
using System;
using System.Threading;
using System.Collections.Generic;

namespace DoubleFile
{
#if false
    partial class WinFileHashVM
    {
        internal readonly SortedDictionary<FolderKeyStruct, UList<LocalTreeNode>> m_dictNodes = new SortedDictionary<FolderKeyStruct, UList<LocalTreeNode>>();
        internal readonly Dictionary<string, string> m_dictDriveInfo = new Dictionary<string, string>();
        internal Tree m_tree = null;
        internal Thread m_threadCollate = null;
        internal Thread m_threadSelect = null;
        internal Thread m_threadSelectCompare = null;

        internal readonly List<LocalLVitem> m_listLVignore = new List<LocalLVitem>();
        internal readonly UList<LocalTreeNode> m_listTreeNodes = new UList<LocalTreeNode>();
        internal readonly List<LocalTreeNode> m_listRootNodes = new List<LocalTreeNode>();

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

        internal void TreeCleanup()
        {
            m_tree = null;
            m_threadCollate = null;
            m_listLVignore.Clear();
            Local.Collate.ClearMem();
        }

        void TreeStatusCallback(LVitem_ProjectVM volStrings, TreeNode rootNode = null, bool bError = false)
        {
            if (GlobalData.AppExit || (m_tree == null) || (m_tree.IsAborted))
            {
                TreeCleanup();
                return;
            }

            UtilProject.CheckAndInvoke(() =>
            {
                if (bError)
                {
                    //           volStrings.SetStatus_BadFile(LV);
                }
                else if (rootNode != null)
                {
                    //lock (TV)
                    //{
                    //    TV.Nodes.Add(rootNode.Text);    // items added to show progress
                    //    volStrings.SetStatus_Done(LV, rootNode);
                    //}

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
            Local.Collate collate = new Local.Collate(m_dictNodes,
                LV_Clones.data, LV_SameVol.data, LV_Solitary.data,
                m_listRootNodes, m_listTreeNodes, m_bCheckboxes,
                m_listLVignore, true);
            DateTime dtStart = DateTime.Now;

            collate.Step1_OnThread();
            MBoxStatic.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;

            if (GlobalData.AppExit)
            {
                TreeCleanup();
                return;
            }

            m_bPutPathInFindEditBox = true;
            MBoxStatic.CheckAndInvoke(m_app.Dispatcher, new Action(collate.Step2_OnForm));
            MBoxStatic.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            collate = null;
            TreeCleanup();

            UtilProject.CheckAndInvoke(() =>
            {
                TV.SetData(m_listRootNodes);
                LV_Clones.SyncData();
                LV_SameVol.SyncData();
                LV_Solitary.SyncData();

                CopyScratchpadListViewVM lvFake = new CopyScratchpadListViewVM(null);   // Hack: check changed event loads the real listviewer

                //    foreach (CopyScratchpadLVitemVM lvItem in m_Browse.LV_CopyScratchpad.Items)
                //    {
                //        lvFake.Items.Add(lvItem.Clone());
                //    }

                //    m_Browse.LV_CopyScratchpad.Items.Clear();
                //    m_Browse.LoadCopyScratchPad(lvFake);
            });
        }

        void DoTree(bool bKill = false)
        {
            UList<LVitem_ProjectVM> list_lvVolStrings = new UList<LVitem_ProjectVM>();

            foreach (VolumeLVitemVM lvItem in LV.Items)
            {
                list_lvVolStrings.Add(new LVitem_ProjectVM(lvItem));
            }

            m_tree = new Local.Tree(list_lvVolStrings, m_dictNodes, m_dictDriveInfo,
                new TreeStatusDelegate(TreeStatusCallback), new Action(TreeDoneCallback));
            m_tree.DoThreadFactory();
        }
    }
#endif
}
