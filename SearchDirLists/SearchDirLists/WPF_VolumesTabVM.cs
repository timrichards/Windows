using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using Forms = System.Windows.Forms;

namespace SearchDirLists
{
    // Class members are ordered: (1) Public properties then (2) internal (including constructors) then (3) private, with data somewhat mixed.

    class ItemsControlVM : ObservableObject
    {
        public String S
        {
            set
            {
                if ((value != null) && (m_list.Contains(value) == false))
                {
                    m_list.Add(value);
                    m_items.Refresh();
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
        public override string ToString() { return m_strCurrent; }

        internal ItemsControlVM(ItemsControl itemsCtl, Action action)
        {
            itemsCtl.DataContext = this;
            m_items = itemsCtl.Items;
            m_Action = action;
        }

        readonly List<String> m_list = new List<String>();
        readonly ItemCollection m_items = null;
        readonly Action m_Action = null;
        String m_strCurrent = null;
    }

    class VolumeLVitemVM : ObservableObject
    {
        public String VolumeName { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Path { get { return marr[1]; } set { SetProperty(1, value); } }
        public String SaveAs { get { return marr[2]; } set { SetProperty(2, value); } }
        public String Status { get { return marr[3]; } set { SetProperty(3, value); } }
        public String IncludeStr { get { return marr[4]; } set { SetProperty(4, value); } }
        public String VolumeGroup { get { return marr[5]; } set { SetProperty(5, value); } }

        internal VolumeLVitemVM(String strVol, String strPath, String strSaveAs, String strStatus, bool bSaveAsExists)
        {
            marr = new string[NumCols];
            VolumeName = strVol;
            Path = strPath;
            SaveAs = strSaveAs;
            Status = strStatus;
            IncludeStr = "Yes";
            SaveAsExists = bSaveAsExists;
        }

        internal VolumeLVitemVM(String[] arrStr)
        {
            marr = new string[NumCols];
            Utilities.Assert(1310.1001, arrStr.Length <= NumCols);
            arrStr.CopyTo(marr, 0);
            for (int i = 0; i < arrStr.Length; ++i) Raise(i);
            SaveAsExists = true;
        }

        internal String this[int i] { get { return marr[i]; } }
        internal bool Include { get { return (IncludeStr == "Yes"); } set { IncludeStr = (value ? "Yes" : "No"); } }

        internal const int NumCols = 6;
        internal static VolumesListViewVM m_LV = null;

        internal bool SaveAsExists = false;     // TODO: set back to false when fail Tree
        internal SDL_TreeNode treeNode = null;

        void Raise(int nCol)
        {
            String strPropName = new String[] { "VolumeName", "Path", "SaveAs", "Status", "IncludeStr", "VolumeGroup" }[nCol];
            RaisePropertyChanged(strPropName);
            m_LV.RaiseColWidth(0.ToString(), "Width" + strPropName);
            m_LV.RaiseColWidth("NaN", "Width" + strPropName);
        }

        void SetProperty(int nCol, String s) { if (this[nCol] != s) { marr[nCol] = s; Raise(nCol); } }

        String[] marr = null;       // all properties (columns/items) get stored here
    }

    class VolumesListViewVM : ObservableObject
    {
        public ObservableCollection<VolumeLVitemVM> Items { get { return m_items; } }

        public String WidthVolumeName { get { return m_strColWidth; } }
        public String WidthPath { get { return m_strColWidth; } }
        public String WidthSaveAs { get { return m_strColWidth; } }
        public String WidthStatus { get { return m_strColWidth; } }
        public String WidthIncludeStr { get { return m_strColWidth; } }
        public String WidthVolumeGroup { get { return m_strColWidth; } }

        internal void RaiseColWidth(String strColWidth, String strPropName) { m_strColWidth = strColWidth; RaisePropertyChanged(strPropName); }

        internal VolumesListViewVM(ItemsControl itemsCtl)
        {
            (m_itemsCtl = itemsCtl).DataContext = this;
            VolumeLVitemVM.m_LV = this;
        }

        internal bool Add(VolumeLVitemVM item)
        {
            m_items.Add(item);
            m_itemsCtl.Items.Refresh();
            RaisePropertyChanged("Items");
            return true;
        }

        internal int Count { get { return m_items.Count; } }
        internal bool ContainsVolumeName(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.VolumeName.ToLower() == s) return true; return false; }
        internal bool ContainsUnsavedPath(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if ((item.Path.ToLower() == s) && (item.SaveAsExists == false)) return true; return false; }
        internal bool ContainsSaveAs(String t) { String s = t.ToLower(); foreach (VolumeLVitemVM item in m_items) if (item.SaveAs.ToLower() == s) return true; return false; }

        String m_strColWidth = "NaN";

        readonly ObservableCollection<VolumeLVitemVM> m_items = new ObservableCollection<VolumeLVitemVM>();
        readonly ItemsControl m_itemsCtl = null;
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
