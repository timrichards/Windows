using System;
using System.Windows.Controls;

namespace SearchDirLists
{
    class CloneLVitemVM : ListViewItemVM
    {
        internal new const int NumCols = 0;
        readonly new static String[] arrPropName = new String[] { };

        CloneLVitemVM(ClonesListViewVM LV)
            : base(LV, NumCols, arrPropName) { }

        internal CloneLVitemVM(ClonesListViewVM LV, String[] arrStr)
            : this(LV)
        {
            CopyInArray(arrStr);
        }
    }

    class ClonesListViewVM : ListViewVM
    {
        internal ClonesListViewVM(ItemsControl itemsCtl) : base(itemsCtl) { }
    }
}
