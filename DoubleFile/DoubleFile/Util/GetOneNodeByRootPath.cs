using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace DoubleFile
{
    static class GetOneNodeByRootPath
    {
        static internal LocalTreeNode
            Go(string strPath, IEnumerable<LocalTreeNode> treeNodeCollection, LVitem_ProjectVM lvItemProjectVM = null)
        {
            return
                GoA(strPath, treeNodeCollection) 
                ?? GoA(strPath.ToLower(), treeNodeCollection, toLower: true);
        }

        static LocalTreeNode
            GoA(string path, IEnumerable<LocalTreeNode> coll, bool toLower = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            if (null == coll)
                return null;

            foreach (var node in coll)
            {
                var nodePath = node.PathFull;

                if (toLower)
                    nodePath = nodePath.ToLower();

                if (path == nodePath)
                    return node;

                if (path.Contains(nodePath) && path.Replace(nodePath.TrimEnd('\\'), "").StartsWith(@"\"))
                    return GoA(path, node.Nodes, toLower);
            }

            return null;
        }
    }
}
