
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
        readonly static string[] marrPropName = new string[] { "Nickname", "SourcePath", "ListingFileNoPath", "Status", "IncludeYN", "VolumeGroup", "DriveModel", "DriveSerial" };
        internal const int NumCols_ = 8;

        internal LVitem_ProjectVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_ProjectVM(LVitem_ProjectVM lvItemTemp)
            : base(null, lvItemTemp.StringValues)
        {
        }

        internal override int NumCols { get { return NumCols_; } }
        protected override string[] PropertyNames { get { return marrPropName; } }

        internal bool Include
        {
            get { return (IncludeYN == FileParse.ksInclude); }
            set { IncludeYN = (value ? FileParse.ksInclude : "No"); } 
        }
    }
}
