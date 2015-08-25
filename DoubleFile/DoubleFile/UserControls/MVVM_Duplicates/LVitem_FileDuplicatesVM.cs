using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FileDuplicatesVM : ListViewItemVM_Base, IListUpdater
    {
        internal IReadOnlyList<string> FileLine;
        internal LVitemProject_Updater<bool> LVitemProject_Updater;

        public string
            Filename => FileLine[0];

        public string
            Path
        {
            get
            {
                LVitemProject_Updater.ListUpdater.LastGet(this);

                return
                    (LVitemProject_Updater.ListUpdater.Value)
                    ? LVitemProject_Updater.InsertNickname(SubItems[0])
                    : SubItems[0];
            }

            private set { SetProperty(0, value); }
        }

        void IListUpdater.RaiseListUpdate() => RaisePropertyChanged("Path");

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 1;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal LVitem_FileDuplicatesVM(IList<string> lsStr)
            : base(null, lsStr)
        {
        }
    }
}
