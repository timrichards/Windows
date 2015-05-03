using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class WinDoubleFile_FoldersVM : IWinProgressClosingCallback
    {
        internal ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>
            DictNodes { get; private set; }
        internal Tree
            Tree { get; private set; }

        internal LocalTreeNode[]
            _arrTreeNodes { get; private set; }
        internal readonly List<LocalTreeNode>
            _listRootNodes = new List<LocalTreeNode>();

        internal void TreeCleanup()
        {
            Tree = null;
            Collate.ClearMem();
        }

        internal void ClearMem_TreeForm()
        {
            TreeCleanup();

            if ((false == _arrTreeNodes.IsEmpty()) &&
                (null != _localTV))
            {
                _localTV._dictVolumeInfo.Clear();
            }

            DictNodes = null;           // m_dictNodes is tested to recreate tree.
            _arrTreeNodes = null;
            _listRootNodes.Clear();
        }

        internal void CreateFileDictStatusCallback(bool bDone = false, double nProgress = double.NaN)
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
        
        void TreeStatusCallback(LVitem_ProjectVM volStrings, LocalTreeNode rootNode = null, bool bError = false)
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
            {
                _listRootNodes.Add(rootNode);
                _winProgress.SetProgress(_ksFolderTreeKey, _listRootNodes.Count / _nCorrelateProgressDenominator * 3 / 4.0);
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
            _localTV._dictVolumeInfo = _dictVolumeInfo;

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
                    _listRootNodes, lsTreeNodes,
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
                _arrTreeNodes = lsTreeNodes.ToArray();
                UtilProject.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds.");
            }

            TreeCleanup();
            TabledString<Tabled_Folders>.GenerationEnded();
            _winProgress.CloseIfNatural();
            DictNodes = null;       // saving memory here.
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
            _winProgress.WindowClosingCallback = new WeakReference<IWinProgressClosingCallback>(this);

            var lsProgressItems = new List<string>();
            var fileDictionary = MainWindow.FileDictionary;

            fileDictionary.ResetAbortFlag();

            if (fileDictionary.IsEmpty)
            {
                lsProgressItems.Add(_ksFileDictKey);
                fileDictionary.DoThreadFactory(_lvProjectVM, CreateFileDictStatusCallback);
            }

            TabledString<Tabled_Folders>.GenerationStarting();

            if (null == DictNodes)
                DictNodes = new ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>();

            Tree =
                new Tree(_lvProjectVM, DictNodes, _dictVolumeInfo,
                    TreeStatusCallback, TreeDoneCallback)
                .DoThreadFactory();

            lsProgressItems.Add(_ksFolderTreeKey);

            _winProgress
                .InitProgress(new string[lsProgressItems.Count], lsProgressItems)
                .ShowDialog();
        }

        bool IWinProgressClosingCallback.WinProgressClosingCallback()
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
