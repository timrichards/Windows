using System.Windows.Input;

namespace DoubleFile
{
    partial class WinDoubleFile_SearchVM : ListViewVM_GenericBase<LVitem_DoubleFile_SearchVM>
    {
        public ICommand Icmd_Folders { get; private set; }
        public ICommand Icmd_FoldersAndFiles { get; private set; }
        public ICommand Icmd_Files { get; private set; }
        public ICommand Icmd_Goto { get; private set; }

        public string SearchText { set; private get; }

        public LVitem_DoubleFile_SearchVM SelectedItem
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
        internal void SelectedItem_Set(LVitem_DoubleFile_SearchVM value)
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
        LVitem_DoubleFile_SearchVM _selectedItem = null;

        public string WidthResults { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_DoubleFile_SearchVM.NumCols_; } }
    }
}
