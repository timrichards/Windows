using System;
using System.Windows.Controls;

namespace SearchDirLists
{
    // In order of appearance on the form
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


    // Used for two listviewers
    class FilesLVitemVM : ListViewItemVM
    {
        public String Filename { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Created { get { return marr[1]; } set { SetProperty(1, value); } }
        public String Modified { get { return marr[2]; } set { SetProperty(2, value); } }
        public String Attributes { get { return marr[3]; } set { SetProperty(3, value); } }
        public String Length { get { return marr[4]; } set { SetProperty(4, value); } }
        public String Error1 { get { return marr[5]; } set { SetProperty(5, value); } }
        public String Error2 { get { return marr[6]; } set { SetProperty(6, value); } }

        internal new const int NumCols = 7;
        readonly new static String[] arrPropName = new String[] { "Filename", "Created", "Modified", "Attributes", "Length", "Error1", "Error2"};

        FilesLVitemVM(FilesListViewVM LV)
            : base(LV, NumCols, arrPropName) { }

        internal FilesLVitemVM(FilesListViewVM LV, String[] arrStr)
            : this(LV)
        {
            CopyInArray(arrStr);
        }
    }

    class FilesListViewVM : ListViewVM
    {
        public String WidthFilename { get { return SCW; } }
        public String WidthCreated { get { return SCW; } }
        public String WidthModified { get { return SCW; } }
        public String WidthAttributes { get { return SCW; } }
        public String WidthLength { get { return SCW; } }
        public String WidthError1 { get { return SCW; } }
        public String WidthError2 { get { return SCW; } }

        internal FilesListViewVM(ItemsControl itemsCtl) : base(itemsCtl) { }
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
