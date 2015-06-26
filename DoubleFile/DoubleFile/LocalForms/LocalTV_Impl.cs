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
            Collate.ClearMem();

            if ((null != _allNodes) &&
                (false == _allNodes.IsEmpty()))
            {
                _dictVolumeInfo.Clear();
            }

            _dictNodes = null;           // m_dictNodes is tested to recreate tree.
            _allNodes = null;
            _rootNodes = null;
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
                    MBoxStatic.Assert(99954, false);
                    return false;
                }
            }

            if (false == _lvProjectVM.ItemsCast.Any(lvItem => lvItem.CanLoad))
                return false;

            _bTreeDone = false;
            App.FileDictionary.ResetAbortFlag();

            var lsProgressItems = new List<string> { _ksFolderTreeKey };

            if (App.FileDictionary.IsEmpty)
                lsProgressItems.Insert(0, _ksFileDictKey);

            (_winProgress = new WinProgress(new string[lsProgressItems.Count], lsProgressItems, x =>
            {
                if (App.FileDictionary.IsEmpty)
                    App.FileDictionary.DoThreadFactory(_lvProjectVM, new WeakReference<ICreateFileDictStatus>(this));

                TabledString<Tabled_Folders>.GenerationStarting();

                if (null == _dictNodes)
                    _dictNodes = new ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>();

                _tree =
                    new Tree(_lvProjectVM, _dictNodes, _dictVolumeInfo, new WeakReference<ITreeStatus>(this))
                    .DoThreadFactory();
            })
            {
                Title = "Initializing Explorer",
                WindowClosingCallback = new WeakReference<IWinProgressClosing>(this)
            })
                .ShowDialog();

            TabledString<Tabled_Folders>.GenerationEnded();
            _winProgress = null;
            return _bTreeDone;
        }

        void ICreateFileDictStatus.Callback(bool bDone, double nProgress)
        {
            if (App.LocalExit ||
                (null == App.FileDictionary) ||
                App.FileDictionary.IsAborted)
            {
                WithWinProgress(w => w
                    .SetAborted());

                return;
            }

            if (bDone)
            {
                WithWinProgress(w => w
                    .SetCompleted(_ksFileDictKey)
                    .CloseIfNatural());

                _bFileDictDone = true;
            }
            else if (0 <= nProgress)
            {
                WithWinProgress(w => w
                    .SetProgress(_ksFileDictKey, nProgress));
            }
        }
        
        void ITreeStatus.Status(LVitem_ProjectVM volStrings, LocalTreeNode rootNode, bool bError)
        {
            if (App.LocalExit ||
                (null == App.FileDictionary) ||
                App.FileDictionary.IsAborted ||
                ((null != _tree) && (_tree.IsAborted)))
            {
                ClearMem_TreeForm();

                WithWinProgress(w => w
                    .SetAborted());

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

                WithWinProgress(w => w
                    .SetProgress(_ksFolderTreeKey, _rootNodes.Length * _knProgMult));
            }
            else
            {
                MBoxStatic.Assert(1304.5309m, false);
            }
        }

        void ITreeStatus.Done()
        {
            if ((null == _rootNodes) ||
                _rootNodes.IsEmpty())
            {
                WithWinProgress(w => w
                    .SetAborted()
                    .Close());

                return;
            }

            _topNode = _rootNodes[0];
            LocalTreeNode.SetLevel(_rootNodes);
            _tree = null;
            Collate.ClearMem();

            var localLVclones = new LocalLV();
            var localLVsameVol = new LocalLV();
            var localLVsolitary = new LocalLV();
            var lsLocalLVignore = new List<LocalLVitem>();  // when implementing, replace the Forms ListViewItem.Tag in LocalLVItem
            var nProgress = 0d;

            using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .Subscribe(x => WithWinProgress(w => w.SetProgress(_ksFolderTreeKey, (3 + nProgress) / 4))))
            {
                var lsTreeNodes = new List<LocalTreeNode>();

                var collate = new Collate(
                    _dictNodes,
                    localLVclones, localLVsameVol, localLVsolitary,
                    _rootNodes, lsTreeNodes,
                    lsLVignore: lsLocalLVignore, bLoose: true);

                var dtStart = DateTime.Now;

                collate.Step1(d => nProgress = d);
                Util.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds / 1000d + " seconds.");
                dtStart = DateTime.Now;

                if (App.LocalExit)
                {
                    Collate.ClearMem();
                    return;
                }

                WithWinProgress(w => w
                    .SetCompleted(_ksFolderTreeKey));

                collate.Step2();

                if (null == LocalTV.SelectedNode)      // gd.m_bPutPathInFindEditBox is set in TreeDoneCallback()
                    LocalTV.SelectedNode = _topNode;
                _allNodes = lsTreeNodes.ToArray();
                Util.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000d + " seconds.");
            }

            Collate.ClearMem();
            _bTreeDone = true;      // should preceed closing status dialog: returns true to the caller

            WithWinProgress(w => w
                .CloseIfNatural());

            _dictNodes = null;      // saving memory here.
        }

        bool IWinProgressClosing.ConfirmClose()
        {
            if (false == Util.Closure(() =>
            {
                if (App.FileDictionary
                    .IsAborted)
                {
                    return true;
                }

                if (_bFileDictDone &&
                    _bTreeDone)
                {
                    return true;
                }

                return
                    (MessageBoxResult.Yes ==
                    MBoxStatic.ShowDialog("Do you want to cancel?", WithWinProgress(w => w.Title), MessageBoxButton.YesNo, _winProgress));
            }))
            {
                return false;
            }

            App.FileDictionary
                .Abort();
                    
            if (null != _tree)
                _tree.EndThread();

            _tree = null;
            Collate.ClearMem();
            return true;
        }

        T WithWinProgress<T>(Func<WinProgress, T> doSomethingWith) where T: class
        {
            return (null != _winProgress)
                ? doSomethingWith(_winProgress)
                : null;
        }
        WinProgress _winProgress = null;
        
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
