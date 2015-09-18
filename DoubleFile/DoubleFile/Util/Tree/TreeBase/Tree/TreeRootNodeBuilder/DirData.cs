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
                internal DirData(uint nFirstLineNo)
                {
                    _rootNode.PrevLineNo = nFirstLineNo;
                }

                internal void AddToTree(string str_in, uint nLineNo, ulong nLength, int nAllFilesHash, IReadOnlyList<int> lsFilesHereHashes)
                {
                    var str = str_in.TrimEnd('\\');

                    _rootNode.Nodes.Add(str, new Node(str, nLineNo, nLength, nAllFilesHash, lsFilesHereHashes, _rootNode));
                }

                internal LocalTreeNode
                    AddToTree() =>
                    _rootNode.Nodes.Values
                    .Select(rootNode => rootNode.AddToTree())
                    .FirstOrDefault();

                RootNode
                    _rootNode = new RootNode();
            }
        }
    }
}
