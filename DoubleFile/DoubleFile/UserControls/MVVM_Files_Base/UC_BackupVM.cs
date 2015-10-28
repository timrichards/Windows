using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_BackupVM : UC_FolderListVM_Base
    {
        internal bool
            IsDisposed { get; private set; } = false;

        public string FolderSel => _folderSel?.PathFullGet(UseNicknames);
        LocalTreeNode _folderSel = null;

        public ICommand Icmd_Pick { get; }
        public ICommand Icmd_Remove { get; }

        public Visibility ProgressbarVisibility { get; private set; } = Visibility.Visible;

        public LV_FilesVM_Compare LV_Files { get; }

        bool CanPick { set { _bCanPick = value; Util.UIthread(99913, () => CommandManager.InvalidateRequerySuggested()); } }
        bool _bCanPick;

        internal UC_BackupVM()
        {
            Icmd_Pick = new RelayCommand(Add, () => _bCanPick);
            Icmd_Remove = new RelayCommand(() => Items.Remove(_selectedItem), () => null != _selectedItem);
            LV_Files = new LV_FilesVM_Compare();
            _folderSel = LocalTV.TreeSelect_FolderDetail?.treeNode;

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

        void RaisePathFull()
        {
            RaisePropertyChanged("FolderSel");
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
                    MBoxStatic.ShowOverlay("The folder you selected is a child or parent of a folder already in the list."); //, owner: LocalOwner);
                    return;
                }

                if (ItemsCast.Take(1).Any(lvItem => false == ReferenceEquals(_folderSel.Root, lvItem.TreeNode.Root)))
                {
                    MBoxStatic.ShowOverlay("Only one volume at a time is currently supported."); //, owner: LocalOwner);
                    return;
                }

                Util.UIthread(99582, () => Add(new LVitem_FolderListVM(_folderSel, _nicknameUpdater)));
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
                        LV_Files.Add(lsFiles.Select(asLine =>
                        new LVitem_CompareVM { TreeNode = asLine.Item1, FileLine = asLine.Item2 })));
                }

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
