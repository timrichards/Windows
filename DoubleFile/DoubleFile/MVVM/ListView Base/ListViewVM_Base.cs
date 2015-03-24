﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

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

        internal abstract int
            NumCols { get; }

        internal void Add(ListViewItemVM_Base item, bool bQuiet = false)
        {
            item.LVVM = this;
            _items.Add(item);

            if (false == bQuiet)
            {
                RaiseItems();
            }
        }

        internal virtual bool
            Add(string[] arrStr, bool bQuiet = false) { MBoxStatic.Assert(99994, false); return false; }
        internal virtual bool
            Add(IReadOnlyList<string[]> laStr, bool bQuiet = false) { MBoxStatic.Assert(99994, false); return false; }

        internal void Add(IEnumerable<ListViewItemVM_Base> lsItems, bool bQuiet = false)
        {
            var dt = DateTime.Now;
            var nCounter = 0;

            foreach (var item in lsItems)
            {
                Add(item, bQuiet: true);

                if (++nCounter < 100)
                    continue;

                // When there are too many items you get UI thread lockup.

                if ((DateTime.Now - dt).Milliseconds > 20)
                {
                    DispatcherFrame blockingFrame = null;

                    LocalTimer timer = null;
                    var timer_ = new LocalTimer(33.0, () =>
                    {
                        timer.Stop();
                        blockingFrame.Continue = false;
                    }).Start();
                    timer = timer_;

                    Dispatcher.PushFrame(blockingFrame = new DispatcherFrame(true));
                    dt = DateTime.Now;
                }

                nCounter = 0;
            }

            if ((false == Items.IsEmpty()) &&
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

        internal static string SCW = double.NaN.ToString();     // frankenhoek

        internal int Count { get { return _items.Count; } }

        protected void RaiseItems()
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
