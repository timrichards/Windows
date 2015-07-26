using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    struct LVitemNameComparerStruct : IEqualityComparer<LocalLVitemVM>
    {
        public bool Equals(LocalLVitemVM x, LocalLVitemVM y)
        {
            return (x != null) &&
                (y != null) &&
                (false == string.IsNullOrWhiteSpace(x.Name)) &&
                x.Name.Equals(y.Name);
        }

        public int GetHashCode(LocalLVitemVM obj) => obj.Name?.GetHashCode() ?? 0;

        static internal void NameItems(ListView.ListViewItemCollection list)
        {
            foreach (LocalLVitemVM item in list)
            {
                item.Name = item.Folder;

                if ((item.SubItems?.Count ?? 0) > FileParse.knColLengthLV)
                    item.Name += item.SubItems[FileParse.knColLengthLV];         // name + size
            }
        }

        static internal void MarkItemsFrom1notIn2(LocalLVVM lv1, LocalLVVM lv2)
        {
            if ((0 == lv1.Items.Count) || (0 == lv2.Items.Count))
                return;

            var list = lv1.Items.Cast<LocalLVitemVM>().Except(lv2.Items.Cast<LocalLVitemVM>(), new LVitemNameComparerStruct());

            lv1.TopItem = list.FirstOrDefault();

            foreach (var item in list)
                item.ForeColor = UtilColor.Red;
        }

        static internal void SetTopItem(LocalLVVM lv1, LocalLVVM lv2)
        {
            if (lv1.TopItem == null) { return; }
            if (lv1.TopItem.Index > 0) { return; }
            if (lv2.TopItem == null) { return; }

            var lv2Count = lv2.Items?.Count ?? 0;
            var lv1Count = lv1.Items?.Count ?? 0;
            var nIx = lv2.TopItem.Index - Math.Abs(lv2Count - lv1Count);

            if (0 > nIx)
                return;

            if (lv1Count > nIx)
                lv1.TopItem = (LocalLVitemVM)lv1.Items[nIx];
        }
    }
}
