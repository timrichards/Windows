using System.Windows.Input;

namespace DoubleFile
{
    partial class WinClonesVM : ListViewVM_Base<LVitem_ClonesVM>
    {
        public ICommand Icmd_Nicknames { get; private set; }
        public ICommand Icmd_GoTo { get; private set; }

        public bool UseNicknames { private get; set; }

        public LVitem_ClonesVM SelectedItem
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
        internal void SelectedItem_Set(LVitem_ClonesVM value)
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
        LVitem_ClonesVM _selectedItem = null;

        internal LVitem_ClonesVM
            TopItem;

        public string WidthFolder => SCW;                   // franken all NaN
        public string WidthClones => SCW;                   // franken all NaN
        public string WidthClonePaths => SCW;               // franken all NaN

        internal override int NumCols => LVitem_ClonesVM.NumCols_;
    }
}
