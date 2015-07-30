using System.Windows.Input;

namespace DoubleFile
{
    partial class WinClonesVM : LV_ClonesVM
    {
        public ICommand Icmd_GoTo { get; private set; }

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
    }
}
