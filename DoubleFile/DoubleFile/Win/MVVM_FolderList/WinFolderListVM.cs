using System.Windows.Input;

namespace DoubleFile
{
    partial class WinFolderListVM : ListViewVM_Base<LVitem_FolderListVM>
    {
        public ICommand Icmd_GoTo { get; private set; }

        public LVitem_FolderListVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;

                _selectedItem = value;

                if (null != _selectedItem)
                    SelectedItem_AllTriggers();
            }
        }
        internal void SelectedItem_Set(LVitem_FolderListVM value)
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
        LVitem_FolderListVM _selectedItem = null;

        public string WidthFolder { get { return SCW; } }                   // franken all NaN
        public string WidthParent { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_FolderListVM.NumCols_; } }
    }
}
