using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace DoubleFile
{
    class TreeNodeProxy
    {
        internal TreeNodeProxy(LocalTreeNode localTreeNode)
        {
            _localTreeNode = localTreeNode;
        }

        internal TreeNodeProxy(TreeNode localTreeNode)
        {
            _treeNode = localTreeNode;
        }

        internal IEnumerable<TreeNodeProxy> Nodes
        {
            get
            {
                if (null != _localTreeNode)
                    return _localTreeNode.Nodes.Cast<TreeNodeProxy>();
                else
                    return _treeNode.Nodes.Cast<TreeNodeProxy>();
            }
        }

        LocalTreeNode _localTreeNode;
        TreeNode _treeNode;
    }
}
