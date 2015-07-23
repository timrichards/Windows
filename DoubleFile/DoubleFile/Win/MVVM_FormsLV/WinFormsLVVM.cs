using System.Windows.Input;

namespace DoubleFile
{
    partial class WinFormsLVVM : LocalLVVM
    {
        public ICommand Icmd_GoTo { get; private set; }

        public LocalLVitemVM SelectedItem
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
        internal void SelectedItem_Set(LocalLVitemVM value)
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
        LocalLVitemVM _selectedItem = null;
    }
}
