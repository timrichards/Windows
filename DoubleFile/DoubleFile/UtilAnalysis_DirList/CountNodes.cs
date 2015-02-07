using System.Collections.Generic;
using System.Windows.Forms;

namespace DoubleFile
{
    static class CountNodes
    {
        internal static int Go(List<TreeNode> listNodes)
        {
            int nCount = 0;

            foreach (TreeNode treeNode in listNodes)
            {
                nCount += Go(treeNode, bNextNode: false);
            }

            return nCount;
        }

        internal static int Go(TreeNode treeNode_in, bool bNextNode = true)
        {
            TreeNode treeNode = treeNode_in;
            int nCount = 0;

            do
            {
                if ((treeNode.Nodes != null) && (treeNode.Nodes.Count > 0))
                {
                    nCount += Go((TreeNode)treeNode.Nodes[0]);
                }

                ++nCount;
            }
            while (bNextNode && ((treeNode = (TreeNode)treeNode.NextNode) != null));

            return nCount;
        }
    }
}
