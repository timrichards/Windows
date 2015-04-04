using System.Windows.Input;

namespace DoubleFile
{
    partial class WinDoubleFile_DuplicatesVM : ListViewVM_GenericBase<LVitem_FileDuplicatesVM>
    {
        public ICommand Icmd_Goto { get; private set; }

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

                if (null != UpdateFileDetail)
                    UpdateFileDetail(value.FileLine, _treeNode);

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

        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthPath { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FileDuplicatesVM.NumCols_; } }
    }
}