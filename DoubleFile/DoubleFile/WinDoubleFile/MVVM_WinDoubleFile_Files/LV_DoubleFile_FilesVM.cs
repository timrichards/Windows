using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_DoubleFile_FilesVM : ListViewVM_GenericBase<LVitem_DoubleFile_FilesVM>
    {
        internal static event Action<IEnumerable<FileDictionary.DuplicateStruct>, IEnumerable<string>, LocalTreeNode> SelectedFileChanged = null;

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
            if (null != SelectedFileChanged)
            {
                if (null != _selectedItem)
                    SelectedFileChanged(_selectedItem.LSduplicates, _selectedItem.FileLine, _treeNode);
                else
                    SelectedFileChanged(null, null, null);
            }
        }
        LVitem_DoubleFile_FilesVM _selectedItem = null;

        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthDuplicates { get { return SCW; } }
        public string WidthCreated { get { return SCW; } }
        public string WidthModified { get { return SCW; } }
        public string WidthAttributes { get { return SCW; } }
        public string WidthLength { get { return SCW; } }
        public string WidthError1 { get { return SCW; } }
        public string WidthError2{ get { return SCW; } }

        internal override int NumCols { get { return LVitem_DoubleFile_FilesVM.NumCols_; } }
    }
}
