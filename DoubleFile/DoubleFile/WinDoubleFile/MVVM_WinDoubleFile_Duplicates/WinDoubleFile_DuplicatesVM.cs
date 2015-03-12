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
                _selectedItem = value;

                if (null == value)
                {
                    return;
                }

                _winDoubleFile_Detail.UpdateFileDetail(value.FileLine);
            }
        }
        LVitem_FileDuplicatesVM _selectedItem = null;

        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthPath { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FileDuplicatesVM.NumCols_; } }
    }
}