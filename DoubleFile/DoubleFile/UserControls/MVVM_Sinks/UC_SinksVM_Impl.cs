using System;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_SinksVM : IDisposable
    {
        public ICommand Icmd_Nicknames { get; private set; }
        public bool UseNicknames { get; set; }

        internal bool
            IsDisposed { get; private set; } = false;
        internal LocalModernWindowBase
            LocalOwner = null;

        static internal IObservable<Tuple<Tuple<LVitem_ProjectVM, string>, decimal>>
            GoToFolder => _goToFolder;
        static readonly LocalSubject<Tuple<LVitem_ProjectVM, string>> _goToFolder = new LocalSubject<Tuple<LVitem_ProjectVM, string>>();
        static void GoToFolderOnNext(Tuple<LVitem_ProjectVM, string> value) => _goToFolder.LocalOnNext(value, 99990);

        internal UC_SinksVM Init()
        {
            Icmd_Nicknames = new RelayCommand(() => _nicknameUpdater.UpdateViewport(UseNicknames));
            _nicknameUpdater.UpdateViewport(UseNicknames);
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            return this;
        }

        internal UC_SinksVM Reconnect()
        {
            _nicknameUpdater.UpdateViewport(UseNicknames);
            RaisePropertyChanged("UseNicknames");

            if (0 < Items.Count)
                RaiseItems();

            RaisePropertyChanged("SelectedItem");
            return this;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        bool GenerationStarting_And_FullPathFound(string strPath)
        {
            if (0 == Statics.WithLVprojectVM(p => p?.CanLoadCount ?? 0))
                return true;        // found there are no volumes loaded

            _nicknameUpdater.Clear();
            ClearItems();
            NoItemsVisibility = Visibility.Collapsed;
            RaisePropertyChanged("NoItemsVisibility");

            var treeNode = LocalTV.GetOneNodeByRootPath(strPath, null);

            if (null == treeNode)
            {
                return false;
            }

            Util.UIthread(99985, () =>
                Add(new LVitem_SinksVM(new LVitemProject_Updater<bool>(treeNode.RootNodeDatum.LVitemProjectVM, _nicknameUpdater), treeNode)));

            return true;
        }

        void GoTo()
        {
            if (null == _selectedItem)
            {
                Util.Assert(99976, false);    // binding should dim the button
                return;
            }

            _selectedItem.Folder?.GoToFile(null);
        }


        readonly ListUpdater<bool>
            _nicknameUpdater = new ListUpdater<bool>(99902);
    }
}
