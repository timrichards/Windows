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

        void TreeSelect_FolderDetailUpdated(IEnumerable<IEnumerable<string>> ieDetail, LocalTreeNode treeNode)
        {
            UtilProject.UIthread(() =>
            {
                Title = null;
                Items.Clear();

                foreach (var ieLine in ieDetail)
                    Add(new LVitem_FolderDetailVM(ieLine), bQuiet: true);

                if (null == treeNode)
                    return;

                var strFG_Description = UtilColor.Description[treeNode.ForeColor];
                var strBG_Description = UtilColor.Description[treeNode.BackColor];

                if (false == string.IsNullOrEmpty(strFG_Description))
                {
                    Add(new LVitem_FolderDetailVM(new[] { "", strFG_Description })
                    {
                        Foreground = UtilColor.ARGBtoBrush(treeNode.ForeColor)
                    }, bQuiet: true);
                }

                if (false == string.IsNullOrEmpty(strBG_Description))
                {
                    Add(new LVitem_FolderDetailVM(new[] { "", strBG_Description })
                    {
                        Background = UtilColor.ARGBtoBrush(treeNode.BackColor)
                    }, bQuiet: true);
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
