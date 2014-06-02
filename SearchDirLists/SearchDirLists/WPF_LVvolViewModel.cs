using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Threading;

namespace SearchDirLists
{
    class LVvolInterface
    {
        readonly Form1 m_form1 = null;
        internal LVvolInterface() { m_form1 = new Form1(); }
        internal void SetPath() { m_form1.form_btnPath_Click(); }
        internal void SetListingFile() { m_form1.form_btnSaveAs_Click(); }
        internal void LoadVolumeList() { m_form1.form_btnLoadVolumeList_Click(); }
        internal void SaveVolumeList() { m_form1.form_btnSaveVolumeList_Click(); }
        internal void AddVolume() { m_form1.form_btnAddVolume_Click(); }
        internal void RemoveVolume() { m_form1.form_btnRemoveVolume_Click(); }
        internal void ToggleInclude() { m_form1.form_btnToggleInclude_Click(); }
        internal void SetVolumeGroup() { m_form1.form_btnVolGroup_Click(); }
        internal void ModifyFile() { m_form1.form_btnModifyFile_Click(); }
    }

    class LVvolViewModel : ObservableObject
    {
        readonly LVvolInterface m_lvVolInterface = new LVvolInterface();
        //     LVvolStrings m_datumCurrent;
        readonly List<LVvolStrings> m_listData = new List<LVvolStrings>();

        int m_nIndex = -1;
  //      int m_nMaxIndex = -1;

        String m_strVolumeName = null;
        String m_strPath = null;
        String m_strSaveAs = null;
        String m_strStatus = null;
        String m_strInclude = null;
        String m_strVolumeGroup = null;

        public int Index { get { return m_nIndex; } set { if (value != m_nIndex) { m_nIndex = value; OnPropertyChanged("Index"); } } }
        public String VolumeName { get { return m_strVolumeName; } set { if (value != m_strVolumeName) { m_strVolumeName = value; OnPropertyChanged("VolumeName"); } } }
        public String StrPath { get { return m_strPath; } set { if (value != m_strPath) { m_strPath = value; OnPropertyChanged("StrPath"); } } }
        public String SaveAs { get { return m_strSaveAs; } set { if (value != m_strSaveAs) { m_strSaveAs = value; OnPropertyChanged("SaveAs"); } } }
        public String Status { get { return m_strStatus; } set { if (value != m_strStatus) { m_strStatus = value; OnPropertyChanged("Status"); } } }
        public String Include { get { return m_strInclude; } set { if (value != m_strInclude) { m_strInclude = value; OnPropertyChanged("Include"); } } }
        public String VolumeGroup { get { return m_strVolumeGroup; } set { if (value != m_strVolumeGroup) { m_strVolumeGroup = value; OnPropertyChanged("VolumeGroup"); } } }

        bool m_bCanModifyOne = false;
        bool m_bCanModifyMultiple = false;

        bool CanAdd() { return true; }
        bool CanModifyOne() { return CanAdd() && m_bCanModifyOne; }
        bool CanModifyMultiple() { return CanModifyOne() && m_bCanModifyMultiple; }

        public ICommand Icmd_SetPath { get { if (mIcmd_setPath == null) { mIcmd_setPath = new RelayCommand(param => m_lvVolInterface.SetPath(), param => CanAdd()); } return mIcmd_setPath; } } ICommand mIcmd_setPath = null;
        public ICommand Icmd_SetListingFile { get { if (mIcmd_setListingFile == null) { mIcmd_setListingFile = new RelayCommand(param => m_lvVolInterface.SetListingFile(), param => CanAdd()); } return mIcmd_setListingFile; } } ICommand mIcmd_setListingFile = null;
        public ICommand Icmd_LoadVolumeList { get { if (mIcmd_loadVolumeList == null) { mIcmd_loadVolumeList = new RelayCommand(param => m_lvVolInterface.LoadVolumeList(), param => CanAdd()); } return mIcmd_loadVolumeList; } } ICommand mIcmd_loadVolumeList = null;
        public ICommand Icmd_SaveVolumeList { get { if (mIcmd_saveVolumeList == null) { mIcmd_saveVolumeList = new RelayCommand(param => m_lvVolInterface.SaveVolumeList(), param => CanAdd()); } return mIcmd_saveVolumeList; } } ICommand mIcmd_saveVolumeList = null;
        public ICommand Icmd_AddVolume { get { if (mIcmd_addVolume == null) { mIcmd_addVolume = new RelayCommand(param => m_lvVolInterface.AddVolume(), param => CanAdd()); } return mIcmd_addVolume; } } ICommand mIcmd_addVolume = null;
        public ICommand Icmd_RemoveVolume { get { if (mIcmd_removeVolume == null) { mIcmd_removeVolume = new RelayCommand(param => m_lvVolInterface.RemoveVolume(), param => CanModifyMultiple()); } return mIcmd_removeVolume; } } ICommand mIcmd_removeVolume = null;
        public ICommand Icmd_ToggleInclude { get { if (mIcmd_toggleInclude == null) { mIcmd_toggleInclude = new RelayCommand(param => m_lvVolInterface.ToggleInclude(), param => CanModifyMultiple()); } return mIcmd_toggleInclude; } } ICommand mIcmd_toggleInclude = null;
        public ICommand Icmd_VolumeGroup { get { if (mIcmd_volumeGroup == null) { mIcmd_volumeGroup = new RelayCommand(param => m_lvVolInterface.SetVolumeGroup(), param => CanModifyMultiple()); } return mIcmd_volumeGroup; } } ICommand mIcmd_volumeGroup = null;
        public ICommand Icmd_ModifyFile { get { if (mIcmd_modifyFile == null) { mIcmd_modifyFile = new RelayCommand(param => m_lvVolInterface.ModifyFile(), param => CanModifyOne()); } return mIcmd_modifyFile; } } ICommand mIcmd_modifyFile = null;
    }
}
