﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Linq;

namespace DoubleFile
{
    partial class LocalTV : IWinProgressClosing, ICreateFileDictStatus, ITreeStatus
    {
        internal void TreeCleanup()
        {
            _tree = null;
            Collate.ClearMem();
        }

        internal void ClearMem_TreeForm()
        {
            TreeCleanup();

            if (false == _allNodes.IsEmpty())
                _dictVolumeInfo.Clear();

            _dictNodes = null;           // m_dictNodes is tested to recreate tree.
            _allNodes = null;
            _rootNodes = null;
        }

        void DoTree(bool bKill = false)
        {
            if (null != _tree)
            {
                if (bKill)
                {
                    _tree.EndThread();
                    _tree = null;
                    ClearMem_TreeForm();
                }
                else
                {
                    MBoxStatic.Assert(99954, false);
                    return;
                }
            }

            _winProgress.Title = "Initializing Duplicate File Explorer";
            _winProgress.WindowClosingCallback = new WeakReference<IWinProgressClosing>(this);

            var lsProgressItems = new List<string>();
            var fileDictionary = MainWindow.FileDictionary;

            fileDictionary.ResetAbortFlag();

            if (fileDictionary.IsEmpty)
            {
                lsProgressItems.Add(_ksFileDictKey);
                fileDictionary.DoThreadFactory(_lvProjectVM, new WeakReference<ICreateFileDictStatus>(this));
            }

            TabledString<Tabled_Folders>.GenerationStarting();

            if (null == _dictNodes)
                _dictNodes = new ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>();

            _tree =
                new Tree(_lvProjectVM, _dictNodes, _dictVolumeInfo, new WeakReference<ITreeStatus>(this))
                .DoThreadFactory();

            lsProgressItems.Add(_ksFolderTreeKey);

            _winProgress
                .InitProgress(new string[lsProgressItems.Count], lsProgressItems)
                .ShowDialog();
        }

        void ICreateFileDictStatus.Callback(bool bDone, double nProgress)
        {
            if (App.LocalExit ||
                (null == MainWindow.FileDictionary) ||
                MainWindow.FileDictionary.IsAborted)
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
            else if (0 <= nProgress)
            {
                _winProgress.SetProgress(_ksFileDictKey, nProgress);
            }
        }
        
        void ITreeStatus.Status(LVitem_ProjectVM volStrings, LocalTreeNode rootNode, bool bError)
        {
            if (App.LocalExit ||
                (null == MainWindow.FileDictionary) ||
                MainWindow.FileDictionary.IsAborted ||
                ((null != _tree) && (_tree.IsAborted)))
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
                lock (_rootNodesSemaphore)
            {
                if (null == _rootNodes)
                {
                    _rootNodes = new[] { rootNode };
                }
                else
                {
                    // The root volume list is very small so this copy-sort is viable
                    var ls = new List<LocalTreeNode>(_rootNodes);

                    ls.Insert(ls.TakeWhile(node => rootNode.Text.CompareTo(node.Text) > 0).Count(), rootNode);
                    _rootNodes = ls.ToArray();
                }

                _winProgress.SetProgress(_ksFolderTreeKey, _rootNodes.Length / _nCorrelateProgressDenominator * 3 / 4.0);
            }
            else
            {
                MBoxStatic.Assert(1304.5309, false);
            }
        }

        void ITreeStatus.Done()
        {
            if (_rootNodes.IsEmpty())
            {
                _winProgress.Aborted = true;
                UtilProject.UIthread(_winProgress.Close);
                return;
            }

            TopNode = _rootNodes[0];
            LocalTreeNode.SetLevel(_rootNodes);
            TreeCleanup();

            var localLVclones = new LocalLV();
            var localLVsameVol = new LocalLV();
            var localLVsolitary = new LocalLV();
            var lsLocalLVignore = new List<LocalLVitem>();  // when implementing, replace the Forms ListViewItem.Tag in LocalLVItem
            var nProgress = 0.0;

            using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .Subscribe(x => _winProgress.SetProgress(_ksFolderTreeKey, (3 + nProgress) / 4.0)))
            {
                var lsTreeNodes = new List<LocalTreeNode>();

                var collate = new Collate(
                    _dictNodes,
                    localLVclones, localLVsameVol, localLVsolitary,
                    _rootNodes, lsTreeNodes,
                    lsLVignore: lsLocalLVignore, bLoose: true);

                var dtStart = DateTime.Now;

                collate.Step1(d => nProgress = d);
                UtilProject.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds.");
                dtStart = DateTime.Now;

                if (App.LocalExit)
                {
                    TreeCleanup();
                    return;
                }

                _winProgress.SetCompleted(_ksFolderTreeKey);
                collate.Step2();

                if (null == LocalTV.SelectedNode)      // gd.m_bPutPathInFindEditBox is set in TreeDoneCallback()
                    LocalTV.SelectedNode = TopNode;
                _allNodes = lsTreeNodes.ToArray();
                UtilProject.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds.");
            }

            TreeCleanup();
            TabledString<Tabled_Folders>.GenerationEnded();
            _winProgress.CloseIfNatural();
            _dictNodes = null;       // saving memory here.
        }

        bool IWinProgressClosing.ConfirmClose()
        {
            if (false == UtilDirList.Closure(() =>
            {
                if (MainWindow.FileDictionary
                    .IsAborted)
                {
                    return true;
                }

                if (_bFileDictDone &&
                    (null == _tree))
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

            MainWindow.FileDictionary
                .Abort();
                    
            if (null != _tree)
                _tree.EndThread();

            TreeCleanup();
            return true;
        }
        
        const string _ksFileDictKey = "Creating file dictionary";
        const string _ksFolderTreeKey = "Creating folder tree browser";

        Tree
            _tree = null;
        ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>
            _dictNodes = null;

        bool
            _bFileDictDone = false;
    }
}
