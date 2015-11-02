﻿using MoreLinq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_BackupVM : UC_FolderListVM_Base, IProgressOverlayClosing
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

        public string FileCount { get; private set; }
        public string BackupSize { get; private set; }

        public string BackupPath { get; private set; }
        internal string DriveLetter { private get; set; }

        public Visibility ProgressbarVisibility { get; private set; } = Visibility.Visible;
        public string SettingUp { get; private set; } = "setting up Backup view";

        public Visibility
            VisibilityOnItems => Items.Any() ? Visibility.Visible : Visibility.Collapsed;

        bool CanPick
        {
            set
            {
                RaisePathFull();
                ProgressbarVisibility = value ? Visibility.Hidden : Visibility.Visible;
                RaisePropertyChanged("ProgressbarVisibility");
                _bCanPick = value;
                Util.UIthread(99913, () => CommandManager.InvalidateRequerySuggested());
            }
        }
        bool _bCanPick;

        void RaisePathFull()
        {
            RaisePropertyChanged("FileCount");
            RaisePropertyChanged("BackupSize");
            RaisePropertyChanged("FolderSel");
            RaisePropertyChanged("VisibilityOnItems");
            _nicknameUpdater.UpdateViewport(UseNicknames);
        }

        internal UC_BackupVM()
        {
            Icmd_Pick = new RelayCommand(Add, () => _bCanPick);
            Icmd_Remove = new RelayCommand(() => { Items.Remove(_selectedItem); Update(); }, () => null != _selectedItem);
            _folderSel = LocalTV.TreeSelect_FolderDetail?.treeNode;

            Icmd_DestVolume = new RelayCommand(() =>
            {
                var dlg = new FolderBrowserDialog { Description = "Destination to back up to." };

                if (false == ModalThread.Go(darkWindow => dlg.ShowDialog((Window)darkWindow)))
                    return;     // from lambda

                BackupPath = dlg.SelectedPath;
                RaisePropertyChanged("BackupPath");
            });

            Icmd_Backup = new RelayCommand(Go, () => (false == (string.IsNullOrEmpty(BackupPath) || string.IsNullOrEmpty(DriveLetter))));

            Util.ThreadMake(() =>
            {
                _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable
                    .LocalSubscribe(99591, tuple => { _folderSel = tuple.Item1.treeNode; RaisePropertyChanged("FolderSel"); }));

                LocalTV.AllFileHashes_AddRef();
                CanPick = true;
                SettingUp = null;
                RaisePropertyChanged("SettingUp");
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
                FileCount = null;
                BackupSize = null;
                _selectedItem = null;
                _lsFiles = new List<Tuple<LocalTreeNode, IReadOnlyList<string>>> { };
                CanPick = false;

                IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>> ieHashesGrouped = new Tuple<LocalTreeNode, IReadOnlyList<int>>[] { };
                _dictDupeFileHit = new ConcurrentDictionary<int, bool>();

                foreach (var treeNode in ItemsCast.Select(lvItem => lvItem.TreeNode))
                    ieHashesGrouped = ieHashesGrouped.Concat(GetHashesHere(treeNode));

                _dictDupeFileHit = null;

                var nHashColumn = Statics.DupeFileDictionary.HashColumn - 3;

                try
                {
                    var treeNode = ItemsCast.Select(lvItem => lvItem.TreeNode).FirstOrDefault();

                    if (null != treeNode)
                    {
                        _lsFiles =
                            treeNode
                            .GetFileLines(ieHashesGrouped)
                            .DistinctBy(tuple => HashTuple.FileIndexedIDfromString(tuple.Item2[nHashColumn], tuple.Item2[FileParse.knColLengthLV]))
                            .ToList();
                    }

                    ulong nLengthTotal = 0;

                    foreach (var tuple in _lsFiles)
                        nLengthTotal += tuple.Item2[FileParse.knColLengthLV].ToUlong();

                    FileCount = "" + _lsFiles.Count;
                    BackupSize = nLengthTotal.FormatSize(bytes: true);
                }
                catch (OutOfMemoryException)
                {
                    MBoxStatic.ShowOverlay("Out of memory exception.", owner: LocalOwner);
                }

                CanPick = true;
            });
        }

        void Go()
        {
            var strKey = "Backing up";

            new ProgressOverlay(new[] { "" }, new[] { strKey }, progress =>
            {
                Util.ThreadMake(() =>
                {
                    progress.WindowClosingCallback = new WeakReference<IProgressOverlayClosing>(this);
                    _bCancel = false;

                    try
                    {
                        bool? bIgnoreError = null;
                        double nCompleted = 0;      // double preserves mantissa

                        foreach (var tuple in _lsFiles)
                        {
                            if (_bCancel)
                                break;

                            var sourceFile = DriveLetter + tuple.Item1.PathFullGet(false).Substring(1) + "\\" + tuple.Item2[0];

                            if (false == File.Exists(sourceFile))
                            {
                                if (null == bIgnoreError)
                                    bIgnoreError = MessageBoxResult.Yes == MBoxStatic.ShowOverlay(tuple.Item2[0] + " does not exist. Continue without further warnings?", buttons: MessageBoxButton.YesNo, owner: LocalOwner);

                                if (false == bIgnoreError.Value)
                                    break;
                            }

                            int? nDupeFilename = null;
                            Func<string> backupPath = () => BackupPath + "\\" + tuple.Item2[0] + nDupeFilename?.ToString("_000");

                            while (File.Exists(backupPath()))
                                nDupeFilename = (nDupeFilename ?? 0) + 1;

                            File.Copy(sourceFile, backupPath());
                            progress.SetProgress(strKey, ++nCompleted / _lsFiles.Count);
                        }

                        if (false == _bCancel)
                            MBoxStatic.ShowOverlay("Backup completed, with _000 numeric incrememt suffix on duplicate filenames.", owner: LocalOwner);
                    }
                    catch (Exception e)
                    {
                        MBoxStatic.ShowOverlay("Exception backing up: " + e.GetBaseException().Message, owner: LocalOwner);
                    }
                    finally
                    {
                        ProgressOverlay.CloseForced();
                    }
                });
            })
                .ShowOverlay();
        }

        bool IProgressOverlayClosing.ConfirmClose()
        {
            if (_bCancel)
                return true;

            _bCancel =
                (MessageBoxResult.Yes == ProgressOverlay.WithProgressOverlay(w =>
                MBoxStatic.AskToCancel(w.Title)));

            return _bCancel;
        }

        IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
            GetHashesHere(LocalTreeNode treeNode)
        {
            var foundSet =
                treeNode.NodeDatum.Hashes_FilesHere.Where(nFileID =>
            {
                if (_dictDupeFileHit.ContainsKey(nFileID))
                    return false;     // from lambda

                _dictDupeFileHit[nFileID] = true;

                return false == (Statics.DupeFileDictionary.IsDupeSepVolume(nFileID) ?? false);     // from lambda
            })
                .ToList();

            IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
                ieFiles =
                (0 < foundSet.Count)
                ? new[] { Tuple.Create(treeNode, (IReadOnlyList<int>)foundSet.ToArray()) }
                : new Tuple<LocalTreeNode, IReadOnlyList<int>>[] { };

            if (null != treeNode.Nodes)
            {
                foreach (var subNode in treeNode.Nodes)
                    ieFiles = ieFiles.Concat(GetHashesHere(subNode));      // recurse
            }

            return ieFiles;
        }

        bool
            _bCancel = false;
        IDictionary<int, bool>
            _dictDupeFileHit = null;
        List<Tuple<LocalTreeNode, IReadOnlyList<string>>>
            _lsFiles = null;
    }
}