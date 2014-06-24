using System.Windows.Input;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace SearchDirLists
{
    public static class CB_DP_KeyUp
    {
        public static readonly DependencyProperty EventProperty = DependencyProperty.RegisterAttached
        ("Event", typeof(bool), typeof(CB_DP_KeyUp), new UIPropertyMetadata(false, OnDPchanged));

        public static bool GetEvent(FrameworkElement element) { return (bool)element.GetValue(EventProperty); }
        public static void SetEvent(FrameworkElement element, bool value) { element.SetValue(EventProperty, value); }

        static void OnDPchanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        // This is where you modify (a) the type; and (b) the event handled.
        { ComboBox item = depObj as ComboBox; if ((bool)e.NewValue) { item.KeyUp += OnEvent; } else { item.KeyUp -= OnEvent; } }

        static void OnEvent(object sender, RoutedEventArgs e)
        {
            if (new Key[] { Key.Enter, Key.Return }.Contains(((KeyEventArgs)e).Key))
            {
#if (WPF)
                btnAddVolume.Command.Execute(null);
#endif
            }
        }

        public static Button btnAddVolume = null;
    }

    public partial class BrowseTabVM
    {
        // In order of appearance on the form
        public ICommand Icmd_Collapse { get { return marrIcmd[0]; } }
        public ICommand Icmd_CompareCheck { get { return marrIcmd[1]; } }
        public ICommand Icmd_Compare { get { return marrIcmd[2]; } }
        public ICommand Icmd_Up { get { return marrIcmd[3]; } }
        public ICommand Icmd_Back { get { return marrIcmd[4]; } }
        public ICommand Icmd_Forward { get { return marrIcmd[5]; } }
        public ICommand Icmd_Copy { get { return marrIcmd[6]; } }
        public ICommand Icmd_SearchFolders { get { return marrIcmd[7]; } }
        public ICommand Icmd_SearchFoldersAndFiles { get { return marrIcmd[8]; } }
        public ICommand Icmd_SearchFiles { get { return marrIcmd[9]; } }

        public ICommand Icmd_CopyScratchpad_Script { get { return marrIcmd[10]; } }
        public ICommand Icmd_CopyScratchpad_Load { get { return marrIcmd[11]; } }
        public ICommand Icmd_CopyScratchpad_Save { get { return marrIcmd[12]; } }
        public ICommand Icmd_CopyScratchpad_Clear { get { return marrIcmd[13]; } }

        public ICommand Icmd_Ignore_Loose { get { return marrIcmd[14]; } }
        public ICommand Icmd_Ignore_Add { get { return marrIcmd[15]; } }
        public ICommand Icmd_Ignore_Delete { get { return marrIcmd[16]; } }
        public ICommand Icmd_Ignore_Load { get { return marrIcmd[17]; } }
        public ICommand Icmd_Ignore_Save { get { return marrIcmd[18]; } }
        public ICommand Icmd_Ignore_Clear { get { return marrIcmd[19]; } }

        internal BrowseTabVM(MainWindow app)
        {
            m_app = app;
            CB_DP_KeyUp.btnAddVolume = m_app.xaml_btnAddVolume;
            m_app.xaml_tabItemBrowse.DataContext = this;
            gd = GlobalData.GetInstance();
            CB_FindBox = new ItemsControlVM(m_app.xaml_cbFindbox, new Action(() => { }));
            TV = new TreeViewVM(m_app.xaml_treeViewBrowse);
            LV_CopyScratchpad = new CopyScratchpadListViewVM(m_app.xaml_lvCopyScratchpad);
            LV_Ignore = new IgnoreListViewVM(m_app.xaml_lvIgnore);
            LV_Files = new FilesListViewVM(m_app.xaml_lvFiles);
        //    LV_CompareFiles = new FilesListViewVM(m_app.xaml_lvCompareFiles);
            LV_Detail = new DetailListViewVM(m_app.xaml_lvDetail);
            LV_DetailVol = new DetailListViewVM(m_app.xaml_lvDetailVol);
            LV_Solitary = new SolitaryListViewVM(m_app.xaml_lvSolitary);
            LV_SameVol = new ClonesListViewVM(m_app.xaml_lvSameVol);
            LV_Clones = new ClonesListViewVM(m_app.xaml_lvClones);
            marrIcmd = new ICommand[]
            {
                new RelayCommand(param => Collapse()),
                new RelayCommand(param => CompareCheck()),
                new RelayCommand(param => Compare()),
                new RelayCommand(param => Up()),
                new RelayCommand(param => Back()),
                new RelayCommand(param => Forward()),
                new RelayCommand(param => Copy()),
                new RelayCommand(param => SearchFolders(), param => (Utilities.NotNull(m_app.xaml_cbFindbox.Text).Trim().Length > 0)),
                new RelayCommand(param => SearchFoldersAndFiles(), param => (Utilities.NotNull(m_app.xaml_cbFindbox.Text).Trim().Length > 0)),
                new RelayCommand(param => SearchFiles(), param => (Utilities.NotNull(m_app.xaml_cbFindbox.Text).Trim().Length > 0)),

                new RelayCommand(param => CopyScratchpad_Script(), param => LV_CopyScratchpad.SelectedAny),
                new RelayCommand(param => CopyScratchpad_Load()),
                new RelayCommand(param => CopyScratchpad_Save(), param => LV_CopyScratchpad.HasItems),
                new RelayCommand(param => CopyScratchpad_Clear(), param => LV_CopyScratchpad.HasItems),

                new RelayCommand(param => Ignore_Loose()),
                new RelayCommand(param => Ignore_Add()),
                new RelayCommand(param => Ignore_Delete(), param => LV_Ignore.SelectedAny),
                new RelayCommand(param => Ignore_Load()),
                new RelayCommand(param => Ignore_Save(), param => LV_Ignore.HasItems),
                new RelayCommand(param => Ignore_Clear(), param => LV_Ignore.HasItems)
            };
        }

        readonly ItemsControlVM CB_FindBox = null;

        // In order of appearance on the form
        readonly CopyScratchpadListViewVM LV_CopyScratchpad = null;
        internal readonly TreeViewVM TV = null;
        readonly IgnoreListViewVM LV_Ignore = null;
        readonly FilesListViewVM LV_Files = null;
     //   readonly FilesListViewVM LV_CompareFiles = null;
        readonly DetailListViewVM LV_Detail = null;
        readonly DetailListViewVM LV_DetailVol = null;
        internal readonly SolitaryListViewVM LV_Solitary = null;
        internal readonly ClonesListViewVM LV_SameVol = null;
        internal readonly ClonesListViewVM LV_Clones = null;

        readonly ICommand[] marrIcmd = null;

        readonly MainWindow m_app = null;
        readonly GlobalData gd = null;
    }
}
