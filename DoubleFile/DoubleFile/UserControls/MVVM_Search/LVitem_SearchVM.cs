using System.Collections.Generic;
using System.Text;

namespace DoubleFile
{
    class LVitem_SearchVM : ListViewItemVM_Base, IListUpdater
    {
        internal readonly LVitemProject_Updater<bool>
            LVitemProject_Updater;

        internal PathBuilder
            Directory => _datum.As<PathBuilder>();
        string
            StrDirectory =>
            (LVitemProject_Updater.ListUpdater.Value)
            ? LVitemProject_Updater.InsertNickname("" + Directory)
            : "" + Directory;

        internal readonly TabledString<TabledStringType_Files>
            TabledStringFilename;

        internal LocalTreeNode
            Folder => _datum.As<LocalTreeNode>();
        readonly object _datum = null;

        public string
            In { get; private set; } = " in ";    // interned

        public int
            Alternate { get; } = 0;

        internal override string
            ExportLine
        {
            get
            {
                var sbCopyText = new StringBuilder(Folder?.PathFullGet(LVitemProject_Updater.ListUpdater.Value) ?? StrDirectory);

                if (null != TabledStringFilename)
                {
                    sbCopyText.Append("\\");
                    sbCopyText.Append("" + TabledStringFilename);
                }

                return "" + sbCopyText;
            }
        }

        LVitem_SearchVM(LVitemProject_Updater<bool> lvItemProject_Updater)
        {
            LVitemProject_Updater = lvItemProject_Updater;
        }

        internal LVitem_SearchVM(LVitemProject_Updater<bool> lvItemProject_Updater, PathBuilder directory)
            : this(lvItemProject_Updater)
        {
            _datum = directory;
        }

        internal LVitem_SearchVM(LVitemProject_Updater<bool> lvItemProject_Updater, PathBuilder directory,
            TabledString<TabledStringType_Files> tabledFilename, int nAlternate)
            : this(lvItemProject_Updater, directory)
        {
            TabledStringFilename = tabledFilename;
            Alternate = nAlternate;
        }

        internal LVitem_SearchVM(LVitemProject_Updater<bool> lvItemProject_Updater, LocalTreeNode folder)
            : this(lvItemProject_Updater)
        {
            _datum = folder;
        }

        void 
            IListUpdater.RaiseListUpdate()
        {
            RaisePropertyChanged("FolderOrFile");
            RaisePropertyChanged("Parent");
        }

        public string
            FolderOrFile
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
                else
                {
                    if (null != TabledStringFilename)
                        return "" + TabledStringFilename;

                    strRet = StrDirectory;
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
            : (null != Folder)
            ? Folder.Parent?.PathFullGet(LVitemProject_Updater.ListUpdater.Value)
            : StrDirectory;

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;
    }
}
