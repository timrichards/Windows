using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DoubleFile
{
    abstract class ListViewItemVM_Base : ObservableObjectBase
    {
        internal string this[int i] { get { return marr[i]; } }

        internal string[] StringValues
        {
            get { return marr; }
            set
            {
                MBoxStatic.Assert(99996, value.Length <= NumCols);
                marr = value;

                foreach (var propName in PropNames)
                {
                    RaisePropertyChanged(propName);
                    RaiseColumnWidth(propName);
                }
            }
        }

        ListViewItemVM_Base(ListViewVM_Base lvvm)
        {
            LVVM = lvvm;
        }

        // NumCols, and columns, are covariant: while all subclasses have columns; the subclasses vary in the number of columns.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal ListViewItemVM_Base(ListViewVM_Base lvvm, IList<string> lsStr)     // e.g. Volumes LV: marr
            : this(lvvm)
        {
            if (null != lsStr)
            {
                var nCount = lsStr.Count;

                marr =
                    ((nCount < NumCols)
                        ? lsStr.Concat(new string[NumCols - nCount])
                        : lsStr)
                    .ToArray();
            }
            else
            {
                marr = new string[NumCols];
            }

            MBoxStatic.Assert(99995, marr.Length == NumCols);

            if (null != LVVM)
                RaiseColumnWidths();
        }

        public virtual bool LocalEquals(ListViewItemVM_Base other)
        {
            if (null == other)
                return false;

            if (NumCols != other.NumCols)
                return false;

            if (SearchCol != other.SearchCol)
                return false;

            if ((null == marr) !=
                (null == other.marr))
            {
                return false;
            }

            if (null != marr)
            {
                if (0 !=
                    string.Join("", marr).CompareTo(
                    string.Join("", other.marr)))
                {
                    return false;
                }
            }

            // ignore the LVVM
            return true;
        }

        internal void RaiseColumnWidths()
        {
            foreach (var propName in PropNames)
                RaiseColumnWidth(propName);
        }

        void RaiseColumnWidth(string strPropName)
        {
            if (null == LVVM)
                return;

            // Column widths are only set for visible rows in the listviewer.
            // So far column widths are not set for datum: only marr, but they're not switched off for datum: Clones LVs do not use it.
            ListViewVM_Base.SCW = "" + 50;
            LVVM.RaisePropertyChanged("Width" + strPropName);     // some reasonable arbitrary value in case it gets stuck there
            ListViewVM_Base.SCW = "" + double.NaN;
            LVVM.RaisePropertyChanged("Width" + strPropName);
        }

        internal string SearchValue
        {
            get
            {
                return marr[SearchCol].ToLower();
            }
        }

        protected void SetProperty(int nCol, string s, [CallerMemberName]string propertyName = null)
        {
            if (this[nCol] != s)
            {
                marr[nCol] = s;

                MBoxStatic.Assert(99937, null != propertyName);
                RaisePropertyChanged(propertyName);
                RaiseColumnWidth(propertyName);
            }
        }

        internal ListViewVM_Base LVVM = null;

        internal abstract int NumCols { get; }
        protected virtual int SearchCol { get { return 0; } }

        protected string[] marr = null;

        IEnumerable<string> PropNames
        {
            get
            {
                return
                    _propNames
                    ?? (_propNames =
                        GetType().GetProperties().Where(pi => pi.PropertyType == typeof(string))
                        .Select(pi => pi.Name));
            }
        }

        IEnumerable<string> _propNames = null;
    }
}
