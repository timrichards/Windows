﻿using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_VolumeDetailVM : IDisposable
    {
        internal LV_VolumeDetailVM(GlobalData_Base gd)
        {
            _gd = gd;
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
                Title = null;
                Items.Clear();

                foreach (var asLine in lasDetail)
                    Add(new LVitem_VolumeDetailVM(asLine), bQuiet: true);

                Title = strTitle;
                RaiseItems();
            });
        }

        GlobalData_Base _gd = null;
    }
}