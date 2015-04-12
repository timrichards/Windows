﻿using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace DoubleFile
{
    class LocalTreeNode : LocalColorItemBase
    {
        static internal event Action<LocalTreeNode> Selected = null;
        static internal event Action<string> SelectedFile = null;

        public LocalTreeNode[]
            Nodes { get; protected set; }
        internal virtual string
            Text { get { return _Text; } set { _Text = value; } } TabledString<TypedArray0> _Text = null;
        static internal LocalTV
            TreeView { get; set; }
        internal LocalTreeNode
            FirstNode { get { return ((null != Nodes) && (0 < Nodes.Length)) ? Nodes[0] : null; } }
        public virtual LocalTreeNode
            NextNode { get; protected set; }
        public virtual LocalTreeNode
            Parent { get; protected set; }
        internal NodeDatum
            NodeDatum { get; set; }

        internal string
            Name { get { return Text; } }
        internal int
            Level { get { return Datum8bits; } set { Datum8bits = value; } }
        internal int
            SelectedImageIndex { get { return Datum16bits; } set { Datum16bits = value; } }

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

        internal LocalTreeNode(TabledString<TypedArray0> strContent)
            : this()
        {
            Text = strContent;
        }

        internal LocalTreeNode(string strContent, IEnumerable<LocalTreeNode> lsNodes)
            : this(strContent)
        {
            Nodes = lsNodes.ToArray();
        }

        internal void DetachFromTree()
        {
            TreeView = null;
            Level = -1;

            if (null == Nodes)
                return;

            foreach (var treeNode in Nodes)
                treeNode.DetachFromTree();
        }

        internal string FullPath
        {
            get
            {
                var sbPath = new StringBuilder();
                var treeNode = this;

                do
                {
                    sbPath
                        .Insert(0, '\\')
                        .Insert(0, treeNode.Text);
                } while (null != (treeNode = treeNode.Parent));

                return sbPath.ToString().TrimEnd('\\');
            }
        }

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

        internal static void SetLevel(IEnumerable<LocalTreeNode> nodes, LocalTreeNode nodeParent = null, int nLevel = 0)
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
    }
}
