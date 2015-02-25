using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DoubleFile;

namespace Local
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

        internal static void NameItems(ListView.ListViewItemCollection list)
        {
            foreach (LocalLVitem item in list)
            {
                item.Name = item.Text;

                if (item.SubItems.Count > FileParse.knColLengthLV)
                {
                    item.Name += item.SubItems[FileParse.knColLengthLV].Text;         // name + size
                }
            }
        }

        internal static void MarkItemsFrom1notIn2(LocalLV lv1, LocalLV lv2)
        {
            if ((lv1._Items.IsEmpty()) || (lv2._Items.IsEmpty())) { return; }

            var list = lv1._Items.Except(lv2._Items, new LVitemNameComparerStruct());

            list.Take(1).First(item => lv1._TopItem = item);

            foreach (var item in list)
            {
                item.ForeColor = UtilColor.Red;
            }
        }

        internal static void SetTopItem(LocalLV lv1, LocalLV lv2)
        {
            if (lv1._TopItem == null) { return; }
            if (lv1._TopItem.Index > 0) { return; }
            if (lv2._TopItem == null) { return; }

            var nIx = lv2._TopItem.Index - Math.Abs(lv2._Items.Count - lv1._Items.Count);

            if (nIx < 0)
            {
                return;
            }

            if (lv1._Items.Count > nIx)
            {
                lv1._TopItem = lv1._Items[nIx];
            }
        }
    }
}
