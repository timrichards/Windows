namespace DoubleFile
{
    partial class LV_FileHash_FilesVM : ListViewVM_GenericBase<LVitem_FileHash_FilesVM>
    {
        public LVitem_FileHash_FilesVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                if (null == value)
                {
                    return;
                }

                _winFileHash_Duplicates.TreeFileSelChanged(value.LSduplicates, value.FileLine);
            }
        }
        LVitem_FileHash_FilesVM _selectedItem = null;

        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthDuplicates { get { return SCW; } }

        internal override int NumCols { get { return LVitem_FileHash_FilesVM.NumCols_; } }
    }
}