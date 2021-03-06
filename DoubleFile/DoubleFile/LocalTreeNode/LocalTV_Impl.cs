﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Linq;
using DokanNet;

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
            _bFileDictDone = false == Statics.DupeFileDictionary.IsEmpty || (Statics.Namespace != GetType().Namespace);     // VolTreemap assembly: skip it

            if (false == _bFileDictDone)
                lsProgressItems.Insert(0, _ksDupeFileDictKey);

            Util.Block(1000);
            GC.Collect();

            new ProgressOverlay(new string[lsProgressItems.Count], lsProgressItems, x =>
            {
                Util.ThreadMake(() =>
                {
                    if (false == _bFileDictDone)
                        Statics.DupeFileDictionary.DoThreadFactory(_lvProjectVM, new WeakReference<ICreateDupeFileDictStatus>(this));

                    TabledString<TabledStringType_Folders>.GenerationStarting();

                    if (Statics.DupeFileDictionary.IsAborted)
                        return;     // from lambda

                    (_tree =
                        new Tree(
                        _lvProjectVM,
                        new TreeBase(_dictVolumeInfo, new WeakReference<ITreeStatus>(this))))
                        .Go();

                    if (null == _topNode)
                        TabledString<TabledStringType_Folders>.GenerationEnded();
                });
            })
            {
                Title = "Initializing Explorer",
                WindowClosingCallback = new WeakReference<IProgressOverlayClosing>(this)
            }
                .ShowOverlay();

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
            _tree = null;

            var nCount = ((ILocalTreeNode_SetLevel)_topNode).Set(_rootNodes);
            double nProgress = 0;

            using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .LocalSubscribe(99761, x => ProgressOverlay.WithProgressOverlay(w => w.SetProgress(_ksFolderTreeKey, (1 + nProgress) / 2))))
            {
                while (false == _bFileDictDone)     // this block is here is to SetAllFilesHashes() 
                    Util.Block(200);

                _progress = 0;

                double nProgressDenominator = 2 * nCount;

                using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                    .LocalSubscribe(99614, x => nProgress = _progress / nProgressDenominator * 4 / 5))
                {
                    AllFileHashes_AddRef();             //  _progress
                    SetAllFilesHashes(RootNodes);       //  _progress
                }

                IDictionary<int, List<LocalTreeNode>> dictNodes = new Dictionary<int, List<LocalTreeNode>>();

                {
                    var dictLength = new Dictionary<int, ulong>();
                    var dictNodes_ = new Dictionary<int, List<LocalTreeNode>>();

                    SetDictNodes(dictNodes_, RootNodes);

                    foreach (var kvp in dictNodes_)
                    {
                        var hashes = kvp.Value.First().NodeDatum.Hashes_SubnodeFiles_Scratch;

                        Util.Assert(99598, kvp.Value.All(treeNode => treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch.SequenceEqual(hashes)));
                        dictLength[kvp.Key] = kvp.Value.Max(treeNode => treeNode.NodeDatum.LengthTotal);
                    }

                    var nFolderIndexedID = 1;   // first folder ID is 1: zero is all empty folders: no unchecked/overflow/wraparound (<<1.5M is reasonable)

                    foreach (var kvp in dictLength.OrderByDescending(kvp => kvp.Value))     // sort by total length descending
                    {
                        var lsTreeNodes = dictNodes_[kvp.Key];

                        foreach (var treeNode in lsTreeNodes)
                            ((ISetNodeDatum_Hash_AllFiles)treeNode.NodeDatum).Set(nFolderIndexedID);

                        dictNodes[nFolderIndexedID] = lsTreeNodes;
                        ++nFolderIndexedID;
                    }
                }

                _allNodes = new List<LocalTreeNode> { };

                var stopwatch = Stopwatch.StartNew();

                new Collate
                {
                    RootNodes = _rootNodes,
                    DictNodes = dictNodes,
                    AllNodes = _allNodes,
                    LVclones = _lvClones,
                    LVsameVol = _lvSameVol,
                    LVsolitary = _lvSolitary,
                }
                    .Go(d => nProgress = (4 + d) / 5);

                stopwatch.Stop();
                Util.WriteLine("collate.Go " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
                AllFileHashes_DropRef();
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
            Util.ThreadMake(() => { RootNodes[0].Mount("s:\\", DokanOptions.DebugMode | DokanOptions.StderrOutput); });
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
