using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class LV_FileDetailVM : IDisposable
    {
        internal LV_FileDetailVM(GlobalData_Base gd)
        {
            _gd = gd;
            Local.TreeSelect.FolderDetailUpdated += TreeSelect_FolderDetail;
        }

        public void Dispose()
        {
            Local.TreeSelect.FolderDetailUpdated -= TreeSelect_FolderDetail;
        }

        void TreeSelect_FolderDetail(IEnumerable<string[]> lasDetail)
        {
            UtilProject.UIthread(() =>
            {
                Items.Clear();

                foreach (var asLine in lasDetail)
                    Add(new LVitem_FileDetailVM(asLine), bQuiet: true);

                RaiseItems();
            });
        }

        GlobalData_Base _gd = null;
    }
}
