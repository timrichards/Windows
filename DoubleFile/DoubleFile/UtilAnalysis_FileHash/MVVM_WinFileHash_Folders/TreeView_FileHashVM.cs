using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;

namespace DoubleFile
{
    class TreeView_DoubleFileVM : IDisposable
    {
        internal TreeViewItem_DoubleFileVM
            SelectedItem { get; set; }
        internal KeyList<TreeViewItem_DoubleFileVM>
            _listExpanded = new KeyList<TreeViewItem_DoubleFileVM>();
        internal readonly TreeView
            _TVFE = null;
        internal Dictionary<string, string>
            _dictVolumeInfo = new Dictionary<string, string>();

        internal TreeView_DoubleFileVM(TreeView tvfe)
        {
            _TVFE = tvfe;
            WinDoubleFile_DuplicatesVM.GoToFile += GoToFile;
        }

        public void Dispose()
        {
            WinDoubleFile_DuplicatesVM.GoToFile -= GoToFile;
        }

        internal void SetData(IReadOnlyList<LocalTreeNode> rootNodes)
        {
            var nIndex = -1;

            foreach (var treeNode in rootNodes)
            {
                _Items.Add(new TreeViewItem_DoubleFileVM(this, treeNode, ++nIndex));
            }

            UtilProject.UIthread(() => _TVFE.DataContext = _Items);

            if (0 < _Items.Count)
                _Items[0].SelectProgrammatic(true);
        }

        private void GoToFile(LVitem_ProjectVM lvItem_ProjectVM, string strPath, string strFile)
        {
            _Items
                .Where(item => lvItem_ProjectVM.ListingFile == 
                    ((Local.RootNodeDatum)item._datum.NodeDatum).ListingFile)
                .FirstOnlyAssert(item =>
                    item.GoToFile(
                        strPath
                            .Replace(((Local.RootNodeDatum)item._datum.NodeDatum).RootPath, "")
                            .TrimStart('\\')
                            .Split('\\'),
                        strFile)
                );
        }

        readonly ObservableCollection<TreeViewItem_DoubleFileVM>
            _Items = new ObservableCollection<TreeViewItem_DoubleFileVM>();
    }
}
