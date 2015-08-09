using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_FilesVM : ListViewVM_Base<LVitem_FilesVM>
    {
        static internal IObservable<Tuple<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyList<string>, LocalTreeNode>, int>>
            SelectedFileChanged => _selectedFileChanged;
        static readonly LocalSubject<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyList<string>, LocalTreeNode>> _selectedFileChanged = new LocalSubject<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyList<string>, LocalTreeNode>>();
        static void SelectedFileChangedOnNext(Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyList<string>, LocalTreeNode> value, int nInitiator)
        {
            _selectedFileChanged.LocalOnNext(value, 99852, nInitiator);
            LastSelectedFile = value;
        }
        static internal Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyList<string>, LocalTreeNode>
            LastSelectedFile
        {
            get { return _wr.Get(lv => lv._lastSelectedFile); }
            private set { _wr.Get(lv => lv._lastSelectedFile = value); }
        }
        Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyList<string>, LocalTreeNode>
            _lastSelectedFile = null;

        public LVitem_FilesVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;

                _selectedItem = value;

                if (null != _selectedItem)
                    SelectedItem_AllTriggers(0);
            }
        }
        internal void SelectedItem_Set(LVitem_FilesVM value, int nInitiator)
        {
            if (value == _selectedItem)
                return;

            _selectedItem = value;
            RaisePropertyChanged("SelectedItem");
            SelectedItem_AllTriggers(nInitiator);
        }
        void SelectedItem_AllTriggers(int nInitiator)
        {
            if (null != _selectedItem)
                SelectedFileChangedOnNext(Tuple.Create(_selectedItem.LSduplicates, 
                    (IReadOnlyList<string>)_selectedItem.FileLine, _treeNode), nInitiator);
            else
                SelectedFileChangedOnNext(null, nInitiator);
        }
        LVitem_FilesVM _selectedItem = null;

        public string WidthFilename => SCW;                  // franken all NaN
        public string WidthDuplicates => SCW;
        public string WidthCreated => SCW;
        public string WidthModified => SCW;
        public string WidthAttributes => SCW;
        public string WidthLength => SCW;
        public string WidthError1 => SCW;
        public string WidthError2 => SCW;

        internal override int NumCols => LVitem_FilesVM.NumCols_;
    }
}
