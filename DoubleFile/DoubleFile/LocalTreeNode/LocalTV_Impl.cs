using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Linq;

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
            _lsh = null;
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

                var dtStart = DateTime.Now;

                collate.Step1(d => nProgress = d);
                Util.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds / 1000d + " seconds.");
                dtStart = DateTime.Now;

                if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                    return;

                ProgressOverlay.WithProgressOverlay(w => w
                    .SetCompleted(_ksFolderTreeKey));

                collate.Step2();

                if (null == _selectedNode)      // gd.m_bPutPathInFindEditBox is set in TreeDoneCallback()
                    _selectedNode = _topNode;

                Util.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000d + " seconds.");

                // Locality sensitive hashing
                CalcMinHashes(new MinHash(1 << 7, 20), _rootNodes);
                _lsh = new LocalitySensitiveHashing(Util.CreateRectangularArray(GetMinHashes(_rootNodes)), 20);

                //for (int i = 0; i < _lshIndex; ++i)
                //{
                //    var thisNode = _dictLSH[i];
                //    var similarNodes = _lsh.GetNearest(i).Select(n => _dictLSH[n]).ToList();

                //    if (0 == similarNodes.Count)
                //        continue;

                //    if (1 << 30 < thisNode.NodeDatum.TotalLength)
                //        continue;
                // }
            }

            _bTreeDone = true;      // should precede closing status dialog: returns true to the caller

            ProgressOverlay.WithProgressOverlay(w => w
                .CloseIfNatural());

            _dictNodes = null;      // saving memory here.
        }

        internal List<IReadOnlyList<int>>
            CalcMinHashes(MinHash minHashClassObj, IReadOnlyList<LocalTreeNode> nodes, bool bStart = true)
        {
            if (null == nodes)
                return null;

            var lsAllFilesHashes = new List<IReadOnlyList<int>> { };

            foreach (var treeNode in nodes)
            {
                var lsAllFileHashes_treeNode = treeNode.NodeDatum.FilesHereHashes.ToList();

                if (null != treeNode.Nodes)
                {
                    foreach (var allFileHashes in CalcMinHashes(minHashClassObj, treeNode.Nodes, bStart: false))    // recurse
                        lsAllFileHashes_treeNode.AddRange(allFileHashes);
                }

                treeNode.NodeDatum.FilesHereHashes = minHashClassObj.GetMinHash(lsAllFileHashes_treeNode);          // Coopt - A

                if (false == bStart)
                    lsAllFilesHashes.Add(lsAllFileHashes_treeNode);
            }

            _lshIndex = 0;
            return lsAllFilesHashes;
        }

        internal List<IReadOnlyList<int>>
            GetMinHashes(IReadOnlyList<LocalTreeNode> nodes, bool bStart = true)
        {
            if (null == nodes)
                return null;

            var lsMinhashes = new List<IReadOnlyList<int>> { };

            foreach (var treeNode in nodes)
            {
                var nIndex = 0;

                if (_dictClones.TryGetValue(treeNode.NodeDatum.AllFilesHash, out nIndex))
                {
                    treeNode.NodeDatum.LSH_Index = nIndex;
                    continue;
                }

                if (null != treeNode.Nodes)
                    lsMinhashes.AddRange(GetMinHashes(treeNode.Nodes, bStart: false));                              // recurse

                if (_dictClones.TryGetValue(treeNode.NodeDatum.AllFilesHash, out nIndex))
                {
                    treeNode.NodeDatum.LSH_Index = nIndex;
                    continue;
                }

                lsMinhashes.Add(treeNode.NodeDatum.FilesHereHashes);                                                // Coopt - B
                treeNode.NodeDatum.LSH_Index = _lshIndex++;
                _dictClones.Add(treeNode.NodeDatum.AllFilesHash, treeNode.NodeDatum.LSH_Index);
                _dictLSH.Add(treeNode.NodeDatum.LSH_Index, treeNode);
            }

            if (bStart)
                _dictClones = null;

            return lsMinhashes;
        }
        int _lshIndex = 0;
        Dictionary<int, LocalTreeNode>
            _dictLSH = new Dictionary<int, LocalTreeNode>();
        Dictionary<int, int>
            _dictClones = new Dictionary<int, int>();

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
