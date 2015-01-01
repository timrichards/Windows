using System.Collections.Generic;

namespace DoubleFile
{
    public partial class VolumeListVM
    {
        // Menu items

        internal void LoadProject() { System.Windows.MessageBox.Show("LoadProject"); }
        internal void SaveProject() { System.Windows.MessageBox.Show("SaveProject"); }

        internal void NewVolume()
        {
            m_lvVM.NewItem(new string[] { "Nickname", "Path", "Listing file", "Status", "Yes", "test" });
       //     System.Windows.MessageBox.Show("NewVolume");
        }
        internal void LoadVolume() { System.Windows.MessageBox.Show("LoadVolume"); }
    }
}
