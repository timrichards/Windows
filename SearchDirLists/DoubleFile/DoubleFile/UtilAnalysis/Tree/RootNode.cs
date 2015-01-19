using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class TreeRootNodeBuilder : TreeBase
    {
        class RootNode
        {
            internal SortedDictionary<string, Node> Nodes = new SortedDictionary<string, Node>();
            internal uint FirstLineNo = 0;
        }
    }
}
