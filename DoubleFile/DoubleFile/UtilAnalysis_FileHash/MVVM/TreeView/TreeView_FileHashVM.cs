using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DoubleFile
{
    class TreeView_FileHashVM
    {
        internal TreeView_FileHashVM(TreeView tvfe)
        {
            _TVFE = tvfe;
        }

        internal void SetData(IReadOnlyList<LocalTreeNode> rootNodes)
        {
            var nIndex = -1;

            foreach (var treeNode in rootNodes)
            {
                _Items.Add(new TreeViewItem_FileHashVM(this, treeNode, ++nIndex));
            }

            UtilProject.UIthread(() => _TVFE.DataContext = _Items);
        }

        internal TreeViewItem_FileHashVM
            _SelectedItem = null;
        internal UList<TreeViewItem_FileHashVM>
            _listExpanded = new UList<TreeViewItem_FileHashVM>();
        internal readonly TreeView
            _TVFE = null;

        readonly ObservableCollection<TreeViewItem_FileHashVM>
            _Items = new ObservableCollection<TreeViewItem_FileHashVM>();
    }
}
