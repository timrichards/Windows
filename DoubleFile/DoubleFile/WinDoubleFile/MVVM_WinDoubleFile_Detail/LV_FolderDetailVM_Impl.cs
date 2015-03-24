using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_FolderDetailVM : IDisposable
    {
        internal LV_FolderDetailVM()
        {
            TreeSelect.FolderDetailUpdated += TreeSelect_FolderDetailUpdated;
        }

        public void Dispose()
        {
            TreeSelect.FolderDetailUpdated -= TreeSelect_FolderDetailUpdated;
        }

        void TreeSelect_FolderDetailUpdated(IEnumerable<string[]> lasDetail, LocalTreeNode treeNode)
        {
            UtilProject.UIthread(() =>
            {
                Title = null;
                Items.Clear();

                foreach (var asLine in lasDetail)
                    Add(new LVitem_FolderDetailVM(asLine), bQuiet: true);

                var strFG_Description = UtilColor.Description[treeNode.ForeColor];
                var strBG_Description = UtilColor.Description[treeNode.BackColor];

                if (false == string.IsNullOrEmpty(strFG_Description))
                {
                    var lvItem = new LVitem_FolderDetailVM(new[] { "", strFG_Description });

                    lvItem.Foreground = UtilColor.ARGBtoBrush(treeNode.ForeColor);
                    Add(lvItem, bQuiet: true);
                }

                if (false == string.IsNullOrEmpty(strBG_Description))
                {
                    var lvItem = new LVitem_FolderDetailVM(new[] { "", strBG_Description });

                    lvItem.Background = UtilColor.ARGBtoBrush(treeNode.BackColor);
                    Add(lvItem, bQuiet: true);
                }
#if DEBUG
                Add(new LVitem_FolderDetailVM(new[] { "Hash Parity", "" + treeNode.NodeDatum.HashParity }), bQuiet: true);
#endif
                Title = treeNode.Text;
                RaiseItems();
            });
        }
    }
}
