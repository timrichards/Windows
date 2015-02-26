using System;
using System.Collections.ObjectModel;

namespace DoubleFile
{
    abstract class ListViewVM_Base : ObservableObject_OwnerWindow
    {
        internal Func<bool>
            SelectedOne = () => { DesignModeOK(); return false; };
        internal Func<bool>
            SelectedAny = () => { DesignModeOK(); return false; };

        public ObservableCollection<ListViewItemVM_Base>
            Items { get { return _items; } }
        readonly protected ObservableCollection<ListViewItemVM_Base>
            _items = new ObservableCollection<ListViewItemVM_Base>();

        internal virtual bool
            NewItem(string[] arrStr, bool bQuiet = false) { MBoxStatic.Assert(99994, false); return false; }
        internal abstract int
            NumCols { get; }

        internal void Add(ListViewItemVM_Base item, bool bQuiet = false)
        {
            item.LVVM = this;
            _items.Add(item);

            if (bQuiet == false)
            {
                RaiseItems();
            }
        }

        internal static string SCW = double.NaN.ToString();     // frankenhoek

        internal int Count { get { return _items.Count; } }

        internal ListViewItemVM_Base this[string s_in]
        {
            get
            {
                var s = s_in.ToLower();

                foreach (var o in _items)
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

            if (false == _items.IsEmpty())
            {
                _items[0].RaiseColumnWidths();
            }
            else
            {
                MBoxStatic.Assert(99993, false);
            }
        }
    }
}
