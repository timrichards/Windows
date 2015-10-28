using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DoubleFile
{
    class UC_FolderListVM_Base : ListViewVM_Base<LVitem_FolderListVM>, IDisposable
    {
        public ICommand Icmd_Nicknames { get; protected set; }
        public ICommand Icmd_GoTo { get; protected set; }

        public bool UseNicknames { internal get; set; }

        public LVitem_FolderListVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;

                _selectedItem = value;

                if (null != _selectedItem)
                    SelectedItem_AllTriggers();
            }
        }
        internal void SelectedItem_Set(LVitem_FolderListVM value)
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
        protected LVitem_FolderListVM _selectedItem = null;

        public virtual void                         // call base.Dispose() in the derived class
            Dispose()
        {
            Util.LocalDispose(_lsDisposable);
        }

        public string WidthPathShort => SCW;        // franken all NaN
        public string WidthIn => SCW;
        public string WidthParent => SCW;

        internal override int NumCols => LVitem_FolderListVM.NumCols_;

        protected readonly ListUpdater<bool>
            _nicknameUpdater = new ListUpdater<bool>(99667);
        protected readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
