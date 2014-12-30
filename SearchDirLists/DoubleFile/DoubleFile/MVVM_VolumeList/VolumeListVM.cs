using System.Windows.Input;

using Forms = System.Windows.Forms;

namespace DoubleFile
{
    partial class VolumeListVM : ObservableObject
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

        internal VolumeListVM(MainWindow app)
        {
            m_app = app;
            //m_app.xaml_tabControlMain.DataContext = this;
            //gd = GlobalData.GetInstance();
            //CB_VolumeName = new ItemsControlVM(m_app.xaml_cbVolumeName, new Action(() => { gd.m_strVolumeName = CB_VolumeName.Current; }));
            //CB_Path = new ItemsControlVM(m_app.xaml_cbPath, new Action(() => { gd.m_strPath = CB_Path.Current; }));
            //CB_SaveAs = new ItemsControlVM(m_app.xaml_cbSaveAs, new Action(() => { gd.m_strSaveAs = CB_SaveAs.Current; }));
            //LV = new VolumesListViewVM(m_app.xaml_lvVolumesMain);
            marrIcmd = new ICommand[]
            {
                //new RelayCommand(param => SetPath()),
                //new RelayCommand(param => SaveAs()),
                //new RelayCommand(param => LoadVolumeList_Click()),
                //new RelayCommand(param => SaveVolumeList(), param => LV.HasItems),
                //new RelayCommand(param => AddVolume(), param => (Utilities.NotNull(m_app.xaml_cbSaveAs.Text).Trim().Length > 0)),
                //new RelayCommand(param => RemoveVolume(), param => LV.SelectedAny),
                //new RelayCommand(param => ToggleInclude(), param => LV.SelectedAny),
                //new RelayCommand(param => SetVolumeGroup(), param => LV.SelectedAny),
                //new RelayCommand(param => ModifyFile(), param => LV.SelectedOne),
                //new RelayCommand(param => SaveDirLists(), param => LV.HasItems)
            };
        }

        //readonly ItemsControlVM CB_VolumeName = null;
        //readonly ItemsControlVM CB_Path = null;
        //readonly ItemsControlVM CB_SaveAs = null;
        //readonly VolumesListViewVM LV = null;
        readonly ICommand[] marrIcmd = null;

        static readonly Forms.FolderBrowserDialog folderBrowserDialog1 = new Forms.FolderBrowserDialog();
        readonly MainWindow m_app = null;
  //      readonly GlobalData gd = null;
    }
}
