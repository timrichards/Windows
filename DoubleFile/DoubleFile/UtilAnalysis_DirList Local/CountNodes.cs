using System.Collections.Generic;
using DoubleFile;

namespace Local
{
    static class CountNodes
    {
        internal static int Go(List<LocalTreeNode> listNodes)
        {
            int nCount = 0;

            foreach (LocalTreeNode treeNode in listNodes)
            {
                nCount += Go(treeNode, bNextNode: false);
            }

            return nCount;
        }

        internal static int Go(LocalTreeNode treeNode_in, bool bNextNode = true)
        {
            LocalTreeNode treeNode = treeNode_in;
            int nCount = 0;

            do
            {
                if ((treeNode.Nodes != null) && (treeNode.Nodes.Count > 0))
                {
                    nCount += Go((LocalTreeNode)treeNode.Nodes[0]);
                }

                ++nCount;
            }
            while (bNextNode && ((treeNode = (LocalTreeNode)treeNode.NextNode) != null));

            return nCount;
        }
    }
}
