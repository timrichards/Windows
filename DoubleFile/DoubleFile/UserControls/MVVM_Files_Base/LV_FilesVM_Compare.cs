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
    }
}
