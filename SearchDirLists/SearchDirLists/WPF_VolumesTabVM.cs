using System;
using System.Windows.Input;
using System.Windows.Controls;

using Forms = System.Windows.Forms;

namespace SearchDirLists
{
    class VolumeLVitemVM : ListViewItemVM
    {
        public string VolumeName { get { return marr[0]; } set { SetProperty(0, value); } }
        public string Path { get { return marr[1]; } set { SetProperty(1, value); } }
        public string SaveAs { get { return marr[2]; } set { SetProperty(2, value); } }
        public string Status { get { return marr[3]; } set { SetProperty(3, value); } }
        public string IncludeStr { get { return marr[4]; } set { SetProperty(4, value); } }
        public string VolumeGroup { get { return marr[5]; } set { SetProperty(5, value); } }
        readonly static string[] marrPropName = new string[] { "VolumeName", "Path", "SaveAs", "Status", "IncludeStr", "VolumeGroup" };
        internal const int NumCols_ = 6;

        internal VolumeLVitemVM(VolumesListViewVM LV, string[] arrStr)
            : base(LV, arrStr)
        {
            SaveAsExists = (Status == Utilities.mSTRusingFile);                 // TODO: check dup drive letter, and if drive is mounted.
        }

        internal override int NumCols { get { return NumCols_; } }
        protected override string[] PropertyNames { get { return marrPropName; } }

        internal bool Include { get { return (IncludeStr == "Yes"); } set { IncludeStr = (value ? "Yes" : "No"); } }

        internal bool SaveAsExists = false;                                     // TODO: set back to false when fail Tree
        internal SDL_TreeNode treeNode = null;
    }

    class VolumesListViewVM : ListViewVM_Generic<VolumeLVitemVM>
    {
        public string WidthVolumeName { get { return SCW; } }                   // franken all NaN
        public string WidthPath { get { return SCW; } }
        public string WidthSaveAs { get { return SCW; } }
        public string WidthStatus { get { return SCW; } }
        public string WidthIncludeStr { get { return SCW; } }
        public string WidthVolumeGroup { get { return SCW; } }

        internal VolumesListViewVM(ListView lv) : base(lv) {}
        internal override void NewItem(string[] arrStr) { Add(new VolumeLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return VolumeLVitemVM.NumCols_; } }

        internal bool ContainsVolumeName(string t) { string s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.VolumeName.ToLower() == s) return true; return false; }
        internal bool ContainsUnsavedPath(string t) { string s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if ((item.Path.ToLower() == s) && (item.SaveAsExists == false)) return true; return false; }
        internal bool ContainsSaveAs(string t) { string s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.SaveAs.ToLower() == s) return true; return false; }
    }

    partial class VolumesTabVM : ObservableObject
    {
        // In order of appearance on the form
        public ICommand Icmd_SetPath { get { return marrIcmd[0]; } }
        public ICommand Icmd_SaveAs { get { return marrIcmd[1]; } }
        public ICommand Icmd_LoadVolumeList { get { return marrIcmd[2]; } }
        public ICommand Icmd_SaveVolumeList { get { return marrIcmd[3]; } }
        public ICommand Icmd_AddVolume { get { return marrIcmd[4]; } }
        public ICommand Icmd_RemoveVolume { get { return marrIcmd[5]; } }
        public ICommand Icmd_ToggleInclude { get { return marrIcmd[6]; } }
        public ICommand Icmd_VolumeGroup { get { return marrIcmd[7]; } }
        public ICommand Icmd_ModifyFile { get { return marrIcmd[8]; } }
        public ICommand Icmd_SaveDirLists { get { return marrIcmd[9]; } }

        internal VolumesTabVM(MainWindow app)
        {
            m_app = app;
            m_app.xaml_tabControlMain.DataContext = this;
            gd = GlobalData.GetInstance();
            CB_VolumeName = new ItemsControlVM(m_app.xaml_cbVolumeName, new Action(() => { gd.m_strVolumeName = CB_VolumeName.Current; }));
            CB_Path = new ItemsControlVM(m_app.xaml_cbPath, new Action(() => { gd.m_strPath = CB_Path.Current; }));
            CB_SaveAs = new ItemsControlVM(m_app.xaml_cbSaveAs, new Action(() => { gd.m_strSaveAs = CB_SaveAs.Current; }));
            LV = new VolumesListViewVM(m_app.xaml_lvVolumesMain);
            marrIcmd = new ICommand[]
            {
                new RelayCommand(param => SetPath()),
                new RelayCommand(param => SaveAs()),
                new RelayCommand(param => LoadVolumeList_Click()),
                new RelayCommand(param => SaveVolumeList(), param => LV.HasItems),
                new RelayCommand(param => AddVolume(), param => ((m_app.xaml_cbSaveAs.Text ?? "").Trim().Length > 0)),
                new RelayCommand(param => RemoveVolume(), param => LV.SelectedAny),
                new RelayCommand(param => ToggleInclude(), param => LV.SelectedAny),
                new RelayCommand(param => SetVolumeGroup(), param => LV.SelectedAny),
                new RelayCommand(param => ModifyFile(), param => LV.SelectedOne),
                new RelayCommand(param => SaveDirLists(), param => LV.HasItems)
            };
        }

        readonly ItemsControlVM CB_VolumeName = null;
        readonly ItemsControlVM CB_Path = null;
        readonly ItemsControlVM CB_SaveAs = null;
        readonly VolumesListViewVM LV = null;
        readonly ICommand[] marrIcmd = null;

        static readonly Forms.FolderBrowserDialog folderBrowserDialog1 = new Forms.FolderBrowserDialog();
        readonly MainWindow m_app = null;
        readonly GlobalData gd = null;
    }
}
