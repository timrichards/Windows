using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DoubleFile
{
    partial class UC_FolderListVM_Base : IDisposable
    {
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

        public virtual void                         // call base.Dispose() in the derived class
            Dispose()
        {
            _bDisposed = true;
            _cts.Cancel();
            Util.LocalDispose(_lsDisposable);
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

            _prevSearchFolder = searchFolder;
            ClearItems();
            SetFoldersHeader();
            NoResultsVisibility = Visibility.Collapsed;
            RaisePropertyChanged("NoResultsVisibility");
            ProgressbarVisibility = Visibility.Visible;
            RaisePropertyChanged("ProgressbarVisibility");

            Util.ThreadMake(() =>
            {
                _cts.Cancel();

                while (_bSearching)
                    Util.Block(20);

                if (_bDisposed)
                    return;     // from lambda

                if (null != searchFolder.Nodes)
                {
                    _cts = new CancellationTokenSource();
                    _bSearching = true;

                    var ieLVitems = FillList(searchFolder);

                    if (ieLVitems?.Any() ?? false)
                        Util.UIthread(99619, () => Add(ieLVitems));

                    Clear();
                }

                if (false == Items.Any())
                {
                    NoResultsFolder = searchFolder.Text;
                    RaisePropertyChanged("NoResultsFolder");
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

            _selectedItem.Folder.GoToFile(null);
        }

        LocalTreeNode
            _prevSearchFolder = null;
        bool
            _bSearching = false;
        bool
            _bDisposed = false;
        protected CancellationTokenSource
            _cts { get; private set; } = new CancellationTokenSource();
        protected readonly ListUpdater<bool>
            _nicknameUpdater = new ListUpdater<bool>(99667);
        protected readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
