using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DoubleFile
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

        internal String this[int i] { get { return marr[i]; } }
        internal int Index = -1;

        ListViewItemVM(ListViewVM lvvm)
        {
            Index = lvvm.Count;
            LVVM = lvvm;
        }

        internal ListViewItemVM(ListViewVM lvvm, String[] arrStr)     // e.g. Volumes LV: marr
            : this(lvvm)
        {
            System.Diagnostics.Debug.Assert(arrStr.Length <= NumCols);
            marr = new string[NumCols];
            arrStr.CopyTo(marr, 0);
            RaiseColumnWidths();
        }

        internal ListViewItemVM(ListViewVM lvvm, ListViewItem datum_in)   // e.g. Clones LVs: datum
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
                String strPropName = PropertyNames[nCol];

                ListViewVM.SCW = 50.ToString();
                LVVM.RaisePropertyChanged("Width" + strPropName);     // some reasonable arbitrary value in case it gets stuck there
                ListViewVM.SCW = double.NaN.ToString();
                LVVM.RaisePropertyChanged("Width" + strPropName);
            }
        }

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

        internal readonly ListViewVM LVVM = null;

        internal abstract int NumCols { get; }
        protected abstract String[] PropertyNames { get; }
        protected virtual int SearchCol { get { return 0; } }

        internal readonly ListViewItem datum = null;
        protected String[] marr = null;                     // unless using datum: properties (cell values) get stored here

        protected bool m_bSelected = false;
    }

    public abstract class ListViewVM : ObservableObject
    {
        internal delegate bool BoolQuery();

        internal BoolQuery SelectedOne = null;
        internal BoolQuery SelectedAny = null;
        internal Action Refresh = null;

        public ObservableCollection<ListViewItemVM> Items { get { return m_items; } }

        internal virtual void NewItem(String[] arrStr) { System.Diagnostics.Debug.Assert(false); }
        internal virtual void NewItem(ListViewItem datum_in, bool bQuiet = false) { System.Diagnostics.Debug.Assert(false); }
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
            System.Diagnostics.Debug.Assert(m_items.Count == 0);
            m_items.Clear();

            foreach (ListViewItem lvItem in data.Items)
            {
                NewItem(lvItem, bQuiet: true);
            }

            RaiseItems();
        }

        void RaiseItems()
        {
            Refresh();
            RaisePropertyChanged("Items");

            if (m_items.Count > 0)
            {
                m_items[0].RaiseColumnWidths();
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }

        readonly internal ListView data = new ListView();
        readonly protected ObservableCollection<ListViewItemVM> m_items = new ObservableCollection<ListViewItemVM>();
    }

    public abstract class ListViewVM_Generic<T> : ListViewVM where T : ListViewItemVM
    {
        internal delegate IEnumerable<T> IEnumerableQuery();

        internal IEnumerableQuery Selected = null;

        internal ListViewVM_Generic() { }

        internal IEnumerable<T> ItemsCast { get { return m_items.Cast<T>(); } }
    }
}

namespace Template      // prevents smart tag rename command from renaming the templates after you've copied them and rename them
{
    using DoubleFile;

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

        internal override void NewItem(String[] arrStr) { Add(new Template_LVitemVM(this, arrStr)); }
        internal override int NumCols { get { return Template_LVitemVM.NumCols_; } }
    }
}
