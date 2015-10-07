using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    class LV_FilesVM_Base : ListViewVM_Base<LVitem_FilesVM>
    {
        public virtual Visibility DupColVisibility => LVitem_FilesVM.ShowDuplicates ? Visibility.Visible : Visibility.Collapsed;

        internal LV_FilesVM_Base()
        {
            _wr.SetTarget(this);
        }

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

            static internal readonly IObservable<Tuple<SelectedFileChanged, decimal>>
                Observable = new LocalSubject<SelectedFileChanged>();
        }
        static protected void
            SelectedFileChangedOnNext(SelectedFileChanged value, decimal nInitiator)
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
        protected SelectedFileChanged _lastSelectedFile = null;

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
        internal void SelectedItem_Set(LVitem_FilesVM value, decimal nInitiator)
        {
            if (value == _selectedItem)
                return;

            _selectedItem = value;
            RaisePropertyChanged("SelectedItem");
            SelectedItem_AllTriggers(nInitiator);
        }
        void SelectedItem_AllTriggers(decimal nInitiator)
        {
            if (null != _selectedItem)
                ShowDuplicates_SelectedFileChangedOnNext(nInitiator);
            else
                SelectedFileChangedOnNext(null, nInitiator);
        }
        protected virtual void ShowDuplicates_SelectedFileChangedOnNext(decimal nInitiator) { }
        protected LVitem_FilesVM _selectedItem = null;

        internal override IEnumerable<LVitem_FilesVM>
            this[string s_in]
        {
            get
            {
                if (null == s_in)
                    return null;
            
                var s = s_in.ToLower();

                return ItemsCast.Where(o => ("" + o.Filename).ToLower().Equals(s));
            }
        }

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

        static readonly WeakReference<LV_FilesVM_Base>
            _wr = new WeakReference<LV_FilesVM_Base>(null);
    }
}
