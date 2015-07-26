using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DoubleFile
{
    abstract class ListViewItemVM_Base : ObservableObjectBase
    {
        internal string this[int i] => _subItems[i];

        internal IList<string> SubItems
        {
            get { return _subItems; }
            set
            {
                if (null == value)
                {
                    _subItems = new string[NumCols];
                    return;
                }

                Util.Assert(99996, value.Count <= NumCols);

                var nTack = NumCols - value.Count;

                _subItems =
                    (0 == nTack)
                    ? value
                    : value.Concat(new string[nTack]).ToList();

                Util.Assert(99995, _subItems.Count == NumCols);

                foreach (var propName in PropNames)
                    RaisePropertyChanged(propName);

                RaiseColumnWidths();
            }
        }
        IList<string> _subItems = null;

        ListViewItemVM_Base(ListViewVM_Base lvvm)
        {
            LVVM = lvvm;
        }

        // NumCols, and columns, are covariant: while all subclasses have columns; the subclasses vary in the number of columns.
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected ListViewItemVM_Base(ListViewVM_Base lvvm = null, IList<string> lsStr = null)     // e.g. Volumes LV: marr
            : this(lvvm)
        {
            SubItems = lsStr;
        }

        public virtual bool
            LocalEquals(ListViewItemVM_Base other)
        {
            if (null == other)
                return false;

            if (NumCols != other.NumCols)
                return false;

            if (SearchCol != other.SearchCol)
                return false;

            if ((null == _subItems) !=
                (null == other._subItems))
            {
                return false;
            }

            if (null != _subItems)
            {
                if (false ==
                    string.Join("", _subItems).Equals(
                    string.Join("", other._subItems)))
                {
                    return false;
                }
            }

            // Could use
            //if (false == PropNames.SequenceEqual(other.PropNames))
            //    return false;
            // but PropNames is a reference to a static so
            if (false == ReferenceEquals(PropNames, other.PropNames))
                return false;

            // in fact in the same vein, but this always uses
            // reflection, whereas static PropNames just does it once:
            //if (GetType() != other.GetType())
            //    return false;

            // ignore the LVVM
            return true;
        }

        internal void
            RaiseColumnWidths()
        {
            if (null == LVVM)
                return;

            foreach (var propName in PropNames)
                RaiseColumnWidth(propName);
        }

        void
            RaiseColumnWidth(string strPropName)
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

        internal string
            SearchValue => _subItems[SearchCol].ToLower();

        protected void
            SetProperty(int nCol, string s, [CallerMemberName]string propertyName = null)
        {
            if (this[nCol] == s)
                return;

            _subItems[nCol] = s;
            Util.AssertNutNull(99937, propertyName);
            RaisePropertyChanged(propertyName);
            RaiseColumnWidth(propertyName);
        }

        internal ListViewVM_Base
            LVVM = null;

        abstract internal int
            NumCols { get; }
        virtual protected int
            SearchCol { get { return 0; } }

        string[] PropNames
        {
            get
            {
                return
                    _propNames
                    ?? (_propNames =
                        GetType().GetProperties().Where(pi => typeof(string) == pi.PropertyType)
                        .Select(pi => pi.Name).OrderBy(s => s)
                        .ToArray());
            }
        }
        abstract protected string[] _propNames { get; set; }
    }
}
