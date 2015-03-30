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
        internal Font
            Font { get; set; }
        internal bool
            CheckBoxes { get; set; }
        internal bool
            Enabled { get; set; }
        
        internal LocalTV(IEnumerable<LocalTreeNode> lsRootNodes = null)
        {
            if (null != lsRootNodes)
                Nodes = lsRootNodes.ToArray();
            
            WinDoubleFile_DuplicatesVM.GoToFile += GoToFile;
            WinDoubleFile_SearchVM.GoToFile += GoToFile;
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

        private void GoToFile(LVitem_ProjectVM lvItemProjectVM, string strPath, string strFile)
        {
            if (null == Nodes)
                return;

            var treeNode = GetOneNodeByRootPath.Go(strPath, Nodes, lvItemProjectVM);

            if (null == treeNode)
                return;

            treeNode.GoToFile(strFile);
        }

        public void Dispose()
        {
            WinDoubleFile_DuplicatesVM.GoToFile -= GoToFile;
            WinDoubleFile_SearchVM.GoToFile -= GoToFile;
        }
    }
}
