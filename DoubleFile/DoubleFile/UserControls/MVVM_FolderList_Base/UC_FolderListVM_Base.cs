using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_FolderListVM_Base : ListViewVM_Base<LVitem_FolderListVM>
    {
        public string FoldersHeader { get; private set; } = _ksFoldersHeader;
        const string _ksFoldersHeader = "Folders";
        protected void
            SetFoldersHeader(string strHeader = null) { FoldersHeader = _ksFoldersHeader + " " + strHeader; RaisePropertyChanged("FoldersHeader"); }

        public ICommand Icmd_Nicknames { get; protected set; }
        public ICommand Icmd_GoTo { get; protected set; }

        public Visibility ProgressbarVisibility { get; protected set; } = Visibility.Visible;
        public Visibility NoResultsVisibility { get; protected set; } = Visibility.Visible;
        public string NoResultsFolder { get; protected set; } = "setting up Nearest view";

        public bool UseNicknames { internal get; set; }

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
        protected LVitem_FolderListVM _selectedItem = null;

        public string WidthFolder => SCW;                   // franken all NaN
        public string WidthIn => SCW;
        public string WidthParent => SCW;

        internal override int NumCols => LVitem_FolderListVM.NumCols_;
    }
}
