using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace DoubleFile
{
    class LocalTreeNode : LocalColorItemBase
    {
        internal readonly LocalTreeNodeCollection Nodes = null;
        internal UString Text = null;
 //       internal string ToolTipText = null;
        internal string Name { get { return Text; } }

        internal LocalTV TreeView = null;
        internal LocalTreeNode FirstNode = null;
        internal LocalTreeNode NextNode = null;
        internal LocalTreeNode Parent = null;
        internal int Level { get { return Datum6bits; } set { Datum6bits = value; } }
 //       internal bool Checked = false;
        internal int SelectedImageIndex { get { return Datum16bits; } set { Datum16bits = value; } }
        internal object Tag = null;

 //       internal TreeViewItem_FileHashVM TVIVM = null;
 //       internal LVitem_FileHashVM LVIVM = null;

        internal LocalTreeNode()
        {
            Nodes = new LocalTreeNodeCollection(TreeView);
        }

        internal LocalTreeNode(string strContent)
            : this()
        {
            Level = -1;
            SelectedImageIndex = -1;
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

    //    string _strFullPath = null;
    }
}
