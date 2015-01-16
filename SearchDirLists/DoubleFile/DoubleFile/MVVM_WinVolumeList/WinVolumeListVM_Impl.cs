using System.Collections.Generic;

namespace DoubleFile
{
    partial class WinVolumeListVM
    {
        // Menu items

        internal void LoadProject() { MBox.ShowDialog("LoadProject"); }
        internal void SaveProject()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();

            if (dlg.ShowDialog(GetWindow()) ?? false)
            {
                new ProjectFile().SaveProject(m_lvVM.ItemsCast, dlg.FileName);
            }
        }

        internal void NewListingFile()
        {
            var lvItemVolumeTemp = new LVitem_VolumeVM(new string[] { });

            while (true)
            {
                var newVolume = new WinVolumeNew();

                newVolume.LVitemVolumeTemp = new LVitem_VolumeVM(lvItemVolumeTemp);

                if ((newVolume.ShowDialog(GetWindow()) ?? false) == false)
                {
                    // user cancelled
                    break;
                }

                if ((false == m_lvVM.AlreadyInProject(null, newVolume.LVitemVolumeTemp.ListingFile)) &&
                    (false == m_lvVM.FileExists(newVolume.LVitemVolumeTemp.ListingFile)))
                {
                    m_lvVM.NewItem(newVolume.LVitemVolumeTemp);
                    break;
                }

                lvItemVolumeTemp = new LVitem_VolumeVM(newVolume.LVitemVolumeTemp);
            }
        }

        internal void LoadListingFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            if (dlg.ShowDialog(GetWindow()) ?? false)
            {
                LVitem_VolumeVM lvItem = null;

                if (FileParse.ReadHeader(dlg.FileName, out lvItem))
                {
                    m_lvVM.NewItem(lvItem);
                }
                else
                {
                    MBox.ShowDialog("Bad listing file.", "Open Listing File");
                }
            }
        }

    }
}
