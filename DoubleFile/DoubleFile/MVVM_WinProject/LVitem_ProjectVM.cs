
namespace DoubleFile
{
    class LVitem_ProjectVM : ListViewItemVM_Base
    {
        public string Nickname { get { return marr[0]; } set { SetProperty(0, value); } }
        public string SourcePath { get { return marr[1]; } set { SetProperty(1, value); } }

        internal string ListingFile { get { return marr[2]; } set { SetProperty(2, value); } }
        public string ListingFileNoPath { get { return System.IO.Path.GetFileName(marr[2]); } }

        public string Status { get { return marr[3]; } set { SetProperty(3, value); } }
        public string IncludeYN { get { return marr[4]; } set { SetProperty(4, value); } }
        public string VolumeGroup { get { return marr[5]; } set { SetProperty(5, value); } }
        public string DriveModel { get { return marr[6]; } set { SetProperty(6, value); } }
        public string DriveSerial { get { return marr[7]; } set { SetProperty(7, value); } }
        public string ScannedLength { get { return UtilAnalysis_DirList.FormatSize(marr[8]); } set { SetProperty(8, value); } }
        
        protected override string[] PropertyNames { get { return marrPropName; } }
        readonly static string[] marrPropName = new string[] { "Nickname", "SourcePath", "ListingFileNoPath", "Status", "IncludeYN", "VolumeGroup", "DriveModel", "DriveSerial", "ScannedLength" };

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 9;

        internal LVitem_ProjectVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_ProjectVM(LVitem_ProjectVM lvItemTemp)
            : base(null, lvItemTemp.StringValues)
        {
        }

        internal bool Include
        {
            get { return (IncludeYN == FileParse.ksInclude); }
            set { IncludeYN = (value ? FileParse.ksInclude : "No"); } 
        }

        internal bool WouldSave
        {
            get { return (false == (_ksFileExistsCheck + FileParse.ksError).Contains(Status)); }
        }

        internal bool CanLoad
        {
            get { return (Include && _ksFileExistsCheck.Contains(Status) && (FileParse.ksError != Status)); }
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
