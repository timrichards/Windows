using System.Collections.Generic;

namespace DoubleFile
{
    partial class WinVolumeListVM
    {
        // Menu items

        internal void LoadProject() { System.Windows.MessageBox.Show("LoadProject"); }
        internal void SaveProject() { System.Windows.MessageBox.Show("SaveProject"); }

        internal void NewVolume()
        {
            var newVolume = new WinVolumeNew();

            if (newVolume.ShowDialog(GetWindow()) ?? false)
            {
                m_lvVM.NewItem(newVolume.StringValues);
            }
        }

        internal void LoadVolume()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            if (dlg.ShowDialog(GetWindow()) ?? false)
            {
                string strVolumeName = null;
                string strPath = null;

                if (m_lvVM.ContainsSaveAs(dlg.FileName))
                {
                    System.Windows.MessageBox.Show("File already in list of volumes.");
                    return;
                }

                if (FileParse.ReadHeader(dlg.FileName, out strVolumeName, out strPath))
                {
                    m_lvVM.NewItem(new string[] {strVolumeName, strPath, dlg.FileName, "Using file.", "Yes"});
                }
                else
                {
                    System.Windows.MessageBox.Show("Bad file.");
                }
            }
        }

    }
}
