using System;
using System.Windows.Input;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class WinDuplicatesVM : ListViewVM_Base<LVitem_FileDuplicatesVM>
    {
        public ICommand Icmd_GoTo { get; }

        public LVitem_FileDuplicatesVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;

                _selectedItem = value;

                if (null == value)
                    return;

                UpdateFileDetailOnNext(Tuple.Create(
                    (IReadOnlyCollection<string>)Array.AsReadOnly(value.FileLine), _treeNode), 0 /* UI Initiator */);

                SelectedItem_AllTriggers();
            }
        }
        internal void SelectedItem_Set(LVitem_FileDuplicatesVM value)
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
        LVitem_FileDuplicatesVM _selectedItem = null;

        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthPath { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FileDuplicatesVM.NumCols_; } }
    }
}