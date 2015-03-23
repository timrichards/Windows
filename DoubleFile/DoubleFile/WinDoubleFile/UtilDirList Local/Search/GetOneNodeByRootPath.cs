﻿using System;
using System.Linq;
using DoubleFile;
using System.Collections.Generic;

namespace Local
{
    static class GetOneNodeByRootPath
    {
        static internal LocalTreeNode Go(string path, IReadOnlyList<LocalTreeNode> treeNodeCollection)
        {
            return
                Go_A(path, treeNodeCollection) 
                ?? Go_A(path, treeNodeCollection, bIgnoreCase: true);
        }

        static LocalTreeNode Go_A(string strPath, IReadOnlyList<LocalTreeNode> treeNodeCollection, bool bIgnoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(strPath))
                return null;

            if (bIgnoreCase)
                strPath = strPath.ToLower();

            LocalTreeNode nodeRet = null;

            foreach (var topNode in treeNodeCollection)
            {
                var strNode = ((string)((RootNodeDatum)topNode.NodeDatum).RootPath).TrimEnd('\\').Replace(@"\\", @"\");

                if (bIgnoreCase)
                    strNode = strNode.ToLower();

                var arrPath = strPath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                var nPathLevelLength = arrPath.Length;

                if (strNode.Contains('\\'))
                {
                    var nCount = strNode.Count(c => '\\' == c);

                    for (var n = 0; n < nPathLevelLength - 1; ++n)
                    {
                        if (n < nCount)
                            arrPath[0] += '\\' + arrPath[n + 1];
                    }

                    for (var n = 1; n < nPathLevelLength - 1; ++n)
                    {
                        if ((nCount + n) < arrPath.Length)
                            arrPath[n] = arrPath[nCount + n];
                    }

                    if (1 < nPathLevelLength)
                    {
                        MBoxStatic.Assert(1308.9329, 0 < (nPathLevelLength - nCount));
                        nPathLevelLength -= nCount;
                        nPathLevelLength = Math.Max(0, nPathLevelLength);
                    }
                }

                if (strNode != arrPath[0])
                    continue;

                nodeRet = topNode;

                if ((1 < nPathLevelLength) &&
                    (null != nodeRet))
                {
                    nodeRet = GetSubNode(nodeRet, arrPath, 1, nPathLevelLength, bIgnoreCase);

                    if (null != nodeRet)
                        return nodeRet;
                }
            }

            return nodeRet;
        }

        static LocalTreeNode GetSubNode(LocalTreeNode node, string[] pathLevel, int nLevel, int nPathLevelLength, bool bIgnoreCase)
        {
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
