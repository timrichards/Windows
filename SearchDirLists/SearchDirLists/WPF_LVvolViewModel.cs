using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls;

namespace SearchDirLists
{
    class LVvolViewModel : ObservableObject
    {
   //     LVvolStrings m_datumCurrent;
        List<LVvolStrings> m_listData = new List<LVvolStrings>();

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

        bool CanAdd()
        {
            return true;
        }

        bool m_bCanModifyOne = false;
        bool m_bCanModifyMultiple = false;

        bool CanModifyOne()
        {
            return CanAdd() && m_bCanModifyOne;
          //  return (m_datumCurrent != null)
        }

        bool CanModifyMultiple()
        {
            return CanModifyOne() && m_bCanModifyMultiple;
        }

        public ICommand Icmd_AddVolume { get { if (mIcmd_addVolume == null) { mIcmd_addVolume = new RelayCommand(param => AddVolume(), param => CanAdd()); } return mIcmd_addVolume; } }
        ICommand mIcmd_addVolume = null;
        void AddVolume()
        {
            m_bCanModifyOne = true;
            WPF_Form.WPFMessageBox("AddVolume()");
        }

        public ICommand Icmd_RemoveVolume { get { if (mIcmd_removeVolume == null) { mIcmd_removeVolume = new RelayCommand(param => RemoveVolume(), param => CanModifyMultiple()); } return mIcmd_removeVolume; } }
        ICommand mIcmd_removeVolume = null;
        void RemoveVolume()
        {
            WPF_Form.WPFMessageBox("RemoveVolume()");
        }

        public ICommand Icmd_ToggleInclude { get { if (mIcmd_toggleInclude == null) { mIcmd_toggleInclude = new RelayCommand(param => ToggleInclude(), param => CanModifyMultiple()); } return mIcmd_toggleInclude; } }
        ICommand mIcmd_toggleInclude = null;
        void ToggleInclude()
        {
            WPF_Form.WPFMessageBox("ToggleInclude()");
        }

        public ICommand Icmd_VolumeGroup { get { if (mIcmd_volumeGroup == null) { mIcmd_volumeGroup = new RelayCommand(param => SetVolumeGroup(), param => CanModifyMultiple()); } return mIcmd_volumeGroup; } }
        ICommand mIcmd_volumeGroup = null;
        void SetVolumeGroup()
        {
            WPF_Form.WPFMessageBox("SetVolumeGroup()");
        }

        public ICommand Icmd_ModifyFile { get { if (mIcmd_modifyFile == null) { mIcmd_modifyFile = new RelayCommand(param => ModifyFile(), param => CanModifyOne()); } return mIcmd_modifyFile; } }
        ICommand mIcmd_modifyFile = null;
        void ModifyFile()
        {
            m_bCanModifyMultiple = true;
            WPF_Form.WPFMessageBox("ModifyFile()");
        }
    }
}
