using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_VolumeDetailVM : IDisposable
    {
        internal LV_VolumeDetailVM()
        {
            TreeSelect.VolumeDetailUpdated += TreeSelect_VolumeDetail;
        }

        public void Dispose()
        {
            TreeSelect.VolumeDetailUpdated -= TreeSelect_VolumeDetail;
        }

        void TreeSelect_VolumeDetail(IEnumerable<IEnumerable<string>> ieDetail, string strTitle)
        {
            UtilProject.UIthread(() =>
            {
                Title = strTitle;
                Items.Clear();

                if (null == ieDetail)
                    return;     // from lambda

                foreach (var ieLine in ieDetail)
                    Add(new LVitem_VolumeDetailVM(ieLine), bQuiet: true);

                RaiseItems();
            });
        }
    }
}
