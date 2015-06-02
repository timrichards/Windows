using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_ProjectVM : ListViewItemVM_Base
    {
        public string Nickname { get { return marr[0]; } internal set { SetProperty(0, value); } }
        public string SourcePath { get { return marr[1]; } internal set { SetProperty(1, value); } }

        internal string ListingFile { get { return marr[2]; } set { SetProperty(2, value); } }
        public string ListingFileNoPath { get { return System.IO.Path.GetFileName(marr[2]); } }

        public string Status { get { return marr[3]; } internal set { SetProperty(3, value); } }
        public string IncludeYN { get { return marr[4]; } private set { SetProperty(4, value); } }
        public string VolumeGroup { get { return marr[5]; } internal set { SetProperty(5, value); } }
        public string DriveModel { get { return marr[6]; } internal set { SetProperty(6, value); } }
        public string DriveSerial { get { return marr[7]; } internal set { SetProperty(7, value); } }
        public string ScannedLength { get { return Util.FormatSize(marr[8]); } internal set { SetProperty(8, value); } }
        public ulong ScannedLengthRaw { get { return ulong.Parse(marr[8]); } }

        protected override string[] PropertyNames { get { return new[] { "Nickname", "SourcePath", "ListingFileNoPath", "Status", "IncludeYN", "VolumeGroup", "DriveModel", "DriveSerial", "ScannedLength" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 9;

        internal LVitem_ProjectVM(IEnumerable<string> ieString = null)
            : base(null, ieString)
        {
            if (null == Status)
                Status = "" + App.Current.Resources["Status_NotSaved"];
        }

        internal LVitem_ProjectVM(LVitem_ProjectVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }

        internal bool Include
        {
            get { return (IncludeYN == FileParse.ksInclude); }
            set { IncludeYN = (value ? FileParse.ksInclude : "" + App.Current.Resources["Include_No"]); } 
        }

        internal bool WouldSave
        {
            get { return (false == (_ksFileExistsCheck + FileParse.ksError).Contains(Status)); }
        }

        internal bool CanLoad
        {
            get { return (Include && _ksFileExistsCheck.Contains(Status) && (FileParse.ksError != Status)); }
        }

        internal string Volume
        {
            get { return string.IsNullOrWhiteSpace(VolumeGroup) ? SourcePath : VolumeGroup; }
        }

        internal void SetSaved()
        {
            LVitem_ProjectVM lvItem = null;

            if (FileParse.ReadHeader(ListingFile, out lvItem))
            {
                StringValues = lvItem.StringValues;
                Status = FileParse.ksSaved;
            }
            else
            {
                Status = FileParse.ksError;
            }
        }

        const string _ksFileExistsCheck = FileParse.ksUsingFile + FileParse.ksSaved;
    }
}
