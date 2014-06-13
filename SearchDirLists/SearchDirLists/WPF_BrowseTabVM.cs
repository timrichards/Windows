using System.Windows.Input;
using System;

namespace SearchDirLists
{
    partial class BrowseTabVM
    {
        // In order of appearance on the form
        public ICommand Icmd_Collapse { get { return mIcommands[0]; } }
        public ICommand Icmd_CompareCheck { get { return mIcommands[1]; } }
        public ICommand Icmd_Compare { get { return mIcommands[2]; } }
        public ICommand Icmd_Up { get { return mIcommands[3]; } }
        public ICommand Icmd_Back { get { return mIcommands[4]; } }
        public ICommand Icmd_Forward { get { return mIcommands[5]; } }
        public ICommand Icmd_Copy { get { return mIcommands[6]; } }
        public ICommand Icmd_SearchFolders { get { return mIcommands[7]; } }
        public ICommand Icmd_SearchFoldersAndFiles { get { return mIcommands[8]; } }
        public ICommand Icmd_SearchFiles { get { return mIcommands[9]; } }

        public ICommand Icmd_CopyScratchpad_Script { get { return mIcommands[10]; } }
        public ICommand Icmd_CopyScratchpad_Load { get { return mIcommands[11]; } }
        public ICommand Icmd_CopyScratchpad_Save { get { return mIcommands[12]; } }
        public ICommand Icmd_CopyScratchpad_Clear { get { return mIcommands[13]; } }

        public ICommand Icmd_Ignore_Loose { get { return mIcommands[14]; } }
        public ICommand Icmd_Ignore_Add { get { return mIcommands[15]; } }
        public ICommand Icmd_Ignore_Delete { get { return mIcommands[16]; } }
        public ICommand Icmd_Ignore_Load { get { return mIcommands[17]; } }
        public ICommand Icmd_Ignore_Save { get { return mIcommands[18]; } }
        public ICommand Icmd_Ignore_Clear { get { return mIcommands[19]; } }

        internal BrowseTabVM(MainWindow app)
        {
            m_app = app;
            m_app.xaml_tabItemBrowse.DataContext = this;
            gd = GlobalData.GetInstance();
            CBFindBox = new ItemsControlVM(m_app.xaml_cbFindbox, new Action(() => { }));
            LV_CopyScratchpad = new CopyScratchpadListViewVM(m_app.xaml_lvCopyScratchpad);
            LV_Ignore = new IgnoreListViewVM(m_app.xaml_lvIgnore);
            LV_Files = new FilesListViewVM(m_app.xaml_lvFiles);
        //    LV_CompareFiles = new FilesListViewVM(m_app.xaml_lvCompareFiles);
            LV_Detail = new DetailListViewVM(m_app.xaml_lvDetail);
            LV_DetailVol = new DetailListViewVM(m_app.xaml_lvDetailVol);
            LV_Solitary = new SolitaryListViewVM(m_app.xaml_lvSolitary);
            LV_SameVol = new ClonesListViewVM(m_app.xaml_lvSameVol);
            LV_Clones = new ClonesListViewVM(m_app.xaml_lvClones);
            mIcommands = new ICommand[]
            {
                new RelayCommand(param => Collapse()),
                new RelayCommand(param => CompareCheck()),
                new RelayCommand(param => Compare()),
                new RelayCommand(param => Up()),
                new RelayCommand(param => Back()),
                new RelayCommand(param => Forward()),
                new RelayCommand(param => Copy()),
                new RelayCommand(param => SearchFolders()),
                new RelayCommand(param => SearchFoldersAndFiles()),
                new RelayCommand(param => SearchFiles()),

                new RelayCommand(param => CopyScratchpad_Script(), param => LV_CopyScratchpad.Selected),
                new RelayCommand(param => CopyScratchpad_Load()),
                new RelayCommand(param => CopyScratchpad_Save(), param => LV_CopyScratchpad.HasItems),
                new RelayCommand(param => CopyScratchpad_Clear(), param => LV_CopyScratchpad.HasItems),

                new RelayCommand(param => Ignore_Loose()),
                new RelayCommand(param => Ignore_Add()),
                new RelayCommand(param => Ignore_Delete(), param => LV_Ignore.Selected),
                new RelayCommand(param => Ignore_Load()),
                new RelayCommand(param => Ignore_Save(), param => LV_Ignore.HasItems),
                new RelayCommand(param => Ignore_Clear(), param => LV_Ignore.HasItems)
            };
        }

        readonly ItemsControlVM CBFindBox = null;

        // In order of appearance on the form
        readonly CopyScratchpadListViewVM LV_CopyScratchpad = null;
        readonly IgnoreListViewVM LV_Ignore = null;
        readonly FilesListViewVM LV_Files = null;
     //   readonly FilesListViewVM LV_CompareFiles = null;
        readonly DetailListViewVM LV_Detail = null;
        readonly DetailListViewVM LV_DetailVol = null;
        readonly SolitaryListViewVM LV_Solitary = null;
        readonly ClonesListViewVM LV_SameVol = null;
        readonly ClonesListViewVM LV_Clones = null;

        readonly ICommand[] mIcommands = null;

        readonly MainWindow m_app = null;
        readonly GlobalData gd = null;
    }
}
