using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class LVitemNameComparer : IEqualityComparer<SDL_ListViewItem>
    {
        public bool Equals(SDL_ListViewItem x, SDL_ListViewItem y)
        {
            return (x != null) && (y != null) && (false == string.IsNullOrWhiteSpace(x.Name)) && x.Name.Equals(y.Name);
        }

        public int GetHashCode(SDL_ListViewItem obj)
        {
            return obj.Name == null ? 0 : obj.Name.GetHashCode();
        }

        internal static void NameItems(ItemCollection list)
        {
            foreach (SDL_ListViewItem item in list)
            {
                item.Name = item.Text;

                if (item.SubItems.Count > Utilities.knColLengthLV)
                {
                    item.Name += item.SubItems[Utilities.knColLengthLV].Text;         // name + size
                }
            }
        }

        internal static void MarkItemsFrom1notIn2(SDL_ListView lv1, SDL_ListView lv2)
        {
            if ((lv1.Items.Count <= 0) || (lv2.Items.Count <= 0)) { return; }

            List<SDL_ListViewItem> list = lv1.Items.Cast<SDL_ListViewItem>().Except(lv2.Items.Cast<SDL_ListViewItem>(), new LVitemNameComparer()).ToList();

            if (list.Count > 0)
            {
                lv1.TopItem = list[0];
            }

            foreach (SDL_ListViewItem item in list)
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
