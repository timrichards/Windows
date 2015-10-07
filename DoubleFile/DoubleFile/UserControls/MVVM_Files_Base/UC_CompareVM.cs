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

        public string FolderSel { get; private set; }
        public string Folder1 { get; private set; }
        public string Folder2 { get; private set; }
        public string Results { get; private set; }

        public ICommand Icmd_Pick1 { get; }
        public ICommand Icmd_Pick2 { get; }

        public Visibility ProgressbarVisibility { get; private set; } = Visibility.Visible;
        public Visibility NoResultsVisibility { get; private set; } = Visibility.Visible;
        public string NoResultsFolder { get; private set; } = "setting up Compare view";

        public LV_FilesVM_Compare LV_Both { get; }
        public LV_FilesVM_Compare LV_First { get; }
        public LV_FilesVM_Compare LV_Second { get; }

        internal UC_CompareVM()
        {
            Icmd_Pick1 = new RelayCommand(() => { _folder1 = LocalTV.TreeSelect_FolderDetail.treeNode; Update(); });
            Icmd_Pick2 = new RelayCommand(() => { _folder2 = LocalTV.TreeSelect_FolderDetail.treeNode; Update(); });
            LV_Both = new LV_FilesVM_Compare();
            LV_First = new LV_FilesVM_Compare();
            LV_Second = new LV_FilesVM_Compare();

            Util.ThreadMake(() =>
            {
                LocalTV.AllFileHashes_AddRef();

                _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable
                    .LocalSubscribe(99613, tuple => { FolderSel = tuple.Item1.treeNode.PathFull; RaisePropertyChanged("FolderSel"); }));

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
            Update(LocalTreeNode folderSel = null)
        {
            Util.ThreadMake(() => Update_(folderSel));
            return this;
        }

        UC_CompareVM
            Update_(LocalTreeNode folderSel)
        {
            LV_Both.ClearItems();
            LV_First.ClearItems();
            LV_Second.ClearItems();

            if (null == folderSel)
                folderSel = LocalTV.TreeSelect_FolderDetail.treeNode;

            FolderSel = folderSel.PathFull;
            RaisePropertyChanged("FolderSel");
            Folder1 = _folder1?.PathFull;
            RaisePropertyChanged("Folder1");
            Folder2 = _folder2?.PathFull;
            RaisePropertyChanged("Folder2");
            Results = "Same, nested, or no folder selected";
            RaisePropertyChanged("Results");
            NoResultsVisibility = Visibility.Visible;
            RaisePropertyChanged("NoResultsVisibility");

            if (null == _folder1)
                return this;

            if (null == _folder2)
                return this;

            if (ReferenceEquals(_folder1, _folder2))
                return this;

            if (_folder1.IsChildOf(_folder2))
                return this;

            if (_folder2.IsChildOf(_folder1))
                return this;

            var lsFolder1 = _folder1.NodeDatum.Hashes_FilesHere.Union(_folder1.NodeDatum.Hashes_SubnodeFiles_Scratch).Distinct().ToList();
            var lsFolder2 = _folder2.NodeDatum.Hashes_FilesHere.Union(_folder2.NodeDatum.Hashes_SubnodeFiles_Scratch).Distinct().ToList();
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
            var lsDiff2 = GetFileLines(_folder2, lsDiff2_).OrderBy(asLine => asLine[0]).ToList();

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
            var searchSet = new HashSet<int>(ieHashes);

            var lsHashesGrouped =
                GetHashesHere(treeNode, ref searchSet)
                .OrderBy(tuple => tuple.Item1.NodeDatum.LineNo)
                .ToList();

            Util.Assert(99608, false == searchSet.Any());

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
                    .Select(asLine => new { a = HashTuple.FileIndexedIDFromString(asLine[nHashColumn]), b = (IReadOnlyList<string>)asLine.Skip(3).ToArray() })
                    .Where(sel => tuple.Item2.Contains(sel.a))
                    .Select(sel => sel.b));
            }

            LocalTreeNode.GetFileList_Done();
            return ieFiles;
        }

        IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
            GetHashesHere(LocalTreeNode treeNode, ref HashSet<int> searchSet, List<int> lsExpectNone = null)
        {
            if (false == searchSet.Any())
                return new Tuple<LocalTreeNode, IReadOnlyList<int>>[] { };

            var foundSet = new HashSet<int>(searchSet.Intersect(treeNode.NodeDatum.Hashes_FilesHere));
            var newSearchSet = searchSet.Except(foundSet);
#if (DEBUG)
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
#if (DEBUG)
                lsExpectNone = new List<int> { };
#else
                return ieFiles;
#endif

            foreach (var subNode in treeNode.Nodes)
            {
                ieFiles = ieFiles.Concat(GetHashesHere(subNode, ref searchSet, lsExpectNone));
#if (DEBUG)
                if (0 < lsExpectNone?.Count)
                    Util.Assert(99604, false);
#endif
                if (false == searchSet.Any())
                    return ieFiles;
            }

            return ieFiles;
        }

        LocalTreeNode
            _folder1;
        LocalTreeNode
            _folder2;
        readonly List<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
