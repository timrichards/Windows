using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_FilesVM : ListViewVM_Base<LVitem_FilesVM>
    {
        internal class SelectedFileChanged
        {
            internal readonly IReadOnlyList<Tuple<LVitemProject_Updater<bool>, IReadOnlyList<string>>> lsDupDirFileLines;
            internal readonly IReadOnlyList<string> fileLine;
            internal readonly LocalTreeNode treeNode;

            internal SelectedFileChanged(IReadOnlyList<Tuple<LVitemProject_Updater<bool>, IReadOnlyList<string>>> lsDupDirFileLines_, IReadOnlyList<string> fileLine_, LocalTreeNode treeNode_)
            {
                lsDupDirFileLines = lsDupDirFileLines_;
                fileLine = fileLine_;
                treeNode = treeNode_;
            }

            static internal readonly IObservable<Tuple<SelectedFileChanged, int>>
                Observable = new LocalSubject<SelectedFileChanged>();
        }
        static void
            SelectedFileChangedOnNext(SelectedFileChanged value, int nInitiator)
        {
            ((LocalSubject<SelectedFileChanged>)SelectedFileChanged.Observable).LocalOnNext(value, 99852, nInitiator);
            LastSelectedFile = value;
        }
        static internal SelectedFileChanged
            LastSelectedFile
        {
            get { return _wr.Get(lv => lv._lastSelectedFile); }
            private set { _wr.Get(lv => lv._lastSelectedFile = value); }
        }
        SelectedFileChanged _lastSelectedFile = null;

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
                ShowDuplicates_SelectedFileChangedOnNext(nInitiator);
            else
                SelectedFileChangedOnNext(null, nInitiator);
        }
        LVitem_FilesVM _selectedItem = null;

        public string WidthFilename => SCW;                     // franken all NaN
        public string WidthDuplicates => SCW;
        public string WidthCreated => SCW;
        public string WidthModified => SCW;
        public string WidthAttributes => SCW;
        public string WidthLength => SCW;
        public string WidthError1 => SCW;
        public string WidthError2 => SCW;

        public string WidthDuplicate => SCW;
        public string WidthIn => SCW;
        public string WidthParent => SCW;

        internal override int NumCols => LVitem_FilesVM.NumCols_;
    }
}
