
using System.Collections.Generic;
namespace DoubleFile
{
    partial class LV_FileHashVM
    {
        internal LV_FileHashVM(GlobalData_Base gd)
        {
            _gd = gd;

            TreeViewItem_FileHashVM.SelectedItemChanged += (lsFiles, strListingFile) =>
            {
                UtilProject.UIthread(Items.Clear);

                if (null == lsFiles)
                    return;

                foreach (var strFile in lsFiles)
                {
                    var lsDuplicates = gd.FileDictionary.GetDuplicates(strFile, strListingFile);
                    var strCount = (null != lsDuplicates) ? "" + (lsDuplicates.Count - 1) : "";
                    var asFile = strFile.Split('\t');

                    UtilProject.UIthread(() => Add(new LVitem_FileHashVM(new[] { asFile[3], strCount })));
                }
            };
        }

        GlobalData_Base _gd = null;
    }
}
