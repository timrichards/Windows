using System;
using System.Collections.Generic;
using System.Windows.Input;
using Forms = System.Windows.Forms;
using System.Windows.Controls;
using System.IO;
using System.Collections.ObjectModel;

namespace SearchDirLists
{
    class ItemsControlVM : ObservableObject
    {
        public List<String> m_list = new List<String>();
        String m_strCurrent = null;
        ItemCollection items = null;
        Action m_Action = null;

        public ItemsControlVM(ItemsControl itemsCtl, Action action)
        {
            itemsCtl.DataContext = this;
            items = itemsCtl.Items;
            m_Action = action;
        }

        public String S
        {
            set
            {
                if ((value != null) && (m_list.Contains(value) == false))
                {
                    m_list.Add(value);
                    items.Refresh();
                    RaisePropertyChanged("List");
                }

                if (value == m_strCurrent) return;

                m_strCurrent = value;
                m_Action();
                RaisePropertyChanged("S");
            }

            get { return m_strCurrent; }
        }
    
        public List<String> List { get { return m_list; } }

        public override string ToString()
        {
            return m_strCurrent;
        }
    }

    class VolumeLVitemVM : ObservableObject
    {
        internal VolumeLVitemVM(String strVol, String strPath, String strSaveAs, String strStatus, bool bSaveAsExists)
        {
            VolumeName = strVol;
            Path = strPath;
            SaveAs = strSaveAs;
            Status = strStatus;
            IncludeStr = "Yes";
            SaveAsExists = bSaveAsExists;
        }

        internal VolumeLVitemVM(String[] arrStr)
        {
            Utilities.Assert(1310.1001, arrStr.Length <= NumCols);

            if (arrStr.Length > 0) VolumeName = arrStr[0];
            if (arrStr.Length > 1) Path = arrStr[1];
            if (arrStr.Length > 2) SaveAs = arrStr[2];
            if (arrStr.Length > 3) Status = arrStr[3];
            if (arrStr.Length > 4) IncludeStr = arrStr[4];
            if (arrStr.Length > 5) VolumeGroup = arrStr[5];
            SaveAsExists = true;
        }

        internal String this[int i] { get { return new String[] { VolumeName, Path, SaveAs, Status, IncludeStr, VolumeGroup }[i]; } }

        internal const int NumCols = 6;
        String m_strVolumeName = null;
        String m_strPath = null;
        String m_strSaveAs = null;
        String m_strStatus = null;
        String m_strInclude = null;
        String m_strVolumeGroup = null;

        internal bool SaveAsExists = false;     // TODO: set back to false when fail Tree

        public String VolumeName { get { return m_strVolumeName; } set { if (value != m_strVolumeName) { m_strVolumeName = value; RaisePropertyChanged("VolumeName"); } } }
        public String Path { get { return m_strPath; } set { if (value != m_strPath) { m_strPath = value; RaisePropertyChanged("Path"); } } }
        public String SaveAs { get { return m_strSaveAs; } set { if (value != m_strSaveAs) { m_strSaveAs = value; RaisePropertyChanged("SaveAs"); } } }
        public String Status { get { return m_strStatus; } set { if (value != m_strStatus) { m_strStatus = value; RaisePropertyChanged("Status"); } } }
        public String IncludeStr { get { return m_strInclude; } set { if (value != m_strInclude) { m_strInclude = value; RaisePropertyChanged("IncludeStr"); } } }
        public String VolumeGroup { get { return m_strVolumeGroup; } set { if (value != m_strVolumeGroup) { m_strVolumeGroup = value; RaisePropertyChanged("VolumeGroup"); } } }

        internal SDL_TreeNode treeNode = null;
        internal bool Include { get { return (IncludeStr == "Yes"); } set { IncludeStr = (value ? "Yes" : "No"); } }
    }

    class VolumesListViewVM : ObservableObject
    {
        ObservableCollection<VolumeLVitemVM> m_items = new ObservableCollection<VolumeLVitemVM>();
        ItemsControl m_itemsCtl = null;

        internal VolumesListViewVM(ItemsControl itemsCtl)
        {
            (m_itemsCtl = itemsCtl).DataContext = this;
        }

        internal bool Add(VolumeLVitemVM item)
        {
            m_items.Add(item);
            m_itemsCtl.Items.Refresh();
            RaisePropertyChanged("Items");
            return true;
        }

        public ObservableCollection<VolumeLVitemVM> Items { get { return m_items; } }
        internal int Count { get { return m_items.Count; } }
        internal bool ContainsVolumeName(String strVolumeName) { foreach (VolumeLVitemVM item in m_items) if (item.VolumeName == strVolumeName) return true; return false; }
        internal bool ContainsUnsavedPath(String strPath) { foreach (VolumeLVitemVM item in m_items) if ((item.Path == strPath) && (item.SaveAsExists == false)) return true; return false; }
        internal bool ContainsSaveAs(String strSaveAs) { foreach (VolumeLVitemVM item in m_items) if (item.SaveAs == strSaveAs) return true; return false; }
    }

    partial class VolumeTabVM : ObservableObject
    {
        readonly GlobalData gd = null;
        readonly MainWindow m_app = null;
        static readonly Forms.FolderBrowserDialog folderBrowserDialog1 = new Forms.FolderBrowserDialog();

        readonly ItemsControlVM CBVolumeName = null;
        readonly ItemsControlVM CBPath = null;
        readonly ItemsControlVM CBSaveAs = null;
        readonly VolumesListViewVM mo_lvVolViewModelList = null;

        public VolumeTabVM(MainWindow app)
        {
            gd = new GlobalData();
            m_app = app;
            gd.m_blinky = new Blinky(m_app.xaml_cbFindbox);
            CBVolumeName = new ItemsControlVM(m_app.xaml_cbVolumeName, new Action(() => { gd.m_strVolumeName = CBVolumeName.S; }));
            CBPath = new ItemsControlVM(m_app.xaml_cbPath, new Action(() => { gd.m_strPath = CBPath.S; }));
            CBSaveAs = new ItemsControlVM(m_app.xaml_cbSaveAs, new Action(() => { gd.m_strSaveAs = CBSaveAs.S; }));
            mo_lvVolViewModelList = new VolumesListViewVM(m_app.xaml_lvVolumesMain);
        }

        bool SelectedOne { get { return m_app.xaml_lvVolumesMain.SelectedItems.Count == 1; } }
        bool Selected { get { return m_app.xaml_lvVolumesMain.SelectedItems.Count > 0; } }
        bool HasItems { get { return mo_lvVolViewModelList.Count > 0; } }

        // In order of appearance on the form
        public ICommand Icmd_SetPath { get { if (mIcmd_setPath == null) { mIcmd_setPath = new RelayCommand(param => SetPath()); } return mIcmd_setPath; } } ICommand mIcmd_setPath = null;
        public ICommand Icmd_SaveAs { get { if (mIcmd_SaveAs == null) { mIcmd_SaveAs = new RelayCommand(param => SaveAs()); } return mIcmd_SaveAs; } } ICommand mIcmd_SaveAs = null;
        public ICommand Icmd_LoadVolumeList { get { if (mIcmd_loadVolumeList == null) { mIcmd_loadVolumeList = new RelayCommand(param => LoadVolumeList_Click()); } return mIcmd_loadVolumeList; } } ICommand mIcmd_loadVolumeList = null;
        public ICommand Icmd_SaveVolumeList { get { if (mIcmd_saveVolumeList == null) { mIcmd_saveVolumeList = new RelayCommand(param => SaveVolumeList(), param => HasItems); } return mIcmd_saveVolumeList; } } ICommand mIcmd_saveVolumeList = null;
        public ICommand Icmd_AddVolume { get { if (mIcmd_addVolume == null) { mIcmd_addVolume = new RelayCommand(param => AddVolume()); } return mIcmd_addVolume; } } ICommand mIcmd_addVolume = null;
        public ICommand Icmd_RemoveVolume { get { if (mIcmd_removeVolume == null) { mIcmd_removeVolume = new RelayCommand(param => RemoveVolume(), param => Selected); } return mIcmd_removeVolume; } } ICommand mIcmd_removeVolume = null;
        public ICommand Icmd_ToggleInclude { get { if (mIcmd_toggleInclude == null) { mIcmd_toggleInclude = new RelayCommand(param => ToggleInclude(), param => Selected); } return mIcmd_toggleInclude; } } ICommand mIcmd_toggleInclude = null;
        public ICommand Icmd_VolumeGroup { get { if (mIcmd_volumeGroup == null) { mIcmd_volumeGroup = new RelayCommand(param => SetVolumeGroup(), param => Selected); } return mIcmd_volumeGroup; } } ICommand mIcmd_volumeGroup = null;
        public ICommand Icmd_ModifyFile { get { if (mIcmd_modifyFile == null) { mIcmd_modifyFile = new RelayCommand(param => ModifyFile(), param => SelectedOne); } return mIcmd_modifyFile; } } ICommand mIcmd_modifyFile = null;
        public ICommand Icmd_SaveDirLists { get { if (mIcmd_saveDirLists == null) { mIcmd_saveDirLists = new RelayCommand(param => SaveDirLists(), param => HasItems); } return mIcmd_saveDirLists; } } ICommand mIcmd_saveDirLists = null;
    }
}
