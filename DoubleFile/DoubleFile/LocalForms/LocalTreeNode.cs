using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleFile
{
    class LocalTreeNode : LocalColorItemBase
    {
        public LocalTreeNode[]
            Nodes { get; protected set; }
        internal virtual string
            Text { get { return _Text; } set { _Text = value; } } TabledString<Tabled_Folders> _Text = null;
        internal LocalTreeNode
            FirstNode { get { return ((null != Nodes) && Nodes.LocalAny()) ? Nodes[0] : null; } }
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

        internal LocalTreeNode Root
        {
            get
            {
                var nodeParent = this;

                while (nodeParent.Parent != null)
                    nodeParent = nodeParent.Parent;

                return nodeParent;
            }
        }

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

        internal LocalTreeNode(TabledString<Tabled_Folders> strContent)
            : this()
        {
            Text = strContent;
        }

        internal LocalTreeNode(string strContent, IEnumerable<LocalTreeNode> lsNodes)
            : this(strContent)
        {
            Nodes = lsNodes.ToArray();
        }

        internal LocalTreeNode DetachFromTree()
        {
            Level = -1;

            if (null == Nodes)
                return this;

            foreach (var treeNode in Nodes)
                treeNode.DetachFromTree();

            return this;
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
                }
                while (null != (treeNode = treeNode.Parent));

                return ("" + sbPath).TrimEnd('\\');
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

        static internal void SetLevel(IEnumerable<LocalTreeNode> nodes, LocalTreeNode nodeParent = null, int nLevel = 0)
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

        internal LocalTreeNode GoToFile(string strFile)
        {
            TreeSelect.DoThreadFactory(this, 0 /* UI Initiator */, strFile);
            return this;
        }
    }
}
