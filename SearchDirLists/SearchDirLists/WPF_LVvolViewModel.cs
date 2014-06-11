using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using System.Windows.Controls;
using System.ComponentModel;
using System.IO;
using Drawing = System.Drawing;

namespace SearchDirLists
{
    class SDL_ItemsControl : ObservableObject
    {
        public List<String> m_list = new List<String>();
        String m_strCurrent = null;
        ItemCollection items = null;
        Action m_Action = null;

        public SDL_ItemsControl(ItemsControl itemsCtl, Action action)
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

    class LVvolViewModel
    {
  //      LVvolStrings datum = new LVvolStrings();

        RaisePropertyChangedDelegate RaisePropertyChanged = null;

        internal LVvolViewModel(RaisePropertyChangedDelegate propchgDelegate)
        {
            RaisePropertyChanged = propchgDelegate;
        }

        String m_strVolumeName = null;
        String m_strPath = null;
        String m_strSaveAs = null;
        String m_strStatus = null;
        String m_strInclude = null;
        String m_strVolumeGroup = null;

        public String VolumeName { get { return m_strVolumeName; } set { if (value != m_strVolumeName) { m_strVolumeName = value; RaisePropertyChanged("LVItem_VolumeName"); } } }
        public String Path { get { return m_strPath; } set { if (value != m_strPath) { m_strPath = value; RaisePropertyChanged("LVItem_Path"); } } }
        public String SaveAs { get { return m_strSaveAs; } set { if (value != m_strSaveAs) { m_strSaveAs = value; RaisePropertyChanged("LVItem_SaveAs"); } } }
        public String Status { get { return m_strStatus; } set { if (value != m_strStatus) { m_strStatus = value; RaisePropertyChanged("LVItem_Status"); } } }
        public String Include { get { return m_strInclude; } set { if (value != m_strInclude) { m_strInclude = value; RaisePropertyChanged("LVItem_Include"); } } }
        public String VolumeGroup { get { return m_strVolumeGroup; } set { if (value != m_strVolumeGroup) { m_strVolumeGroup = value; RaisePropertyChanged("LVItem_VolumeGroup"); } } }
    }

    class LVvolViewModel_List
    {
        List<LVvolViewModel> list = new List<LVvolViewModel>();

        internal bool ContainsVolumeName(String strVolumeName)
        {
            return false;
        }

        internal bool ContainsPath(String strPath)
        {
            return false;
        }

        internal bool ContainsSaveAs(String strSaveAs)
        {
            return false;
        }
    }

    partial class VolumeTabViewModel : ObservableObject
    {
        readonly GlobalData gd = null;
        readonly MainWindow m_app = null;
        static readonly Forms.FolderBrowserDialog folderBrowserDialog1 = new Forms.FolderBrowserDialog();

        readonly SDL_ItemsControl CBVolumeName = null;
        readonly SDL_ItemsControl CBPath = null;
        readonly SDL_ItemsControl CBSaveAs = null;

        public VolumeTabViewModel(MainWindow app)
        {
            gd = new GlobalData();
            m_app = app;
            gd.m_blinky = new Blinky(m_app.xaml_cbFindbox);
            CBVolumeName = new SDL_ItemsControl(m_app.xaml_cbVolumeName, new Action(() => { gd.m_strVolumeName = CBVolumeName.S; }));
            CBPath = new SDL_ItemsControl(m_app.xaml_cbPath, new Action(() => { gd.m_strPath = CBPath.S; }));
            CBSaveAs = new SDL_ItemsControl(m_app.xaml_cbSaveAs, new Action(() => { gd.m_strSaveAs = CBSaveAs.S; }));
        }

        readonly LVvolViewModel_List mo_lvVolViewModelList = new LVvolViewModel_List();

        bool m_bCanModifyOne = false;
        bool m_bCanModifyMultiple = false;

        bool CanAdd() { return true; }
        bool CanModifyOne() { return CanAdd() && m_bCanModifyOne; }
        bool CanModifyMultiple() { return CanModifyOne() && m_bCanModifyMultiple; }

        public ICommand Icmd_SetPath { get { if (mIcmd_setPath == null) { mIcmd_setPath = new RelayCommand(param => WPF_btnSetPath_Click(), param => CanAdd()); } return mIcmd_setPath; } } ICommand mIcmd_setPath = null;
        public ICommand Icmd_SaveAs { get { if (mIcmd_SaveAs == null) { mIcmd_SaveAs = new RelayCommand(param => WPF_btnSaveAs_Click(), param => CanAdd()); } return mIcmd_SaveAs; } } ICommand mIcmd_SaveAs = null;
        public ICommand Icmd_LoadVolumeList { get { if (mIcmd_loadVolumeList == null) { mIcmd_loadVolumeList = new RelayCommand(param => WPF_btnLoadVolumeList_Click(), param => CanAdd()); } return mIcmd_loadVolumeList; } } ICommand mIcmd_loadVolumeList = null;
        public ICommand Icmd_SaveVolumeList { get { if (mIcmd_saveVolumeList == null) { mIcmd_saveVolumeList = new RelayCommand(param => WPF_btnSaveVolumeList_Click(), param => CanAdd()); } return mIcmd_saveVolumeList; } } ICommand mIcmd_saveVolumeList = null;
        public ICommand Icmd_AddVolume { get { if (mIcmd_addVolume == null) { mIcmd_addVolume = new RelayCommand(param => WPF_btnAddVolume_Click(), param => CanAdd()); } return mIcmd_addVolume; } } ICommand mIcmd_addVolume = null;
        public ICommand Icmd_RemoveVolume { get { if (mIcmd_removeVolume == null) { mIcmd_removeVolume = new RelayCommand(param => WPF_btnRemoveVolume_Click(), param => CanModifyMultiple()); } return mIcmd_removeVolume; } } ICommand mIcmd_removeVolume = null;
        public ICommand Icmd_ToggleInclude { get { if (mIcmd_toggleInclude == null) { mIcmd_toggleInclude = new RelayCommand(param => WPF_btnToggleInclude_Click(), param => CanModifyMultiple()); } return mIcmd_toggleInclude; } } ICommand mIcmd_toggleInclude = null;
        public ICommand Icmd_VolumeGroup { get { if (mIcmd_volumeGroup == null) { mIcmd_volumeGroup = new RelayCommand(param => WPF_btnSetVolumeGroup_Click(), param => CanModifyMultiple()); } return mIcmd_volumeGroup; } } ICommand mIcmd_volumeGroup = null;
        public ICommand Icmd_ModifyFile { get { if (mIcmd_modifyFile == null) { mIcmd_modifyFile = new RelayCommand(param => WPF_btnModifyFile_Click(), param => CanModifyOne()); } return mIcmd_modifyFile; } } ICommand mIcmd_modifyFile = null;

        internal void WPF_btnSetPath_Click()
        {
            if (folderBrowserDialog1.ShowDialog() == Forms.DialogResult.OK)
            {
                CBPath.S = folderBrowserDialog1.SelectedPath;
            }
        }

        internal void WPF_btnSaveAs_Click()
        {
            SDL_File.Init();
            SDL_File.SFD.Filter = SDL_File.FileAndDirListFileFilter + "|" + SDL_File.BaseFilter;

            if (Utilities.StrValid(CBSaveAs.S))
            {
                SDL_File.SFD.InitialDirectory = Path.GetDirectoryName(CBSaveAs.S);
            }

            if (SDL_File.SFD.ShowDialog() == Forms.DialogResult.OK)
            {
                CBSaveAs.S = SDL_File.SFD.FileName;

                if (File.Exists(CBSaveAs.S))
                {
                    CBVolumeName.S = null;
                    CBPath.S = null;
                }
            }
        }

        internal void WPF_btnLoadVolumeList_Click() { }
        internal void WPF_btnSaveVolumeList_Click() { }

        internal void WPF_btnAddVolume_Click()
        {
            gd.InterruptTreeTimerWithAction(new BoolAction(AddVolume));
        }

        internal void WPF_btnRemoveVolume_Click() { }
        internal void WPF_btnToggleInclude_Click() { }
        internal void WPF_btnSetVolumeGroup_Click() { }
        internal void WPF_btnModifyFile_Click() { }
    }
}
