
namespace DoubleFile
{
    partial class VolumeListVM
    {
        internal void LoadProject() { System.Windows.MessageBox.Show("LoadProject"); }
        internal void SaveProject() { System.Windows.MessageBox.Show("SaveProject"); }

        internal void NewVolume() { System.Windows.MessageBox.Show("NewVolume"); }
        internal void LoadVolume() { System.Windows.MessageBox.Show("LoadVolume"); }
        internal void SaveVolume() { System.Windows.MessageBox.Show("SaveVolume"); }

        internal void EditVolume() { new VolumeEdit().ShowDialog(); }
        internal void RemoveVolume() { System.Windows.MessageBox.Show("RemoveVolume"); }

        internal void ToggleInclude() { System.Windows.MessageBox.Show("ToggleInclude"); }
    }
}
