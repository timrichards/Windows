using System.Linq;

namespace DoubleFile
{
    class LV_VolumeVM : ListViewVM_Generic<LVitem_VolumeVM>
    {
        public string WidthVolumeName { get { return SCW; } }                   // franken all NaN
        public string WidthPath { get { return SCW; } }
        public string WidthSaveAs { get { return SCW; } }
        public string WidthStatus { get { return SCW; } }
        public string WidthIncludeStr { get { return SCW; } }
        public string WidthVolumeGroup { get { return SCW; } }
        public string WidthDriveModel { get { return SCW; } }
        public string WidthDriveSerial { get { return SCW; } }

        internal override void NewItem(string[] arrStr, bool bQuiet = false)
        {
            var lvItem = new LVitem_VolumeVM(this, arrStr);

            if (ContainsSaveAs(lvItem.SaveAs))
            {
                System.Windows.MessageBox.Show("File already in list of volumes.");
                return;
            }

            Add(lvItem, bQuiet);
        }

        internal override int NumCols { get { return LVitem_VolumeVM.NumCols_; } }

        internal bool ContainsVolumeName(string t) { string s = t.ToLower(); foreach (LVitem_VolumeVM item in m_items) if (item.VolumeName.ToLower() == s) return true; return false; }
        internal bool ContainsUnsavedPath(string t) { string s = t.ToLower(); foreach (LVitem_VolumeVM item in m_items) if ((item.Path.ToLower() == s) && (item.SaveAsExists == false)) return true; return false; }
        internal bool ContainsSaveAs(string t) { string s = t.ToLower(); foreach (LVitem_VolumeVM item in m_items) if (item.SaveAs.ToLower() == s) return true; return false; }

        internal void SetPartner(WinVolumeListVM windowVM)
        {
            m_windowVM = windowVM;
        }

        WinVolumeListVM m_windowVM = null;

        // these need to go in impl file but this whole file needs reorg
        internal void EditVolume()
        {
            var dlg = new WinVolumeEdit();

            Selected().FirstOnlyAssert(lvItem =>
            {
                dlg.StringValues = lvItem.StringValues;

                if (dlg.ShowDialog(GetWindow()) ?? false)
                {
                    lvItem.StringValues = dlg.StringValues;
                }
            });
        }

        internal void RemoveVolume()
        {
            Selected().ToArray().ForEach(lvItem => { Items.Remove(lvItem); });
        }

        internal void ToggleInclude()
        {
            Selected().ForEach(lvItem => { lvItem.Include = (lvItem.Include == false); });
        }

        internal void SetVolumeGroup() { System.Windows.MessageBox.Show("SetVolumeGroup"); }
    }
}
