﻿using System.Windows.Input;

namespace DoubleFile
{
    partial class WinSearchVM : ListViewVM_Base<LVitem_SearchVM>
    {
        public ICommand Icmd_Folders { get; private set; }
        public ICommand Icmd_FoldersAndFiles { get; private set; }
        public ICommand Icmd_Files { get; private set; }
        public ICommand Icmd_GoTo { get; private set; }

        public string SearchText { set; private get; }

        public LVitem_SearchVM SelectedItem
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
        internal void SelectedItem_Set(LVitem_SearchVM value)
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
        LVitem_SearchVM _selectedItem = null;

        public string WidthResults { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_SearchVM.NumCols_; } }
    }
}