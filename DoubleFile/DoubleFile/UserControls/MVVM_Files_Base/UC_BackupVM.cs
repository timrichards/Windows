using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_BackupVM : UC_FolderListVM_Base, IDisposable
    {
        internal bool
            IsDisposed { get; private set; } = false;

        public string FolderSel => _folderSel?.PathFullGet(UseNicknames);
        LocalTreeNode _folderSel = null;

        public ICommand Icmd_Pick { get; }
        public ICommand Icmd_Remove { get; }

        public LV_FilesVM_Compare LV_Files { get; }

        bool CanPick { set { _bCanPick = value; Util.UIthread(99777, () => CommandManager.InvalidateRequerySuggested()); } }
        bool _bCanPick;

        internal UC_BackupVM()
        {
            Icmd_Pick = new RelayCommand(() => { Add(new LVitem_FolderListVM(_folderSel, _nicknameUpdater)); Update(); }, () => _bCanPick);
            Icmd_GoTo = new RelayCommand(() => _selectedItem.TreeNode.GoToFile(null), () => null != _selectedItem);
            Icmd_Remove = new RelayCommand(() => Items.Remove(_selectedItem), () => null != _selectedItem);
            LV_Files = new LV_FilesVM_Compare();
            Icmd_Nicknames = new RelayCommand(RaisePathFull);
            _folderSel = LocalTV.TreeSelect_FolderDetail?.treeNode;

            Util.ThreadMake(() =>
            {
                LocalTV.AllFileHashes_AddRef();

                _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable
                    .LocalSubscribe(99613, tuple => { _folderSel = tuple.Item1.treeNode; RaisePropertyChanged("FolderSel"); }));

                CanPick = true;
            });
        }

        public override void Dispose()
        {
            LocalTV.AllFileHashes_DropRef();
            base.Dispose();
        }

        void RaisePathFull()
        {
            RaisePropertyChanged("FolderSel");
            RaisePropertyChanged("Folder");
        }

        internal UC_BackupVM
            Update()
        {
            Util.ThreadMake(() =>
            {
                LV_Files.ClearItems();

                _selectedItem = null;
                RaisePathFull();

                CanPick = false;

                CanPick = true;
            });

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
    }
}
