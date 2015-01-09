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
            string[] stringValues = null;

            while (true)
            {
                var newVolume = new WinVolumeNew();

                newVolume.StringValues = stringValues;

                if ((newVolume.ShowDialog(GetWindow()) ?? false) == false)
                {
                    // user cancelled
                    break;
                }

                // user accepted
                if (m_lvVM.NewItem(newVolume.StringValues))
                {
                    break;
                }

                // volume file is in use
                stringValues = newVolume.StringValues;
            }
        }

        internal void LoadVolume()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            if (dlg.ShowDialog(GetWindow()) ?? false)
            {
                string strVolumeName = null;
                string strPath = null;

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
