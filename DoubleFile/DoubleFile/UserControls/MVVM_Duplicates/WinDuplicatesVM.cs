using System;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_DuplicatesVM : ListViewVM_Base<LVitem_FileDuplicatesVM>
    {
        public ICommand Icmd_Nicknames { get; private set; }
        public ICommand Icmd_GoTo { get; private set; }

        public bool UseNicknames { private get; set; }

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

                UpdateFileDetailOnNext(Tuple.Create(value.FileLine, _treeNode), 0 /* UI Initiator */);
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

        public string WidthFilename => SCW;       // franken all NaN
        public string WidthPath => SCW;

        internal override int NumCols => LVitem_FileDuplicatesVM.NumCols_;
    }
}