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
    public class ItemsControlVM : ObservableObject
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

    public abstract class ListViewItemVM : ObservableObject
    {
        internal String this[int i] { get { return marr[i]; } }
        internal int Index = -1;

        ListViewItemVM(ListViewVM LV)
        {
            Index = LV.Count;
            LV_RaisePropertyChanged = LV.RaisePropertyChanged;
        }

        internal ListViewItemVM(ListViewVM LV, String[] arrStr)     // e.g. Volumes LV: marr
            : this(LV)
        {
            Utilities.Assert(1310.1001, arrStr.Length <= NumCols);
            marr = new string[NumCols];
            arrStr.CopyTo(marr, 0);
            RaiseColumnWidths();
        }

        internal ListViewItemVM(ListViewVM LV, SDL_ListViewItem datum_in)   // e.g. Clones LVs: datum
            : this(LV)
        {
            datum = datum_in;
            // ListViewVM raises property changed after all items are added. Clones LVs do not use it.
        }

        internal void RaiseColumnWidths()
        {
            // Column widths are only set for visible rows in the listviewer.
            // So far column widths are not set for datum: only marr, but they're not switched off for datum: Clones LVs do not use it.
            for (int nCol = 0; nCol < NumCols; ++nCol)
            {
                String strPropName = PropertyNames[nCol];

                ListViewVM.SCW = 50.ToString();
                LV_RaisePropertyChanged("Width" + strPropName);     // some reasonable arbitrary value in case it gets stuck there
                ListViewVM.SCW = double.NaN.ToString();
                LV_RaisePropertyChanged("Width" + strPropName);
            }
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
                RaisePropertyChanged(PropertyNames[nCol]);
            }

            RaiseColumnWidths();
        }

        internal abstract int NumCols { get; }
        protected abstract String[] PropertyNames { get; }
        protected virtual int SearchCol { get { return 0; } }

        internal readonly SDL_ListViewItem datum = null;
        protected String[] marr = null;                     // unless using datum, properties, cell values, get stored here
    }

    public abstract class ListViewVM : ObservableObject
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
                RaiseItems();
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
            Utilities.Assert(0, m_items.Count == 0);
            m_items.Clear();

            foreach (SDL_ListViewItem lvItem in data.Items)
            {
                NewItem(lvItem, bQuiet: true);
            }

            RaiseItems();
        }

        internal virtual void SelectionChanged(SelectionChangedEventArgs e) { }

        void RaiseItems()
        {
            m_lv.Items.Refresh();
            RaisePropertyChanged("Items");

            if (Utilities.Assert(0, m_items.Count > 0))
            {
                m_items[0].RaiseColumnWidths();
            }
        }

        readonly internal SDL_ListView data = new SDL_ListView();
        readonly protected ObservableCollection<ListViewItemVM> m_items = new ObservableCollection<ListViewItemVM>();
        readonly protected ListView m_lv = null;
    }

    public abstract class ListViewVM_Generic<T> : ListViewVM where T : ListViewItemVM
    {
        internal ListViewVM_Generic(ListView lv) : base(lv) { }

        internal IEnumerable<T> ItemsCast { get { return m_items.Cast<T>(); } }
        internal IEnumerable<T> Selected { get { return m_lv.SelectedItems.Cast<T>(); } }
    }
}

namespace Template      // prevents smart tag rename command from renaming the templates after you've copied them and rename them
{
    using SearchDirLists;

    public class Template_LVitemVM : ListViewItemVM
    {
        public String ColumnNameHere { get { return marr[0]; } set { SetProperty(0, value); } }

        readonly static String[] marrPropName = new String[] { };

        internal Template_LVitemVM(Template_ListViewVM LV, String[] arrStr)
            : base(LV, arrStr) { }

        internal const int NumCols_ = 0;
        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
    }

    public class Template_ListViewVM : ListViewVM_Generic<Template_LVitemVM>
    {
        public String WidthColumnNameHere { get { return SCW; } }

        internal Template_ListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new Template_LVitemVM(this, arrStr)); }
        internal override int NumCols { get { return Template_LVitemVM.NumCols_; } }
    }
}
