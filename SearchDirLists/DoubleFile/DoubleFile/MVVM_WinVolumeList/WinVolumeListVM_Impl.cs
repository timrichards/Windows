using System.Collections.Generic;

namespace DoubleFile
{
    partial class WinVolumeListVM
    {
        // Menu items

        internal void OpenProject()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "Open Project";

            if (dlg.ShowDialog(GetWindow()) ?? false)
            {
                new ProjectFile().OpenProject(dlg.FileName, OpenListingFiles);
            }
        }

        internal void SaveProject()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.Title = "Save Project";

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

        internal void OpenListingFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "Open Listing File";
            dlg.Multiselect = true;

            if (dlg.ShowDialog(GetWindow()) ?? false)
            {
                OpenListingFiles(dlg.FileNames);
            }
        }

        internal void OpenListingFiles(IEnumerable<string> listFiles, bool bClearItems = false)
        {
            LVitem_VolumeVM lvItem = null;
            var sbBadFiles = new System.Text.StringBuilder();
            bool bMultiBad = true;

            if (bClearItems)
            {
                m_lvVM.Items.Clear();
            }

            foreach (var fileName in listFiles)
            {
                if (FileParse.ReadHeader(fileName, out lvItem))
                {
                    m_lvVM.NewItem(lvItem);
                }
                else
                {
                    bMultiBad = (sbBadFiles.Length > 0);
                    sbBadFiles.Append("• ").Append(System.IO.Path.GetFileName(fileName)).Append("\n");
                }
            }

            if (sbBadFiles.Length > 0)
            {
                MBox.ShowDialog("Bad listing file" + (bMultiBad ? "s" : "") + ".\n" + sbBadFiles, "Open Listing File");
            }
        }
    }
}
