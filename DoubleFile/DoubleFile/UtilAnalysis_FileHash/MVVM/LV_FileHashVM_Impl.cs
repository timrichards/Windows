using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_FileHashVM : IDisposable
    {
        internal LV_FileHashVM(GlobalData_Base gd)
        {
            _gd = gd;

            _selectedItemChanged = (lsFiles, strListingFile) =>
            {
                UtilProject.UIthread(Items.Clear);

                if (null == lsFiles)
                    return;

                foreach (var strFile in lsFiles)
                {
                    var lsDuplicates = gd.FileDictionary.GetDuplicates(strFile, strListingFile);
                    var nCount = (null != lsDuplicates) ? (lsDuplicates.Count - 1) : 0;
                    var strCount = (nCount > 1) ? "" + nCount : "";
                    var asFile = strFile.Split('\t');

                    UtilProject.UIthread(() => Add(new LVitem_FileHashVM(new[] { asFile[3], strCount })));
                }
            };

            TreeViewItem_FileHashVM.SelectedItemChanged += _selectedItemChanged;
            
        }

        public void Dispose()
        {
            TreeViewItem_FileHashVM.SelectedItemChanged -= _selectedItemChanged;
        }

        GlobalData_Base _gd = null;
        Action<IEnumerable<string>, string> _selectedItemChanged = null;
    }
}
