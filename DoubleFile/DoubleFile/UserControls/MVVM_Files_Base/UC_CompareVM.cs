using MoreLinq;
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

        public string FolderSel => _folderSel?.PathFullGet(UseNicknames);
        LocalTreeNode _folderSel = null;

        public string Folder1 => _folder1?.PathFullGet(UseNicknames);
        LocalTreeNode _folder1 = null;

        public string Folder2 => _folder2?.PathFullGet(UseNicknames);
        LocalTreeNode _folder2 = null;

        public string Results { get; private set; }

        public ICommand Icmd_Pick1 { get; }
        public ICommand Icmd_Pick2 { get; }
        public ICommand Icmd_GoTo1 { get; }
        public ICommand Icmd_GoTo2 { get; }
        public ICommand Icmd_GoTo { get; }

        public ICommand Icmd_Nicknames { get; }
        public bool UseNicknames { get; set; }

        public Visibility ProgressbarVisibility { get; private set; } = Visibility.Visible;
        public Visibility NoResultsVisibility { get; private set; } = Visibility.Visible;
        public string NoResultsText { get; private set; } = "setting up Compare view";

        public LV_FilesVM_Compare LV_Both { get; }
        public LV_FilesVM_Compare LV_First { get; }
        public LV_FilesVM_Compare LV_Second { get; }

        internal UC_CompareVM()
        {
            Icmd_Pick1 = new RelayCommand(() => { _folder1 = LocalTV.TreeSelect_FolderDetail.treeNode; Update(); });
            Icmd_Pick2 = new RelayCommand(() => { _folder2 = LocalTV.TreeSelect_FolderDetail.treeNode; Update(); });
            Icmd_GoTo1 = new RelayCommand(() => _folder1?.GoToFile(null), () => null != _folder1);
            Icmd_GoTo2 = new RelayCommand(() => _folder2?.GoToFile(null), () => null != _folder2);
            Icmd_GoTo = new RelayCommand(() => _selectedItem.TreeNode.GoToFile(_selectedItem.Filename), () => null != _selectedItem);
            LV_Both = new LV_FilesVM_Compare { SelectedItemChanged = v => { _selectedItem = v; LV_First.ClearSelection(); LV_Second.ClearSelection(); } };
            LV_First = new LV_FilesVM_Compare { SelectedItemChanged = v => { _selectedItem = v; LV_Both.ClearSelection(); LV_Second.ClearSelection(); } };
            LV_Second = new LV_FilesVM_Compare { SelectedItemChanged = v => { _selectedItem = v; LV_First.ClearSelection(); LV_Both.ClearSelection(); } };
            Icmd_Nicknames = new RelayCommand(RaisePathFull);

            Util.ThreadMake(() =>
            {
                LocalTV.AllFileHashes_AddRef();

                _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable
                    .LocalSubscribe(99613, tuple => { _folderSel = tuple.Item1.treeNode; RaisePropertyChanged("FolderSel"); }));

                NoResultsText = null;
                RaisePropertyChanged("NoResultsText");
                ProgressbarVisibility = Visibility.Collapsed;
                RaisePropertyChanged("ProgressbarVisibility");
            });

            _folderSel = LocalTV.TreeSelect_FolderDetail?.treeNode;
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

        void RaisePathFull()
        {
            RaisePropertyChanged("FolderSel");
            RaisePropertyChanged("Folder1");
            RaisePropertyChanged("Folder2");
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

            _selectedItem = null;
            RaisePathFull();
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

            NoResultsVisibility = Visibility.Collapsed;
            RaisePropertyChanged("NoResultsVisibility");
            ProgressbarVisibility = Visibility.Visible;
            RaisePropertyChanged("ProgressbarVisibility");

            const int kMax = 1 << 7;
            var lsFolder1 = _folder1.NodeDatum.Hashes_FilesHere.Union(_folder1.NodeDatum.Hashes_SubnodeFiles_Scratch).Distinct().ToList();
            var lsFolder2 = _folder2.NodeDatum.Hashes_FilesHere.Union(_folder2.NodeDatum.Hashes_SubnodeFiles_Scratch).Distinct().ToList();
            var lsIntersect_ = lsFolder1.Intersect(lsFolder2).Distinct().ToList();
            var lsDiff1_ = lsFolder1.Except(lsIntersect_).Take(kMax).ToList();
            var lsDiff2_ = lsFolder2.Except(lsIntersect_).Take(kMax).ToList();

            lsIntersect_ = lsIntersect_.Take(kMax).ToList();
            lsFolder1 = null;
            lsFolder2 = null;

            Util.Closure(() =>
            {
                if (0 == lsIntersect_.Count)
                {
                    Results = "These folders have nothing in common.";
                    RaisePropertyChanged("Results");
                    return;     // from lambda
                }

                Results = null;
                RaisePropertyChanged("Results");

                var lsIntersect = GetFileLines(_folder1, lsIntersect_).OrderBy(tuple => tuple.Item2[0]).ToList();
                var lsDiff1 = GetFileLines(_folder1, lsDiff1_).OrderBy(tuple => tuple.Item2[0]).ToList();
                var lsDiff2 = GetFileLines(_folder2, lsDiff2_).OrderBy(tuple => tuple.Item2[0]).ToList();
                Func<int, string> f = n => ((n < kMax) ? "" + n : "at least " + n) + " file" + ((1 != n) ? "s" : "");

                Util.Assert(99602, lsIntersect.Count == lsIntersect_.Count);
                Util.Assert(99601, lsDiff1.Count == lsDiff1_.Count);
                Util.Assert(99600, lsDiff2.Count == lsDiff2_.Count);
                Results = f(lsIntersect_.Count) + " in common; " + f(lsDiff1_.Count) + " and " + f(lsDiff2_.Count) + " unique respectively.";
                RaisePropertyChanged("Results");

                Util.UIthread(99606, () =>
                {
                    if (0 < lsIntersect.Count)
                        LV_Both.Add(lsIntersect.Select(asLine => new LVitem_CompareVM { TreeNode = asLine.Item1, FileLine = asLine.Item2 }));

                    if (0 < lsDiff1.Count)
                        LV_First.Add(lsDiff1.Select(asLine => new LVitem_CompareVM { TreeNode = asLine.Item1, FileLine = asLine.Item2 }));

                    if (0 < lsDiff2.Count)
                        LV_Second.Add(lsDiff2.Select(asLine => new LVitem_CompareVM { TreeNode = asLine.Item1, FileLine = asLine.Item2 }));
                });

                NoResultsVisibility = Visibility.Collapsed;
                RaisePropertyChanged("NoResultsVisibility");
            });

            ProgressbarVisibility = Visibility.Collapsed;
            RaisePropertyChanged("ProgressbarVisibility");
            return this;
        }

        IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<string>>>
            GetFileLines(LocalTreeNode treeNode, IEnumerable<int> ieHashes)
        {
            IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<string>>> ieFiles = new Tuple<LocalTreeNode, IReadOnlyList<string>>[] { };
            var searchSet = new HashSet<int>(ieHashes);

            if (false == searchSet.Any())
                return ieFiles;

            var hashesHere = GetHashesHere(treeNode, ref searchSet);

            var lsHashesGrouped =
                hashesHere
                .OrderBy(tuple => tuple.Item1.NodeDatum.LineNo)
                .ToList();

            Util.Assert(99608, false == searchSet.Any());

            if (0 == lsHashesGrouped.Count)
                return ieFiles;

            var nHashColumn =
                Statics.DupeFileDictionary.AllListingsHashV2
                ? 11
                : 10;

            foreach (var tuple in lsHashesGrouped)
            {
                var fileList = tuple.Item1.GetFileList(bReadAllLines: true);

                ieFiles =
                    ieFiles.Concat(
                    fileList
                    .Select(strLine => strLine.Split('\t'))
                    .Where(asLine => nHashColumn < asLine.Length)
                    .DistinctBy(asLine => asLine[nHashColumn])
                    .Select(asLine => new { a = HashTuple.HashCodeFromString(asLine[nHashColumn], asLine[FileParse.knColLength]), b = asLine })
                    .Where(sel => tuple.Item2.Contains(sel.a))
                    .Select(sel => Tuple.Create(tuple.Item1, (IReadOnlyList<string>)sel.b.Skip(3).ToArray())));
            }                                       // makes this an LV line: knColLengthLV----^

            LocalTreeNode.GetFileList_Done();
            return ieFiles;
        }

        IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>>
            GetHashesHere(LocalTreeNode treeNode, ref HashSet<int> searchSet, List<int> lsExpectNone = null)
        {
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
                if (false == searchSet.Any())
                    return ieFiles;

                ieFiles = ieFiles.Concat(GetHashesHere(subNode, ref searchSet, lsExpectNone));
#if (DEBUG)
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
