using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_FolderListVM : ListViewVM_Base<LVitem_FolderListVM>
    {
        public ICommand Icmd_Nicknames { get; private set; }
        public ICommand Icmd_GoTo { get; private set; }

        public Visibility NoResultsVisibility { get; private set; } = Visibility.Visible;
        public string NoResultsFolder { get; private set; } = "setting up Nearest view";

        public bool UseNicknames { private get; set; }

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

        public string WidthFolder => SCW;                   // franken all NaN
        public string WidthIn => SCW;
        public string WidthParent => SCW;

        internal override int NumCols => LVitem_FolderListVM.NumCols_;
    }
}
