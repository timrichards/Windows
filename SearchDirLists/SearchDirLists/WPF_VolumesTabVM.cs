using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using Forms = System.Windows.Forms;

namespace SearchDirLists
{
    class VolumeLVitemVM : ListViewItemVM
    {
        public String VolumeName { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Path { get { return marr[1]; } set { SetProperty(1, value); } }
        public String SaveAs { get { return marr[2]; } set { SetProperty(2, value); } }
        public String Status { get { return marr[3]; } set { SetProperty(3, value); } }
        public String IncludeStr { get { return marr[4]; } set { SetProperty(4, value); } }
        public String VolumeGroup { get { return marr[5]; } set { SetProperty(5, value); } }

        internal new const int NumCols = 6;
        readonly new static String[] arrPropName = new String[] { "VolumeName", "Path", "SaveAs", "Status", "IncludeStr", "VolumeGroup" };

        VolumeLVitemVM(VolumesListViewVM LV)
            : base(LV, NumCols, arrPropName) {}

        internal VolumeLVitemVM(VolumesListViewVM LV, String strVol, String strPath, String strSaveAs, String strStatus, bool bSaveAsExists)
            : this(LV)
        {
            VolumeName = strVol;
            Path = strPath;
            SaveAs = strSaveAs;
            Status = strStatus;
            IncludeStr = "Yes";
            SaveAsExists = bSaveAsExists;
        }

        internal VolumeLVitemVM(VolumesListViewVM LV, String[] arrStr)
            : this(LV)
        {
            CopyInArray(arrStr);
            SaveAsExists = (Status == Utilities.mSTRusingFile);                 // TODO: check dup drive letter, and if letter is mounted.
        }

        internal bool Include { get { return (IncludeStr == "Yes"); } set { IncludeStr = (value ? "Yes" : "No"); } }

        internal bool SaveAsExists = false;                                     // TODO: set back to false when fail Tree
        internal SDL_TreeNode treeNode = null;
    }

    class VolumesListViewVM : ListViewVM
    {
        public String WidthVolumeName { get { return SCW; } }                   // franken all NaN
        public String WidthPath { get { return SCW; } }
        public String WidthSaveAs { get { return SCW; } }
        public String WidthStatus { get { return SCW; } }
        public String WidthIncludeStr { get { return SCW; } }
        public String WidthVolumeGroup { get { return SCW; } }

        internal VolumesListViewVM(ListView lv) : base(lv) {}

        internal bool ContainsVolumeName(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.VolumeName.ToLower() == s) return true; return false; }
        internal bool ContainsUnsavedPath(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if ((item.Path.ToLower() == s) && (item.SaveAsExists == false)) return true; return false; }
        internal bool ContainsSaveAs(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.SaveAs.ToLower() == s) return true; return false; }
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
            CB_VolumeName = new ItemsControlVM(m_app.xaml_cbVolumeName, new Action(() => { gd.m_strVolumeName = CB_VolumeName.S; }));
            CB_Path = new ItemsControlVM(m_app.xaml_cbPath, new Action(() => { gd.m_strPath = CB_Path.S; }));
            CB_SaveAs = new ItemsControlVM(m_app.xaml_cbSaveAs, new Action(() => { gd.m_strSaveAs = CB_SaveAs.S; }));
            LV = new VolumesListViewVM(m_app.xaml_lvVolumesMain);
            marrIcmd = new ICommand[]
            {
                new RelayCommand(param => SetPath()),
                new RelayCommand(param => SaveAs()),
                new RelayCommand(param => LoadVolumeList_Click()),
                new RelayCommand(param => SaveVolumeList(), param => LV.HasItems),
                new RelayCommand(param => AddVolume(), param => (Utilities.NotNull(m_app.xaml_cbSaveAs.Text).Trim().Length > 0)),
                new RelayCommand(param => RemoveVolume(), param => LV.Selected),
                new RelayCommand(param => ToggleInclude(), param => LV.Selected),
                new RelayCommand(param => SetVolumeGroup(), param => LV.Selected),
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
