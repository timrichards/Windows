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
        internal LocalTreeNodeCollection
            Nodes { get; private set; }
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
        
        internal LocalTV()
        {
            Nodes = new LocalTreeNodeCollection(this);
            WinDoubleFile_DuplicatesVM.GoToFile += GoToFile;
        }

        internal int GetNodeCount(bool includeSubTrees = false)
        {
            return includeSubTrees ? CountSubnodes(Nodes) : Nodes.Count;
        }

        internal void Select() { }

        static int CountSubnodes(IEnumerable<LocalTreeNode> nodes)
        {
            var nRet = 0;

            foreach (var treeNode in nodes)
            {
                nRet += CountSubnodes(treeNode.Nodes);
                ++nRet;
            }

            return nRet;
        }

        private void GoToFile(LVitem_ProjectVM lvItem_ProjectVM, string strPath, string strFile)
        {
            var treeNode = Local.GetOneNodeByRootPath.Go(strPath, Nodes);

            if (null == treeNode)
                return;

            treeNode.GoToFile(strFile);

            //Nodes.Keys
            //    .Where(item => lvItem_ProjectVM.ListingFile == 
            //        ((Local.RootNodeDatum)item.NodeDatum).ListingFile)
            //    .FirstOnlyAssert(item =>
            //        item.GoToFile(
            //            strPath
            //                .Replace(((Local.RootNodeDatum)item.NodeDatum).RootPath, "")
            //                .TrimStart('\\')
            //                .Split('\\'),
            //            strFile)
            //    );
        }

        public void Dispose()
        {
            WinDoubleFile_DuplicatesVM.GoToFile -= GoToFile;
        }
    }
}
