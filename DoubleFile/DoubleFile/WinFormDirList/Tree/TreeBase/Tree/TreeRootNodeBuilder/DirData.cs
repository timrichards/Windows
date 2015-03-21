using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            // can't be struct because of null
            class DirData
            {
                internal DirData(RootNode rootNode)
                {
                    _rootNode = rootNode;
                }

                internal void AddToTree(string str_in, uint nLineNo, ulong nLength, int nHashParity)
                {
                    string str = str_in.TrimEnd('\\');

                    _rootNode.Nodes.Add(str, new Node(str, nLineNo, nLength, nHashParity, _rootNode));
                }

                internal TreeNode AddToTree(string strVolumeName, out string strRootPath)
                {
                    TreeNode rootTreeNode = null;
                    string strRootPath_out = null;

                    _rootNode.Nodes.Values
                        .First(rootNode =>
                            rootTreeNode =
                                rootNode
                                .AddToTree(strVolumeName, out strRootPath_out)
                        );

                    strRootPath = strRootPath_out;
                    return rootTreeNode;
                }

                RootNode
                    _rootNode = null;
            }
        }
    }
}
