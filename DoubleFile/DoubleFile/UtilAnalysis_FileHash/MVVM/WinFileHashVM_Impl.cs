using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.Concurrent;

namespace DoubleFile
{
    delegate void CreateFileDictStatusDelegate(bool bDone = false, double nProgress = double.NaN);
    
    partial class WinFileHashVM
    {
        internal readonly ConcurrentDictionary<FolderKeyStruct, UList<LocalTreeNode>> m_dictNodes = new ConcurrentDictionary<FolderKeyStruct, UList<LocalTreeNode>>();
        internal readonly Dictionary<string, string> m_dictDriveInfo = new Dictionary<string, string>();
        internal Local.Tree m_tree = null;

        internal readonly UList<LocalTreeNode> m_listTreeNodes = new UList<LocalTreeNode>();
        internal readonly List<LocalTreeNode> m_listRootNodes = new List<LocalTreeNode>();

        internal void TreeCleanup()
        {
            m_tree = null;
            Local.Collate.ClearMem();
        }

        internal void ClearMem_TreeForm()
        {
            TreeCleanup();

            m_dictDriveInfo.Clear();

            // m_dictNodes is tested to recreate tree.
            m_dictNodes.Clear();

            m_listTreeNodes.Clear();
            m_listRootNodes.Clear();
        }

        internal void CreateFileDictStatusCallback(bool bDone = false, double nProgress = double.NaN)
        {
            UtilProject.UIthread(() =>
            {
                if (gd.WindowClosed || (gd.FileDictionary == null) || gd.FileDictionary.IsAborted)
                {
                    m_winProgress.Aborted = true;
                    return;
                }

                if (bDone)
                {
                    m_winProgress.SetCompleted(ksFileDictKey);
                    m_winProgress.CloseIfNatural();
                    m_bFileDictDone = true;
                }
                else if (nProgress >= 0)
                {
                    m_winProgress.SetProgress(ksFileDictKey, nProgress);
                }
            });
        }
        
        void TreeStatusCallback(LVitem_ProjectVM volStrings, LocalTreeNode rootNode = null, bool bError = false)
        {
            UtilProject.UIthread(() =>
            {
                if (gd.WindowClosed || (gd.FileDictionary == null) || gd.FileDictionary.IsAborted ||
                    ((m_tree != null) && (m_tree.IsAborted)))
                {
                    ClearMem_TreeForm();
                    m_winProgress.Aborted = true;
                    return;
                }

                if (bError)
                {
                    //           volStrings.SetStatus_BadFile(LV);
                }
                else if (rootNode != null)
                {
                    m_listRootNodes.Add(rootNode);
                    m_winProgress.SetProgress(ksFolderTreeKey, m_listRootNodes.Count / m_nCorrelateProgressDenominator/2);
                }
                else
                {
                    MBoxStatic.Assert(1304.5309, false);
                }
            });
        }

        void TreeDoneCallback()
        {
            if (m_listRootNodes.IsEmpty())
            {
                m_winProgress.Aborted = true;
                UtilProject.UIthread(() => m_winProgress.Close());
                return;
            }

            TreeCleanup();

            var localTV = new LocalTV();
            var localLVclones = new LocalLV();
            var localLVsameVol = new LocalLV();
            var localLVsolitary = new LocalLV();
            var lsLocalLVignore = new List<LocalLVitem>();
            var nProgress = 0.0;

            using (new SDL_Timer(() => { m_winProgress.SetProgress(ksFolderTreeKey, (1 + nProgress)/2.0); }).Start())
            {
                var collate = new Local.Collate(gd, m_dictNodes,
                    localTV,
                    localLVclones, localLVsameVol, localLVsolitary,
                    m_listRootNodes, m_listTreeNodes, bCheckboxes: true,
                    list_lvIgnore: lsLocalLVignore, bLoose: true);
                var dtStart = DateTime.Now;

                collate.Step1_OnThread(d => nProgress = d);
                UtilProject.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds/1000.0 +
                                      " seconds.");
                dtStart = DateTime.Now;

                if (gd.WindowClosed)
                {
                    TreeCleanup();
                    return;
                }

                m_winProgress.SetCompleted(ksFolderTreeKey);
                UtilProject.UIthread(() => collate.Step2_OnForm());
                UtilProject.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds/1000.0 +
                                      " seconds.");
            }

            TreeCleanup();

            UtilProject.UIthread(() =>
            {
                m_tvVM.SetData(m_listRootNodes);
            });

            m_winProgress.CloseIfNatural();
        }

        void DoTree(bool bKill = false)
        {
            if (m_tree != null)
            {
                if (bKill)
                {
                    m_tree.EndThread();
                    m_tree = null;
                    ClearMem_TreeForm();
                }
                else
                {
                    MBoxStatic.Assert(0, false);
                    return;
                }
            }

            m_winProgress.Title = "Initializing File Hash Explorer";
            m_winProgress.WindowClosingCallback = (() =>
            {
                if (false == UtilAnalysis_DirList.Closure(() =>
                {
                    if (gd.FileDictionary.IsAborted)
                    {
                        return true;
                    }

                    if (m_bFileDictDone && (m_tree == null))
                    {
                        return true;
                    }

                    return (MBoxStatic.ShowDialog("Do you want to cancel?", m_winProgress.Title,
                        MessageBoxButton.YesNo,
                        m_winProgress) ==
                        MessageBoxResult.Yes);
                }))
                {
                    return false;
                }

                gd.FileDictionary.Abort();
                    
                if (m_tree != null)
                    m_tree.EndThread();

                TreeCleanup();
                return true;
            });

            var lssProgressItems = new List<string>();

            gd.FileDictionary.ResetAbortFlag();

            if (gd.FileDictionary.IsEmpty)
            {
                lssProgressItems.Add(ksFileDictKey);
                gd.FileDictionary.DoThreadFactory(m_lvProjectVM, CreateFileDictStatusCallback);
            }

            m_tree = new Local.Tree(gd, m_lvProjectVM, m_dictNodes, m_dictDriveInfo,
                TreeStatusCallback, TreeDoneCallback);
            m_tree.DoThreadFactory();

            lssProgressItems.Add(ksFolderTreeKey);
            m_winProgress.InitProgress(new string[lssProgressItems.Count], lssProgressItems);
            m_winProgress.ShowDialog();
        }

        const string ksFileDictKey = "Creating file dictionary";
        const string ksFolderTreeKey = "Creating folder tree browser";
        bool m_bFileDictDone = false;
    }
}
