using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            // can't be struct because of null
            class RootNode
            {
                internal RootNode(uint nFirstLineNo, char driveLetter)
                {
                    PrevLineNo = nFirstLineNo;

                    Util.Assert(99632, 'A' <= driveLetter);
                    Util.Assert(99631, 'Z' >= driveLetter);
                    DriveLetter = driveLetter;
                }

                internal void AddToTree(string[] asLine, IReadOnlyList<int> lsFilesHereHashes, bool isHashComplete)
                {
                    var strPath = asLine[2].TrimEnd('\\');
                    Util.Assert(99630, ':' == strPath[1]);

                    if (strPath[0] != DriveLetter)
                    {
                        Util.Assert(99628, false);
                        strPath = DriveLetter + strPath.Substring(1);
                    }

                    Nodes.Add(strPath, new Node(strPath, asLine, lsFilesHereHashes, isHashComplete, this));
                }

                internal LocalTreeNode
                    AddToTree() =>
                    Nodes.Values
                    .Select(rootNode => rootNode.AddToTree())
                    .FirstOrDefault();

                internal IDictionary<string, Node>
                    Nodes = new SortedDictionary<string, Node>();
                internal uint
                    PrevLineNo = 0;
                internal readonly char
                    DriveLetter;
            }
        }
    }
}
