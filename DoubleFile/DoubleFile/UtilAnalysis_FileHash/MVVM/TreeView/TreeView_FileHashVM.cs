using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DoubleFile
{
    class TreeView_FileHashVM
    {
        internal TreeViewItem_FileHashVM
            SelectedItem { get; set; }
        internal UList<TreeViewItem_FileHashVM>
            _listExpanded = new UList<TreeViewItem_FileHashVM>();
        internal readonly TreeView
            _TVFE = null;

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

        readonly ObservableCollection<TreeViewItem_FileHashVM>
            _Items = new ObservableCollection<TreeViewItem_FileHashVM>();
    }
}
