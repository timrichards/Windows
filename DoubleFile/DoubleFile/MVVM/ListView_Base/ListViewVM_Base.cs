using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DoubleFile
{
    abstract class ListViewVM_Base : Observable_OwnerWindowBase
    {
        internal Func<bool>
            SelectedOne = () => DesignModeOK(false);
        internal Func<bool>
            SelectedAny = () => DesignModeOK(false);

        public ObservableCollection<ListViewItemVM_Base>
            Items => _items;
        ObservableCollection<ListViewItemVM_Base> _items = new ObservableCollection<ListViewItemVM_Base>();
        internal void
            ClearItems() { Util.UIthread(99867, () => _items.Clear()); RaisePropertyChanged("Items"); }

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

        internal void Add<T>(IEnumerable<T> ieItems, bool bQuiet = false)
            where T : ListViewItemVM_Base
        {
            var stopwatch = Stopwatch.StartNew();
            var nCounter = 0;

            foreach (var item in ieItems)
            {
                Add(item, bQuiet: true);

                if (1000 > ++nCounter)
                    continue;

                if (100 < stopwatch.ElapsedMilliseconds)
                {
                    // When there are too many items you get UI thread lockup.
                    Util.Block(33);
                    stopwatch.Reset();
                    stopwatch.Start();
                }

                nCounter = 0;
            }

            stopwatch.Stop();

            if (false == bQuiet)
                RaiseItems();
        }

        protected void RaiseItems()
        {
            RaisePropertyChanged("Items");

            if (0 < _items.Count)
                _items[0].RaiseColumnWidths();
            else
                Util.Assert(99993, false, bTraceOnly: true);
        }

        protected void Export()
        {
            var dlg = new SaveFileDialog
            {
                Title = "Export List",
                Filter = "Text file|*.txt",
            };

            if (false == ModalThread.Go(darkWindow => dlg.ShowDialog((Window)darkWindow) ?? false))
                return;

            var strKey = "Exporting list";

            new ProgressOverlay(new[] { dlg.FileName }, new[] { strKey }, progress =>
            {
                Util.ThreadMake(() =>
                {
                    using (var sw = new StreamWriter(dlg.FileName))
                    {
                        foreach (var item in Items)
                            sw.WriteLine(item.ExportLine);
                    }

                    progress.SetCompleted(strKey);
                });
            })
                .ShowOverlay();
        }
    }
}
