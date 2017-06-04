using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_SinksVM : ListViewItemVM_Base, IListUpdater
    {
        internal readonly LVitemProject_Updater<bool>
            LVitemProject_Updater;

        internal LocalTreeNode
            Folder = null;

        public string
            In { get; private set; } = " in ";    // interned

        public int
            Alternate { get; } = 0;

        internal override string
            ExportLine => "" + Folder?.PathFullGet(LVitemProject_Updater.ListUpdater.Value);

        LVitem_SinksVM(LVitemProject_Updater<bool> lvItemProject_Updater)
        {
            LVitemProject_Updater = lvItemProject_Updater;
        }

        internal LVitem_SinksVM(LVitemProject_Updater<bool> lvItemProject_Updater, LocalTreeNode folder)
            : this(lvItemProject_Updater)
        {
            Folder = folder;
        }

        void 
            IListUpdater.RaiseListUpdate()
        {
            RaisePropertyChanged("FolderString");
            RaisePropertyChanged("Parent");
        }

        public string
            FolderString
        {
            get
            {
                LVitemProject_Updater.ListUpdater.LastGet(this);

                string strRet = null;

                if (null != Folder)
                {
                    var parent = Folder.Parent;

                    if (null != parent)
                        return Folder.PathShort;

                    strRet = Folder.PathFullGet(LVitemProject_Updater.ListUpdater.Value);
                }

                In = null;
                RaisePropertyChanged("In");
                RaisePropertyChanged("Parent");
                return strRet;
            }
        }

        public string
            Parent =>
            (null == In)
            ? null
            : Folder?.Parent?.PathFullGet(LVitemProject_Updater.ListUpdater.Value);

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;
    }
}
