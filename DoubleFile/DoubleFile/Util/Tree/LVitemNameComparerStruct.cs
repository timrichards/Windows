using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    struct LVitemNameComparerStruct : IEqualityComparer<LocalLVitem>
    {
        public bool Equals(LocalLVitem x, LocalLVitem y)
        {
            return (x != null) &&
                (y != null) &&
                (false == string.IsNullOrWhiteSpace(x.Name)) &&
                x.Name.Equals(y.Name);
        }

        public int GetHashCode(LocalLVitem obj)
        {
            return obj.Name == null ? 0 : obj.Name.GetHashCode();
        }

        static internal void NameItems(ListView.ListViewItemCollection list)
        {
            foreach (LocalLVitem item in list)
            {
                item.Name = item.Text;

                if ((null != item.SubItems) && (item.SubItems.Count > FileParse.knColLengthLV))
                {
                    item.Name += item.SubItems[FileParse.knColLengthLV];         // name + size
                }
            }
        }

        static internal void MarkItemsFrom1notIn2(LocalLV lv1, LocalLV lv2)
        {
            if ((0 == lv1.Items.Length) || (0 == lv2.Items.Length))
                return;

            var list = lv1.Items.Except(lv2.Items, new LVitemNameComparerStruct());

            lv1.TopItem = list.FirstOrDefault();

            foreach (var item in list)
                item.ForeColor = UtilColor.Red;
        }

        static internal void SetTopItem(LocalLV lv1, LocalLV lv2)
        {
            if (lv1.TopItem == null) { return; }
            if (lv1.TopItem.Index > 0) { return; }
            if (lv2.TopItem == null) { return; }

            var lv2Count = (null != lv2.Items) ? lv2.Items.Length : 0;
            var lv1Count = (null != lv1.Items) ? lv1.Items.Length : 0;

            var nIx = lv2.TopItem.Index - Math.Abs(lv2Count - lv1Count);

            if (nIx < 0)
            {
                return;
            }

            if (lv1Count > nIx)
            {
                lv1.TopItem = lv1.Items[nIx];
            }
        }
    }
}
