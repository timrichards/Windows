using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    struct LVitemNameComparerStruct : IEqualityComparer<LVitem_ClonesVM>
    {
        public bool Equals(LVitem_ClonesVM x, LVitem_ClonesVM y)
        {
            return (x != null) &&
                (y != null) &&
                (false == string.IsNullOrWhiteSpace("" + x.Name)) &&
                x.Name.Equals(y.Name);
        }

        public int GetHashCode(LVitem_ClonesVM obj) => obj.Name?.GetHashCode() ?? 0;

        static internal void NameItems(ListView.ListViewItemCollection list)
        {
            foreach (LVitem_ClonesVM item in list)
            {
                var strName = item.Folder;

                if ((item.SubItems?.Count ?? 0) > FileParse.knColLengthLV)
                    strName += item.SubItems[FileParse.knColLengthLV];         // name + size

                item.Name = (TabledString<TabledStringType_Folders>)strName;
            }
        }

        static internal void MarkItemsFrom1notIn2(UC_ClonesVM lv1, UC_ClonesVM lv2)
        {
            if ((0 == lv1.Items.Count) || (0 == lv2.Items.Count))
                return;

            var list = lv1.Items.Cast<LVitem_ClonesVM>().Except(lv2.Items.Cast<LVitem_ClonesVM>(), new LVitemNameComparerStruct());

            lv1.TopItem = list.FirstOrDefault();

            //foreach (var item in list)
            //    item.ForeColor = UtilColor.Red;
        }

        static internal void SetTopItem(UC_ClonesVM lv1, UC_ClonesVM lv2)
        {
            if ((0 < (lv1.TopItem?.Index ?? 1)) ||
                (null == lv2.TopItem))
            {
                return;
            }

            var lv2Count = lv2.Items?.Count ?? 0;
            var lv1Count = lv1.Items?.Count ?? 0;
            var nIx = lv2.TopItem.Index - Math.Abs(lv2Count - lv1Count);

            if (0 > nIx)
                return;

            if (lv1Count > nIx)
                lv1.TopItem = (LVitem_ClonesVM)lv1.Items[nIx];
        }
    }
}
