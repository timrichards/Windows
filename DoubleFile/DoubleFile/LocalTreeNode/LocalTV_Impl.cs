using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Linq;

namespace DoubleFile
{
    partial class LocalTV : IWinProgressClosing, ICreateFileDictStatus, ITreeStatus
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
            Statics.FileDictionary.ResetAbortFlag();

            var lsProgressItems = new List<string> { _ksFolderTreeKey };

            if (Statics.FileDictionary.IsEmpty)
                lsProgressItems.Insert(0, _ksFileDictKey);

            (new WinProgress(new string[lsProgressItems.Count], lsProgressItems, x =>
            {
                if (Statics.FileDictionary.IsEmpty)
                    Statics.FileDictionary.DoThreadFactory(_lvProjectVM, new WeakReference<ICreateFileDictStatus>(this));

                TabledString<TabledStringType_Folders>.GenerationStarting();

                if (Statics.FileDictionary.IsAborted)
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
                WindowClosingCallback = new WeakReference<IWinProgressClosing>(this)
            })
                .ShowDialog();

            if (null == _topNode)
                TabledString<TabledStringType_Folders>.GenerationEnded();

            return _bTreeDone;
        }

        void ICreateFileDictStatus.Callback(bool bDone, double nProgress)
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                Statics.FileDictionary.IsAborted)
            {
                WinProgress.WithWinProgress(w => w
                    .Abort());

                return;
            }

            if (bDone)
            {
                WinProgress.WithWinProgress(w => w
                    .SetCompleted(_ksFileDictKey)
                    .CloseIfNatural());

                _bFileDictDone = true;
            }
            else if (0 <= nProgress)
            {
                WinProgress.WithWinProgress(w => w
                    .SetProgress(_ksFileDictKey, nProgress));
            }
        }
        
        void ITreeStatus.Status(LVitem_ProjectVM volStrings, LocalTreeNode rootNode, bool bError)
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                Statics.FileDictionary.IsAborted ||
                (_tree?.IsAborted ?? false))
            {
                ClearMem_TreeForm();

                WinProgress.WithWinProgress(w => w
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

                WinProgress.WithWinProgress(w => w
                    .SetProgress(_ksFolderTreeKey, _rootNodes.Count * _knProgMult));
            }
        }

        void ITreeStatus.Done()
        {
            if (0 == _rootNodes.Count)
            {
                WinProgress.CloseForced();
                return;
            }

            TabledString<TabledStringType_Folders>.GenerationEnded();
            _rootNodes.Sort((x, y) => x.Text.LocalCompare(y.Text));
            _topNode = _rootNodes[0];
            LocalTreeNode.SetLevel(_rootNodes);
            _tree = null;

            var lsLocalLVignore = new List<LVitem_ClonesVM>();  // when implementing, replace the Forms ListViewItem.Tag in LocalLVItem
            var nProgress = 0d;

            using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .LocalSubscribe(99761, x => WinProgress.WithWinProgress(w => w.SetProgress(_ksFolderTreeKey, (3 + nProgress) / 4))))
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

                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                    return;

                WinProgress.WithWinProgress(w => w
                    .SetCompleted(_ksFolderTreeKey));

                collate.Step2();

                if (null == _selectedNode)      // gd.m_bPutPathInFindEditBox is set in TreeDoneCallback()
                    _selectedNode = _topNode;

                Util.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000d + " seconds.");
            }

            // Mean square from ANOVA: ((folder mean minus total mean) squared * folder total) == ss / (number of folders minus one == df)

            var grandTotalMean =
                Util.Closure(() =>
            {
                var grandTotalFolderScore = new[] { 0U, 0U, 0U }  // Weighted folder scores: HashParity (random); largest; smallest
                    .AsEnumerable();

                var grandTotalFileCount = 0U;

                foreach (var folder in _rootNodes)
                {
                    grandTotalFolderScore =
                        grandTotalFolderScore
                        .Zip(folder.NodeDatum.FolderScore, (n1, n2) => n1 + n2);

                    grandTotalFileCount += folder.NodeDatum.FileCountTotal;
                }

                return grandTotalFolderScore.Select(n => n / (double)grandTotalFileCount)
                    .ToArray();     // from lambda
            });

            var dt = DateTime.Now;

            Util.ParallelForEach(_allNodes, folder => folder.NodeDatum.FolderScore = // mean square
                grandTotalMean
                .Zip(folder.NodeDatum.FolderScore, (totalMean, folderScore) =>
            {
                var mean = folderScore / (double)folder.NodeDatum.FileCountTotal;
                var meanDiff = (mean - totalMean);
                var sumOfSquares = meanDiff * meanDiff * folder.NodeDatum.FileCountTotal;
                return (uint) (sumOfSquares / (_allNodes.Count - 1));      // from lamnda
            })
                .ToArray());

            Util.WriteLine("Completed ANOVA in " + (DateTime.Now - dt).TotalMilliseconds + " ms");   // 350 ms
            _allNodes.Sort((y, x) => x.NodeDatum.FolderScore[1].CompareTo(y.NodeDatum.FolderScore[1]));
            _bTreeDone = true;      // should preceed closing status dialog: returns true to the caller

            WinProgress.WithWinProgress(w => w
                .CloseIfNatural());

            _dictNodes = null;      // saving memory here.
        }

        bool IWinProgressClosing.ConfirmClose()
        {
            if (false == Util.Closure(() =>
            {
                if (Statics.FileDictionary
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
                    (MessageBoxResult.Yes == WinProgress.WithWinProgress(w =>
                    MBoxStatic.ShowDialog("Do you want to cancel?", w.Title, MessageBoxButton.YesNo, w)));
            }))
            {
                return false;       // Iff user said no then cancel and don't close.
            }

            Util.WriteLine("IWinProgressClosing.ConfirmClose B");

            Statics.FileDictionary
                .Abort();
                    
            _tree?.EndThread();
            _tree = null;
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
        bool
            _bTreeDone = false;
    }
}
