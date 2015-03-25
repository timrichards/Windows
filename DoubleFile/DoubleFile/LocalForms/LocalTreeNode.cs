using System.Collections.Generic;
using System.Linq;
using System;

namespace DoubleFile
{
    class LocalTreeNode : LocalColorItemBase
    {
        static internal event Action<LocalTreeNode> Selected = null;
        static internal event Action<string> SelectedFile = null;

        public LocalTreeNode[]
            Nodes { get; protected set; }
        internal virtual string
            Text { get { return _Text; } set { _Text = value; } } TabledString _Text = null;
        static internal LocalTV
            TreeView { get; set; }
        internal LocalTreeNode
            FirstNode { get { return ((null != Nodes) && (0 < Nodes.Length)) ? Nodes[0] : null; } }
        internal LocalTreeNode
            NextNode { get; private set; }
        internal LocalTreeNode
            Parent { get; private set; }
        internal NodeDatum
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
            Nodes = lsNodes.ToArray();
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

            if (null == Nodes)
                return;

            foreach (var treeNode in Nodes)
                treeNode.DetachFromTree();
        }

  //      internal void EnsureVisible() { }

        internal bool IsChildOf(LocalTreeNode treeNode)
        {
            if (Level <= treeNode.Level)
                return false;

            var parentNode = Parent;

            while (parentNode != null)
            {
                if (parentNode == treeNode)
                    return true;

                parentNode = parentNode.Parent;
            }

            return false;
        }

        internal LocalTreeNode Root()
        {
            var nodeParent = this;

            while (nodeParent.Parent != null)
                nodeParent = nodeParent.Parent;

            return nodeParent;
        }

        internal static void SetLevel(IReadOnlyList<LocalTreeNode> nodes, LocalTreeNode nodeParent = null, int nLevel = 0)
        {
            if (null == nodes)
                return;

            LocalTreeNode nodePrev = null;

            foreach (var treeNode in nodes)
            {
                if (null != nodePrev)
                    nodePrev.NextNode = treeNode;

                nodePrev = treeNode;
                treeNode.Parent = nodeParent;
                treeNode.Level = nLevel;
                SetLevel(treeNode.Nodes, treeNode, nLevel + 1);
            }
        }

        internal void GoToFile(string strFile)
        {
            if (null != Selected)
                Selected(this);

            if (null != SelectedFile)
                SelectedFile(strFile);
        }

        //string _strFullPath = null;
    }
}
