﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LocalTV
    {
        static internal LV_ProjectVM
            LVprojectVM => _wr.Get(s => s._lvProjectVM);

        static internal IReadOnlyList<LocalTreeNode>
            AllNodes => _wr.Get(o => o._allNodes);
        List<LocalTreeNode> _allNodes = new List<LocalTreeNode> { };

        static internal IReadOnlyList<LocalTreeNode>
            RootNodes => _wr.Get(o => o._rootNodes);
        List<LocalTreeNode> _rootNodes = new List<LocalTreeNode> { };

        static internal IReadOnlyList<LocalTreeNode>
            Mounted => _wr.Get(o => o._mounted);
        List<LocalTreeNode> _mounted = new List<LocalTreeNode> { };

        static internal IReadOnlyList<LocalTreeNode>
            Sinks => _wr.Get(o => o._sinks);
        List<LocalTreeNode> _sinks = new List<LocalTreeNode> { };

        static internal LocalTreeNode
            TopNode => _wr.Get(o => o._topNode);
        LocalTreeNode _topNode = null;

        static internal UC_ClonesVM
            LVclones => _wr.Get(o => o._lvClones);
        UC_ClonesVM _lvClones = new UC_ClonesVM();

        static internal UC_ClonesVM
            LVsameVol => _wr.Get(o => o._lvSameVol);
        UC_ClonesVM _lvSameVol = new UC_ClonesVM();

        static internal UC_ClonesVM
            LVsolitary => _wr.Get(o => o._lvSolitary);
        UC_ClonesVM _lvSolitary = new UC_ClonesVM();

        static internal IReadOnlyDictionary<string, string>
            DictVolumeInfo => _wr.Get(o => o._dictVolumeInfo);
        readonly Dictionary<string, string> _dictVolumeInfo = new Dictionary<string, string>();

        static internal TreeSelect.FileListUpdated
            TreeSelect_FileList => _wr.Get(o => o._treeSelect_FileList);
        TreeSelect.FileListUpdated _treeSelect_FileList;

        static internal TreeSelect.FolderDetailUpdated
            TreeSelect_FolderDetail => _wr.Get(o => o._treeSelect_FolderDetail);
        TreeSelect.FolderDetailUpdated _treeSelect_FolderDetail = null;

        static internal TreeSelect.VolumeDetailUpdated
            TreeSelect_VolumeDetail => _wr.Get(o => o._treeSelect_VolumeDetail);
        TreeSelect.VolumeDetailUpdated _treeSelect_VolumeDetail = null;

        static internal bool
            FactoryCreate(LV_ProjectVM lvProjectVM)
        {
            if (null != _instance)
            {
                Util.Assert(99858, false);
                return false;
            }

            if (0 == (lvProjectVM?.CanLoadCount ?? 0))
                return false;

            var lvProjectExplorer = new LV_ProjectVM(lvProjectVM);
            var lvItems = lvProjectExplorer.ItemsCast.ToList();

            lvProjectExplorer.ClearItems();
            lvProjectExplorer.Add(lvItems.Select(lvItem => new LVitemProject_Explorer(lvItem)));

            _wr.SetTarget(
                _instance =
                new LocalTV(lvProjectExplorer));

            return WithLocalTV(localTV =>
                localTV.DoTree());
        }

        LocalTV(LV_ProjectVM lvProjectVM)
        {
            _lvProjectVM = lvProjectVM;
            _knProgMult = 1 / 2d * (_lvProjectVM?.CanLoadCount ?? double.Epsilon);

            if (0 < (_lvProjectVM?.CanLoadCount ?? 0))
                TabledString<TabledStringType_Folders>.AddRef();

            _lsDisposable.Add(UC_DuplicatesVM.GoToFile.LocalSubscribe(99766, WinDuplicatesVM_GoToFile));
            _lsDisposable.Add(UC_SearchVM.GoToFile.LocalSubscribe(99765, WinSearchVM_GoToFile));
            _lsDisposable.Add(TreeSelect.FileListUpdated.Observable.LocalSubscribe(99764, v => _treeSelect_FileList = v.Item1));
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99763, v => _treeSelect_FolderDetail = v.Item1));
            _lsDisposable.Add(TreeSelect.VolumeDetailUpdated.Observable.LocalSubscribe(99762, v => _treeSelect_VolumeDetail = v.Item1));
        }

        internal LocalTV
            LocalDispose()
        {
            if (null == _instance)
                return this;

            _wr.SetTarget(_instance = null);

            if (0 < (_lvProjectVM?.CanLoadCount ?? 0))
                TabledString<TabledStringType_Folders>.DropRef();

            Util.LocalDispose(_lsDisposable);
            return this;
        }

        static internal LocalTreeNode
            GetOneNodeByRootPath(string path, LVitem_ProjectVM lvItemProjectVM = null, IEnumerable<LocalTreeNode> coll = null)
        {
            if (null == coll)
                coll = RootNodes;

            return GetOneNodeByRootPath(path, coll, lvItemProjectVM, toLower: false)
                ?? GetOneNodeByRootPath(path.ToLower(), coll, lvItemProjectVM, toLower: true);
        }

        static LocalTreeNode
            GetOneNodeByRootPath(string path, IEnumerable<LocalTreeNode> coll, LVitem_ProjectVM lvItemProjectVM, bool toLower)
        {
            if (null == coll)
                return null;

            foreach (var node in coll)
            {
                var nodePath = (toLower) ? node.PathFull.ToLower() : node.PathFull;

                if ((path == nodePath) && (lvItemProjectVM?.ListingFile.Equals(node.RootNodeDatum.LVitemProjectVM.ListingFile) ?? true))
                    return node;

                if (path.Contains(nodePath) && path.Replace(nodePath.TrimEnd('\\'), "").StartsWith(@"\"))
                    return GetOneNodeByRootPath(path, node.Nodes, lvItemProjectVM, toLower);
            }

            return null;
        }

        internal int
            GetNodeCount(bool includeSubTrees = false) =>
            includeSubTrees
            ? CountSubnodes(_rootNodes)
            : _rootNodes?.Count ?? 0;

        static int
            CountSubnodes(IEnumerable<LocalTreeNode> nodes)
        {
            var nRet = 0;

            nodes?.ForEach(treeNode =>
                nRet += 1 + CountSubnodes(treeNode.Nodes));

            return nRet;
        }

        void
            WinDuplicatesVM_GoToFile(Tuple<Tuple<LVitem_ProjectVM, string, string>, decimal> initiatorTuple)
        {
            Util.Write("C");
            GoToFile(initiatorTuple.Item1);
        }

        void
            WinSearchVM_GoToFile(Tuple<Tuple<LVitem_ProjectVM, string, string>, decimal> initiatorTuple)
        {
            Util.Write("D");
            GoToFile(initiatorTuple.Item1);
        }

        void
            GoToFile(Tuple<LVitem_ProjectVM, string, string> tuple) =>
            GetOneNodeByRootPath(tuple.Item2, tuple.Item1)?
            .GoToFile(tuple.Item3);

        static internal T
            WithLocalTV<T>(Func<LocalTV, T> doSomethingWith) => _wr.Get(o => doSomethingWith(o));
        static readonly WeakReference<LocalTV> _wr = new WeakReference<LocalTV>(null);
        static LocalTV _instance = null;

        readonly LV_ProjectVM
            _lvProjectVM = null;
        readonly double
            _knProgMult = 0;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
