using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace DoubleFile
{
    class LocalTreeNode : LocalColorItemBase
    {
        static internal IObservable<Tuple<LocalTreeNode, int>>
            Selected { get { return _selected.AsObservable(); } }
        static readonly LocalSubject<LocalTreeNode> _selected = new LocalSubject<LocalTreeNode>();
        void SelectedOnNext() { _selected.LocalOnNext(this, 99851); }

        static internal IObservable<Tuple<string, int>>
            SelectedFile { get { return _selectedFile.AsObservable(); } }
        static readonly LocalSubject<string> _selectedFile = new LocalSubject<string>();
        static void SelectedFileOnNext(string value) { _selectedFile.LocalOnNext(value, 99850); }

        public LocalTreeNode[]
            Nodes { get; protected set; }
        internal virtual string
            Text { get { return _Text; } set { _Text = value; } } TabledString<Tabled_Folders> _Text = null;
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

        internal void DetachFromTree()
        {
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

        internal LocalTreeNode Root()
        {
            var nodeParent = this;

            while (nodeParent.Parent != null)
                nodeParent = nodeParent.Parent;

            return nodeParent;
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

        internal void GoToFile(string strFile)
        {
            SelectedOnNext();
            SelectedFileOnNext(strFile);
        }
    }
}
