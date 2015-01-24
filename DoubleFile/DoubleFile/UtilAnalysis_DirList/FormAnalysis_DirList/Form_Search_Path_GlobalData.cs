using System;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    partial class GlobalData_Search_Path    // Get one node by path
    {
        GlobalData gd
        {
            get { return _gd; }
            set
            {
                MBox.Assert(0, _gd == null);
                _gd = value;
            }
        }
        GlobalData _gd = null;

        internal GlobalData_Search_Path(GlobalData gd_in)
        {
            gd = gd_in;
        }

        internal TreeNode GetNodeByPath(string path, SDL_TreeView treeView)
        {
            return GetNodeByPath_A(path, treeView) ?? GetNodeByPath_A(path, treeView, bIgnoreCase: true);
        }

        internal TreeNode GetNodeByPath_A(string strPath, SDL_TreeView treeView, bool bIgnoreCase = false)
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

            foreach (Object obj in treeView.Nodes)
            {
                TreeNode topNode = (TreeNode)obj;
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
                        MBox.Assert(1308.9329, (nPathLevelLength - nCount) > 0);
                        nPathLevelLength -= nCount;
                        nPathLevelLength = Math.Max(0, nPathLevelLength);
                    }
                }

                if (strNode == arrPath[nLevel])
                {
                    nodeRet = (TreeNode)topNode;
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

        TreeNode GetSubNode(TreeNode node, string[] pathLevel, int i, int nPathLevelLength, bool bIgnoreCase)
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
