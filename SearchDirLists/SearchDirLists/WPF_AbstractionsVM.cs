using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

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

    abstract class ListViewItemVM : ObservableObject
    {
        internal String this[int i] { get { return marr[i]; } }
        internal int Index = -1;

        protected ListViewItemVM(ListViewVM LV)
        {
            marr = new string[NumCols];
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
            String strPropName = PropertyNames[nCol];

            RaisePropertyChanged(strPropName);
            ListViewVM.SCW = 50.ToString(); LV_RaisePropertyChanged("Width" + strPropName);     // some reasonable arbitrary value in case it gets stuck there
            ListViewVM.SCW = double.NaN.ToString(); LV_RaisePropertyChanged("Width" + strPropName);
        }

        internal String SearchValue { get { return marr[SearchCol].ToLower(); } }
        protected void SetProperty(int nCol, String s) { if (this[nCol] != s) { marr[nCol] = s; Raise(nCol); } }

        internal abstract int NumCols { get; }
        protected abstract String[] PropertyNames { get; }
        protected abstract RaisePropertyChangedDelegate LV_RaisePropertyChanged { get; }    // frankenhoek
        protected abstract int SearchCol { get; }

        protected String[] marr = null;                                                     // all properties (columns/items) get stored here
    }

    abstract class ListViewVM : ObservableObject
    {
        public ObservableCollection<ListViewItemVM> Items { get { return m_items; } }

        internal ListViewVM(ListView lv)
        {
            (m_lv = lv).DataContext = this;
        }

        internal abstract void NewItem(String[] arrStr);
        internal abstract int NumCols { get; }

        internal bool Add(ListViewItemVM item)
        {
            m_items.Add(item);
            m_lv.Items.Refresh();
            RaisePropertyChanged("Items");
            return true;
        }

        internal static String SCW = double.NaN.ToString();                                 // frankenhoek

        internal int Count { get { return m_items.Count; } }
        internal bool HasItems { get { return m_items.Count > 0; } }
        internal bool SelectedOne { get { return m_lv.SelectedItems.Count == 1; } }
        internal bool SelectedAny { get { return m_lv.SelectedItems.Count > 0; } }
        internal bool Contains(String s) { return (this[s] != null); }
        internal ListViewItemVM this[String s_in] { get { String s = s_in.ToLower(); foreach (var o in m_items) if (o.SearchValue == s) return o; return null; } }

        readonly protected ObservableCollection<ListViewItemVM> m_items = new ObservableCollection<ListViewItemVM>();
        readonly protected ListView m_lv = null;
    }

    abstract class ListViewVM_Generic<T> : ListViewVM where T : ListViewItemVM
    {
        internal ListViewVM_Generic(ListView lv) : base(lv) { }

        internal IEnumerable<T> ItemsCast { get { return m_items.Cast<T>(); } }
        internal IEnumerable<T> Selected { get { return m_lv.SelectedItems.Cast<T>(); } }
    }
}

namespace Template      // prevents smart tag rename command from renaming the templates after you've copied them and rename them
{
    using SearchDirLists;

    class Template_LVitemVM : ListViewItemVM
    {
        public String ColumnNameHere { get { return marr[0]; } set { SetProperty(0, value); } }

        readonly static String[] marrPropName = new String[] { };

        internal Template_LVitemVM(Template_ListViewVM LV, String[] arrStr)
            : base(LV)
        {
            mPropChanged_LV = LV.RaisePropertyChanged;
            CopyInArray(arrStr);
        }

        internal const int NumCols_ = 0;
        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
        protected override RaisePropertyChangedDelegate LV_RaisePropertyChanged { get { return mPropChanged_LV; } } RaisePropertyChangedDelegate mPropChanged_LV = null;
        protected override int SearchCol { get { return 0; } }
    }

    class Template_ListViewVM : ListViewVM_Generic<Template_LVitemVM>
    {
        public String WidthColumnNameHere { get { return SCW; } }

        internal Template_ListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new Template_LVitemVM(this, arrStr)); }
        internal override int NumCols { get { return Template_LVitemVM.NumCols_; } }
    }

    class Test
    {
        Template_ListViewVM t = new Template_ListViewVM(null);

        Test()
        {
            Template_LVitemVM t1 = t.ItemsCast.ElementAt(0);
        }
    }
}
