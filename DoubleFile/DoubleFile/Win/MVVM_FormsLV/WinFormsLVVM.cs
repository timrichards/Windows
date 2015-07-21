﻿using System.Windows.Input;

namespace DoubleFile
{
    partial class WinFormsLVVM : ListViewVM_Base<LVitem_FormsLVVM>
    {
        public ICommand Icmd_GoTo { get; private set; }

        public LVitem_FormsLVVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;

                _selectedItem = value;

                if (null == value)
                    return;

                SelectedItem_AllTriggers();
            }
        }
        internal void SelectedItem_Set(LVitem_FormsLVVM value)
        {
            if (value == _selectedItem)
                return;

            _selectedItem = value;
            RaisePropertyChanged("SelectedItem");
            SelectedItem_AllTriggers();
        }
        void SelectedItem_AllTriggers()
        {
        }
        LVitem_FormsLVVM _selectedItem = null;

        public string WidthFolder { get { return SCW; } }                   // franken all NaN
        public string WidthClones { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_FormsLVVM.NumCols_; } }
    }
}
