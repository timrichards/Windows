using System.Collections.Generic;
using DoubleFile;

namespace WPF
{
    static class CountNodes
    {
        internal static int Go(List<SDL_TreeNode> listNodes)
        {
            int nCount = 0;

            foreach (SDL_TreeNode treeNode in listNodes)
            {
                nCount += Go(treeNode, bNextNode: false);
            }

            return nCount;
        }

        internal static int Go(SDL_TreeNode treeNode_in, bool bNextNode = true)
        {
            SDL_TreeNode treeNode = treeNode_in;
            int nCount = 0;

            do
            {
                if ((treeNode.Nodes != null) && (treeNode.Nodes.Count > 0))
                {
                    nCount += Go((SDL_TreeNode)treeNode.Nodes[0]);
                }

                ++nCount;
            }
            while (bNextNode && ((treeNode = (SDL_TreeNode)treeNode.NextNode) != null));

            return nCount;
        }
    }
}
