using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    partial class LV_FilesVM_Compare : LV_FilesVM_Base
    {
        public override Visibility
            DupColVisibility => Visibility.Collapsed;
        public Visibility
            VisibilityOnItems => Items.Any() ? Visibility.Visible : Visibility.Collapsed;

        internal Action<LVitem_CompareVM>
            SelectedItemChanged = null;
        internal void
            ClearSelection() => SelectedItem_Set(null, /* UI initiator */ 0);

        internal override void Add<T>(IEnumerable<T> ieItems, bool bQuiet = false)
        {
            base.Add(ieItems, bQuiet);

            if (false == bQuiet)
                RaisePropertyChanged("VisibilityOnItems");
        }

        internal override void ClearItems()
        {
            base.ClearItems();
            RaisePropertyChanged("VisibilityOnItems");
        }

        protected override void SelectedItem_AllTriggers(decimal nInitiator)
        {
            base.SelectedItem_AllTriggers(nInitiator);

            if (null != _selectedItem)
                SelectedItemChanged?.Invoke((LVitem_CompareVM)_selectedItem);
        }
    }
}
