using System.Drawing;

namespace DoubleFile
{
    class TreeViewVMhack
    {
        internal TreeViewVMhack()
        {
            Nodes = new SDL_TreeNodeCollection(this);
        }

        internal int GetNodeCount(bool includeSubTrees = false)
        {
            if (includeSubTrees)
            {
                return CountSubnodes(Nodes);
            }
            else
            {
                return Nodes.Count;
            }
        }

        internal void Select() { }

        readonly internal SDL_TreeNodeCollection Nodes = null;
        internal SDL_TreeNode SelectedNode = null;
        internal SDL_TreeNode TopNode = null;
        internal Font Font = null;
        internal bool CheckBoxes = false;
        internal bool Enabled = false;

        int CountSubnodes(SDL_TreeNodeCollection nodes)
        {
            int nRet = 0;

            foreach (SDL_TreeNode treeNode in nodes)
            {
                nRet += CountSubnodes(treeNode.Nodes);
                ++nRet;
            }

            return nRet;
        }
    }
}
