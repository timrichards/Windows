using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class LV_FolderDetailVM : IDisposable
    {
        internal LV_FolderDetailVM(GlobalData_Base gd)
        {
            _gd = gd;
            Local.TreeSelect.FolderDetailUpdated += TreeSelect_FolderDetail;
        }

        public void Dispose()
        {
            Local.TreeSelect.FolderDetailUpdated -= TreeSelect_FolderDetail;
        }

        void TreeSelect_FolderDetail(IEnumerable<string[]> lasDetail, string strTitle)
        {
            UtilProject.UIthread(() =>
            {
                Title = null;
                Items.Clear();

                foreach (var asLine in lasDetail)
                    Add(new LVitem_FolderDetailVM(asLine), bQuiet: true);

                Add(new LVitem_FolderDetailVM(), bQuiet: true);
                Title = strTitle;
                RaiseItems();
            });
        }

        GlobalData_Base _gd = null;
    }
}
