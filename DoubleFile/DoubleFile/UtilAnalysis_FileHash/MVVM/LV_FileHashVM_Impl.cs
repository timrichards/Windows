﻿
namespace DoubleFile
{
    partial class LV_FileHashVM
    {
        internal LV_FileHashVM(GlobalData_Base gd = null, LV_FileHashVM lvFileHashVM = null)
        {
            _gd = gd;

            TreeViewItem_FileHashVM.SelectedItemChanged += (lasFiles) =>
            {
                UtilProject.UIthread(Items.Clear);

                foreach (var asFile in lasFiles)
                {
                    UtilProject.UIthread(() => Add(new LVitem_FileHashVM(new[] { (string)asFile[0] })));
                }
            };

            if (null == lvFileHashVM)
            {
                return;
            }

            if (null == _gd)
            {
                _gd = lvFileHashVM._gd;
            }

            foreach (var lvItemVM in lvFileHashVM.ItemsCast)
            {
                Add(new LVitem_FileHashVM(lvItemVM), bQuiet: true);
            }
        }

        GlobalData_Base _gd = null;
    }
}
