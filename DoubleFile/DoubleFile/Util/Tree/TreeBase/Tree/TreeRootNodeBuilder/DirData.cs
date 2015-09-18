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
                    _nPrevLineNo = nFirstLineNo;
                }

                internal void AddToTree(string str_in, uint nLineNo, ulong nLength, int nAllFilesHash, IReadOnlyList<int> lsFilesHereHashes)
                {
                    var str = str_in.TrimEnd('\\');

                    _nodes.Add(str, new Node(str, nLineNo, nLength, nAllFilesHash, lsFilesHereHashes, _nodes, _nPrevLineNo));
                }

                internal LocalTreeNode AddToTree() =>
                    _nodes.Values
                    .Select(rootNode => rootNode.AddToTree())
                    .FirstOrDefault();

                IDictionary<string, Node>
                    _nodes = new SortedDictionary<string, Node>();
                uint
                    _nPrevLineNo = 0;
            }
        }
    }
}
