namespace DoubleFile
{
    partial class LV_DoubleFile_FilesVM : ListViewVM_GenericBase<LVitem_DoubleFile_FilesVM>
    {
        public LVitem_DoubleFile_FilesVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                if (null == value)
                {
                    return;
                }

                _winDoubleFile_Duplicates.TreeFileSelChanged(value.LSduplicates, value.FileLine);
            }
        }
        LVitem_DoubleFile_FilesVM _selectedItem = null;

        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthDuplicates { get { return SCW; } }

        internal override int NumCols { get { return LVitem_DoubleFile_FilesVM.NumCols_; } }
    }
}