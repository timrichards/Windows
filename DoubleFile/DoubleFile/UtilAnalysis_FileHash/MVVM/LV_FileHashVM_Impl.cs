using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_FileHashVM : IDisposable
    {
        internal LV_FileHashVM(GlobalData_Base gd)
        {
            _gd = gd;
            TreeViewItem_FileHashVM.TreeSelect_FileList += SelectedItemChanged;
        }

        public void Dispose()
        {
            TreeViewItem_FileHashVM.TreeSelect_FileList -= SelectedItemChanged;
        }

        void SelectedItemChanged(IEnumerable<string> lsFiles, string strListingFile)
        {
            UtilProject.UIthread(() =>
            {
                Items.Clear();

                if (null == lsFiles)
                    return;

                foreach (var strFile in lsFiles)
                {
                    string strFilename = null;
                    var lsDuplicates = _gd.FileDictionary.GetDuplicates(strFile, out strFilename, strListingFile);
                    var nCount = (null != lsDuplicates) ? lsDuplicates.Count() : 0;
                    var strCount = (nCount > 0) ? "" + nCount : null;
                    var lvItem = new LVitem_FileHashVM(new[] { strFilename, strCount });

                    if (nCount > 0)
                    {
                        lvItem.LSduplicates = lsDuplicates;
                    }
                    
                    Add(lvItem, bQuiet: true);
                }

                RaiseItems();
            });
        }

        GlobalData_Base _gd = null;
    }
}
