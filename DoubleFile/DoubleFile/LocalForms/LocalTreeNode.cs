using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace DoubleFile
{
    class LocalTreeNode : LocalColorItemBase
    {
        internal LocalTreeNodeCollection
            Nodes { get; private set; }
        internal virtual string
            Text { get { return _Text; } set { _Text = value; } } TabledString _Text = null;
        internal LocalTV
            TreeView { get; private set; }
        internal LocalTreeNode
            FirstNode { get; private set; }
        internal LocalTreeNode
            NextNode { get; private set; }
        internal LocalTreeNode
            Parent { get; private set; }
        internal Local.NodeDatum
            NodeDatum { get; set; }

        internal string
            Name { get { return Text; } }
        internal int
            Level { get { return Datum8bits; } set { Datum8bits = value; } }
        internal int
            SelectedImageIndex { get { return Datum16bits; } set { Datum16bits = value; } }

//       internal TreeViewItem_DoubleFileVM TVIVM = null;
//       internal LVitem_DoubleFileVM LVIVM = null;
//       internal string ToolTipText = null;
//       internal bool Checked = false;

        internal LocalTreeNode()
        {
            Level = -1;
            SelectedImageIndex = -1;
            Nodes = new LocalTreeNodeCollection(TreeView);
        }

        internal LocalTreeNode(string strContent)
            : this()
        {
            Text = strContent;
        }

        internal LocalTreeNode(TabledString strContent)
            : this()
        {
            Text = strContent;
        }

        internal LocalTreeNode(string strContent, IReadOnlyList<LocalTreeNode> lsNodes)
            : this(strContent)
        {
            Nodes.AddRange(lsNodes);
        }

        //internal string FullPath
        //{
        //    get
        //    {
        //        if (_strFullPath != null)
        //        {
        //            return _strFullPath;
        //        }

        //        var stack = new Stack<LocalTreeNode>(8);
        //        var nodeParent = Parent;

        //        while (nodeParent != null)
        //        {
        //            stack.Push(nodeParent);
        //            nodeParent = nodeParent.Parent;
        //        }

        //        var sb = new StringBuilder();

        //        while (false == stack.IsEmpty())
        //        {
        //            nodeParent = stack.Pop();
        //            sb.Append(nodeParent.Text + '\\');
        //        }

        //        sb.Append(Text);
        //        _strFullPath = sb.ToString();
        //        return _strFullPath;
        //    }
        //}

        internal void DetachFromTree()
        {
 //           TVIVM = null;
            TreeView = null;
            Level = -1;
 //           _strFullPath = null;

            foreach (var treeNode in Nodes)
            {
                treeNode.DetachFromTree();
            }
        }

  //      internal void EnsureVisible() { }

        internal bool IsChildOf(LocalTreeNode treeNode)
        {
            if (Level <= treeNode.Level)
            {
                return false;
            }

            var parentNode = Parent;

            while (parentNode != null)
            {
                if (parentNode == treeNode)
                {
                    return true;
                }

                parentNode = parentNode.Parent;
            }

            return false;
        }

        internal LocalTreeNode Root()
        {
            var nodeParent = this;

            while (nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            return nodeParent;
        }

        internal static void SetLevel(LocalTV treeView,
            IReadOnlyList<LocalTreeNode> nodes, LocalTreeNode nodeParent = null, int nLevel = 0)
        {
            LocalTreeNode nodePrev = null;

            if ((nodeParent != null) && (false == nodes.IsEmptyA()))
            {
                nodeParent.FirstNode = nodes[0];
            }

            foreach (var treeNode in nodes)
            {
                if (nodePrev != null)
                {
                    nodePrev.NextNode = treeNode;
                }

                // same assert that Forms generates: must remove it from the other tree first.
                MBoxStatic.Assert(99999, (treeNode.TreeView == null) || (treeNode.TreeView == treeView));

                nodePrev = treeNode;
                treeNode.TreeView = treeView;
                treeNode.Parent = nodeParent;
                treeNode.Level = nLevel;
                SetLevel(treeView, treeNode.Nodes, treeNode, nLevel + 1);
            }
        }

        //string _strFullPath = null;
    }
}
