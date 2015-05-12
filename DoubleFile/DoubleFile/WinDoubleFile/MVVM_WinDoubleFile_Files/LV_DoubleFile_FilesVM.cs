using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Linq;

namespace DoubleFile
{
    partial class LV_DoubleFile_FilesVM : ListViewVM_Base<LVitem_DoubleFile_FilesVM>
    {
        static internal IObservable<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IEnumerable<string>, LocalTreeNode>>
            SelectedFileChanged { get { return _selectedFileChanged.AsObservable(); } }
        static readonly Subject<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IEnumerable<string>, LocalTreeNode>> _selectedFileChanged = new Subject<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IEnumerable<string>, LocalTreeNode>>();
        static readonly int _nSelectedFileChangedOnNextID = ExtensionMethodsStatic.OnNextID;

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
            if (null != _selectedItem)
                _selectedFileChanged.LocalOnNext(Tuple.Create(_selectedItem.LSduplicates, _selectedItem.FileLine.AsEnumerable(), _treeNode), _nSelectedFileChangedOnNextID);
            else
                _selectedFileChanged.LocalOnNext(null, _nSelectedFileChangedOnNextID);
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
