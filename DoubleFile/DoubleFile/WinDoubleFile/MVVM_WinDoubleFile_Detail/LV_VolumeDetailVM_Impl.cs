using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_VolumeDetailVM : IDisposable
    {
        internal LV_VolumeDetailVM()
        {
            Local.TreeSelect.VolumeDetailUpdated += TreeSelect_VolumeDetail;
        }

        public void Dispose()
        {
            Local.TreeSelect.VolumeDetailUpdated -= TreeSelect_VolumeDetail;
        }

        void TreeSelect_VolumeDetail(IEnumerable<string[]> lasDetail, string strTitle)
        {
            UtilProject.UIthread(() =>
            {
                Title = strTitle;
                Items.Clear();

                if (null == lasDetail)
                    return;     // from lambda

                foreach (var asLine in lasDetail)
                    Add(new LVitem_VolumeDetailVM(asLine), bQuiet: true);

                RaiseItems();
            });
        }
    }
}
