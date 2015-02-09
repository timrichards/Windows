using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DoubleFile;
using System.Windows.Media;

namespace Local
{
    struct LVitemNameComparerStruct : IEqualityComparer<LocalLVitem>
    {
        public bool Equals(LocalLVitem x, LocalLVitem y)
        {
            return (x != null) && (y != null) && (false == string.IsNullOrWhiteSpace(x.Name)) && x.Name.Equals(y.Name);
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
            if ((lv1.Items.Count <= 0) || (lv2.Items.Count <= 0)) { return; }

            List<LocalLVitem> list = lv1.Items.Cast<LocalLVitem>().Except(lv2.Items.Cast<LocalLVitem>(), new LVitemNameComparerStruct()).ToList();

            if (list.Count > 0)
            {
                lv1.TopItem = list[0];
            }

            foreach (LocalLVitem item in list)
            {
                item.ForeColor = UtilColor.Red;
            }
        }

        internal static void SetTopItem(LocalLV lv1, LocalLV lv2)
        {
            if (lv1.TopItem == null) { return; }
            if (lv1.TopItem.Index > 0) { return; }
            if (lv2.TopItem == null) { return; }

            int nIx = lv2.TopItem.Index - Math.Abs(lv2.Items.Count - lv1.Items.Count);

            if (nIx < 0)
            {
                return;
            }

            if (lv1.Items.Count > nIx)
            {
                lv1.TopItem = lv1.Items[nIx];
            }
        }
    }
}
