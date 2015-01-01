using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SearchDirLists
{
    // Class members are generally ordered public on down, which mixes constructors and types.
    // XAML-binding properties are always public, which puts them at the top, and private storage is at the end. Sorta reverse of C.

    // e.g. <ComboBox Name="xaml_cbVolumeName" Grid.Column="1" ItemsSource="{Binding List}" SelectedValue="{Binding S}"/>
    // e.g. CBVolumeName = new ItemsControlVM(m_app.xaml_cbVolumeName, new Action(() => { gd.m_strVolumeName = CBVolumeName.S; }));
    public class ItemsControlVM : ObservableObject
    {
        public string Current
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

        public ObservableCollection<string> Items { get { return m_items; } }
        public override string ToString() { return m_strCurrent; }

        internal ItemsControlVM(ItemsControl itemsCtl, Action action)
        {
            itemsCtl.DataContext = this;
            m_xaml = itemsCtl.Items;
            m_Action = action;
        }

        readonly ObservableCollection<string> m_items = new ObservableCollection<string>();
        readonly ItemCollection m_xaml = null;
        readonly Action m_Action = null;
        string m_strCurrent = null;
    }

    public abstract class ListViewItemVM : ObservableObject
    {
        public bool IsSelected
        {
            get { return m_bSelected; }
            set
            {
                if (value != m_bSelected)
                {
                    m_bSelected = value;
                    ActOnDirectSelChange();
                }
            }
        }

        internal void SelectProgrammatic(bool bSelected)
        {
            if (m_bSelected != bSelected)
            {
                m_bSelected = bSelected;
                RaisePropertyChanged("IsSelected");
            }
        }

        internal virtual void KeyUp(KeyEventArgs e) { }
        internal virtual void MouseUp() { }

        protected virtual void ActOnDirectSelChange() { }

        internal string this[int i] { get { return marr[i]; } }
        internal int Index = -1;

        ListViewItemVM(ListViewVM lvvm)
        {
            Index = lvvm.Count;
            LVVM = lvvm;
        }

        internal ListViewItemVM(ListViewVM lvvm, string[] arrStr)     // e.g. Volumes LV: marr
            : this(lvvm)
        {
            Utilities.Assert(1310.1001, arrStr.Length <= NumCols);
            marr = new string[NumCols];
            arrStr.CopyTo(marr, 0);
            RaiseColumnWidths();
        }

        internal ListViewItemVM(ListViewVM lvvm, SDL_ListViewItem datum_in)   // e.g. Clones LVs: datum
            : this(lvvm)
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
                string strPropName = PropertyNames[nCol];

                ListViewVM.SCW = 50.ToString();
                LVVM.RaisePropertyChanged("Width" + strPropName);     // some reasonable arbitrary value in case it gets stuck there
                ListViewVM.SCW = double.NaN.ToString();
                LVVM.RaisePropertyChanged("Width" + strPropName);
            }
        }

        internal string SearchValue
        {
            get
            {
                return marr[SearchCol].ToLower();
            }
        }

        protected void SetProperty(int nCol, string s)
        {
            if (this[nCol] != s)
            {
                marr[nCol] = s;
                RaisePropertyChanged(PropertyNames[nCol]);
            }

            RaiseColumnWidths();
        }

        internal readonly ListViewVM LVVM = null;

        internal abstract int NumCols { get; }
        protected abstract string[] PropertyNames { get; }
        protected virtual int SearchCol { get { return 0; } }

        internal readonly SDL_ListViewItem datum = null;
        protected string[] marr = null;                     // unless using datum: properties (cell values) get stored here

        protected bool m_bSelected = false;
    }

    public abstract class ListViewVM : ObservableObject
    {
        public ObservableCollection<ListViewItemVM> Items { get { return m_items; } }

        internal ListViewVM(ListView lv)
        {
            if (lv != null)     // null e.g. fake LVs for loading and saving data
            {
                (LVFE = lv).DataContext = this;
            }
        }

        internal virtual void NewItem(string[] arrStr) { Utilities.Assert(0, false); }
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

        internal static string SCW = double.NaN.ToString();     // frankenhoek
        internal readonly ListView LVFE = null;

        internal int Count { get { return m_items.Count; } }
        internal bool HasItems { get { return m_items.Count > 0; } }
        internal bool SelectedOne { get { return LVFE.SelectedItems.Count == 1; } }
        internal bool SelectedAny { get { return LVFE.SelectedItems.Count > 0; } }
        internal bool Contains(string s) { return (this[s] != null); }

        internal ListViewItemVM this[string s_in]
        {
            get
            {
                string s = s_in.ToLower();

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

        void RaiseItems()
        {
            LVFE.Items.Refresh();
            RaisePropertyChanged("Items");

            if (Utilities.Assert(0, m_items.Count > 0))
            {
                m_items[0].RaiseColumnWidths();
            }
        }

        readonly internal SDL_ListView data = new SDL_ListView();
        readonly protected ObservableCollection<ListViewItemVM> m_items = new ObservableCollection<ListViewItemVM>();
    }

    public abstract class ListViewVM_Generic<T> : ListViewVM where T : ListViewItemVM
    {
        internal ListViewVM_Generic(ListView lv) : base(lv) { }

        internal IEnumerable<T> ItemsCast { get { return m_items.Cast<T>(); } }
        internal IEnumerable<T> Selected { get { return LVFE.SelectedItems.Cast<T>(); } }
    }
}

namespace Template      // prevents smart tag rename command from renaming the templates after you've copied them and rename them
{
    using SearchDirLists;

    public class Template_LVitemVM : ListViewItemVM
    {
        public string ColumnNameHere { get { return marr[0]; } set { SetProperty(0, value); } }

        readonly static string[] marrPropName = new string[] { };

        internal Template_LVitemVM(Template_ListViewVM LV, string[] arrStr)
            : base(LV, arrStr) { }

        internal const int NumCols_ = 0;
        internal override int NumCols { get { return NumCols_; } }
        protected override string[] PropertyNames { get { return marrPropName; } }
    }

    public class Template_ListViewVM : ListViewVM_Generic<Template_LVitemVM>
    {
        public string WidthColumnNameHere { get { return SCW; } }

        internal Template_ListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(string[] arrStr) { Add(new Template_LVitemVM(this, arrStr)); }
        internal override int NumCols { get { return Template_LVitemVM.NumCols_; } }
    }
}
