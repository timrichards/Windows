using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DoubleFile
{
    class UC_FolderListVM : UC_FolderListVM_Base
    {
        public string FoldersHeader { get; private set; } = _ksFoldersHeader;
        const string _ksFoldersHeader = "Folders";
        protected void
            SetFoldersHeader(string strHeader = null) { FoldersHeader = _ksFoldersHeader + " " + strHeader; RaisePropertyChanged("FoldersHeader"); }

        public Visibility ProgressbarVisibility { get; protected set; } = Visibility.Visible;
        public Visibility NoResultsVisibility { get; protected set; } = Visibility.Visible;
        public string NoResultsText { get; protected set; } = null;

        protected void                              // new to hide then call base.Init() in the derived class and return itself
            Init()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            Icmd_Nicknames = new RelayCommand(() => _nicknameUpdater.UpdateViewport(UseNicknames));
            _nicknameUpdater.Clear();           // in case Init() is reused on an existing list: future proof
            _nicknameUpdater.UpdateViewport(UseNicknames);

            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99701, initiatorTuple =>
                StartSearch(initiatorTuple.Item1)));
        }

        public override void                        // call base.Dispose() in the derived class
            Dispose()
        {
            _bDisposed = true;
            _cts.Cancel();
            base.Dispose();
        }

        protected void                              // HideProgressbar() is useful in the derived classes
            HideProgressbar()
        {
            ProgressbarVisibility = Visibility.Collapsed;
            RaisePropertyChanged("ProgressbarVisibility");
        }

        protected virtual IEnumerable<LVitem_FolderListVM>
            FillList(LocalTreeNode searchFolder) { Util.Assert(99618, false); return null; }

        protected virtual void
            Clear() { }

        protected void StartSearch() => StartSearch(null);
        protected void StartSearch(TreeSelect.FolderDetailUpdated folderDetail)
        {
            var searchFolder = folderDetail?.treeNode ?? _prevSearchFolder;

            if (null == searchFolder)
                return;

            Util.ThreadMake(() =>
            {
                _cts.Cancel();

                while (_bSearching)
                    Util.Block(20);

                _bSearching = true;
                _cts = new CancellationTokenSource();
                _prevSearchFolder = searchFolder;
                ClearItems();
                SetFoldersHeader();
                NoResultsVisibility = Visibility.Collapsed;
                RaisePropertyChanged("NoResultsVisibility");
                ProgressbarVisibility = Visibility.Visible;
                RaisePropertyChanged("ProgressbarVisibility");

                if (_bDisposed)
                    return;     // from lambda

                if (null != searchFolder.Nodes)
                {
                    var ieLVitems = FillList(searchFolder);

                    if (ieLVitems?.Any() ?? false)
                        Util.UIthread(99619, () => Add(ieLVitems));

                    Clear();
                }

                if (false == Items.Any())
                {
                    NoResultsText = searchFolder.PathShort;
                    RaisePropertyChanged("NoResultsText");
                    NoResultsVisibility = Visibility.Visible;
                    RaisePropertyChanged("NoResultsVisibility");
                }

                HideProgressbar();
                _bSearching = false;
            });
        }

        void GoTo()
        {
            if (null == _selectedItem)
            {
                Util.Assert(99897, false);      // binding should dim the button
                return;
            }

            _selectedItem.TreeNode.GoToFile(null);
        }

        LocalTreeNode
            _prevSearchFolder = null;
        bool
            _bSearching = false;
        bool
            _bDisposed = false;
        protected CancellationTokenSource
            _cts { get; private set; } = new CancellationTokenSource();
    }
}
