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

            LV_Both.Add(new LVitem_FilesVM { FileLine = new[] { "1", "1", "1", "1", "1", "1", "1", "1", "1", "1", "1", "1", "1", "1", "1", } });

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

            var lsFolder1 = _folder1.NodeDatum.Hashes_FilesHere.Union(_folder1.NodeDatum.Hashes_SubnodeFiles_Scratch).ToList();
            var lsFolder2 = folder2.NodeDatum.Hashes_FilesHere.Union(folder2.NodeDatum.Hashes_SubnodeFiles_Scratch).ToList();
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
