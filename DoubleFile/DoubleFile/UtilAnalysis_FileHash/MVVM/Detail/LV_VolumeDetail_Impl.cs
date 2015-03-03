using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        void TreeSelect_VolumeDetail(IEnumerable<string[]> lasDetail)
        {
            UtilProject.UIthread(() =>
            {
                Items.Clear();

                foreach (var asLine in lasDetail)
                    Add(new LVitem_VolumeDetailVM(asLine), bQuiet: true);

                RaiseItems();
            });
        }

        GlobalData_Base _gd = null;
    }
}
