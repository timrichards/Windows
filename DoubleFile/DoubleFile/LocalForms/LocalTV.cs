using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LocalTV
    {
        static internal IReadOnlyList<LocalTreeNode>
            AllNodes { get { return Util.WR(_wr, o => o._allNodes); } }
        List<LocalTreeNode> _allNodes = new List<LocalTreeNode> { };

        static internal IReadOnlyList<LocalTreeNode>
            RootNodes { get { return Util.WR(_wr, o => o._rootNodes); } }
        List<LocalTreeNode> _rootNodes = new List<LocalTreeNode> { };

        static internal LocalTreeNode
            TopNode { get { return Util.WR(_wr, o => o._topNode); } }
        LocalTreeNode _topNode = null;

        static internal LocalTreeNode
            SelectedNode
        {
            get { return Util.WR(_wr, o => o._selectedNode); }
            set { Util.WR(_wr, o => o._selectedNode = value); }
        }
        LocalTreeNode _selectedNode = null;

        static internal LocalLVVM
            Clones { get { return Util.WR(_wr, o => o._clones); } }
        LocalLVVM _clones = new LocalLVVM();

        static internal LocalLVVM
            SameVol { get { return Util.WR(_wr, o => o._sameVol); } }
        LocalLVVM _sameVol = new LocalLVVM();

        static internal LocalLVVM
            Solitary { get { return Util.WR(_wr, o => o._solitary); } }
        LocalLVVM _solitary = new LocalLVVM();

        static internal IReadOnlyDictionary<string, string>
            DictVolumeInfo { get { return Util.WR(_wr, o => o._dictVolumeInfo); } }
        readonly Dictionary<string, string> _dictVolumeInfo = new Dictionary<string, string>();

        static internal TreeSelect.FileListUpdated
            TreeSelect_FileList { get { return Util.WR(_wr, o => o._treeSelect_FileList); } }
        TreeSelect.FileListUpdated _treeSelect_FileList;

        static internal TreeSelect.FolderDetailUpdated
            TreeSelect_FolderDetail { get { return Util.WR(_wr, o => o._treeSelect_FolderDetail); } }
        TreeSelect.FolderDetailUpdated _treeSelect_FolderDetail = null;

        static internal TreeSelect.VolumeDetailUpdated
            TreeSelect_VolumeDetail { get { return Util.WR(_wr, o => o._treeSelect_VolumeDetail); } }
        TreeSelect.VolumeDetailUpdated _treeSelect_VolumeDetail = null;

        static internal bool
            FactoryCreate(LV_ProjectVM lvProjectVM)
        {
            if (null != _instance)
            {
                MBoxStatic.Assert(99858, false);
                return false;
            }

            if (0 == lvProjectVM?.CanLoadCount)
                return false;

            _wr.SetTarget(
                _instance =
                new LocalTV(lvProjectVM));

            return WithLocalTV(localTV =>
                localTV.DoTree());
        }

        LocalTV(LV_ProjectVM lvProjectVM)
        {
            _lvProjectVM = lvProjectVM;
            _knProgMult = 3 / (4d * _lvProjectVM?.CanLoadCount ?? 0);

            if (0 < _lvProjectVM?.CanLoadCount)
                TabledString<Tabled_Folders>.AddRef();

            _lsDisposable.Add(WinDuplicatesVM.GoToFile.Subscribe(WinDuplicatesVM_GoToFile));
            _lsDisposable.Add(WinSearchVM.GoToFile.Subscribe(WinSearchVM_GoToFile));
            _lsDisposable.Add(TreeSelect.FileListUpdated.Observable.Subscribe(v => _treeSelect_FileList = v.Item1));
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.Subscribe(v => _treeSelect_FolderDetail = v.Item1));
            _lsDisposable.Add(TreeSelect.VolumeDetailUpdated.Observable.Subscribe(v => _treeSelect_VolumeDetail = v.Item1));
        }

        internal LocalTV
            LocalDispose()
        {
            if ((0 < _lvProjectVM?.CanLoadCount))
                TabledString<Tabled_Folders>.DropRef();

            Util.LocalDispose(_lsDisposable);
            _wr.SetTarget(_instance = null);
            return this;
        }

        static internal LocalTreeNode
            GetOneNodeByRootPathA(string strPath, LVitem_ProjectVM lvItemProjectVM)
        {
            return 
                (null != RootNodes)
                ? GetOneNodeByRootPath.Go(strPath, RootNodes, lvItemProjectVM)
                : null;
        }

        internal int
            GetNodeCount(bool includeSubTrees = false)
        {
            return includeSubTrees
                ? CountSubnodes(_rootNodes)
                : _rootNodes?.Count ?? 0;
        }

        static int
            CountSubnodes(IEnumerable<LocalTreeNode> nodes)
        {
            var nRet = 0;

            nodes?.ForEach(treeNode =>
                nRet += 1 + CountSubnodes(treeNode.Nodes));

            return nRet;
        }

        void
            WinDuplicatesVM_GoToFile(Tuple<Tuple<LVitem_ProjectVM, string, string>, int> initiatorTuple)
        {
            Util.Write("C"); GoToFile(initiatorTuple.Item1);
        }

        void
            WinSearchVM_GoToFile(Tuple<Tuple<LVitem_ProjectVM, string, string>, int> initiatorTuple)
        {
            Util.Write("D"); GoToFile(initiatorTuple.Item1);
        }

        void
            GoToFile(Tuple<LVitem_ProjectVM, string, string> tuple)
        {
            GetOneNodeByRootPathA(tuple.Item2, tuple.Item1)?
                .GoToFile(tuple.Item3);
        }

        static internal T
            WithLocalTV<T>(Func<LocalTV, T> doSomethingWith)
        {
            LocalTV localTV = null;

            _wr.TryGetTarget(out localTV);

            return
                (null != localTV)
                ? doSomethingWith(localTV)
                : default(T);
        }
        static readonly WeakReference<LocalTV>
            _wr = new WeakReference<LocalTV>(null);
        static LocalTV
            _instance = null;

        readonly LV_ProjectVM
            _lvProjectVM = null;
        readonly double
            _knProgMult = 0;
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
