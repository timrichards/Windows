using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace DoubleFile
{
    class BothNodes
    {
        internal BothNodes(TreeNode treeNode)
        {
            m_treeNode = treeNode;
        }

        internal BothNodes(SDL_TreeNode sdlNode)
        {
            m_sdlNode = sdlNode;
        }

        public static explicit operator BothNodes(TreeNode treeNode)
        {
            return new BothNodes(treeNode);
        }

        public static explicit operator BothNodes(SDL_TreeNode sdlNode)
        {
            return new BothNodes(sdlNode);
        }

        public static explicit operator TreeNode(BothNodes bothNodes)
        {
            return bothNodes.m_treeNode;
        }

        public static explicit operator SDL_TreeNode(BothNodes bothNodes)
        {
            return bothNodes.m_sdlNode;
        }

        internal bool IsChildOf(BothNodes treeNode)
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

        internal BothNodes Root
        {
            get
            {
                var nodeParent = this;

                while (nodeParent.Parent != null)
                {
                    nodeParent = nodeParent.Parent;
                }

                return nodeParent;
            }
        }

        internal Color BackColor { get { return (Color)GetProperty("BackColor"); } set { SetProperty("BackColor", value); } }
        internal bool Checked { get { return (bool)GetProperty("Checked"); } set { SetProperty("Checked", value); } }
        internal Color ForeColor { get { return (Color)GetProperty("ForeColor"); } set { SetProperty("ForeColor", value); } }
        internal BothNodes FirstNode { get { return (BothNodes)GetProperty("FirstNode"); } set { SetProperty("FirstNode", value); } }
        internal int Level { get { return (int)GetProperty("Level"); } set { SetProperty("Level", value); } }
        internal BothNodes NextNode { get { return (BothNodes)GetProperty("NextNode"); } set { SetProperty("NextNode", value); } }
        internal BothNodesCollection Nodes { get { return (BothNodesCollection)GetProperty("Nodes"); } set { SetProperty("Nodes", value); } }
        internal BothNodes Parent { get { return (BothNodes)GetProperty("Parent"); } set { SetProperty("Parent", value); } }
        internal object Tag { get { return GetProperty("Tag"); } set { SetProperty("Tag", value); } }
        internal string Text { get { return (string)GetProperty("Text"); } set { SetProperty("Text", value); } }
        internal TreeView TreeView { get { return (TreeView)GetProperty("TreeView"); } set { SetProperty("TreeView", value); } }

        object GetProperty(string strPropName)
        {
            var obj = m_treeNode ?? (object)m_sdlNode;

            return (string)obj.GetType().InvokeMember(strPropName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                Type.DefaultBinder, obj, null);
        }

        void SetProperty(string strPropName, object value)
        {
            var obj = m_treeNode ?? (object)m_sdlNode;

            obj.GetType().InvokeMember(strPropName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                Type.DefaultBinder, obj, new object[] { value });
        }

        //internal BothNodes Nodes
        //{
        //    get
        //    {
        //        if (m_treeNode != null)
        //        {
        //            return m_treeNode.Nodes;
        //        }
        //        else if (m_sdlNode != null)
        //        {
        //            return m_sdlNode.Nodes;
        //        }
        //    }
        //}

        TreeNode m_treeNode = null;
        SDL_TreeNode m_sdlNode = null;
    }

    class BothNodesCollection : IEnumerable<BothNodes>
    {
        internal BothNodesCollection(TreeNodeCollection treeNodeCollection)
        {
            m_treeNodeCollection = treeNodeCollection;
        }

        internal BothNodesCollection(SDL_TreeNodeCollection sdlCollection)
        {
            m_sdlNodeCollection = sdlCollection;
        }

        public static explicit operator BothNodesCollection(TreeNodeCollection treeNodeCollection)
        {
            return new BothNodesCollection(treeNodeCollection);
        }

        public static explicit operator BothNodesCollection(SDL_TreeNodeCollection sdlCollection)
        {
            return new BothNodesCollection(sdlCollection);
        }

        public IEnumerator<BothNodes> GetEnumerator()
        {
            if (m_treeNodeCollection != null)
                return (IEnumerator<BothNodes>)m_treeNodeCollection.GetEnumerator();
            else
                return (IEnumerator<BothNodes>)m_sdlNodeCollection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public static explicit operator TreeNodeCollection(BothNodesCollection bothNodesCollection)
        {
            return bothNodesCollection.m_treeNodeCollection;
        }

        public static explicit operator SDL_TreeNodeCollection(BothNodesCollection bothNodesCollection)
        {
            return bothNodesCollection.m_sdlNodeCollection;
        }

        internal BothNodes this[int i]
        {
            get
            {
                if (m_treeNodeCollection != null)
                    return (BothNodes)m_treeNodeCollection[i];
                else
                    return (BothNodes)m_sdlNodeCollection[i];
            }
        }

        internal int Count { get { return (int)GetProperty("Count"); } }

        object GetProperty(string strPropName)
        {
            var obj = m_treeNodeCollection ?? (object)m_sdlNodeCollection;

            return (string)obj.GetType().InvokeMember(strPropName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                Type.DefaultBinder, obj, null);
        }

        void SetProperty(string strPropName, object value)
        {
            var obj = m_treeNodeCollection ?? (object)m_sdlNodeCollection;

            obj.GetType().InvokeMember(strPropName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                Type.DefaultBinder, obj, new object[] { value });
        }

        //internal BothNodes Nodes
        //{
        //    get
        //    {
        //        if (m_treeNode != null)
        //        {
        //            return m_treeNode.Nodes;
        //        }
        //        else if (m_sdlNode != null)
        //        {
        //            return m_sdlNode.Nodes;
        //        }
        //    }
        //}

        TreeNodeCollection m_treeNodeCollection = null;
        SDL_TreeNodeCollection m_sdlNodeCollection = null;
    }

    class SDL_TreeNode
    {
        internal TreeViewItem_FileHashVM TVIVM = null;
        internal LVitem_FileHashVM LVIVM = null;

        public static explicit operator TreeNode(SDL_TreeNode sdlNode)
        {
            return new TreeNode();
        }

        internal SDL_TreeNode()
        {
            Nodes = new SDL_TreeNodeCollection(TreeView);
        }

        internal SDL_TreeNode(string strContent)
            : this()
        {
            Text = strContent;
        }

        internal SDL_TreeNode(string strContent, SDL_TreeNode[] arrNodes)
            : this(strContent)
        {
            Nodes.AddRange(arrNodes);
        }

        internal string FullPath
        {
            get
            {
                if (m_strFullPath != null)
                {
                    return m_strFullPath;
                }

                Stack<SDL_TreeNode> stack = new Stack<SDL_TreeNode>(8);
                SDL_TreeNode nodeParent = Parent;

                while (nodeParent != null)
                {
                    stack.Push(nodeParent);
                    nodeParent = nodeParent.Parent;
                }

                StringBuilder sb = new StringBuilder();

                while (stack.Count > 0)
                {
                    nodeParent = stack.Pop();
                    sb.Append(nodeParent.Text + '\\');
                }

                sb.Append(Text);
                m_strFullPath = sb.ToString();
                return m_strFullPath;
            }
        }

        internal void DetachFromTree()
        {
            TVIVM = null;
            TreeView = null;
            Level = -1;
            m_strFullPath = null;

            foreach (SDL_TreeNode treeNode in Nodes)
            {
                treeNode.DetachFromTree();
            }
        }

        internal void EnsureVisible() { }

        readonly internal SDL_TreeNodeCollection Nodes = null;
        internal string Text = null;
        internal string ToolTipText = null;
        internal string Name = null;
        internal TreeViewVMhack TreeView = null;
        internal SDL_TreeNode FirstNode = null;
        internal SDL_TreeNode NextNode = null;
        internal SDL_TreeNode Parent = null;
        internal int Level = -1;
        internal bool Checked = false;
        internal int SelectedImageIndex = -1;
        internal object Tag = null;

        internal Color BackColor = Color.Empty;
        internal Color ForeColor = Color.Empty;

        string m_strFullPath = null;
    }
}
