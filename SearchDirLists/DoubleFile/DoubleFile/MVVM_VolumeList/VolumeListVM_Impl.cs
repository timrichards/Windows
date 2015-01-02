using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    public partial class VolumeListVM
    {
        // Menu items

        internal void LoadProject() { System.Windows.MessageBox.Show("LoadProject"); }
        internal void SaveProject() { System.Windows.MessageBox.Show("SaveProject"); }

        internal void NewVolume()
        {
            var newVolume = new VolumeNew();

            if (newVolume.ShowDialog() ?? false)
            {
                m_lvVM.NewItem(newVolume.StringValues);
            }
        }

        internal void LoadVolume() { System.Windows.MessageBox.Show("LoadVolume"); }

        internal void RemoveVolume()
        {
            m_lvVM.Selected().ToArray().ForEach(lvItem => { m_lvVM.Items.Remove(lvItem); });
        }
    }
}
