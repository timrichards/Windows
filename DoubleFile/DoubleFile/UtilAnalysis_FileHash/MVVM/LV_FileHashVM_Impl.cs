using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_FileHashVM : IDisposable
    {
        internal LV_FileHashVM(GlobalData_Base gd)
        {
            _gd = gd;
            TreeViewItem_FileHashVM.SelectedItemChanged += SelectedItemChanged;
        }

        public void Dispose()
        {
            TreeViewItem_FileHashVM.SelectedItemChanged -= SelectedItemChanged;
        }

        void SelectedItemChanged(IEnumerable<string> lsFiles, string strListingFile)
        {
            UtilProject.UIthread(() =>
            {
                Items.Clear();
                //WidthFilename = double.NaN;
                //WidthDuplicates = double.NaN;

                if (null == lsFiles)
                    return;

                foreach (var strFile in lsFiles)
                {
                    string strFilename = null;
                    var lsDuplicates = _gd.FileDictionary.GetDuplicates(strFile, out strFilename, strListingFile);
                    var nCount = (null != lsDuplicates) ? (lsDuplicates.Count - 1) : 0;
                    var strCount = (nCount > 0) ? "" + nCount : null;

                    Add(new LVitem_FileHashVM(new[] { strFilename, strCount }), bQuiet: true);
                }

                RaiseItems();
            });
        }

        GlobalData_Base _gd = null;
    }
}
