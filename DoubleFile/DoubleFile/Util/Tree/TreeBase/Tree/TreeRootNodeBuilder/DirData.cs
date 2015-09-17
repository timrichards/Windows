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

                    Nodes.Add(str, new Node(str, nLineNo, nLength, nAllFilesHash, lsFilesHereHashes, Nodes, _nPrevLineNo));
                    _nPrevLineNo = nLineNo;
                }

                internal LocalTreeNode AddToTree() =>
                    Nodes.Values
                    .Select(rootNode => rootNode.AddToTree())
                    .FirstOrDefault();

                internal readonly IDictionary<string, Node>
                    Nodes = new SortedDictionary<string, Node>();
                uint
                    _nPrevLineNo = 0;
            }
        }
    }
}
