using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LocalTV
    {
        static internal IEnumerable<LocalTreeNode>
            TreeNodes { get { var o = _weakReference.Target as LocalTV; return (null != o) ? o._arrTreeNodes : null; } }

        static internal LocalTV Instance { get; private set; }

        internal LocalTreeNode[]
            Nodes { get; set; }
        internal LocalTreeNode
            SelectedNode { get; set; }
        internal LocalTreeNode
            TopNode { get; set; }

        static internal Dictionary<string, string>
            DictVolumeInfo
        {
            get
            {
                var o = _weakReference.Target as LocalTV;

                if (null == o)
                    return null;

                return o._dictVolumeInfo;
            }
        }

        static internal void FactoryCreate(TreeView_DoubleFileVM tvVM, LV_ProjectVM lvProjectVM)
        {
            if (null != Instance)
            {
                MBoxStatic.Assert(99858, false);
                return;
            }

            _weakReference.Target =
                Instance =
                new LocalTV(tvVM, lvProjectVM);

            Instance.DoTree();
        }

        LocalTV(TreeView_DoubleFileVM tvVM, LV_ProjectVM lvProjectVM)
        {
            _lvProjectVM = lvProjectVM;

            if ((null != _lvProjectVM) &&
                (0 < _lvProjectVM.Count))
            {
                _nCorrelateProgressDenominator = _lvProjectVM.Count;
                _tvVM = tvVM;
                TabledString<Tabled_Folders>.AddRef();
            }

            _weakReference.Target = this;            
            _lsDisposable.Add(WinDoubleFile_DuplicatesVM.GoToFile.Subscribe(GoToFileA));
            _lsDisposable.Add(WinDoubleFile_SearchVM.GoToFile.Subscribe(GoToFileB));
        }

        static internal void LocalDispose()
        {
            if (null == Instance)
            {
                MBoxStatic.Assert(99857, false);
                return;
            }

            Instance.Dispose();
            Instance = null;
        }

        public void Dispose()
        {
            _weakReference.Target = null;

            if ((null != Instance._lvProjectVM) &&
                (0 < Instance._lvProjectVM.Count))
            {
                TabledString<Tabled_Folders>.DropRef();
            }

            foreach (var d in _lsDisposable)
                d.Dispose();
        }

        static internal LocalTreeNode
            GetOneNodeByRootPathA(string strPath, LVitem_ProjectVM lvItemProjectVM)
        {
            var o = _weakReference.Target as LocalTV;

            if (null == o)
                return null;

            return GetOneNodeByRootPath.Go(strPath, o.Nodes, lvItemProjectVM);
        }

        internal int GetNodeCount(bool includeSubTrees = false)
        {
            return includeSubTrees ? CountSubnodes(Nodes) : (null != Nodes) ? Nodes.Length : 0;
        }

        static int CountSubnodes(IEnumerable<LocalTreeNode> nodes)
        {
            if (null == nodes)
                return 0;

            var nRet = 0;

            foreach (var treeNode in nodes)
            {
                nRet += CountSubnodes(treeNode.Nodes);
                ++nRet;
            }

            return nRet;
        }

        void GoToFileA(Tuple<LVitem_ProjectVM, string, string> tuple) { UtilDirList.Write("C"); GoToFile(tuple); }
        void GoToFileB(Tuple<LVitem_ProjectVM, string, string> tuple) { UtilDirList.Write("D"); GoToFile(tuple); }
        private void GoToFile(Tuple<LVitem_ProjectVM, string, string> tuple)
        {
            if (null == Nodes)
                return;

            var treeNode = GetOneNodeByRootPathA(tuple.Item2, tuple.Item1);

            if (null == treeNode)
                return;

            treeNode.GoToFile(tuple.Item3);
        }

        readonly TreeView_DoubleFileVM
            _tvVM = null;
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
