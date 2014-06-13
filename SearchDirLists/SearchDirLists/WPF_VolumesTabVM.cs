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

        internal VolumesListViewVM(ItemsControl itemsCtl) : base(itemsCtl) {}

        internal bool ContainsVolumeName(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.VolumeName.ToLower() == s) return true; return false; }
        internal bool ContainsUnsavedPath(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if ((item.Path.ToLower() == s) && (item.SaveAsExists == false)) return true; return false; }
        internal bool ContainsSaveAs(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.SaveAs.ToLower() == s) return true; return false; }
    }

    partial class VolumesTabVM : ObservableObject
    {
        // In order of appearance on the form
        public ICommand Icmd_SetPath { get { return mIcommands[0]; } }
        public ICommand Icmd_SaveAs { get { return mIcommands[1]; } }
        public ICommand Icmd_LoadVolumeList { get { return mIcommands[2]; } }
        public ICommand Icmd_SaveVolumeList { get { return mIcommands[3]; } }
        public ICommand Icmd_AddVolume { get { return mIcommands[4]; } }
        public ICommand Icmd_RemoveVolume { get { return mIcommands[5]; } }
        public ICommand Icmd_ToggleInclude { get { return mIcommands[6]; } }
        public ICommand Icmd_VolumeGroup { get { return mIcommands[7]; } }
        public ICommand Icmd_ModifyFile { get { return mIcommands[8]; } }
        public ICommand Icmd_SaveDirLists { get { return mIcommands[9]; } }

        internal VolumesTabVM(MainWindow app)
        {
            gd = new GlobalData();
            m_app = app;
            gd.m_blinky = new Blinky(m_app.xaml_cbFindbox);
            CBVolumeName = new ItemsControlVM(m_app.xaml_cbVolumeName, new Action(() => { gd.m_strVolumeName = CBVolumeName.S; }));
            CBPath = new ItemsControlVM(m_app.xaml_cbPath, new Action(() => { gd.m_strPath = CBPath.S; }));
            CBSaveAs = new ItemsControlVM(m_app.xaml_cbSaveAs, new Action(() => { gd.m_strSaveAs = CBSaveAs.S; }));
            LV = new VolumesListViewVM(m_app.xaml_lvVolumesMain);
            mIcommands = new ICommand[]
            {
                new RelayCommand(param => SetPath()),
                new RelayCommand(param => SaveAs()),
                new RelayCommand(param => LoadVolumeList_Click()),
                new RelayCommand(param => SaveVolumeList()),
                new RelayCommand(param => AddVolume()),
                new RelayCommand(param => RemoveVolume(), param => Selected),
                new RelayCommand(param => ToggleInclude(), param => Selected),
                new RelayCommand(param => SetVolumeGroup(), param => Selected),
                new RelayCommand(param => ModifyFile(), param => SelectedOne),
                new RelayCommand(param => SaveDirLists(), param => HasItems)
            };
        }

        readonly ItemsControlVM CBVolumeName = null;
        readonly ItemsControlVM CBPath = null;
        readonly ItemsControlVM CBSaveAs = null;
        readonly VolumesListViewVM LV = null;
        readonly ICommand[] mIcommands = null;

        bool SelectedOne { get { return m_app.xaml_lvVolumesMain.SelectedItems.Count == 1; } }
        bool Selected { get { return m_app.xaml_lvVolumesMain.SelectedItems.Count > 0; } }
        bool HasItems { get { return LV.Count > 0; } }

        readonly GlobalData gd = null;
        readonly MainWindow m_app = null;
        static readonly Forms.FolderBrowserDialog folderBrowserDialog1 = new Forms.FolderBrowserDialog();
    }
}
