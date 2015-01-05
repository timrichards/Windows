
namespace DoubleFile
{
    class LVitem_VolumeVM : ListViewItemVM
    {
        public string VolumeName { get { return marr[0]; } set { SetProperty(0, value); } }
        public string Path { get { return marr[1]; } set { SetProperty(1, value); } }
        public string SaveAs { get { return marr[2]; } set { SetProperty(2, value); } }
        public string Status { get { return marr[3]; } set { SetProperty(3, value); } }
        public string IncludeStr { get { return marr[4]; } set { SetProperty(4, value); } }
        public string VolumeGroup { get { return marr[5]; } set { SetProperty(5, value); } }
        public string DriveModel { get { return marr[6]; } set { SetProperty(6, value); } }
        public string DriveSerial { get { return marr[7]; } set { SetProperty(7, value); } }
        readonly static string[] marrPropName = new string[] { "VolumeName", "Path", "SaveAs", "Status", "IncludeStr", "VolumeGroup", "DriveModel", "DriveSerial" };
        internal const int NumCols_ = 8;

        internal LVitem_VolumeVM(LV_VolumeVM LV, string[] arrStr)
            : base(LV, arrStr)
        {
            //        SaveAsExists = (Status == Utilities.mSTRusingFile);                 // TODO: check dup drive letter, and if drive is mounted.
        }

        internal override int NumCols { get { return NumCols_; } }
        protected override string[] PropertyNames { get { return marrPropName; } }

        internal bool Include { get { return (IncludeStr == "Yes"); } set { IncludeStr = (value ? "Yes" : "No"); } }

        internal bool CanLoad
        {
            get
            {
                return (Include &&
                    ((FileParse.mSTRusingFile + FileParse.mSTRsaved).Contains(Status)));
            }
        }

        internal bool SaveAsExists = false;                                     // TODO: set back to false when fail Tree
        //    internal SDL_TreeNode treeNode = null;
    }
}
