using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace DoubleFile
{
    class LocalTreeNode : LocalColorItemBase
    {
        internal readonly LocalTreeNodeCollection
            _Nodes = null;
        internal UString
            _Text = null;
        internal LocalTV
            _TreeView = null;
        internal LocalTreeNode
            _FirstNode = null;
        internal LocalTreeNode
            _NextNode = null;
        internal LocalTreeNode
            _Parent = null;
        internal object
            _Tag = null;

        internal string
            Name { get { return _Text; } }
        internal int
            Level { get { return Datum6bits; } set { Datum6bits = value; } }
        internal int
            SelectedImageIndex { get { return Datum16bits; } set { Datum16bits = value; } }

//       internal TreeViewItem_FileHashVM TVIVM = null;
//       internal LVitem_FileHashVM LVIVM = null;
//       internal string ToolTipText = null;
//       internal bool Checked = false;

        internal LocalTreeNode()
        {
            _Nodes = new LocalTreeNodeCollection(_TreeView);
        }

        internal LocalTreeNode(string strContent)
            : this()
        {
            Level = -1;
            SelectedImageIndex = -1;
            _Text = strContent;
        }

        internal LocalTreeNode(string strContent, IReadOnlyList<LocalTreeNode> lsNodes)
            : this(strContent)
        {
            _Nodes.AddRange(lsNodes);
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
            _TreeView = null;
            Level = -1;
 //           _strFullPath = null;

            foreach (var treeNode in _Nodes)
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

            var parentNode = _Parent;

            while (parentNode != null)
            {
                if (parentNode == treeNode)
                {
                    return true;
                }

                parentNode = parentNode._Parent;
            }

            return false;
        }

        internal LocalTreeNode Root()
        {
            var nodeParent = this;

            while (nodeParent._Parent != null)
            {
                nodeParent = nodeParent._Parent;
            }

            return nodeParent;
        }

    //    string _strFullPath = null;
    }
}
