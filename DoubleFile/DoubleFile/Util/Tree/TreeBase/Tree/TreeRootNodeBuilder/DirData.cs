using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            // can't be struct because of null
            class DirData
            {
                internal DirData(int nFirstLineNo)
                {
                    _rootNode.FirstLineNo = (uint)nFirstLineNo;
                }

                internal void AddToTree(string str_in, uint nLineNo, ulong nLength, IReadOnlyList<int> hashcodes)
                {
                    var str = str_in.TrimEnd('\\');

                    _rootNode.Nodes.Add(str, new Node(str, nLineNo, nLength, hashcodes, _rootNode));
                }

                internal LocalTreeNode AddToTree() =>
                    _rootNode.Nodes.Values
                    .Select(rootNode => rootNode.AddToTree())
                    .FirstOrDefault();

                readonly RootNode
                    _rootNode = new RootNode { };
            }
        }
    }
}
