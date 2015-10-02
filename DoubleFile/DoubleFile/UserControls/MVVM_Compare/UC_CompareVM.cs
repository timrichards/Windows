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

        public ICommand Icmd_Pick { get; private set; }

        public Visibility ProgressbarVisibility { get; protected set; } = Visibility.Visible;
        public Visibility NoResultsVisibility { get; protected set; } = Visibility.Visible;
        public string NoResultsFolder { get; protected set; } = "setting up Compare view";

        internal UC_CompareVM()
        {
            Icmd_Pick = new RelayCommand(() => { _folder1 = LocalTV.TreeSelect_FolderDetail.treeNode; Connect(); });

            Util.ThreadMake(() =>
            {
                LocalTV.AllFileHashes_AddRef();

                _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable
                    .LocalSubscribe(99613, tuple => Connect(tuple.Item1.treeNode)));

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
            Connect(LocalTreeNode folder2 = null)
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

            var lsFolder1 = _folder1.NodeDatum.Hashes_FilesHere.Concat(_folder1.NodeDatum.Hashes_SubnodeFiles_Scratch).ToList();
            var lsFolder2 = folder2.NodeDatum.Hashes_FilesHere.Concat(folder2.NodeDatum.Hashes_SubnodeFiles_Scratch).ToList();
            var lsIntersect = lsFolder1.Intersect(lsFolder2).ToList();
            var lsDiff = lsFolder1.Union(lsFolder2).Except(lsIntersect).ToList();

            Results = "These folders have ";

            if (0 == lsIntersect.Count)
            {
                Results += "nothing in common.";
                RaisePropertyChanged("Results");
                return this;
            }

            Results += lsIntersect.Count + " files in common. " + lsDiff.Count + " files are unique.";
            RaisePropertyChanged("Results");

       //     Results = string.Join(" ", lsIntersect);
            NoResultsVisibility = Visibility.Collapsed;
            RaisePropertyChanged("NoResultsVisibility");
            return this;
        }

        LocalTreeNode
            _folder1;
        readonly List<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
