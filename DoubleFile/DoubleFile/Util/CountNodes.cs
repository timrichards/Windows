using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    static class CountNodes
    {
        static internal int Go(IEnumerable<LocalTreeNode> listNodes)
        {
            return listNodes.Sum(treeNode => Go(treeNode, bNextNode: false));
        }

        static internal int Go(LocalTreeNode treeNode_in, bool bNextNode = true)
        {
            var treeNode = treeNode_in;
            var nCount = 0;

            do
            {
                if (0 < (treeNode.Nodes?.Count ?? 0))
                    nCount += Go(treeNode.Nodes[0]);

                ++nCount;
            }
            while (bNextNode &&
                (null !=
                (treeNode = treeNode.NextNode)));

            return nCount;
        }
    }
}
