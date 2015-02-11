using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DoubleFile
{
    class TreeView_FileHashVM
    {
        internal TreeView_FileHashVM(TreeView tvfe)
        {
            TVI_DependencyProperty.TVFE = TVFE = tvfe;
        }

        internal void SetData(IReadOnlyList<LocalTreeNode> rootNodes)
        {
            var nIndex = -1;

            foreach (var treeNode in rootNodes)
            {
                m_Items.Add(new TreeViewItem_FileHashVM(this, treeNode, ++nIndex));
            }

            TVFE.DataContext = m_Items;
        }

        internal TreeViewItem_FileHashVM SelectedItem = null;
        internal UList<TreeViewItem_FileHashVM> m_listExpanded = new UList<TreeViewItem_FileHashVM>();
        internal readonly TreeView TVFE = null;

        readonly List<TreeViewItem_FileHashVM> m_Items = new List<TreeViewItem_FileHashVM>();
    }
}
