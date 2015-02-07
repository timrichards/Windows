using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoubleFile;

namespace WPF
{
    struct LVitemNameComparerStruct : IEqualityComparer<WPF_LVitem>
    {
        public bool Equals(WPF_LVitem x, WPF_LVitem y)
        {
            return (x != null) && (y != null) && (false == string.IsNullOrWhiteSpace(x.Name)) && x.Name.Equals(y.Name);
        }

        public int GetHashCode(WPF_LVitem obj)
        {
            return obj.Name == null ? 0 : obj.Name.GetHashCode();
        }

        internal static void NameItems(ListView.ListViewItemCollection list)
        {
            foreach (WPF_LVitem item in list)
            {
                item.Name = item.Text;

                if (item.SubItems.Count > FileParse.knColLengthLV)
                {
                    item.Name += item.SubItems[FileParse.knColLengthLV].Text;         // name + size
                }
            }
        }

        internal static void MarkItemsFrom1notIn2(WPF_ListView lv1, WPF_ListView lv2)
        {
            if ((lv1.Items.Count <= 0) || (lv2.Items.Count <= 0)) { return; }

            List<WPF_LVitem> list = lv1.Items.Cast<WPF_LVitem>().Except(lv2.Items.Cast<WPF_LVitem>(), new LVitemNameComparerStruct()).ToList();

            if (list.Count > 0)
            {
                lv1.TopItem = list[0];
            }

            foreach (WPF_LVitem item in list)
            {
                item.ForeColor = Color.Red;
            }
        }

        internal static void SetTopItem(WPF_ListView lv1, WPF_ListView lv2)
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
