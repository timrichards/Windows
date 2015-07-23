using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;

namespace DoubleFile
{
    partial class LV_FilesVM : ListViewVM_Base<LVitem_FilesVM>
    {
        static internal IObservable<Tuple<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyCollection<string>, LocalTreeNode>, int>>
            SelectedFileChanged { get { return _selectedFileChanged; } }
        static readonly LocalSubject<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyCollection<string>, LocalTreeNode>> _selectedFileChanged = new LocalSubject<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyCollection<string>, LocalTreeNode>>();
        static void SelectedFileChangedOnNext(Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyCollection<string>, LocalTreeNode> value, int nInitiator)
        {
            _selectedFileChanged.LocalOnNext(value, 99852, nInitiator);
            LastSelectedFile = value;
        }
        static internal Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyCollection<string>, LocalTreeNode>
            LastSelectedFile
        {
            get { return WithLV_FilesVM(lv => lv._lastSelectedFile); }
            private set { WithLV_FilesVM(lv => lv._lastSelectedFile = value); }
        }
        static T WithLV_FilesVM<T>(Func<LV_FilesVM, T> doSomethingWith) where T : class
        {
            LV_FilesVM vm = null;

            _wr.TryGetTarget(out vm);

            return (null != vm)
                ? doSomethingWith(vm)
                : null;
        }
        Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyCollection<string>, LocalTreeNode>
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
                    (IReadOnlyCollection<string>)Array.AsReadOnly(_selectedItem.FileLine), _treeNode), nInitiator);
            else
                SelectedFileChangedOnNext(null, nInitiator);
        }
        LVitem_FilesVM _selectedItem = null;

        public string WidthFilename { get { return SCW; } }                   // franken all NaN
        public string WidthDuplicates { get { return SCW; } }
        public string WidthCreated { get { return SCW; } }
        public string WidthModified { get { return SCW; } }
        public string WidthAttributes { get { return SCW; } }
        public string WidthLength { get { return SCW; } }
        public string WidthError1 { get { return SCW; } }
        public string WidthError2{ get { return SCW; } }

        internal override int NumCols { get { return LVitem_FilesVM.NumCols_; } }
    }
}
