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
        public String Current
        {
            set
            {
                if ((value != null) && (value.Trim().Length > 0) && (m_items.Contains(value) == false))      // TODO: ignore case
                {
                    m_items.Add(value);
                    m_xaml.Refresh();
                    RaisePropertyChanged("Items");
                }

                if (value != m_strCurrent)
                {
                    m_strCurrent = value;
                    m_Action();
                    RaisePropertyChanged("Current");
                }
            }

            get { return m_strCurrent; }
        }

        public ObservableCollection<String> Items { get { return m_items; } }
        public override string ToString() { return m_strCurrent; }

        internal ItemsControlVM(ItemsControl itemsCtl, Action action)
        {
            itemsCtl.DataContext = this;
            m_xaml = itemsCtl.Items;
            m_Action = action;
        }

        readonly ObservableCollection<String> m_items = new ObservableCollection<String>();
        readonly ItemCollection m_xaml = null;
        readonly Action m_Action = null;
        String m_strCurrent = null;
    }

    abstract class ListViewItemVM : ObservableObject
    {
        internal String this[int i] { get { return marr[i]; } }
        internal int Index = -1;

        internal ListViewItemVM(ListViewVM LV, String[] arrStr)
        {
            Index = LV.Count;
            LV_RaisePropertyChanged = LV.RaisePropertyChanged;
            Utilities.Assert(1310.1001, arrStr.Length <= NumCols);
            marr = new string[NumCols];
            arrStr.CopyTo(marr, 0);

            for (int i = 0; i < arrStr.Length; ++i)
            {
                Raise(i);
            }
        }

        internal ListViewItemVM(ListViewVM LV, SDL_ListViewItem datum_in)
        {
            Index = LV.Count;
            LV_RaisePropertyChanged = LV.RaisePropertyChanged;
            datum = datum_in;
            // ListViewVM raises property changed after all items are added
        }

        void Raise(int nCol)
        {
            String strPropName = PropertyNames[nCol];

            RaisePropertyChanged(strPropName);
            ListViewVM.SCW = 50.ToString();
            LV_RaisePropertyChanged("Width" + strPropName);     // some reasonable arbitrary value in case it gets stuck there
            ListViewVM.SCW = double.NaN.ToString();
            LV_RaisePropertyChanged("Width" + strPropName);
        }

        RaisePropertyChangedDelegate LV_RaisePropertyChanged = null;

        internal String SearchValue
        {
            get
            {
                return marr[SearchCol].ToLower();
            }
        }

        protected void SetProperty(int nCol, String s)
        {
            if (this[nCol] != s)
            {
                marr[nCol] = s;
                Raise(nCol);
            }
        }

        internal abstract int NumCols { get; }
        protected abstract String[] PropertyNames { get; }
        protected virtual int SearchCol { get { return 0; } }
        protected readonly SDL_ListViewItem datum = null;

        protected String[] marr = null;                         // all properties (columns/items) get stored here
    }

    abstract class ListViewVM : ObservableObject
    {
        public ObservableCollection<ListViewItemVM> Items { get { return m_items; } }

        internal ListViewVM(ListView lv)
        {
            if (lv != null)
            {
                (m_lv = lv).DataContext = this;
            }
        }

        internal virtual void NewItem(String[] arrStr) { Utilities.Assert(0, false); }
        internal virtual void NewItem(SDL_ListViewItem datum_in, bool bQuiet = false) { Utilities.Assert(0, false); }
        internal abstract int NumCols { get; }

        internal void Add(ListViewItemVM item, bool bQuiet = false)
        {
            m_items.Add(item);

            if (bQuiet == false)
            {
                m_lv.Items.Refresh();
                RaisePropertyChanged("Items");
            }
        }

        internal static String SCW = double.NaN.ToString();     // frankenhoek

        internal int Count { get { return m_items.Count; } }
        internal bool HasItems { get { return m_items.Count > 0; } }
        internal bool SelectedOne { get { return m_lv.SelectedItems.Count == 1; } }
        internal bool SelectedAny { get { return m_lv.SelectedItems.Count > 0; } }
        internal bool Contains(String s) { return (this[s] != null); }

        internal ListViewItemVM this[String s_in]
        {
            get
            {
                String s = s_in.ToLower();

                foreach (var o in m_items)
                {
                    if (o.SearchValue == s)
                    {
                        return o;
                    }
                }

                return null;
            }
        }

        internal void SyncData()
        {
            Utilities.Assert(0, Items.Count == 0);
            Items.Clear();

            foreach (SDL_ListViewItem lvItem in data.Items)
            {
                NewItem(lvItem, bQuiet: true);
            }

            m_lv.Items.Refresh();
            RaisePropertyChanged("Items");
        }

        readonly internal SDL_ListView data = new SDL_ListView();
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
            : base(LV, arrStr) { }

        internal const int NumCols_ = 0;
        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
    }

    class Template_ListViewVM : ListViewVM_Generic<Template_LVitemVM>
    {
        public String WidthColumnNameHere { get { return SCW; } }

        internal Template_ListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new Template_LVitemVM(this, arrStr)); }
        internal override int NumCols { get { return Template_LVitemVM.NumCols_; } }
    }
}
