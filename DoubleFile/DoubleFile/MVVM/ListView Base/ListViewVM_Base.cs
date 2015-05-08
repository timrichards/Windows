using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Threading;

namespace DoubleFile
{
    abstract class ListViewVM_Base : Observable_OwnerWindowBase
    {
        internal Func<bool>
            SelectedOne = () => { DesignModeOK(); return false; };
        internal Func<bool>
            SelectedAny = () => { DesignModeOK(); return false; };

        public ObservableCollection<ListViewItemVM_Base>
            Items { get { return _items; } }
        ObservableCollection<ListViewItemVM_Base> _items = new ObservableCollection<ListViewItemVM_Base>();

        internal abstract int
            NumCols { get; }

        internal void Add(ListViewItemVM_Base item, bool bQuiet = false)
        {
            item.LVVM = this;
            _items.Add(item);

            if (false == bQuiet)
                RaiseItems();
        }

        internal virtual bool
            Add(string[] arrStr, bool bQuiet = false) { MBoxStatic.Assert(99994, false); return false; }
        internal virtual bool
            Add(IEnumerable<string[]> laStr, bool bQuiet = false) { MBoxStatic.Assert(99994, false); return false; }

        internal void Add(IEnumerable<ListViewItemVM_Base> lsItems, bool bQuiet = false, Func<bool> Cancel = null)
        {
            var dt = DateTime.Now;
            var nCounter = 0;
            var blockingFrame = new DispatcherFrame(true);

            foreach (var item in lsItems)
            {
                if ((null != Cancel) &&
                    Cancel())
                {
                    return;
                }

                Add(item, bQuiet: true);

                if (1000 > ++nCounter)
                    continue;

                if (100 < (DateTime.Now - dt).Milliseconds)
                {
                    // When there are too many items you get UI thread lockup.
                    Observable.Timer(TimeSpan.FromMilliseconds(33)).Timestamp()
                        .Subscribe(x => blockingFrame.Continue = false);

                    blockingFrame.Continue = true;
                    Dispatcher.PushFrame(blockingFrame);
                    dt = DateTime.Now;
                }

                nCounter = 0;
            }

            if ((false == _items.IsEmpty()) &&
                (false == bQuiet))
            {
                RaiseItems();
            }
        }

        //internal void Add<T>(IEnumerable<T> lsItems, bool bQuiet = false)
        //    where T : ListViewItemVM_Base, new()
        //{
        //    foreach (var item in lsItems)
        //        Add(new T(item), bQuiet: true);

        //    if (bQuiet == false)
        //    {
        //        RaiseItems();
        //    }
        //}

        static internal string SCW = "" + double.NaN;     // frankenhoek

        internal int Count { get { return _items.Count; } }

        protected void RaiseItems()
        {
            RaisePropertyChanged("Items");

            if (false == _items.IsEmpty())
                _items[0].RaiseColumnWidths();
            else
                MBoxStatic.Assert(99993, false);
        }
    }
}
