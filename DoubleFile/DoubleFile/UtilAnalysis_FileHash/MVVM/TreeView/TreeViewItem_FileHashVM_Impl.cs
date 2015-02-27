
using System.Collections.Generic;
namespace DoubleFile
{
    partial class TreeViewItem_FileHashVM
    {
        static internal event System.Action<IEnumerable<string>, string> SelectedItemChanged;

        void DoTreeSelect()
        {
            if ((false == _bSelected) ||
                (null == SelectedItemChanged))
            {
                return;
            }

            var dictDriveInfo = new Dictionary<string, string>();

            new Local.TreeSelect(_datum, dictDriveInfo, false, false,
                (lvItemDetails, itemArray, lvVolDetails, bSecondComparePane, lvFileItem) => { },
                (lsFiles, strListingFile) =>
            {
                SelectedItemChanged(lsFiles, strListingFile);
            }
                ).DoThreadFactory();
        }
    }

}
