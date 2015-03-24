using System;

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
            for (var nCol = 0; nCol < NumCols; ++nCol)
            {
                var strPropName = PropertyNames[nCol];

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
}
