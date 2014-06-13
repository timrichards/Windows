using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace SearchDirLists
{
    // Class members are generally ordered public on down, which mixes constructors and types.
    // XAML-binding properties are always public, which puts them at the top, and private storage is at the end. Sorta reverse of C.

    // e.g. <ComboBox Name="xaml_cbVolumeName" Grid.Column="1" ItemsSource="{Binding List}" SelectedValue="{Binding S}"/>
    // e.g. CBVolumeName = new ItemsControlVM(m_app.xaml_cbVolumeName, new Action(() => { gd.m_strVolumeName = CBVolumeName.S; }));
    class ItemsControlVM : ObservableObject
    {
        public String S
        {
            set
            {
                if ((value != null) && (value.Trim().Length > 0) && (m_list.Contains(value) == false))      // TODO: ignore case
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

        public ObservableCollection<String> List { get { return m_list; } }
        public override string ToString() { return m_strCurrent; }

        internal ItemsControlVM(ItemsControl itemsCtl, Action action)
        {
            itemsCtl.DataContext = this;
            m_items = itemsCtl.Items;
            m_Action = action;
        }

        readonly ObservableCollection<String> m_list = new ObservableCollection<String>();
        readonly ItemCollection m_items = null;
        readonly Action m_Action = null;
        String m_strCurrent = null;
    }

    class ListViewItemVM : ObservableObject
    {
        internal readonly int NumCols = -1;
        internal String this[int i] { get { return marr[i]; } }
        internal int Index = -1;

        protected ListViewItemVM(ListViewVM LV, int nNumCols, String[] arrPropName_in)
        {
            LV_RaisePropertyChanged = LV.RaisePropertyChanged;
            marr = new string[NumCols = nNumCols];
            arrPropName = arrPropName_in;
            Index = LV.Count;
        }

        protected void CopyInArray(String[] arrStr)
        {
            Utilities.Assert(1310.1001, arrStr.Length <= NumCols);
            arrStr.CopyTo(marr, 0);
            for (int i = 0; i < arrStr.Length; ++i) Raise(i);
        }

        void Raise(int nCol)
        {
            String strPropName = arrPropName[nCol];

            RaisePropertyChanged(strPropName);
            VolumesListViewVM.SCW = 50.ToString(); LV_RaisePropertyChanged("Width" + strPropName);     // some reasonable arbitrary value in case it gets stuck there
            VolumesListViewVM.SCW = double.NaN.ToString(); LV_RaisePropertyChanged("Width" + strPropName);
        }

        protected readonly String[] arrPropName = null;

        protected void SetProperty(int nCol, String s) { if (this[nCol] != s) { marr[nCol] = s; Raise(nCol); } }

        readonly protected RaisePropertyChangedDelegate LV_RaisePropertyChanged = null;     // frankenhoek
        protected String[] marr = null;                                                     // all properties (columns/items) get stored here
    }

    class ListViewVM : ObservableObject
    {
        public ObservableCollection<VolumeLVitemVM> Items { get { return m_items; } }

        internal ListViewVM(ItemsControl itemsCtl)
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

        internal static String SCW = double.NaN.ToString();                                 // frankenhoek

        internal int Count { get { return m_items.Count; } }

        readonly protected ObservableCollection<VolumeLVitemVM> m_items = new ObservableCollection<VolumeLVitemVM>();
        readonly ItemsControl m_itemsCtl = null;
    }
}

namespace Template      // prevents smart tag rename command from renaming the templates after you've copied them and rename them
{
    using SearchDirLists;

    class Template_LVitemVM : ListViewItemVM
    {
        public String ColumnNameHere { get { return marr[0]; } set { SetProperty(0, value); } }

        internal new const int NumCols = 0;
        readonly new static String[] arrPropName = new String[] { };

        Template_LVitemVM(Template_ListViewVM LV)
            : base(LV, NumCols, arrPropName) { }

        internal Template_LVitemVM(Template_ListViewVM LV, String[] arrStr)
            : this(LV)
        {
            CopyInArray(arrStr);
        }
    }

    class Template_ListViewVM : ListViewVM
    {
        public String WidthColumnNameHere { get { return SCW; } }

        internal Template_ListViewVM(ItemsControl itemsCtl) : base(itemsCtl) { }
    }
}
