using System;
using System.Windows.Controls;

namespace DoubleFile
{
    class VolumeListViewVM : ListViewVM_Generic<VolumeLVitemVM>
    {
        public String WidthVolumeName { get { return SCW; } }                   // franken all NaN
        public String WidthPath { get { return SCW; } }
        public String WidthSaveAs { get { return SCW; } }
        public String WidthStatus { get { return SCW; } }
        public String WidthIncludeStr { get { return SCW; } }
        public String WidthVolumeGroup { get { return SCW; } }

        internal VolumeListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new VolumeLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return VolumeLVitemVM.NumCols_; } }

        internal bool ContainsVolumeName(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.VolumeName.ToLower() == s) return true; return false; }
        internal bool ContainsUnsavedPath(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if ((item.Path.ToLower() == s) && (item.SaveAsExists == false)) return true; return false; }
        internal bool ContainsSaveAs(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.SaveAs.ToLower() == s) return true; return false; }
    }
}
