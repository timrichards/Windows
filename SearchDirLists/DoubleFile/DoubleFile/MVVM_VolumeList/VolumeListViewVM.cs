
namespace DoubleFile
{
    class VolumeListViewVM : ListViewVM_Generic<VolumeLVitemVM>
    {
        public string WidthVolumeName { get { return SCW; } }                   // franken all NaN
        public string WidthPath { get { return SCW; } }
        public string WidthSaveAs { get { return SCW; } }
        public string WidthStatus { get { return SCW; } }
        public string WidthIncludeStr { get { return SCW; } }
        public string WidthVolumeGroup { get { return SCW; } }

        internal override void NewItem(string[] arrStr) { Add(new VolumeLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return VolumeLVitemVM.NumCols_; } }

        internal bool ContainsVolumeName(string t) { string s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.VolumeName.ToLower() == s) return true; return false; }
        internal bool ContainsUnsavedPath(string t) { string s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if ((item.Path.ToLower() == s) && (item.SaveAsExists == false)) return true; return false; }
        internal bool ContainsSaveAs(string t) { string s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.SaveAs.ToLower() == s) return true; return false; }

        internal void SetPartner(VolumeListVM windowVM)
        {
            m_windowVM = windowVM;
        }

        VolumeListVM m_windowVM = null;

        // these need to go in impl file but this whole file needs reorg
        internal void SaveVolume() { System.Windows.MessageBox.Show("SaveVolume"); }

        internal void EditVolume()
        {
            var dlg = new VolumeEdit();

            this.Selected().FirstOnlyAssert(t => { dlg.StringValues = t.StringValues; });

            if (dlg.ShowDialog() ?? false)
            {
                this.Selected().FirstOnlyAssert(t => { t.StringValues = dlg.StringValues; });
            }
        }

        internal void ToggleInclude() { System.Windows.MessageBox.Show("ToggleInclude"); }
    }
}
