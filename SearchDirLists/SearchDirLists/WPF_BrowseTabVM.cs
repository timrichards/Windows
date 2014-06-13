using System;
using System.Windows.Controls;

namespace SearchDirLists
{
    class CopyScratchpadLVitemVM : ListViewItemVM
    {
        public String Folder { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Path { get { return marr[1]; } set { SetProperty(1, value); } }

        internal new const int NumCols = 2;
        readonly new static String[] arrPropName = new String[] { "Folder", "Path" };

        CopyScratchpadLVitemVM(CopyScratchpadListViewVM LV)
            : base(LV, NumCols, arrPropName) { }

        internal CopyScratchpadLVitemVM(CopyScratchpadListViewVM LV, String[] arrStr)
            : this(LV)
        {
            CopyInArray(arrStr);
        }
    }

    class CopyScratchpadListViewVM : ListViewVM
    {
        public String WidthFolder { get { return SCW; } }
        public String WidthPath { get { return SCW; } }

        internal CopyScratchpadListViewVM(ItemsControl itemsCtl) : base(itemsCtl) { }
    }


    // Used for two listviewers
    class DetailLVitemVM : ListViewItemVM
    {
        public String Heading { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Detail { get { return marr[1]; } set { SetProperty(1, value); } }

        internal new const int NumCols = 2;
        readonly new static String[] arrPropName = new String[] { "Heading", "Detail" };

        DetailLVitemVM(DetailListViewVM LV)
            : base(LV, NumCols, arrPropName) { }

        internal DetailLVitemVM(DetailListViewVM LV, String[] arrStr)
            : this(LV)
        {
            CopyInArray(arrStr);
        }
    }

    class DetailListViewVM : ListViewVM
    {
        public String WidthHeading { get { return SCW; } }
        public String WidthDetail { get { return SCW; } }

        internal DetailListViewVM(ItemsControl itemsCtl) : base(itemsCtl) { }
    }


    // Used for two listviewers
    class FileLVitemVM : ListViewItemVM
    {
        public String ColumnNameHere { get { return marr[0]; } set { SetProperty(0, value); } }

        internal new const int NumCols = 0;
        readonly new static String[] arrPropName = new String[] { };

        FileLVitemVM(FileListViewVM LV)
            : base(LV, NumCols, arrPropName) { }

        internal FileLVitemVM(FileListViewVM LV, String[] arrStr)
            : this(LV)
        {
            CopyInArray(arrStr);
        }
    }

    class FileListViewVM : ListViewVM
    {
        public String WidthColumnNameHere { get { return SCW; } }

        internal FileListViewVM(ItemsControl itemsCtl) : base(itemsCtl) { }
    }


    class IgnoreLVitemVM : ListViewItemVM
    {
        public String Folder { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Level { get { return marr[1]; } set { SetProperty(1, value); } }

        internal new const int NumCols = 2;
        readonly new static String[] arrPropName = new String[] { "Folder", "Level" };

        IgnoreLVitemVM(IgnoreListViewVM LV)
            : base(LV, NumCols, arrPropName) { }

        internal IgnoreLVitemVM(IgnoreListViewVM LV, String[] arrStr)
            : this(LV)
        {
            CopyInArray(arrStr);
        }
    }

    class IgnoreListViewVM : ListViewVM
    {
        public String WidthFolder { get { return SCW; } }
        public String WidthLevel { get { return SCW; } }

        internal IgnoreListViewVM(ItemsControl itemsCtl) : base(itemsCtl) { }
    }


    class SolitaryLVitemVM : ListViewItemVM
    {
        public String Folder { get { return marr[0]; } set { SetProperty(0, value); } }

        internal new const int NumCols = 1;
        readonly new static String[] arrPropName = new String[] { "Folder" };

        SolitaryLVitemVM(SolitaryListViewVM LV)
            : base(LV, NumCols, arrPropName) { }

        internal SolitaryLVitemVM(SolitaryListViewVM LV, String[] arrStr)
            : this(LV)
        {
            CopyInArray(arrStr);
        }
    }

    class SolitaryListViewVM : ListViewVM
    {
        public String WidthFolder { get { return SCW; } }

        internal SolitaryListViewVM(ItemsControl itemsCtl) : base(itemsCtl) { }
    }


    // Used for two listviewers
    class ClonesLVitemVM : ListViewItemVM
    {
        public String Folder { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Occurrences { get { return marr[1]; } set { SetProperty(1, value); } }

        internal new const int NumCols = 2;
        readonly new static String[] arrPropName = new String[] { "Folder", "Occurrences" };

        ClonesLVitemVM(ClonesListViewVM LV)
            : base(LV, NumCols, arrPropName) { }

        internal ClonesLVitemVM(ClonesListViewVM LV, String[] arrStr)
            : this(LV)
        {
            CopyInArray(arrStr);
        }
    }

    class ClonesListViewVM : ListViewVM
    {
        public String WidthFolder { get { return SCW; } }
        public String WidthOccurrences { get { return SCW; } }

        internal ClonesListViewVM(ItemsControl itemsCtl) : base(itemsCtl) { }
    }
}
