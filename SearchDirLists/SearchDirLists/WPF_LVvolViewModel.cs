using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using System.Windows.Controls;
using System.ComponentModel;
using System.IO;


namespace SearchDirLists
{
    class SDL_ItemsControl
    {
        public List<String> m_list = new List<String>();
        public String m_strPropName = "";

        public String Current = null;

        RaisePropertyChangedDelegate PropertyChanged = null;
        ItemCollection items = null;

        public SDL_ItemsControl(String strName, RaisePropertyChangedDelegate chgDelegate, ItemCollection items_in)
        {
            m_strPropName = strName;
            PropertyChanged = chgDelegate;
            items = items_in;
        }

        public void Set(String str)
        {
            if ((str != null) && (m_list.Contains(str) == false))
            {
                m_list.Add(str);
                items.Refresh();
                PropertyChanged("List" + m_strPropName);
            }

            if (str == Current) return;

            Current = str;
            PropertyChanged(m_strPropName);
        }

        public List<String> Get() { return m_list; }
    }

    partial class LVvolViewModel : ObservableObject
    {
        readonly GlobalData gd = null;
        MainWindow m_app = null;

        public LVvolViewModel(MainWindow app)
        {
            gd = new GlobalData();
            m_app = app;
            mSDL_CBVolumeName = new SDL_ItemsControl("CBVolumeName", RaisePropertyChanged, m_app.xaml_cbVolumeName.Items);
            mSDL_CBPath = new SDL_ItemsControl("CBPath", RaisePropertyChanged, m_app.xaml_cbPath.Items);
            mSDL_CBSaveAs = new SDL_ItemsControl("CBSaveAs", RaisePropertyChanged, m_app.xaml_cbSaveAs.Items);
            m_form1 = new Form1();
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "Text files|*.txt|All files|*.*";
            saveFileDialog1.OverwritePrompt = false;
            mSDL_LVVolumesMain = new SDL_ItemsControl("LVVolumesMain", RaisePropertyChanged, m_app.xaml_lvVolumesMain.Items);
        }

        //     LVvolStrings m_datumCurrent;
        readonly List<LVvolStrings> m_listData = new List<LVvolStrings>();

        int m_nIndex = -1;
  //      int m_nMaxIndex = -1;

        SDL_ItemsControl mSDL_CBVolumeName = null;
        SDL_ItemsControl mSDL_CBPath = null;
        SDL_ItemsControl mSDL_CBSaveAs = null;
        String m_strStatus = null;
        String m_strInclude = null;
        String m_strVolumeGroup = null;
        SDL_ItemsControl mSDL_LVVolumesMain = null;

        public int Index { get { return m_nIndex; } set { if (value != m_nIndex) { m_nIndex = value; RaisePropertyChanged("Index"); } } }
        public String CBVolumeName { get { return mSDL_CBVolumeName.Current; } set { mSDL_CBVolumeName.Set(gd.m_strVolumeName = value); } }
        public String CBPath { get { return mSDL_CBPath.Current; } set { mSDL_CBPath.Set(gd.m_strPath = value); } }
        public String CBSaveAs { get { return mSDL_CBSaveAs.Current; } set { mSDL_CBSaveAs.Set(gd.m_strSaveAs = value); } }
        public String StrStatus { get { return m_strStatus; } set { if (value != m_strStatus) { m_strStatus = value; RaisePropertyChanged("StrStatus"); } } }
        public String StrPath { get { return m_strInclude; } set { if (value != m_strInclude) { m_strInclude = value; RaisePropertyChanged("StrPath"); } } }
        public String StrVolumeGroup { get { return m_strVolumeGroup; } set { if (value != m_strVolumeGroup) { m_strVolumeGroup = value; RaisePropertyChanged("StrVolumeGroup"); } } }
        public String LVVolumesMain { get { return mSDL_LVVolumesMain.Current; } set { mSDL_LVVolumesMain.Set(value); } }

        public List<String> ListVolumeName { get { return mSDL_CBVolumeName.Get(); } }
        public List<String> ListPath { get { return mSDL_CBPath.Get(); } }
        public List<String> ListSaveAs { get { return mSDL_CBSaveAs.Get(); } }
        public List<String> ListVolumesMain { get { return mSDL_LVVolumesMain.Get(); } }

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

        readonly Form1 m_form1 = null;
        readonly Forms.FolderBrowserDialog folderBrowserDialog1 = new Forms.FolderBrowserDialog();
        readonly Forms.SaveFileDialog saveFileDialog1 = new Forms.SaveFileDialog();

        internal void WPF_btnSetPath_Click()
        {
            if (folderBrowserDialog1.ShowDialog() == Forms.DialogResult.OK)
            {
                CBPath = folderBrowserDialog1.SelectedPath;
            }
        }

        internal void WPF_btnSaveAs_Click()
        {
            saveFileDialog1.Filter = SDL_File.FileAndDirListFileFilter + "|" + SDL_File.BaseFilter;

            if (Utilities.StrValid(CBSaveAs))
            {
                saveFileDialog1.InitialDirectory = Path.GetDirectoryName(CBSaveAs);
            }

            if (saveFileDialog1.ShowDialog() == Forms.DialogResult.OK)
            {
                CBSaveAs = saveFileDialog1.FileName;

                if (File.Exists(CBSaveAs))
                {
                    CBVolumeName = null;
                    CBPath = null;
                }
            }
        }

        internal void WPF_btnLoadVolumeList_Click() { m_form1.form_btnLoadVolumeList_Click(); }
        internal void WPF_btnSaveVolumeList_Click() { m_form1.form_btnSaveVolumeList_Click(); }

        internal void WPF_btnAddVolume_Click()
        {
            gd.InterruptTreeTimerWithAction(new BoolAction(AddVolume));
        }

        internal void WPF_btnRemoveVolume_Click() { m_form1.form_btnRemoveVolume_Click(); }
        internal void WPF_btnToggleInclude_Click() { m_form1.form_btnToggleInclude_Click(); }
        internal void WPF_btnSetVolumeGroup_Click() { m_form1.form_btnVolGroup_Click(); }
        internal void WPF_btnModifyFile_Click() { m_form1.form_btnModifyFile_Click(); }

        bool AddVolume()
        {
            bool bFileOK = false;

            if (SaveFields(false) == false)
            {
                return false;
            }
#if (WPF)
            if (Utilities.StrValid(gd.m_strSaveAs) == false)
            {
                gd.FormError(m_app.xaml_cbSaveAs, "Must have a file to load or save directory listing to.", "Volume Save As");
                return false;
            }

            if (m_app.xaml_lvVolumesMain.FindItemWithText(gd.m_strSaveAs) != null)
            {
                gd.FormError(m_app.xaml_cbSaveAs, "File already in use in list of volumes.", "Volume Save As");
                return false;
            }

            bool bOpenedFile = (Utilities.StrValid(gd.m_strPath) == false);

            if (File.Exists(gd.m_strSaveAs) && Utilities.StrValid(gd.m_strPath))
            {
                gd.m_blinky.Go(m_app.xaml_cbSaveAs, clr: Color.Red);

                if (MessageBox(gd.m_strSaveAs + " already exists. Overwrite?", "Volume Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Color.Yellow, Once: true);
                    m_app.xaml_cbVolumeName.Text = String.Empty;
                    gd.m_blinky.Go(m_app.xaml_cbPath, clr: Color.Yellow, Once: true);
                    CBPath = String.Empty;
                    Utilities.Assert(1308.9306, SaveFields(false));
                }
            }

            if ((File.Exists(gd.m_strSaveAs) == false) && (Utilities.StrValid(gd.m_strPath) == false))
            {
                gd.m_blinky.Go(m_app.xaml_cbPath, clr: Color.Red);
                MessageBox("Must have a path or existing directory listing file.", "Volume Source Path");
                gd.m_blinky.Go(m_app.xaml_cbPath, clr: Color.Red, Once: true);
                return false;
            }

            if (Utilities.StrValid(gd.m_strPath) && (Directory.Exists(gd.m_strPath) == false))
            {
                gd.m_blinky.Go(m_app.xaml_cbPath, clr: Color.Red);
                MessageBox("Path does not exist.", "Volume Source Path");
                gd.m_blinky.Go(m_app.xaml_cbPath, clr: Color.Red, Once: true);
                return false;
            }

            String strStatus = "Not Saved";

            if (File.Exists(gd.m_strSaveAs))
            {
                if (Utilities.StrValid(gd.m_strPath) == false)
                {
                    bFileOK = ReadHeader();

                    if (bFileOK)
                    {
                        strStatus = Utilities.mSTRusingFile;
                    }
                    else
                    {
                        if (Utilities.StrValid(gd.m_strPath))
                        {
                            strStatus = "File is bad. Will overwrite.";
                        }
                        else
                        {
                            gd.m_blinky.Go(m_app.xaml_cbPath, clr: Color.Red);
                            MessageBox("File is bad and path does not exist.", "Volume Source Path");
                            gd.m_blinky.Go(m_app.xaml_cbPath, clr: Color.Red, Once: true);
                            return false;
                        }
                    }
                }
                else
                {
                    strStatus = "Will overwrite.";
                }
            }

            if ((bFileOK == false) && (m_app.xaml_lvVolumesMain.Items.ContainsKey(gd.m_strPath)))
            {
                gd.FormError(m_app.xaml_cbPath, "Path already added.", "Volume Source Path");
                return false;
            }

            if (Utilities.StrValid(gd.m_strVolumeName))
            {
                SDL_ListViewItem lvItem = (SDL_ListViewItem)m_app.xaml_lvVolumesMain.FindItemWithText(gd.m_strVolumeName);

                if ((lvItem != null) && (lvItem.Text() == gd.m_strVolumeName))
                {
                    gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Color.Red);

                    if (MessageBox("Nickname already in use. Use it for more than one volume?", "Volume Save As", MessageBoxButtons.YesNo)
                        != DialogResult.Yes)
                    {
                        gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Color.Red, Once: true);
                        return false;
                    }
                }
            }
            else if (bOpenedFile == false)
            {
                gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Color.Red);

                if (MessageBox("Continue without entering a nickname for this volume?", "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.Yes)
                {
                    gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Color.Red, Once: true);
                    return false;
                }
            }

            {
                SDL_ListViewItem lvItem = new SDL_ListViewItem(new String[] { gd.m_strVolumeName, gd.m_strPath, gd.m_strSaveAs, strStatus, "Yes" });

                if (bFileOK == false)
                {
                    lvItem.Name = gd.m_strPath;    // indexing by path, only for unsaved volumes
                }

                m_app.xaml_lvVolumesMain.Items.Add(lvItem);
            }

            form_btnSaveDirList.Enabled = true;
#endif
            return bFileOK;
        }
    }
}
