using System.Collections.ObjectModel;

namespace DoubleFile
{
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
}
