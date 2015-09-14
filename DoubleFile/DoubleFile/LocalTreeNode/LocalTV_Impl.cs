using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Linq;
using System.Diagnostics;

namespace DoubleFile
{
    partial class LocalTV : IProgressOverlayClosing, ICreateDupeFileDictStatus, ITreeStatus
    {
        internal void ClearMem_TreeForm()
        {
            _tree = null;

            if (0 < _allNodes.Count)
                _dictVolumeInfo.Clear();

            _dictNodes = null;           // m_dictNodes is tested to recreate tree.
            _allNodes = new List<LocalTreeNode> { };
            _rootNodes = new List<LocalTreeNode> { };
        }

        bool DoTree(bool bKill = false)
        {
            if (null != _tree)
            {
                if (bKill)
                {
                    ClearMem_TreeForm();
                }
                else
                {
                    Util.Assert(99954, false);
                    return false;
                }
            }

            if (0 == _lvProjectVM.CanLoadCount)
                return false;

            _bTreeDone = false;
            Statics.DupeFileDictionary.ResetAbortFlag();

            var lsProgressItems = new List<string> { _ksFolderTreeKey };

            if (Statics.DupeFileDictionary.IsEmpty)
                lsProgressItems.Insert(0, _ksDupeFileDictKey);

            (new ProgressOverlay(new string[lsProgressItems.Count], lsProgressItems, x =>
            {
                if (Statics.DupeFileDictionary.IsEmpty)
                    Statics.DupeFileDictionary.DoThreadFactory(_lvProjectVM, new WeakReference<ICreateDupeFileDictStatus>(this));

                TabledString<TabledStringType_Folders>.GenerationStarting();

                if (Statics.DupeFileDictionary.IsAborted)
                    return;     // from lambda

                if (null == _dictNodes)
                    _dictNodes = new ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>();

                _tree =
                    new Tree(
                        _lvProjectVM,
                        new TreeBase(_dictNodes, _dictVolumeInfo, new WeakReference<ITreeStatus>(this)))
                    .DoThreadFactory();
            })
            {
                Title = "Initializing Explorer",
                WindowClosingCallback = new WeakReference<IProgressOverlayClosing>(this)
            })
                .ShowDialog();

            if (null == _topNode)
                TabledString<TabledStringType_Folders>.GenerationEnded();

            return _bTreeDone;
        }

        void ICreateDupeFileDictStatus.Callback(bool bDone, double nProgress)
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                Statics.DupeFileDictionary.IsAborted)
            {
                ProgressOverlay.WithProgressOverlay(w => w
                    .Abort());

                return;
            }

            if (bDone)
            {
                ProgressOverlay.WithProgressOverlay(w => w
                    .SetCompleted(_ksDupeFileDictKey)
                    .CloseIfNatural());

                _bFileDictDone = true;
            }
            else if (0 <= nProgress)
            {
                ProgressOverlay.WithProgressOverlay(w => w
                    .SetProgress(_ksDupeFileDictKey, nProgress));
            }
        }
        
        void ITreeStatus.Status(LVitem_ProjectVM volStrings, LocalTreeNode rootNode, bool bError)
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                Statics.DupeFileDictionary.IsAborted ||
                (_tree?.IsAborted ?? false))
            {
                ClearMem_TreeForm();

                ProgressOverlay.WithProgressOverlay(w => w
                    .Abort());

                return;
            }

            if (bError)
            {
                //           volStrings.SetStatus_BadFile(LV);
                return;
            }

            lock (_rootNodes)
            {
                _rootNodes.Add(rootNode);

                ProgressOverlay.WithProgressOverlay(w => w
                    .SetProgress(_ksFolderTreeKey, _rootNodes.Count * _knProgMult));
            }
        }

        void ITreeStatus.Done()
        {
            if (0 == _rootNodes.Count)
            {
                ProgressOverlay.CloseForced();
                return;
            }

            TabledString<TabledStringType_Folders>.GenerationEnded();
            _rootNodes.Sort((x, y) => x.Text.LocalCompare(y.Text));
            _topNode = _rootNodes[0];
            LocalTreeNode.SetLevel(_rootNodes);
            _tree = null;

            var lsLocalLVignore = new List<LVitem_ClonesVM> { };  // when implementing, replace the Forms ListViewItem.Tag in LocalLVItem
            double nProgress = 0;

            using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .LocalSubscribe(99761, x => ProgressOverlay.WithProgressOverlay(w => w.SetProgress(_ksFolderTreeKey, (3 + nProgress) / 4))))
            {
                _allNodes = new List<LocalTreeNode> { };

                var collate = new Collate(
                    _dictNodes,
                    _clones, _sameVol, _solitary,
                    _rootNodes, _allNodes,
                    lsLVignore: lsLocalLVignore, bLoose: true);

                var stopwatch = Stopwatch.StartNew();

                collate.Step1(d => nProgress = d / 1.1);
                stopwatch.Stop();
                Util.WriteLine("collate.Step1 " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
                stopwatch.Reset();
                stopwatch.Start();

                if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                    return;

                collate.Step2();
                nProgress = .999;

                if (null == _selectedNode)      // gd.m_bPutPathInFindEditBox is set in TreeDoneCallback()
                    _selectedNode = _topNode;

                stopwatch.Stop();
                Util.WriteLine("collate.Step2 " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
                stopwatch.Reset();
                stopwatch.Start();
                _dictClones = new Dictionary<int, IReadOnlyList<int>>();
                ConsolidateFileHashes(_rootNodes);
                _dictClones = null;
                GC.Collect();
                stopwatch.Stop();
                Util.WriteLine("ConsolidateFileHashes " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");

                ProgressOverlay.WithProgressOverlay(w => w
                    .SetCompleted(_ksFolderTreeKey));
            }

            _bTreeDone = true;      // should precede closing status dialog: returns true to the caller

            ProgressOverlay.WithProgressOverlay(w => w
                .CloseIfNatural());

            _dictNodes = null;      // saving memory here.
        }

        internal List<IReadOnlyList<int>>
            ConsolidateFileHashes(IReadOnlyList<LocalTreeNode> nodes, bool bStart = true)
        {
            if (null == nodes)
                return null;

            var lsAllFilesHashes = new List<IReadOnlyList<int>> { };

            foreach (var treeNode in nodes)
            {
                var lsAllFileHashes_treeNode = treeNode.NodeDatum.FileHashes.ToList();

                if (null != treeNode.Nodes)
                {
                    foreach (var allFileHashes in ConsolidateFileHashes(treeNode.Nodes, bStart: false))    // recurse
                        lsAllFileHashes_treeNode.AddRange(allFileHashes);
                }

                IReadOnlyList<int> lsClone = null;

                if (_dictClones.TryGetValue(treeNode.NodeDatum.AllFilesHash, out lsClone))
                {
                    treeNode.NodeDatum.FileHashes = null;
                    continue;
                }

                treeNode.NodeDatum.FileHashes = lsAllFileHashes_treeNode.Distinct().ToList();
                _dictClones.Add(treeNode.NodeDatum.AllFilesHash, treeNode.NodeDatum.FileHashes);

                if (false == bStart)
                    lsAllFilesHashes.Add(lsAllFileHashes_treeNode);
            }

            return lsAllFilesHashes;
        }
        Dictionary<int, IReadOnlyList<int>>
            _dictClones = null;

        bool IProgressOverlayClosing.ConfirmClose()
        {
            if (false == Util.Closure(() =>
            {
                if (Statics.DupeFileDictionary
                    .IsAborted)
                {
                    return true;    // from lambda Util.Closure
                }

                if (_bFileDictDone &&
                    _bTreeDone)
                {
                    return true;    // from lambda Util.Closure
                }

                Util.WriteLine("IWinProgressClosing.ConfirmClose A");

                return              // from lambda Util.Closure
                    (MessageBoxResult.Yes == ProgressOverlay.WithProgressOverlay(w =>
                    MBoxStatic.ShowDialog("Do you want to cancel?", w.Title, MessageBoxButton.YesNo, w)));
            }))
            {
                return false;       // Iff user said no then cancel and don't close.
            }

            Util.WriteLine("IWinProgressClosing.ConfirmClose B");

            Statics.DupeFileDictionary
                .Abort();
                    
            _tree?.EndThread();
            _tree = null;
            return true;
        }

        const string _ksDupeFileDictKey = "Creating duplicate file dictionary";
        const string _ksFolderTreeKey = "Creating folder tree browser";

        Tree
            _tree = null;
        ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>
            _dictNodes = null;

        bool
            _bFileDictDone = false;
        bool
            _bTreeDone = false;
    }
}
