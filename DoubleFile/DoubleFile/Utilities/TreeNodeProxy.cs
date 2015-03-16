using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;

namespace DoubleFile
{
    interface INodeDatum
    {
        TreeNodeProxy
            TreeMapFiles { get; set; }
        Rectangle
            TreeMapRect { get; set; }
        ulong
            TotalLength { get; set; }
        ulong
            Length { get; }
    }

    interface IRootNodeDatum : INodeDatum
    {
        bool
            VolumeView { get; set; }
        ulong
            VolumeFree { get; }
        ulong
            VolumeLength { get; }
    }

    class TreeNodeProxy
    {
        internal TreeNodeProxy(LocalTreeNode localTreeNode) { _treeNode = localTreeNode; }
        internal TreeNodeProxy(TreeNode treeNode) { _treeNode = treeNode; }

        public static implicit operator LocalTreeNode(TreeNodeProxy value) { return value._LocalTreeNode; }
        public static implicit operator TreeNode(TreeNodeProxy value) { return value._TreeNode; }

        public static implicit operator TreeNodeProxy(LocalTreeNode value) { return new TreeNodeProxy(value); }
        public static implicit operator TreeNodeProxy(TreeNode value) { return new TreeNodeProxy(value); }

        internal void SetTreeViewSelectedNode()
        {
            if (null != _LocalTreeNode)
                _LocalTreeNode.TreeView.SelectedNode = _LocalTreeNode;
            else
                _TreeNode.TreeView.SelectedNode = _TreeNode;
        }

        internal void ClearTreeViewSelectedNode()
        {
            if (null != _LocalTreeNode)
                _LocalTreeNode.TreeView.SelectedNode = null;
            else
                _TreeNode.TreeView.SelectedNode = null;
        }

        internal bool IsTreeViewNull()
        {
            return (null ==
                ((null != _LocalTreeNode)
                ? _LocalTreeNode.TreeView
                : (object)_TreeNode.TreeView));
        }

        internal bool IsChildOf(TreeNodeProxy parentNode)
        {
            return (null != _LocalTreeNode)
                ? _LocalTreeNode.IsChildOf(parentNode)
                : _TreeNode.IsChildOf(parentNode);
        }

        internal INodeDatum NodeDatum { get { return (null != _LocalTreeNode)
            ? _LocalTreeNode.NodeDatum
            : _TreeNode.Tag as INodeDatum; }  set { if (null != _LocalTreeNode)
            _LocalTreeNode.NodeDatum = (Local.NodeDatum)value; else
            _TreeNode.Tag = value; } }
        internal IRootNodeDatum RootNodeDatum { get { return NodeDatum as IRootNodeDatum; } }

        internal IEnumerable<TreeNodeProxy> Nodes { get { return _modifiedNodes ?? ((null != _LocalTreeNode)
            ? _LocalTreeNode.Nodes.Cast<TreeNodeProxy>()
            : _TreeNode.Nodes.Cast<TreeNodeProxy>()); }
            set { _modifiedNodes = value; } }

        internal int ForeColor { get { return (null != _LocalTreeNode)
            ? _LocalTreeNode.ForeColor
            : _TreeNode.ForeColor.ToArgb(); }  set { if (null != _LocalTreeNode)
            _LocalTreeNode.ForeColor = value; else
            _TreeNode.ForeColor = Color.FromArgb(value); } }

        internal TreeNodeProxy Parent { get { return ((TreeNodeProxy) _treeNode).Parent; } }
        internal TreeNodeProxy NextNode { get { return ((TreeNodeProxy) _treeNode).NextNode; } }
        internal string Text { get { return ((TreeNodeProxy) _treeNode).Text; } }

        internal INodeDatum MakeNodeDatum() { return (null != _LocalTreeNode)
            ? new Local.NodeDatum()
            : (INodeDatum) new NodeDatum(); }

        internal TreeNodeProxy MakeTreeNode(string strText) { return (null != _LocalTreeNode)
            ? (TreeNodeProxy)new LocalTreeNode(strText)
            : (TreeNodeProxy)new TreeNode(strText); }

        LocalTreeNode _LocalTreeNode { get { return _treeNode as LocalTreeNode; } }
        TreeNode _TreeNode { get { return _treeNode as TreeNode; } }

        readonly object _treeNode;
        IEnumerable<TreeNodeProxy> _modifiedNodes;
    }
}
