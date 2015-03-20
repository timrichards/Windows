using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_DoubleFile_FilesVM : ListViewVM_GenericBase<LVitem_DoubleFile_FilesVM>
    {
        internal static event Action<IEnumerable<FileDictionary.DuplicateStruct>, string> TreeFileSelChanged = null;

        public LVitem_DoubleFile_FilesVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;

                _selectedItem = value;

                if (null == value)
                    return;

                if (null != TreeFileSelChanged)
                    TreeFileSelChanged(value.LSduplicates, value.FileLine);

                SelectedItem_AllTriggers();
            }
        }
        internal void SelectedItem_Set(LVitem_DoubleFile_FilesVM value)
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
        LVitem_DoubleFile_FilesVM _selectedItem = null;

        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthDuplicates { get { return SCW; } }

        internal override int NumCols { get { return LVitem_DoubleFile_FilesVM.NumCols_; } }
    }
}
