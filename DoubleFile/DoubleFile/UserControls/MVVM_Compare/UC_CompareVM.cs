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
            var lsDiff1_ = lsFolder1.Except(lsIntersect_).ToList();
            var lsDiff2_ = lsFolder2.Except(lsIntersect_).ToList();

            lsFolder1 = null;
            lsFolder2 = null;
            Results = "These folders have ";

            if (0 == lsIntersect_.Count)
            {
                Results += "nothing in common.";
                RaisePropertyChanged("Results");
                return this;
            }

            Results += lsIntersect_.Count + " files in common. " + lsDiff1_.Count + " and " + lsDiff2_.Count + " files are unique in each.";
            RaisePropertyChanged("Results");

            var lsIntersect = GetFileLines(_folder1, lsIntersect_).OrderBy(asLine => asLine[0]).ToList();
            var lsDiff1 = GetFileLines(_folder1, lsDiff1_).OrderBy(asLine => asLine[0]).ToList();
            var lsDiff2 = GetFileLines(folder2, lsDiff2_).OrderBy(asLine => asLine[0]).ToList();

            Util.UIthread(99606, () =>
            {
                LV_Both.ClearItems();
                LV_First.ClearItems();
                LV_Second.ClearItems();
                LV_Both.Add(lsIntersect.Select(asLine => new LVitem_FilesVM { FileLine = asLine }));
                LV_First.Add(lsDiff1.Select(asLine => new LVitem_FilesVM { FileLine = asLine }));
                LV_Second.Add(lsDiff2.Select(asLine => new LVitem_FilesVM { FileLine = asLine }));
            });

            NoResultsVisibility = Visibility.Collapsed;
            RaisePropertyChanged("NoResultsVisibility");
            return this;
        }

        IEnumerable<IReadOnlyList<string>>
            GetFileLines(LocalTreeNode treeNode, List<int> lsHashes)
        {
            var retVal = GetFileLines_(treeNode, lsHashes);

            LocalTreeNode.GetFileList_Done();
            Util.Assert(99608, 0 == lsHashes.Count);
            return retVal;
        }

        IEnumerable<IReadOnlyList<string>>
            GetFileLines_(LocalTreeNode treeNode, List<int> lsHashes)
        {
            IEnumerable<IReadOnlyList<string>> ieFiles =  new string[][] { };

            if (0 == lsHashes.Count)
                return ieFiles;

            if (null != treeNode.Nodes)
            {
                foreach (var subNode in treeNode.Nodes)
                {
                    ieFiles = ieFiles.Concat(GetFileLines_(subNode, lsHashes));

                    if (0 == lsHashes.Count)
                        return ieFiles;
                }
            }

            var lsHashesHere = new List<int> { };

            foreach (var nHash in treeNode.NodeDatum.Hashes_FilesHere)
            {
                var nIx = lsHashes.IndexOf(nHash);

                if (0 > nIx)
                    continue;

                lsHashesHere.Add(nHash);
                lsHashes.RemoveAt(nIx);
            }

            if (0 == lsHashesHere.Count)
                return ieFiles;

            var nHashColumn =
                Statics.DupeFileDictionary.AllListingsHashV2
                ? 11
                : 10;

            return
                ieFiles.Concat(
                treeNode.GetFileList(bReadAllLines: true)
                .Select(strLine => strLine.Split('\t'))                                 // makes this an LV line: knColLengthLV -------v
                .Where(asLine => nHashColumn < asLine.Length)
                .Select(asLine => new { a = HashTuple.HashcodeFromString(asLine[nHashColumn]), b = (IReadOnlyList<string>) asLine.Skip(3).ToArray() })
                .Where(sel => lsHashesHere.Contains(sel.a))
                .Select(sel => sel.b));
        }

        LocalTreeNode
            _folder1;
        readonly List<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
