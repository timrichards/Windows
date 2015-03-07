using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;

namespace DoubleFile
{
    class TreeView_FileHashVM : IDisposable
    {
        internal TreeViewItem_FileHashVM
            SelectedItem { get; set; }
        internal UList<TreeViewItem_FileHashVM>
            _listExpanded = new UList<TreeViewItem_FileHashVM>();
        internal readonly TreeView
            _TVFE = null;
        internal Dictionary<string, string>
            _dictVolumeInfo = new Dictionary<string, string>();

        internal TreeView_FileHashVM(TreeView tvfe)
        {
            _TVFE = tvfe;
            WinFileHash_DuplicatesVM.GoToFile += GoToFile;
        }

        public void Dispose()
        {
            WinFileHash_DuplicatesVM.GoToFile -= GoToFile;
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

        readonly ObservableCollection<TreeViewItem_FileHashVM>
            _Items = new ObservableCollection<TreeViewItem_FileHashVM>();
    }
}
