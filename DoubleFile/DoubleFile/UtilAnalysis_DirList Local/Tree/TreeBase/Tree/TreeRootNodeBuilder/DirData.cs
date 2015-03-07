using System.Linq;
using DoubleFile;

namespace Local
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            // can't be struct because of null
            class DirData
            {
                internal DirData(GlobalData_Base gd, RootNode rootNode)
                {
                    _gd = gd;
                    _rootNode = rootNode;
                }

                internal void AddToTree(string str_in, uint nLineNo, ulong nLength, int nHashParity)
                {
                    var str = str_in.TrimEnd('\\');

                    _rootNode.Nodes.Add(str, new Node(_gd, str, nLineNo, nLength, nHashParity, _rootNode));
                }

                internal LocalTreeNode AddToTree(string strVolumeName, out string strRootPath)
                {
                    LocalTreeNode rootTreeNode = null;
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

                readonly RootNode
                    _rootNode = null;
                readonly GlobalData_Base
                    _gd = null;
            }
        }
    }
}
