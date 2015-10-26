using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_BackupVM : ObservableObjectBase, IDisposable
    {
        internal bool
            IsDisposed { get; private set; } = false;

        public string FolderSel => _folderSel?.PathFullGet(UseNicknames);
        LocalTreeNode _folderSel = null;

        public string Folder => _folder?.PathFullGet(UseNicknames);
        LocalTreeNode _folder = null;

        public string Results { get; private set; }

        public ICommand Icmd_Pick { get; }
        public ICommand Icmd_GoTo1 { get; }
        public ICommand Icmd_GoTo { get; }

        public ICommand Icmd_Nicknames { get; }
        public bool UseNicknames { get; set; }

        public Visibility ProgressbarVisibility { get; private set; } = Visibility.Visible;
        public Visibility NoResultsVisibility { get; private set; } = Visibility.Visible;
        public string NoResultsText { get; private set; } = "setting up Compare view";

        public LV_FilesVM_Compare LV_Files { get; }
        public LV_FilesVM_Compare LV_Folders { get; }

        bool CanPick { set { _bCanPick = value; Util.UIthread(99777, () => CommandManager.InvalidateRequerySuggested()); } }
        bool _bCanPick;

        internal UC_BackupVM()
        {
            Icmd_Pick = new RelayCommand(() => { _folder = LocalTV.TreeSelect_FolderDetail.treeNode; Update(); }, () => _bCanPick);
            Icmd_GoTo1 = new RelayCommand(() => _folder?.GoToFile(null), () => _bCanPick && (null != _folder));
            Icmd_GoTo = new RelayCommand(() => _selectedItem.TreeNode.GoToFile(_selectedItem.Filename), () => null != _selectedItem);
            LV_Files = new LV_FilesVM_Compare();
            LV_Folders = new LV_FilesVM_Compare();
            Icmd_Nicknames = new RelayCommand(RaisePathFull);
            _folderSel = LocalTV.TreeSelect_FolderDetail?.treeNode;

            Util.ThreadMake(() =>
            {
                _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable
                    .LocalSubscribe(99613, tuple => { _folderSel = tuple.Item1.treeNode; RaisePropertyChanged("FolderSel"); }));

                NoResultsText = null;
                RaisePropertyChanged("NoResultsText");
                ProgressbarVisibility = Visibility.Collapsed;
                RaisePropertyChanged("ProgressbarVisibility");
                CanPick = true;
            });
        }

        public void Dispose()
        {
            Util.ThreadMake(() =>
            {
                Util.LocalDispose(_lsDisposable);
                _folder = null;
                IsDisposed = true;
            });
        }

        void RaisePathFull()
        {
            RaisePropertyChanged("FolderSel");
            RaisePropertyChanged("Folder1");
            RaisePropertyChanged("Folder2");
        }

        internal UC_BackupVM
            Update(LocalTreeNode folderSel = null)
        {
            Util.ThreadMake(() =>
            {
                LV_Files.ClearItems();
                LV_Folders.ClearItems();

                if (null == folderSel)
                    folderSel = LocalTV.TreeSelect_FolderDetail.treeNode;

                _selectedItem = null;
                RaisePathFull();
                Results = "Same, nested, or no folder selected";
                RaisePropertyChanged("Results");
                NoResultsVisibility = Visibility.Visible;
                RaisePropertyChanged("NoResultsVisibility");

                if (null == _folder)
                    return;     // from lamnda

                NoResultsVisibility = Visibility.Collapsed;
                RaisePropertyChanged("NoResultsVisibility");
                ProgressbarVisibility = Visibility.Visible;
                RaisePropertyChanged("ProgressbarVisibility");
                CanPick = false;
                Update_(folderSel);
                ProgressbarVisibility = Visibility.Collapsed;
                RaisePropertyChanged("ProgressbarVisibility");
                CanPick = true;
            });

            return this;
        }
        UC_BackupVM Update_(LocalTreeNode folderSel)
        {
            return this;
        }

        IReadOnlyList<Tuple<LocalTreeNode, IReadOnlyList<string>>>
            GetFileLines(LocalTreeNode treeNode, IEnumerable<int> ieHashes)
        {
            IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<string>>> ieFiles = new Tuple<LocalTreeNode, IReadOnlyList<string>>[] { };
            var searchSet = new HashSet<int>(ieHashes);

            if (false == searchSet.Any())
                return ieFiles.ToList();

            var lsHashesGrouped =
                GetHashesHere(treeNode, ref searchSet)
                .OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo)
                .ToList();

            Util.Assert(99608, false == searchSet.Any());

            if (0 == lsHashesGrouped.Count)
                return ieFiles.ToList();

            var nHashColumn = Statics.DupeFileDictionary.HashColumn;

            foreach (var tuple in lsHashesGrouped)
            {
                ieFiles =
                    ieFiles.Concat(
                    tuple.Item1.GetFileList(bReadAllLines: true)
                    .Select(strLine => strLine.Split('\t'))
                    .Where(asLine => nHashColumn < asLine.Length)
                    .DistinctBy(asLine => asLine[nHashColumn])
                    .Select(asLine => new { a = HashTuple.FileIndexedIDfromString(asLine[nHashColumn], asLine[FileParse.knColLength]), b = asLine })
                    .Where(sel => tuple.Item2.Contains(sel.a))
                    .Select(sel => Tuple.Create(tuple.Item1, (IReadOnlyList<string>)sel.b.Skip(3).ToArray())));
            }                                       // makes this an LV line: knColLengthLV----^

            return ieFiles.OrderBy(tuple => tuple.Item2[0]).ToList();   // ToList() enumerates: reads through the file exactly once and closes it
        }

        IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
            GetHashesHere(LocalTreeNode treeNode, ref HashSet<int> searchSet, List<int> lsExpectNone = null)
        {
            var foundSet = new HashSet<int>(searchSet.Intersect(treeNode.NodeDatum.Hashes_FilesHere));
            var newSearchSet = searchSet.Except(foundSet);
#if (DEBUG && FOOBAR)
            if (null != lsExpectNone)
                lsExpectNone.AddRange(searchSet.Except(newSearchSet));
#endif
            searchSet = new HashSet<int>(newSearchSet);

            IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
                ieFiles =
                (0 < foundSet.Count)
                ? new[] { Tuple.Create(treeNode, (IReadOnlyList<int>)foundSet.ToArray()) }
                : new Tuple<LocalTreeNode, IReadOnlyList<int>>[] { };

            if ((false == searchSet.Any()) ||
                (null == treeNode.Nodes))
            {
                return ieFiles;
            }

            if (false == searchSet.Intersect(treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch).Any())
#if (DEBUG && FOOBAR)
                lsExpectNone = new List<int> { };
#else
                return ieFiles;
#endif

            foreach (var subNode in treeNode.Nodes)
            {
                if (false == searchSet.Any())
                    return ieFiles;

                ieFiles = ieFiles.Concat(GetHashesHere(subNode, ref searchSet, lsExpectNone));
#if (DEBUG && FOOBAR)
                if (0 < lsExpectNone?.Count)
                    Util.Assert(99604, false);
#endif
            }

            return ieFiles;
        }

        LVitem_CompareVM
            _selectedItem;
        readonly List<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
