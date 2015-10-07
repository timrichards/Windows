using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_SearchVM : ListViewVM_Base<LVitem_SearchVM>
    {
        public ICommand Icmd_Folders { get; private set; }
        public ICommand Icmd_FoldersAndFiles { get; private set; }
        public ICommand Icmd_Files { get; private set; }
        public ICommand Icmd_Nicknames { get; private set; }
        public ICommand Icmd_GoTo { get; private set; }

        public string SearchText { get; set; }
        public bool Regex { get; set; }
        public bool UseNicknames { get; set; }

        public Visibility NoResultsVisibility { get; private set; } = Visibility.Visible;
        public string NoResultsText { get; private set; } = null;

        public LVitem_SearchVM SelectedItem
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

        public string WidthFolderOrFile => SCW;             // franken all NaN
        public string WidthIn => SCW;
        public string WidthParent => SCW;

        internal override int NumCols => LVitem_SearchVM.NumCols_;
    }
}
