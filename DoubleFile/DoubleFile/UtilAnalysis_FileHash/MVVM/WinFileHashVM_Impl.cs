using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.Concurrent;

namespace DoubleFile
{
    delegate void CreateFileDictStatusDelegate(bool bDone = false, double nProgress = double.NaN);
    
    partial class WinFileHashVM
    {
        internal ConcurrentDictionary<FolderKeyStruct, UList<LocalTreeNode>>
            _dictNodes { get; private set; }
        internal readonly Dictionary<string, string>
            _dictDriveInfo = new Dictionary<string, string>();
        internal Local.Tree
            _tree = null;

        internal readonly UList<LocalTreeNode>
            _listTreeNodes = new UList<LocalTreeNode>();
        internal readonly List<LocalTreeNode>
            _listRootNodes = new List<LocalTreeNode>();

        internal void TreeCleanup()
        {
            _tree = null;
            Local.Collate.ClearMem();
        }

        internal void ClearMem_TreeForm()
        {
            TreeCleanup();

            _dictDriveInfo.Clear();

            // m_dictNodes is tested to recreate tree.
            _dictNodes = null;

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
            var lsLocalLVignore = new List<LocalLVitem>();
            var nProgress = 0.0;

            using (new SDL_Timer(() => { _winProgress.SetProgress(_ksFolderTreeKey, (3 + nProgress)/4.0); }).Start())
            {
                var collate = new Local.Collate(_gd, _dictNodes,
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
            _tvVM.SetData(_listRootNodes);
            _winProgress.CloseIfNatural();
            UString.GenerationEnded();

            // saving memory here.
            _dictNodes = null;
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

                _gd.FileDictionary
                    .Abort();
                    
                if (null != _tree)
                    _tree.EndThread();

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

            UString.GenerationStarting();

            if (null == _dictNodes)
                _dictNodes = new ConcurrentDictionary<FolderKeyStruct, UList<LocalTreeNode>>();

            _tree =
                new Local.Tree(_gd, _lvProjectVM, _dictNodes, _dictDriveInfo, TreeStatusCallback, TreeDoneCallback)
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
