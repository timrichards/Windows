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

        static internal LocalTreeNode
            GetOneNodeByRootPathA(string strPath, LVitem_ProjectVM lvItemProjectVM)
        {
            var o = _weakReference.Target as LocalTV;

            if (null == o)
                return null;

            return GetOneNodeByRootPath.Go(strPath, o.Nodes, lvItemProjectVM);
        }

        internal LocalTV(IEnumerable<LocalTreeNode> lsRootNodes = null)
        {
            _weakReference.Target = this;            

            if (null != lsRootNodes)
                Nodes = lsRootNodes.ToArray();
            
            _lsDisposable.Add(WinDoubleFile_DuplicatesVM.GoToFile.Subscribe(GoToFileA));
            _lsDisposable.Add(WinDoubleFile_SearchVM.GoToFile.Subscribe(GoToFileB));
        }

        public void Dispose()
        {
            _weakReference.Target = null;

            foreach (var d in _lsDisposable)
                d.Dispose();
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
        static readonly WeakReference
            _weakReference = new WeakReference(null);
    }
}
