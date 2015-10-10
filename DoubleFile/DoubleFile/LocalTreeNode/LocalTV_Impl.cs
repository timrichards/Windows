using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Diagnostics;
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
            var bBuildDupes = Statics.DupeFileDictionary.IsEmpty && (Statics.Namespace == GetType().Namespace);     // VolTreemap assembly: skip it

            if (bBuildDupes)
                lsProgressItems.Insert(0, _ksDupeFileDictKey);

            (new ProgressOverlay(new string[lsProgressItems.Count], lsProgressItems, x =>
            {
                if (bBuildDupes)
                    Statics.DupeFileDictionary.DoThreadFactory(_lvProjectVM, new WeakReference<ICreateDupeFileDictStatus>(this));

                TabledString<TabledStringType_Folders>.GenerationStarting();

                if (Statics.DupeFileDictionary.IsAborted)
                    return;     // from lambda

                _tree =
                    new Tree(
                    _lvProjectVM,
                    new TreeBase(_dictVolumeInfo, new WeakReference<ITreeStatus>(this)))
                    .DoThreadFactory();
            })
            {
                Title = "Initializing Explorer",
                WindowClosingCallback = new WeakReference<IProgressOverlayClosing>(this)
            })
                .ShowOverlay();

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
            _rootNodes.Sort((x, y) => x.PathShort.LocalCompare(y.PathShort));
            _topNode = _rootNodes[0];
            LocalTreeNode.SetLevel(_rootNodes);
            _tree = null;

            var lsLocalLVignore = new List<LVitem_ClonesVM> { };  // when implementing, replace the Forms ListViewItem.Tag in LocalLVItem
            double nProgress = 0;

            using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .LocalSubscribe(99761, x => ProgressOverlay.WithProgressOverlay(w => w.SetProgress(_ksFolderTreeKey, (2 + nProgress) / 3))))
            {
                while (false == _bFileDictDone)      // the only reason _bFileDictDone is to SetAllFilesHashes() 
                    Util.Block(200);

                AllFileHashes_AddRef();
                SetAllFilesHashes(RootNodes);

                IDictionary<int, List<LocalTreeNode>> dictNodes = new Dictionary<int, List<LocalTreeNode>>();

                {
                    var dictNodes_ = new Dictionary<int, List<LocalTreeNode>>();

                    SetDictNodes(dictNodes_, RootNodes);

                    var dictLength = new Dictionary<int, ulong>();

                    foreach (var kvp in dictNodes_)
                    {
                        var hashes = kvp.Value.First().NodeDatum.Hashes_SubnodeFiles_Scratch;

                        Util.Assert(99598, kvp.Value.All(treeNode => treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch.SequenceEqual(hashes)));
                        dictLength[kvp.Key] = kvp.Value.Max(treeNode => treeNode.NodeDatum.LengthTotal);
                    }

                    var nFolderIndexedID = 1;   // first folder ID is 1: zero is not valid: no unchecked/overflow/wraparound (<<1.5M is reasonable)

                    foreach (var kvp in dictLength.OrderBy(kvp => kvp.Value))               // sort by total length
                    {
                        var lsTreeNodes = dictNodes_[kvp.Key];

                        foreach (var treeNode in lsTreeNodes)
                            treeNode.NodeDatum.Hash_AllFiles = nFolderIndexedID;

                        dictNodes[nFolderIndexedID] = lsTreeNodes;
                        ++nFolderIndexedID;
                    }
                }

                AllFileHashes_DropRef();
                _allNodes = new List<LocalTreeNode> { };

                var collate = new Collate(
                    dictNodes,
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
                GC.Collect();
                stopwatch.Stop();
                Util.WriteLine("GC.Collect " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
            }

            _bTreeDone = true;      // should precede closing status dialog: returns true to the caller

            ProgressOverlay.WithProgressOverlay(w => w
                .SetCompleted(_ksFolderTreeKey)
                .CloseIfNatural());

            TreeSelect.DoThreadFactory(TopNode, 99959);
        }

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
                    MBoxStatic.AskToCancel(w.Title)));
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

        void SetAllFilesHashes(IEnumerable<LocalTreeNode> treeNodes)
        {
            foreach (var treeNode in treeNodes)
            {
                var ieHashes =
                    treeNode.NodeDatum.Hashes_FilesHere
                    .Union(treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch)
                    .OrderBy(n => n)
                    .Distinct();

                var nCount = 0;

                Util.Assert(99599, 0 == treeNode.NodeDatum.Hash_AllFiles);
                treeNode.NodeDatum.Hash_AllFiles = 0;

                foreach (var nFileIndexedID in ieHashes)
                {
                    unchecked
                    {
                        treeNode.NodeDatum.Hash_AllFiles += nFileIndexedID;
                        treeNode.NodeDatum.Hash_AllFiles *= 37;
                    }

                    ++nCount;
                }

                unchecked
                {
                    treeNode.NodeDatum.Hash_AllFiles += nCount;
                    treeNode.NodeDatum.Hash_AllFiles *= 37;
                }

                if (null != treeNode.Nodes)
                    SetAllFilesHashes(treeNode.Nodes);      // recurse
            }
        }

        void SetDictNodes(IDictionary<int, List<LocalTreeNode>> dictNodes, IEnumerable<LocalTreeNode> treeNodes)
        {
            foreach (var treeNode in treeNodes)
            {
                var nodeDatum = treeNode.NodeDatum;

                if (0 == nodeDatum.Hash_AllFiles)
                    continue;

                for (;;)
                {
                    var lsTreeNodes = dictNodes.TryGetValue(nodeDatum.Hash_AllFiles);

                    if (null != lsTreeNodes)
                    {
                        if (lsTreeNodes[0].NodeDatum.Hashes_SubnodeFiles_Scratch.SequenceEqual(nodeDatum.Hashes_SubnodeFiles_Scratch))
                        {
                            lsTreeNodes.Add(treeNode);
                        }
                        else
                        {
                            // collision: adjust. All its clones will pachenko into the same slot.

                            unchecked
                            {
                                ++nodeDatum.Hash_AllFiles;
                            }

                            if (0 == nodeDatum.Hash_AllFiles)
                                ++nodeDatum.Hash_AllFiles;

                            continue;
                        }
                    }
                    else if (0 < nodeDatum.LengthTotal)
                    {
                        dictNodes[nodeDatum.Hash_AllFiles] = new List<LocalTreeNode> { treeNode };
                    }

                    break;
                }

                if (null != treeNode.Nodes)
                    SetDictNodes(dictNodes, treeNode.Nodes);      // recurse
            }
        }

        const string _ksDupeFileDictKey = "Matching duplicate files";
        const string _ksFolderTreeKey = "Setting up the directory tree";

        Tree
            _tree = null;
        bool
            _bFileDictDone = false;
        bool
            _bTreeDone = false;
    }
}
