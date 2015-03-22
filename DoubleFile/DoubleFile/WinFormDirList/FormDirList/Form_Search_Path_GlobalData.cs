﻿using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    class GetNodeByPath    // Get one node by path
    {
        static internal TreeNode Go(string path, IEnumerable treeNodeCollection)
        {
            return
                GetNodeByPath_A(path, treeNodeCollection) 
                ?? GetNodeByPath_A(path, treeNodeCollection, bIgnoreCase: true);
        }

        static TreeNode GetNodeByPath_A(string strPath, IEnumerable treeNodeCollection, bool bIgnoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(strPath))
            {
                return null;
            }

            if (bIgnoreCase)
            {
                strPath = strPath.ToLower();
            }

            TreeNode nodeRet = null;

            foreach (TreeNode topNode in treeNodeCollection)
            {
                string[] arrPath = null;
                int nPathLevelLength = 0;
                int nLevel = 0;
                string strNode = topNode.Name.TrimEnd('\\').Replace(@"\\", @"\");

                if (bIgnoreCase)
                {
                    strNode = strNode.ToLower();
                }

                arrPath = strPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                nPathLevelLength = arrPath.Length;

                if (strNode.Contains('\\'))
                {
                    int nCount = strNode.Count(c => c == '\\');

                    for (int n = 0; n < nPathLevelLength - 1; ++n)
                    {
                        if (n < nCount)
                        {
                            arrPath[0] += '\\' + arrPath[n + 1];
                        }
                    }

                    for (int n = 1; n < nPathLevelLength - 1; ++n)
                    {
                        if ((nCount + n) < arrPath.Length)
                        {
                            arrPath[n] = arrPath[nCount + n];
                        }
                    }

                    if (nPathLevelLength > 1)
                    {
                        MBoxStatic.Assert(1308.9329, (nPathLevelLength - nCount) > 0);
                        nPathLevelLength -= nCount;
                        nPathLevelLength = Math.Max(0, nPathLevelLength);
                    }
                }

                if (strNode == arrPath[nLevel])
                {
                    nodeRet = topNode;
                    nLevel++;

                    if ((nLevel < nPathLevelLength) && nodeRet != null)
                    {
                        nodeRet = GetSubNode(nodeRet, arrPath, nLevel, nPathLevelLength, bIgnoreCase);

                        if (nodeRet != null)
                        {
                            return nodeRet;
                        }
                    }
                }
            }

            return nodeRet;
        }

        static TreeNode GetSubNode(TreeNode node, string[] pathLevel, int i, int nPathLevelLength, bool bIgnoreCase)
        {
            foreach (TreeNode subNode in node.Nodes)
            {
                string strText = bIgnoreCase ? subNode.Text.ToLower() : subNode.Text;

                if (strText != pathLevel[i])
                {
                    continue;
                }

                if (++i == nPathLevelLength)
                {
                    return subNode;
                }

                return GetSubNode(subNode, pathLevel, i, nPathLevelLength, bIgnoreCase);
            }

            return null;
        }
    }
}
