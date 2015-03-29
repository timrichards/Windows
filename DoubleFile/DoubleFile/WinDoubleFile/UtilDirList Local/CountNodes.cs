using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    static class CountNodes
    {
        internal static int Go(IReadOnlyList<LocalTreeNode> listNodes)
        {
            return listNodes.Sum(treeNode => Go(treeNode, bNextNode: false));
        }

        internal static int Go(LocalTreeNode treeNode_in, bool bNextNode = true)
        {
            var treeNode = treeNode_in;
            var nCount = 0;

            do
            {
                if (null != treeNode.Nodes &&
                    (false == treeNode.Nodes.IsEmpty()))
                {
                    nCount += Go(treeNode.Nodes[0]);
                }

                ++nCount;
            }
            while (bNextNode && ((treeNode = treeNode.NextNode) != null));

            return nCount;
        }
    }
}
