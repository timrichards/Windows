using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    class LV_FilesVM_Base : ListViewVM_Base<LVitem_FilesVM_Base>
    {
        public virtual Visibility DupColVisibility => LVitem_FilesVM.ShowDuplicatesA ? Visibility.Visible : Visibility.Collapsed;

        public LVitem_FilesVM_Base SelectedItem
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
        internal void SelectedItem_Set(LVitem_FilesVM_Base value, decimal nInitiator)
        {
            if (value == _selectedItem)
                return;

            _selectedItem = value;
            RaisePropertyChanged("SelectedItem");
            SelectedItem_AllTriggers(nInitiator);
        }
        protected virtual void SelectedItem_AllTriggers(decimal nInitiator)
        {
        }

        protected LVitem_FilesVM_Base _selectedItem = null;

        internal override IEnumerable<LVitem_FilesVM_Base>
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

        internal override int NumCols => LVitem_FilesVM_Base.NumCols_;
    }
}
