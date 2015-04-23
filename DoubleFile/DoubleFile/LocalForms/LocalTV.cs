using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DoubleFile
{
    class LocalTV : IDisposable
    {
        internal Dictionary<string, string>
            _dictVolumeInfo = null;
        internal LocalTreeNode[]
            Nodes { get; set; }
        internal LocalTreeNode
            SelectedNode { get; set; }
        internal LocalTreeNode
            TopNode { get; set; }
        static internal Func<string, LVitem_ProjectVM, LocalTreeNode>
            GetOneNodeByRootPathA = null;
        static internal LocalTV
            StaticTreeView { get; set; }

        internal LocalTV(IEnumerable<LocalTreeNode> lsRootNodes = null)
        {
            if (null != lsRootNodes)
                Nodes = lsRootNodes.ToArray();
            
            _lsDisposable.Add(WinDoubleFile_DuplicatesVM.GoToFile.Subscribe(GoToFileA));
            _lsDisposable.Add(WinDoubleFile_SearchVM.GoToFile.Subscribe(GoToFileB));
            GetOneNodeByRootPathA = (strPath, lvItemProjectVM) => GetOneNodeByRootPath.Go(strPath, Nodes, lvItemProjectVM);
        }

        public void Dispose()
        {
            foreach (var d in _lsDisposable)
                d.Dispose();

            GetOneNodeByRootPathA = null;
            StaticTreeView = null;
        }

        internal int GetNodeCount(bool includeSubTrees = false)
        {
            return includeSubTrees ? CountSubnodes(Nodes) : (null != Nodes) ? Nodes.Length : 0;
        }

        internal void Select() { }

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

        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
