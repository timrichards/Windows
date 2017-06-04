using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_SinksVM : ListViewVM_Base<LVitem_SinksVM>
    {
        public ICommand Icmd_GoTo { get; private set; }

        public Visibility NoItemsVisibility { get; private set; } = Visibility.Visible;
        public string NoItemsText { get; private set; } = null;

        public LVitem_SinksVM SelectedItem
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
        internal void SelectedItem_Set(LVitem_SinksVM value)
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
        LVitem_SinksVM _selectedItem = null;

        public string WidthFolderOrFile => SCW;             // franken all NaN
        public string WidthIn => SCW;
        public string WidthParent => SCW;

        internal override int NumCols => LVitem_SinksVM.NumCols_;
    }
}
