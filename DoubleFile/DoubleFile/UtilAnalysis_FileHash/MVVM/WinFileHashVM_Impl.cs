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

            var localTV = new LocalTV();
            var localLVclones = new LocalLV();
            var localLVsameVol = new LocalLV();
            var localLVsolitary = new LocalLV();
            var lsLocalLVignore = new List<LocalLVitem>();

            var collate = new Local.Collate(gd, m_dictNodes,
                localTV,
                localLVclones, localLVsameVol, localLVsolitary,
                m_listRootNodes, m_listTreeNodes, bCheckboxes: true,
                list_lvIgnore: lsLocalLVignore, bLoose: true);
            DateTime dtStart = DateTime.Now;

            collate.Step1_OnThread();
            UtilProject.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;

            if (gd.WindowClosed)
            {
                TreeCleanup();
                return;
            }

            UtilProject.CheckAndInvoke(() => collate.Step2_OnForm());
            UtilProject.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            collate = null;
            TreeCleanup();

            UtilProject.CheckAndInvoke(() =>
            {
                m_tvVM.SetData(m_listRootNodes);
                //m_app.BrowseTab.LV_Clones.SyncData();
                //m_app.BrowseTab.LV_SameVol.SyncData();
                //m_app.BrowseTab.LV_Solitary.SyncData();

                //CopyScratchpadListViewVM lvFake = new CopyScratchpadListViewVM(null);   // Hack: check changed event loads the real listviewer

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
            m_tree = new Local.Tree(gd, m_lvProjectVM, m_dictNodes, m_dictDriveInfo,
                TreeStatusCallback, TreeDoneCallback);
            m_tree.DoThreadFactory();
        }
    }
}
