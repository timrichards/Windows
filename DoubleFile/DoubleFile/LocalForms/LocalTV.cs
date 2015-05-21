using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LocalTV
    {
        static internal LocalTV
            Instance { get; private set; }

        static internal IEnumerable<LocalTreeNode>
            AllNodes { get { var o = _weakReference.Target as LocalTV; return (null != o) ? o._allNodes : null; } }
        LocalTreeNode[] _allNodes = null;

        static internal IEnumerable<LocalTreeNode>
            RootNodes { get { var o = _weakReference.Target as LocalTV; return (null != o) ? o._rootNodes : null; } }
        LocalTreeNode[] _rootNodes = null;
        static readonly object _rootNodesSemaphore = new object();

        static internal LocalTreeNode
            TopNode { get { var o = _weakReference.Target as LocalTV; return (null != o) ? o._topNode : null; } }
        LocalTreeNode
            _topNode = null;

        static internal LocalTreeNode
            SelectedNode
        {
            get { var o = _weakReference.Target as LocalTV; return (null != o) ? o._selectedNode : null; }
            set { var o = _weakReference.Target as LocalTV; if (null != o) o._selectedNode = value; }
        }
        LocalTreeNode _selectedNode = null;

        static internal Dictionary<string, string>
            DictVolumeInfo { get { var o = _weakReference.Target as LocalTV; return (null != o) ? o._dictVolumeInfo : null; } }
        readonly Dictionary<string, string> _dictVolumeInfo = new Dictionary<string, string>();

        static internal bool FactoryCreate(LV_ProjectVM lvProjectVM)
        {
            if (null != Instance)
            {
                MBoxStatic.Assert(99858, false);
                return false;
            }

            _weakReference.Target =
                Instance =
                new LocalTV(lvProjectVM);

            return Instance.DoTree();
        }

        LocalTV(LV_ProjectVM lvProjectVM)
        {
            _lvProjectVM = lvProjectVM;

            if ((null != _lvProjectVM) &&
                (0 < _lvProjectVM.Count))
            {
                _nCorrelateProgressDenominator = _lvProjectVM.Count;
                TabledString<Tabled_Folders>.AddRef();
            }

            _weakReference.Target = this;            
            _lsDisposable.Add(WinDuplicatesVM.GoToFile.Subscribe(WinDuplicatesVM_GoToFile));
            _lsDisposable.Add(WinSearchVM.GoToFile.Subscribe(WinSearchVM_GoToFile));
        }

        static internal void LocalDispose()
        {
            _weakReference.Target = null;

            if (null == Instance)
            {
                MBoxStatic.Assert(99857, false);
                return;
            }

            if ((null != Instance._lvProjectVM) &&
                (0 < Instance._lvProjectVM.Count))
            {
                TabledString<Tabled_Folders>.DropRef();
            }

            foreach (var d in Instance._lsDisposable)
                d.Dispose();

            Instance = null;
        }

        static internal LocalTreeNode
            GetOneNodeByRootPathA(string strPath, LVitem_ProjectVM lvItemProjectVM)
        {
            var nodes = LocalTV.RootNodes;

            if (null == nodes)
                return null;

            return GetOneNodeByRootPath.Go(strPath, nodes, lvItemProjectVM);
        }

        internal int GetNodeCount(bool includeSubTrees = false)
        {
            return includeSubTrees
                ? CountSubnodes(_rootNodes)
                : (null != _rootNodes)
                ? _rootNodes.Length
                : 0;
        }

        static int CountSubnodes(IEnumerable<LocalTreeNode> nodes)
        {
            if (null == nodes)
                return 0;

            var nRet = 0;

            foreach (var treeNode in nodes)
                nRet += 1 + CountSubnodes(treeNode.Nodes);

            return nRet;
        }

        void WinDuplicatesVM_GoToFile(Tuple<Tuple<LVitem_ProjectVM, string, string>, int> initiatorTuple)
        {
            UtilDirList.Write("C"); GoToFile(initiatorTuple.Item1);
        }

        void WinSearchVM_GoToFile(Tuple<Tuple<LVitem_ProjectVM, string, string>, int> initiatorTuple)
        {
            UtilDirList.Write("D"); GoToFile(initiatorTuple.Item1);
        }

        void GoToFile(Tuple<LVitem_ProjectVM, string, string> tuple)
        {
            if (null == RootNodes)
                return;

            var treeNode = GetOneNodeByRootPathA(tuple.Item2, tuple.Item1);

            if (null == treeNode)
                return;

            treeNode.GoToFile(tuple.Item3);
        }

        readonly LV_ProjectVM
            _lvProjectVM = null;
        readonly WinProgress
            _winProgress = new WinProgress();
        readonly double
            _nCorrelateProgressDenominator = 0;
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
        static readonly WeakReference
            _weakReference = new WeakReference(null);
    }
}
