﻿using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DoubleFile
{
    class TreeView_FileHashVM
    {
        internal TreeView_FileHashVM(TreeView tvfe, Dispatcher dispatcher)
        {
            TVI_DependencyProperty.TVFE = TVFE = tvfe;
        }

        internal void SetData(List<SDL_TreeNode> rootNodes)
        {
            int nIndex = -1;

            foreach (SDL_TreeNode treeNode in rootNodes)
            {
                m_Items.Add(new TreeViewItem_FileHashVM(this, treeNode, ++nIndex));
            }

            TVFE.DataContext = m_Items;
        }

        internal TreeViewItem_FileHashVM SelectedItem = null;
        internal UList<TreeViewItem_FileHashVM> m_listExpanded = new UList<TreeViewItem_FileHashVM>();
        
        readonly List<TreeViewItem_FileHashVM> m_Items = new List<TreeViewItem_FileHashVM>();
        internal readonly TreeView TVFE = null;

        internal DateTime mDbg_dtProgrammaticExpand = DateTime.MinValue;
    }
}