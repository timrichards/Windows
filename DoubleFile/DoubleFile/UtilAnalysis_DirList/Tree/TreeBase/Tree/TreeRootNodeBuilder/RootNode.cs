﻿using System.Collections.Generic;

namespace SearchDirLists
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            class RootNode
            {
                internal SortedDictionary<string, Node> Nodes = new SortedDictionary<string, Node>();
                internal uint FirstLineNo = 0;
            }
        }
    }
}
