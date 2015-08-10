using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace DoubleFile
{
    static class GetOneNodeByRootPath
    {
        static internal LocalTreeNode
            Go(string strPath, IEnumerable<LocalTreeNode> treeNodeCollection, LVitem_ProjectVM lvItemProjectVM = null)
        {
            return
                GoA(strPath, treeNodeCollection, lvItemProjectVM) 
                ?? GoA(strPath, treeNodeCollection, lvItemProjectVM, bIgnoreCase: true);
        }

        static LocalTreeNode
            GoA(string strPath, IEnumerable<LocalTreeNode> treeNodeCollection, LVitem_ProjectVM lvItemProjectVM, bool bIgnoreCase = false)
        {
            if ((string.IsNullOrWhiteSpace(strPath)) ||
                (null == treeNodeCollection))
            {
                return null;
            }

            if (bIgnoreCase)
                strPath = strPath.ToLower();

            LocalTreeNode nodeRet = null;

            foreach (var topNode in treeNodeCollection)
            {
                var rootNodeDatum = ((RootNodeDatum)topNode.NodeDatum);

                var strNode =
                    (Path.IsPathRooted(strPath))
                    ? "" + rootNodeDatum.LVitemProjectVM.SourcePath.TrimEnd('\\')
                    : topNode.Text;

                if (bIgnoreCase)
                    strNode = strNode.ToLower();

                var arrPath = strPath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                var nFolderDepth = arrPath.Length;

                if (strNode.Contains('\\'))
                {
                    var nCount = strNode.Count(c => '\\' == c);

                    for (var n = 0; n < nFolderDepth - 1; ++n)
                    {
                        if (n < nCount)
                            arrPath[0] += '\\' + arrPath[n + 1];
                    }

                    for (var n = 1; n < nFolderDepth - 1; ++n)
                    {
                        if ((nCount + n) < arrPath.Length)
                            arrPath[n] = arrPath[nCount + n];
                    }

                    if (1 < nFolderDepth)
                    {
                        Util.Assert(1308.9329m, 0 < (nFolderDepth - nCount));
                        nFolderDepth -= nCount;
                        nFolderDepth = Math.Max(0, nFolderDepth);
                    }
                }

                if (strNode != arrPath[0])
                    continue;

                if ((null != lvItemProjectVM) &&
                    (lvItemProjectVM.ListingFile != rootNodeDatum.LVitemProjectVM.ListingFile))
                {
                    continue;
                }

                nodeRet = topNode;

                if ((1 < nFolderDepth) &&
                    (null != nodeRet))
                {
                    nodeRet = GetSubNode(nodeRet, arrPath, 1, nFolderDepth, bIgnoreCase);

                    if (null != nodeRet)
                        return nodeRet;
                }
            }

            return nodeRet;
        }

        static LocalTreeNode
            GetSubNode(LocalTreeNode node, IReadOnlyList<string> pathLevel, int nLevel, int nPathLevelLength, bool bIgnoreCase)
        {
            if (null == node.Nodes)
                return null;

            foreach (LocalTreeNode subNode in node.Nodes)
            {
                var strText =
                    bIgnoreCase
                    ? subNode.Text.ToLower()
                    : subNode.Text;

                if (strText != pathLevel[nLevel])
                    continue;

                if (++nLevel == nPathLevelLength)
                    return subNode;

                return GetSubNode(subNode, pathLevel, nLevel, nPathLevelLength, bIgnoreCase);
            }

            return null;
        }
    }
}
