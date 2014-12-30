using System;

namespace DoubleFile
{
    class VolumeLVitemVM : ListViewItemVM
    {
        public String VolumeName { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Path { get { return marr[1]; } set { SetProperty(1, value); } }
        public String SaveAs { get { return marr[2]; } set { SetProperty(2, value); } }
        public String Status { get { return marr[3]; } set { SetProperty(3, value); } }
        public String IncludeStr { get { return marr[4]; } set { SetProperty(4, value); } }
        public String VolumeGroup { get { return marr[5]; } set { SetProperty(5, value); } }
        readonly static String[] marrPropName = new String[] { "VolumeName", "Path", "SaveAs", "Status", "IncludeStr", "VolumeGroup" };
        internal const int NumCols_ = 6;

        internal VolumeLVitemVM(VolumeListViewVM LV, String[] arrStr)
            : base(LV, arrStr)
        {
            //        SaveAsExists = (Status == Utilities.mSTRusingFile);                 // TODO: check dup drive letter, and if drive is mounted.
        }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }

        internal bool Include { get { return (IncludeStr == "Yes"); } set { IncludeStr = (value ? "Yes" : "No"); } }

        internal bool SaveAsExists = false;                                     // TODO: set back to false when fail Tree
        //    internal SDL_TreeNode treeNode = null;
    }
}
