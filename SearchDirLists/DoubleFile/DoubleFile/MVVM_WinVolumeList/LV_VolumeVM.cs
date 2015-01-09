using System.Linq;

namespace DoubleFile
{
    class LV_VolumeVM : ListViewVM_GenericBase<LVitem_VolumeVM>
    {
        public string WidthVolumeName { get { return SCW; } }                   // franken all NaN
        public string WidthPath { get { return SCW; } }
        public string WidthSaveAs { get { return SCW; } }
        public string WidthStatus { get { return SCW; } }
        public string WidthIncludeStr { get { return SCW; } }
        public string WidthVolumeGroup { get { return SCW; } }
        public string WidthDriveModel { get { return SCW; } }
        public string WidthDriveSerial { get { return SCW; } }

        internal override bool NewItem(string[] arrStr, bool bQuiet = false)
        {
            var lvItem = new LVitem_VolumeVM(this, arrStr);
            var bAlreadyInUse = AlreadyInUse(lvItem);

            if (bAlreadyInUse == false)
            {
                Add(lvItem, bQuiet);
            }

            return (bAlreadyInUse == false);
        }

        internal override int NumCols { get { return LVitem_VolumeVM.NumCols_; } }

        internal LVitem_VolumeVM ContainsSaveAs(LVitem_VolumeVM currentItem, string t = null)
        {
            string s = (t ?? currentItem.SaveAs).ToLower();

            foreach (LVitem_VolumeVM item in m_items)
            {
                if ((item.SaveAs.ToLower() == s)
                    && ((t == null) || (currentItem != item)))
                {
                    return item;
                }
            }

            return null;
        }

        internal void SetPartner(WinVolumeListVM windowVM)
        {
            m_windowVM = windowVM;
        }

        WinVolumeListVM m_windowVM = null;

        // these need to go in impl file but this whole file needs reorg
        internal void EditVolume()
        {
            Selected().FirstOnlyAssert(lvItem =>
            {
                while (true)
                {
                    IWinVolumeEdit dlg = SaveDirListings.WontSave(lvItem) ?
                        (IWinVolumeEdit)new WinVolumeEdit() :
                        new WinVolumeNew();

                    dlg.StringValues = lvItem.StringValues;

                    if ((dlg.ShowDialog(GetWindow()) ?? false) == false)
                    {
                        // user cancelled
                        break;
                    }

                    if (AlreadyInUse(lvItem, dlg.StringValues[2]) == false)
                    {
                        lvItem.StringValues = dlg.StringValues;
                        break;
                    }
                }
            });
        }

        internal void RemoveVolume()
        {
            bool bUnsaved = false;

            Selected().ToArray().ForEach(lvItem =>
            {
                if (bUnsaved)
                {
                    return;     // from lambda
                }

                if (SaveDirListings.WontSave(lvItem) == false)
                {
                    bUnsaved = true;
                }
            });

            if (bUnsaved && (MBox.ShowDialog("Selected listings have not been saved. Continue?", "Remove volume",
                System.Windows.MessageBoxButton.YesNo) !=
                System.Windows.MessageBoxResult.Yes))
            {
                return;
            }

            Selected().ToArray().ForEach(lvItem => { Items.Remove(lvItem); });
        }

        internal void ToggleInclude()
        {
            Selected().ForEach(lvItem => { lvItem.Include = (lvItem.Include == false); });
        }

        internal void SetVolumeGroup() { System.Windows.MessageBox.Show("SetVolumeGroup"); }

        bool AlreadyInUse(LVitem_VolumeVM lvCurrentItem, string strFilename = null)
        {
            bool bAlreadyInUse = (ContainsSaveAs(lvCurrentItem, strFilename) != null);

            if (bAlreadyInUse)
            {
                System.Windows.MessageBox.Show("File already in list of volumes.");
            }

            return bAlreadyInUse;
        }
    }
}
