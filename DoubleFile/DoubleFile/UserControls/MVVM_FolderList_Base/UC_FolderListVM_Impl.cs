using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DoubleFile
{
    partial class UC_FolderListVM : IDisposable
    {
        protected void                              // new to hide then call base.Init() in the derived class and return itself
            Init()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            Icmd_Nicknames = new RelayCommand(() => _nicknameUpdater.UpdateViewport(UseNicknames));
            _nicknameUpdater.Clear();           // in case Init() is reused on an existing list: future proof
            _nicknameUpdater.UpdateViewport(UseNicknames);
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99701, TreeSelect_FolderDetailUpdated));
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

        protected virtual void
            TreeSelect_FolderDetailUpdated(LocalTreeNode searchFolder) { }

        void TreeSelect_FolderDetailUpdated(Tuple<TreeSelect.FolderDetailUpdated, int> initiatorTuple)
        {
            ClearItems();
            NoResultsVisibility = Visibility.Collapsed;
            RaisePropertyChanged("NoResultsVisibility");
            ProgressbarVisibility = Visibility.Visible;
            RaisePropertyChanged("ProgressbarVisibility");

            Util.ThreadMake(() =>
            {
                _cts.Cancel();

                while (_bSearching)
                    Util.Block(20);

                _cts = new CancellationTokenSource();

                if (_bDisposed)
                    return;     // from lambda

                _bSearching = true;
                TreeSelect_FolderDetailUpdated(initiatorTuple.Item1.treeNode);

                if (false == Items.Any())
                {
                    NoResultsFolder = initiatorTuple.Item1.treeNode.Text;
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

            _selectedItem.LocalTreeNode.GoToFile(null);
        }

        bool
            _bSearching = false;
        protected bool
            _bDisposed { get; private set; }
        protected CancellationTokenSource
            _cts { get; private set; } = new CancellationTokenSource();
        protected readonly ListUpdater<bool>
            _nicknameUpdater = new ListUpdater<bool>(99667);
        protected readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
