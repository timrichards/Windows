using System.Collections.Generic;

namespace DoubleFile
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            // can't be struct because of null
            class RootNode
            {
                internal IDictionary<string, Node>
                    Nodes = new SortedDictionary<string, Node>();
                internal uint
                    PrevLineNo = 0;
            }
        }
    }
}
