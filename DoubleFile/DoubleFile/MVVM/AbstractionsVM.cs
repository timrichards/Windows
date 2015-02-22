﻿using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;

namespace DoubleFile
{
    abstract class ListViewItemVM_Base : ObservableObjectBase, IEquatable<ListViewItemVM_Base>
    {
        internal string this[int i] { get { return marr[i]; } }

        internal string[] StringValues
        {
            get { return marr; }
            set
            {
                MBoxStatic.Assert(99996, value.Length <= NumCols);
                marr = value; 

                for (var nCol = 0; nCol < marr.Length; ++nCol)
                {
                    RaisePropertyChanged(PropertyNames[nCol]);
                }

                RaiseColumnWidths();
            }
        }

        ListViewItemVM_Base(ListViewVM_Base lvvm)
        {
            LVVM = lvvm;
        }

        // NumCols, and columns, are covariant: while all subclasses have columns; the subclasses vary in the number of columns.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal ListViewItemVM_Base(ListViewVM_Base lvvm, string[] arrStr)     // e.g. Volumes LV: marr
            : this(lvvm)
        {
            marr = new string[NumCols];

            if (arrStr != null)
            {
                MBoxStatic.Assert(99995, arrStr.Length <= NumCols);
                arrStr.CopyTo(marr, 0);
            }

            if (lvvm != null)
            {
                RaiseColumnWidths();
            }
        }

        public virtual bool Equals(ListViewItemVM_Base other)
        {
            if (null == other)
                return false;

            if (NumCols != other.NumCols)
                return false;

            if (SearchCol != other.SearchCol)
                return false;

            if (0 !=
                string.Join("", marr).CompareTo(
                string.Join("", other.marr)))
            {
                return false;
            }

            if (0 !=
                string.Join("", PropertyNames).CompareTo(
                string.Join("", other.PropertyNames)))
            {
                return false;
            }

            // ignore the LVVM
            return true;
        }

        internal void RaiseColumnWidths()
        {
            // Column widths are only set for visible rows in the listviewer.
            // So far column widths are not set for datum: only marr, but they're not switched off for datum: Clones LVs do not use it.
            for (int nCol = 0; nCol < NumCols; ++nCol)
            {
                string strPropName = PropertyNames[nCol];

                ListViewVM_Base.SCW = 50.ToString();
                LVVM.RaisePropertyChanged("Width" + strPropName);     // some reasonable arbitrary value in case it gets stuck there
                ListViewVM_Base.SCW = double.NaN.ToString();
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

            if (LVVM != null)
            {
                RaiseColumnWidths();
            }
        }

        internal ListViewVM_Base LVVM = null;

        internal abstract int NumCols { get; }
        protected abstract string[] PropertyNames { get; }
        protected virtual int SearchCol { get { return 0; } }

        protected string[] marr = null;
    }

    abstract class ListViewVM_Base : ObservableObject_OwnerWindow
    {
        internal delegate bool BoolQuery();
        internal BoolQuery SelectedOne = () => { DesignModeOK(); return false; };
        internal BoolQuery SelectedAny = () => { DesignModeOK(); return false; };

        public ObservableCollection<ListViewItemVM_Base> Items { get { return m_items; } }

        internal virtual bool NewItem(string[] arrStr, bool bQuiet = false) { MBoxStatic.Assert(99994, false); return false; }
        internal abstract int NumCols { get; }

        internal void Add(ListViewItemVM_Base item, bool bQuiet = false)
        {
            item.LVVM = this;
            m_items.Add(item);

            if (bQuiet == false)
            {
                RaiseItems();
            }
        }

        internal static string SCW = double.NaN.ToString();     // frankenhoek

        internal int Count { get { return m_items.Count; } }

        internal ListViewItemVM_Base this[string s_in]
        {
            get
            {
                var s = s_in.ToLower();

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

        void RaiseItems()
        {
            RaisePropertyChanged("Items");

            if (false == m_items.IsEmpty())
            {
                m_items[0].RaiseColumnWidths();
            }
            else
            {
                MBoxStatic.Assert(99993, false);
            }
        }

        readonly protected ObservableCollection<ListViewItemVM_Base> m_items = new ObservableCollection<ListViewItemVM_Base>();
    }

    abstract class ListViewVM_GenericBase<T> : ListViewVM_Base, IEquatable<ListViewVM_GenericBase<T>>
        where T : ListViewItemVM_Base
    {
        internal delegate IEnumerable<T> EnumerableQuery();
        internal EnumerableQuery Selected = () => { DesignModeOK(); return null; };

        internal IEnumerable<T> ItemsCast { get { return m_items.Cast<T>(); } }

        public virtual bool Equals(ListViewVM_GenericBase<T> other)
        {
            if (null == other)
            {
                return false;
            }

            MBoxStatic.Assert(99992, (false == object.ReferenceEquals(this, other)));

            if (Items.Count != other.Items.Count)
            {
                return false;
            }

            foreach (var item in ItemsCast)
            {
                var otherItem = other[item.SearchValue];

                MBoxStatic.Assert(99991, (false == object.ReferenceEquals(item, otherItem)));

                if (false == item.Equals(otherItem))
                    return false;
            }

            return true;
        }
    }
}

namespace Template      // prevents smart tag rename command from renaming the templates after you've copied them and rename them
{
    using DoubleFile;

    class Template_LVitemVM : ListViewItemVM_Base
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string ColumnNameHere { get { return marr[0]; } set { SetProperty(0, value); } }

        readonly static string[] marrPropName = new string[] { };

        internal Template_LVitemVM(Template_ListViewVM LV, string[] arrStr)
            : base(LV, arrStr) { }

        internal const int NumCols_ = 0;
        internal override int NumCols { get { return NumCols_; } }
        protected override string[] PropertyNames { get { return marrPropName; } }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class Template_ListViewVM : ListViewVM_GenericBase<Template_LVitemVM>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string WidthColumnNameHere { get { return SCW; } }

        internal override bool NewItem(string[] arrStr, bool bQuiet = false) { Add(new Template_LVitemVM(this, arrStr), bQuiet); return true; }
        internal override int NumCols { get { return Template_LVitemVM.NumCols_; } }
    }
}
