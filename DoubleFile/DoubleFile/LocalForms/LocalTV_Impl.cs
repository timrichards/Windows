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
        internal void TreeCleanup()
        {
            Tree = null;
            Collate.ClearMem();
        }

        internal void ClearMem_TreeForm()
        {
            TreeCleanup();

            if (false == _arrTreeNodes.IsEmpty())
                _dictVolumeInfo.Clear();

            DictNodes = null;           // m_dictNodes is tested to recreate tree.
            _arrTreeNodes = null;
            _nodes = null;
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

            if (null == DictNodes)
                DictNodes = new ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>();

            Tree =
                new Tree(_lvProjectVM, DictNodes, _dictVolumeInfo, new WeakReference<ITreeStatus>(this))
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
                lock (_nodesSemaphore)
            {
                if (null == _nodes)
                {
                    _nodes = new[] { rootNode };
                }
                else
                {
                    // The root volume list is very small so this copy-sort is viable
                    var ls = new List<LocalTreeNode>(_nodes);

                    ls.Insert(ls.TakeWhile(node => rootNode.Text.CompareTo(node.Text) > 0).Count(), rootNode);
                    _nodes = ls.ToArray();
                }

                _winProgress.SetProgress(_ksFolderTreeKey, _nodes.Length / _nCorrelateProgressDenominator * 3 / 4.0);
            }
            else
            {
                MBoxStatic.Assert(1304.5309, false);
            }
        }

        void ITreeStatus.Done()
        {
            if (_nodes.IsEmpty())
            {
                _winProgress.Aborted = true;
                UtilProject.UIthread(_winProgress.Close);
                return;
            }

            TopNode = _nodes[0];
            LocalTreeNode.SetLevel(_nodes);
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
                    DictNodes,
                    localLVclones, localLVsameVol, localLVsolitary,
                    _nodes, lsTreeNodes,
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
                _arrTreeNodes = lsTreeNodes.ToArray();
                UtilProject.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds.");
            }

            TreeCleanup();
            TabledString<Tabled_Folders>.GenerationEnded();
            _winProgress.CloseIfNatural();
            DictNodes = null;       // saving memory here.
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

            MainWindow.FileDictionary
                .Abort();
                    
            if (null != Tree)
                Tree.EndThread();

            TreeCleanup();
            return true;
        }
        
        const string _ksFileDictKey = "Creating file dictionary";
        const string _ksFolderTreeKey = "Creating folder tree browser";

        bool _bFileDictDone = false;

        readonly Dictionary<string, string>
            _dictVolumeInfo = new Dictionary<string, string>();
    }
}
