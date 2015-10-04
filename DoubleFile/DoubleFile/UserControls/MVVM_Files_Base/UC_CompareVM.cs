using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_CompareVM : ObservableObjectBase, IDisposable
    {
        internal bool
            IsDisposed { get; private set; } = false;

        public string Folder1 { get; private set; }
        public string Folder2 { get; private set; }
        public string Results { get; private set; }

        public ICommand Icmd_Pick { get; }

        public Visibility ProgressbarVisibility { get; private set; } = Visibility.Visible;
        public Visibility NoResultsVisibility { get; private set; } = Visibility.Visible;
        public string NoResultsFolder { get; private set; } = "setting up Compare view";

        public LV_FilesVM_Compare LV_Both { get; }
        public LV_FilesVM_Compare LV_First { get; }
        public LV_FilesVM_Compare LV_Second { get; }

        internal UC_CompareVM()
        {
            Icmd_Pick = new RelayCommand(() => { _folder1 = LocalTV.TreeSelect_FolderDetail.treeNode; Update(); });
            LV_Both = new LV_FilesVM_Compare();
            LV_First = new LV_FilesVM_Compare();
            LV_Second = new LV_FilesVM_Compare();

            Util.ThreadMake(() =>
            {
                LocalTV.AllFileHashes_AddRef();

                _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable
                    .LocalSubscribe(99613, tuple => Update(tuple.Item1.treeNode)));

                NoResultsFolder = null;
                RaisePropertyChanged("NoResultsFolder");
                ProgressbarVisibility = Visibility.Collapsed;
                RaisePropertyChanged("ProgressbarVisibility");
            });
        }

        public void Dispose()
        {
            Util.ThreadMake(() =>
            {
                LocalTV.AllFileHashes_DropRef();
                Util.LocalDispose(_lsDisposable);
                _folder1 = null;
                IsDisposed = true;
            });
        }

        internal UC_CompareVM
            Update(LocalTreeNode folder2 = null)
        {
            Util.ThreadMake(() => Update_(folder2));
            return this;
        }

        UC_CompareVM
            Update_(LocalTreeNode folder2)
        {
            LV_Both.ClearItems();
            LV_First.ClearItems();
            LV_Second.ClearItems();

            if (null == folder2)
                folder2 = LocalTV.TreeSelect_FolderDetail.treeNode;

            if (null == _folder1)
            {
                Folder1 = folder2.PathFull;
                RaisePropertyChanged("Folder1");
                return this;
            }

            Folder1 = _folder1.PathFull;
            RaisePropertyChanged("Folder1");
            Results = null;
            RaisePropertyChanged("Results");
            NoResultsVisibility = Visibility.Visible;
            RaisePropertyChanged("NoResultsVisibility");

            if (_folder1.IsChildOf(folder2))
                return this;

            if (folder2.IsChildOf(_folder1))
                return this;

            if (ReferenceEquals(_folder1, folder2))
                return this;

            Folder2 = folder2.PathFull;
            RaisePropertyChanged("Folder2");

            var lsFolder1 = _folder1.NodeDatum.Hashes_FilesHere.Union(_folder1.NodeDatum.Hashes_SubnodeFiles_Scratch).Distinct().ToList();
            var lsFolder2 = folder2.NodeDatum.Hashes_FilesHere.Union(folder2.NodeDatum.Hashes_SubnodeFiles_Scratch).Distinct().ToList();
            var lsIntersect_ = lsFolder1.Intersect(lsFolder2).Distinct().ToList();
            var lsDiff1_ = lsFolder1.Except(lsIntersect_).Take(1 << 7).ToList();
            var lsDiff2_ = lsFolder2.Except(lsIntersect_).Take(1 << 7).ToList();

            lsFolder1 = null;
            lsFolder2 = null;
            Results = "These folders have ";

            if (0 == lsIntersect_.Count)
            {
                Results += "nothing in common.";
                RaisePropertyChanged("Results");
                return this;
            }

            var lsIntersect = GetFileLines(_folder1, lsIntersect_).OrderBy(asLine => asLine[0]).ToList();
            var lsDiff1 = GetFileLines(_folder1, lsDiff1_).OrderBy(asLine => asLine[0]).ToList();
            var lsDiff2 = GetFileLines(folder2, lsDiff2_).OrderBy(asLine => asLine[0]).ToList();

            Results += lsIntersect_.Count + " files in common. " + lsDiff1_.Count + " and " + lsDiff2_.Count + " files are unique in each.";
            RaisePropertyChanged("Results");

            Util.UIthread(99606, () =>
            {
                if (0 < lsIntersect.Count)
                    LV_Both.Add(lsIntersect.Select(asLine => new LVitem_FilesVM { FileLine = asLine }));

                if (0 < lsDiff1.Count)
                    LV_First.Add(lsDiff1.Select(asLine => new LVitem_FilesVM { FileLine = asLine }));

                if (0 < lsDiff2.Count)
                    LV_Second.Add(lsDiff2.Select(asLine => new LVitem_FilesVM { FileLine = asLine }));
            });

            NoResultsVisibility = Visibility.Collapsed;
            RaisePropertyChanged("NoResultsVisibility");
            return this;
        }

        IEnumerable<IReadOnlyList<string>>
            GetFileLines(LocalTreeNode treeNode, IEnumerable<int> ieHashes)
        {
            var hashSet = new HashSet<int>(ieHashes);

            var lsHashesGrouped =
                GetHashesHere(treeNode, hashSet)
                .OrderBy(tuple => tuple.Item1.NodeDatum.LineNo)
                .ToList();

            IEnumerable<IReadOnlyList<string>> ieFiles = new string[][] { };

            if (0 == lsHashesGrouped.Count)
                return ieFiles;

            var nHashColumn =
                Statics.DupeFileDictionary.AllListingsHashV2
                ? 11
                : 10;

            foreach (var tuple in lsHashesGrouped)
            {
                ieFiles =
                    ieFiles.Concat(
                    tuple.Item1.GetFileList(bReadAllLines: true)
                    .Select(strLine => strLine.Split('\t'))
                    .Where(asLine => nHashColumn < asLine.Length)                          // makes this an LV line: knColLengthLV -------v
                    .Select(asLine => new { a = HashTuple.HashcodeFromString(asLine[nHashColumn]), b = (IReadOnlyList<string>)asLine.Skip(3).ToArray() })
                    .Where(sel => tuple.Item2.Contains(sel.a))
                    .Select(sel => sel.b));
            }

            LocalTreeNode.GetFileList_Done();
            Util.Assert(99608, 0 == hashSet.Count);
            return ieFiles;
        }

        IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
            GetHashesHere(LocalTreeNode treeNode, HashSet<int> lsHashes)
        {
            if (0 == lsHashes.Count)
                return new Tuple<LocalTreeNode, IReadOnlyList<int>>[] { };

            var lsHashesHere = new List<int> { };

            foreach (var nHash in treeNode.NodeDatum.Hashes_FilesHere)
            {
                if (false == lsHashes.Contains(nHash))
                    continue;

                lsHashesHere.Add(nHash);
                lsHashes.Remove(nHash);
            }

            IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
                ieFiles =
                (0 < lsHashesHere.Count)
                ? new[] { Tuple.Create(treeNode, (IReadOnlyList<int>)lsHashesHere) }
                : new Tuple<LocalTreeNode, IReadOnlyList<int>>[] { };

            if ((0 == lsHashes.Count) ||
                (null == treeNode.Nodes))
            {
                return ieFiles;
            }

            foreach (var subNode in treeNode.Nodes)
            {
                ieFiles = ieFiles.Concat(GetHashesHere(subNode, lsHashes));

                if (0 == lsHashes.Count)
                    return ieFiles;
            }

            return ieFiles;
        }

        LocalTreeNode
            _folder1;
        readonly List<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
