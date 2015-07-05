﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Threading;
using System.Linq;

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
        internal void
            ClearItems() { _items = new ObservableCollection<ListViewItemVM_Base>(); RaisePropertyChanged("Items"); }

        internal abstract int
            NumCols { get; }

        static internal string
            SCW = "" + double.NaN;     // frankenhoek

        internal void
            Add(ListViewItemVM_Base item, bool bQuiet = false)
        {
            item.LVVM = this;
            _items.Add(item);

            if (false == bQuiet)
                RaiseItems();
        }

        internal void Add<T>(IEnumerable<T> ieItems, bool bQuiet = false, Func<bool> Cancel = null)
            where T : ListViewItemVM_Base
        {
            var dt = DateTime.Now;
            var nCounter = 0;
            var blockingFrame = new DispatcherFrame(true) { Continue = true };

            foreach (var item in ieItems)
            {
                if ((null != Cancel) &&
                    Cancel())
                {
                    return;
                }

                Add(item, bQuiet: true);

                if (1000 > ++nCounter)
                    continue;

                if (100 < (DateTime.Now - dt).TotalMilliseconds)
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

            if (false == bQuiet)
                RaiseItems();
        }

        protected void RaiseItems()
        {
            RaisePropertyChanged("Items");

            if (_items.Any())
                _items[0].RaiseColumnWidths();
            else
                MBoxStatic.Assert(99993, false);
        }
    }
}
