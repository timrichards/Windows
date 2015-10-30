using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_BackupVM : UC_FolderListVM_Base
    {
        internal bool
            IsDisposed { get; private set; } = false;
        internal LocalModernWindowBase
            LocalOwner = null;

        public string FolderSel => _folderSel?.PathFullGet(UseNicknames);
        LocalTreeNode _folderSel = null;

        public ICommand Icmd_Pick { get; }
        public ICommand Icmd_Remove { get; } 
        public ICommand Icmd_DestVolume { get; }
        public ICommand Icmd_Backup { get; }

        public string DestVolume { get; private set; }
        public string DriveLetter { private get; set; }

        public Visibility ProgressbarVisibility { get; private set; } = Visibility.Visible;

        public Visibility
            VisibilityOnItems => Items.Any() ? Visibility.Visible : Visibility.Collapsed;

        public LV_FilesVM_Compare LV_Files { get; }

        bool CanPick { set { _bCanPick = value; Util.UIthread(99913, () => CommandManager.InvalidateRequerySuggested()); } }
        bool _bCanPick;

        internal UC_BackupVM()
        {
            Icmd_Pick = new RelayCommand(Add, () => _bCanPick);
            Icmd_Remove = new RelayCommand(() => { Items.Remove(_selectedItem); Update(); }, () => null != _selectedItem);
            LV_Files = new LV_FilesVM_Compare();
            _folderSel = LocalTV.TreeSelect_FolderDetail?.treeNode;

            Icmd_DestVolume = new RelayCommand(() =>
            {
                var dlg = new FolderBrowserDialog { Description = "Destination to back up to." };

                if (false == ModalThread.Go(darkWindow => dlg.ShowDialog((Window)darkWindow)))
                    return;     // from lambda

                DestVolume = dlg.SelectedPath;
                RaisePropertyChanged("DestVolume");
            });

            Icmd_Backup = new RelayCommand(() => { }, () => (false == (string.IsNullOrEmpty(DestVolume) || string.IsNullOrEmpty(DriveLetter))));

            Util.ThreadMake(() =>
            {
                LocalTV.AllFileHashes_AddRef();

                _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable
                    .LocalSubscribe(99591, tuple => { _folderSel = tuple.Item1.treeNode; RaisePropertyChanged("FolderSel"); }));

                CanPick = true;
                ProgressbarVisibility = Visibility.Collapsed;
                RaisePropertyChanged("ProgressbarVisibility");
            });
        }

        public override void Dispose()
        {
            IsDisposed = true;
            LocalTV.AllFileHashes_DropRef();
            base.Dispose();
        }

        internal UC_BackupVM
            Init()
        {
            Icmd_GoTo = new RelayCommand(() => _selectedItem.TreeNode.GoToFile(null), () => null != _selectedItem);
            Icmd_Nicknames = new RelayCommand(RaisePathFull);
            return this;
        }

        internal bool
            CheckDriveLetter(char driveLetter) =>
            ItemsCast.Select(lvItem => lvItem.TreeNode)
            .All(treeNode => Directory.Exists(driveLetter + treeNode.PathFullGet(false).Substring(1)));

        void RaisePathFull()
        {
            RaisePropertyChanged("FolderSel");
            RaisePropertyChanged("VisibilityOnItems");
            _nicknameUpdater.UpdateViewport(UseNicknames);
        }

        void Add()
        {
            Util.ThreadMake(() =>
            {
                if (ItemsCast.Any(lvItem => _folderSel == lvItem.TreeNode))
                    return;

                if (ItemsCast.Any(lvItem => _folderSel.IsChildOf(lvItem.TreeNode) || lvItem.TreeNode.IsChildOf(_folderSel)))
                {
                    MBoxStatic.ShowOverlay("The folder you selected is a child or parent of a folder already in the list.", owner: LocalOwner);
                    return;
                }

                if (ItemsCast.Take(1).Any(lvItem => false == ReferenceEquals(_folderSel.Root, lvItem.TreeNode.Root)))
                {
                    MBoxStatic.ShowOverlay("Only one volume at a time is currently supported.", owner: LocalOwner);
                    return;
                }

                Util.UIthread(99582, () => Add(new LVitem_FolderListVM(_folderSel, _nicknameUpdater)));
                Update();
            });
        }

        void Update()
        {
            Util.ThreadMake(() =>
            {
                LV_Files.ClearItems();

                _selectedItem = null;
                RaisePathFull();

                CanPick = false;

                IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
                    ieHashesGrouped = new Tuple<LocalTreeNode, IReadOnlyList<int>>[] { };

                foreach (var treeNode in ItemsCast.Select(lvItem => lvItem.TreeNode).OrderBy(treeNode => treeNode.NodeDatum.PrevLineNo))
                    ieHashesGrouped = ieHashesGrouped.Concat(GetHashesHere(treeNode));

                var lsFiles = LocalTreeNode.GetFileLines(ieHashesGrouped);

                if (0 < lsFiles.Count)
                {
                    Util.UIthread(99581, () =>
                        LV_Files.Add(lsFiles.Take(1 << 9).Select(asLine =>
                        new LVitem_CompareVM { TreeNode = asLine.Item1, FileLine = asLine.Item2 })));
                }

                RaisePathFull();
                CanPick = true;
            });
        }

        IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
            GetHashesHere(LocalTreeNode treeNode)
        {
            var foundSet =
                treeNode.NodeDatum.Hashes_FilesHere.Where(nFileID => false == (Statics.DupeFileDictionary.IsDupeSepVolume(nFileID) ?? false))
                .ToList();

            IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
                ieFiles =
                (0 < foundSet.Count)
                ? new[] { Tuple.Create(treeNode, (IReadOnlyList<int>)foundSet.ToArray()) }
                : new Tuple<LocalTreeNode, IReadOnlyList<int>>[] { };

            if (null == treeNode.Nodes)
                return ieFiles;

            foreach (var subNode in treeNode.Nodes)
                ieFiles = ieFiles.Concat(GetHashesHere(subNode));      // recurse

            return ieFiles;
        }
    }
}
