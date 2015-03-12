using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.Concurrent;

namespace DoubleFile
{
    delegate void CreateFileDictStatusDelegate(bool bDone = false, double nProgress = double.NaN);
    
    partial class WinFileHash_FoldersVM
    {
        internal ConcurrentDictionary<FolderKeyTuple, KeyList<LocalTreeNode>>
            DictNodes { get; private set; }
        internal Local.Tree
            Tree { get; private set; }

        internal readonly KeyList<LocalTreeNode>
            _listTreeNodes = new KeyList<LocalTreeNode>();
        internal readonly List<LocalTreeNode>
            _listRootNodes = new List<LocalTreeNode>();

        internal void TreeCleanup()
        {
            Tree = null;
            Local.Collate.ClearMem();
        }

        internal void ClearMem_TreeForm()
        {
            TreeCleanup();

            _tvVM._dictVolumeInfo.Clear();

            // m_dictNodes is tested to recreate tree.
            DictNodes = null;

            _listTreeNodes.Clear();
            _listRootNodes.Clear();
        }

        internal void CreateFileDictStatusCallback(bool bDone = false, double nProgress = double.NaN)
        {
            if (_gd.WindowClosed ||
                (null == _gd.FileDictionary) ||
                _gd.FileDictionary.IsAborted)
            {
                _winProgress.Aborted = true;
                return;
            }

            if (bDone)
            {
                _winProgress.SetCompleted(_ksFileDictKey);
                _winProgress.CloseIfNatural();
                _bFileDictDone = true;
            }
            else if (nProgress >= 0)
            {
                _winProgress.SetProgress(_ksFileDictKey, nProgress);
            }
        }
        
        void TreeStatusCallback(LVitem_ProjectVM volStrings, LocalTreeNode rootNode = null, bool bError = false)
        {
            if (_gd.WindowClosed ||
                (null == _gd.FileDictionary) ||
                _gd.FileDictionary.IsAborted ||
                ((null != Tree) && (Tree.IsAborted)))
            {
                ClearMem_TreeForm();
                _winProgress.Aborted = true;
                return;
            }

            if (bError)
            {
                //           volStrings.SetStatus_BadFile(LV);
            }
            else if (rootNode != null)
            {
                _listRootNodes.Add(rootNode);
                _winProgress.SetProgress(_ksFolderTreeKey, _listRootNodes.Count / _nCorrelateProgressDenominator*3/4.0);
            }
            else
            {
                MBoxStatic.Assert(1304.5309, false);
            }
        }

        void TreeDoneCallback()
        {
            if (_listRootNodes.IsEmpty())
            {
                _winProgress.Aborted = true;
                UtilProject.UIthread(_winProgress.Close);
                return;
            }

            TreeCleanup();

            var localTV = new LocalTV();
            var localLVclones = new LocalLV();
            var localLVsameVol = new LocalLV();
            var localLVsolitary = new LocalLV();
            var lsLocalLVignore = new List<LocalLVitem>();  // when implementing, replace the Forms ListViewItem.Tag in LocalLVItem
            var nProgress = 0.0;

            using (new SDL_Timer(() => { _winProgress.SetProgress(_ksFolderTreeKey, (3 + nProgress)/4.0); }).Start())
            {
                var collate = new Local.Collate(_gd, DictNodes,
                    localTV,
                    localLVclones, localLVsameVol, localLVsolitary,
                    _listRootNodes, _listTreeNodes,
                    lsLVignore: lsLocalLVignore, bLoose: true);
                var dtStart = DateTime.Now;

                collate.Step1(d => nProgress = d);
                UtilProject.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds/1000.0 + " seconds.");
                dtStart = DateTime.Now;

                if (_gd.WindowClosed)
                {
                    TreeCleanup();
                    return;
                }

                _winProgress.SetCompleted(_ksFolderTreeKey);
                collate.Step2();
                UtilProject.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds/1000.0 + " seconds.");
            }

            TreeCleanup();
            TabledString.GenerationEnded();
            _winProgress.CloseIfNatural();

            // saving memory here.
            DictNodes = null;

            while (false == _winProgress.LocalIsClosed)
                System.Threading.Thread.Sleep(200);

            UtilProject.UIthread<bool>(ShowWindows);
            _tvVM.SetData(_listRootNodes);
        }

        void DoTree(bool bKill = false)
        {
            if (null != Tree)
            {
                if (bKill)
                {
                    Tree.EndThread();
                    Tree = null;
                    ClearMem_TreeForm();
                }
                else
                {
                    MBoxStatic.Assert(99954, false);
                    return;
                }
            }

            _winProgress.Title = "Initializing Duplicate File Explorer";
            _winProgress.WindowClosingCallback = (() =>
            {
                if (false == UtilAnalysis_DirList.Closure(() =>
                {
                    if (_gd.FileDictionary
                        .IsAborted)
                    {
                        return true;
                    }

                    if (_bFileDictDone &&
                        (null == Tree))
                    {
                        return true;
                    }

                    return
                        (MessageBoxResult.Yes ==
                        MBoxStatic.ShowDialog("Do you want to cancel?", _winProgress.Title, MessageBoxButton.YesNo, _winProgress));
                }))
                {
                    return false;
                }

                _gd.FileDictionary
                    .Abort();
                    
                if (null != Tree)
                    Tree.EndThread();

                TreeCleanup();
                return true;
            });

            var lsProgressItems = new List<string>();

            _gd.FileDictionary.ResetAbortFlag();

            if (_gd.FileDictionary.IsEmpty)
            {
                lsProgressItems.Add(_ksFileDictKey);
                _gd.FileDictionary.DoThreadFactory(_lvProjectVM, CreateFileDictStatusCallback);
            }

            TabledString.GenerationStarting();

            if (null == DictNodes)
                DictNodes = new ConcurrentDictionary<FolderKeyTuple, KeyList<LocalTreeNode>>();

            Tree =
                new Local.Tree(_gd, _lvProjectVM, DictNodes, _tvVM._dictVolumeInfo,
                    TreeStatusCallback, TreeDoneCallback)
                .DoThreadFactory();

            lsProgressItems.Add(_ksFolderTreeKey);
            _winProgress.InitProgress(new string[lsProgressItems.Count], lsProgressItems);
            _winProgress.ShowDialog();
        }

        const string _ksFileDictKey = "Creating file dictionary";
        const string _ksFolderTreeKey = "Creating folder tree browser";

        bool _bFileDictDone = false;
    }
}
