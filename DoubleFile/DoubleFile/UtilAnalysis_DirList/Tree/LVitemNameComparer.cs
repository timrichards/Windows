using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    class LVitemNameComparer : IEqualityComparer<ListViewItem>
    {
        public bool Equals(ListViewItem x, ListViewItem y)
        {
            return (x != null) && (y != null) && (false == string.IsNullOrWhiteSpace(x.Name)) && x.Name.Equals(y.Name);
        }

        public int GetHashCode(ListViewItem obj)
        {
            return obj.Name == null ? 0 : obj.Name.GetHashCode();
        }

        internal static void NameItems(ListView.ListViewItemCollection list)
        {
            foreach (ListViewItem item in list)
            {
                item.Name = item.Text;

                if (item.SubItems.Count > UtilAnalysis_DirList.knColLengthLV)
                {
                    item.Name += item.SubItems[UtilAnalysis_DirList.knColLengthLV].Text;         // name + size
                }
            }
        }

        internal static void MarkItemsFrom1notIn2(SDL_ListView lv1, SDL_ListView lv2)
        {
            if ((lv1.Items.Count <= 0) || (lv2.Items.Count <= 0)) { return; }

            List<ListViewItem> list = lv1.Items.Cast<ListViewItem>().Except(lv2.Items.Cast<ListViewItem>(), new LVitemNameComparer()).ToList();

            if (list.Count > 0)
            {
                lv1.TopItem = list[0];
            }

            foreach (ListViewItem item in list)
            {
                item.ForeColor = Color.Red;
            }
        }

        internal static void SetTopItem(SDL_ListView lv1, SDL_ListView lv2)
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
