using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;

namespace SearchDirLists
{
    // In order of appearance on the form
    class CopyScratchpadLVitemVM : ListViewItemVM
    {
        public String Folders { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Path { get { return marr[1]; } set { SetProperty(1, value); } }
        readonly static String[] marrPropName = new String[] { "Folders", "Path" };
        internal const int NumCols_ = 2;

        internal CopyScratchpadLVitemVM(CopyScratchpadListViewVM LV, String[] arrStr)
            : base(LV, arrStr) { }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }

        internal SDL_TreeNode treeNode = null;
    }

    class CopyScratchpadListViewVM : ListViewVM_Generic<CopyScratchpadLVitemVM>
    {
        public String WidthFolders { get { return SCW; } }
        public String WidthPath { get { return SCW; } }

        internal CopyScratchpadListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new CopyScratchpadLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return CopyScratchpadLVitemVM.NumCols_; } }
    }


    class IgnoreLVitemVM : ListViewItemVM
    {
        public String Folders { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Level { get { return marr[1]; } set { SetProperty(1, value); } }
        readonly static String[] marrPropName = new String[] { "Folders", "Level" };
        internal const int NumCols_ = 2;

        internal IgnoreLVitemVM(IgnoreListViewVM LV, String[] arrStr)
            : base(LV, arrStr) { }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
    }

    class IgnoreListViewVM : ListViewVM_Generic<IgnoreLVitemVM>
    {
        public String WidthFolders { get { return SCW; } }
        public String WidthLevel { get { return SCW; } }

        internal IgnoreListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new IgnoreLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return IgnoreLVitemVM.NumCols_; } }
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
        readonly static String[] marrPropName = new String[] { "Filename", "Created", "Modified", "Attributes", "Length", "Error1", "Error2"};
        internal const int NumCols_ = 7;

        internal FilesLVitemVM(FilesListViewVM LV, String[] arrStr)
            : base(LV, arrStr) { }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
    }

    class FilesListViewVM : ListViewVM_Generic<FilesLVitemVM>
    {
        public String WidthFilename { get { return SCW; } }
        public String WidthCreated { get { return SCW; } }
        public String WidthModified { get { return SCW; } }
        public String WidthAttributes { get { return SCW; } }
        public String WidthLength { get { return SCW; } }
        public String WidthError1 { get { return SCW; } }
        public String WidthError2 { get { return SCW; } }

        internal FilesListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new FilesLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return FilesLVitemVM.NumCols_; } }
    }


    // Used for two listviewers
    class DetailLVitemVM : ListViewItemVM
    {
        public String Heading { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Detail { get { return marr[1]; } set { SetProperty(1, value); } }
        readonly static String[] marrPropName = new String[] { "Heading", "Detail" };
        internal const int NumCols_ = 2;

        internal DetailLVitemVM(DetailListViewVM LV, String[] arrStr)
            : base(LV, arrStr) { }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
    }

    class DetailListViewVM : ListViewVM_Generic<DetailLVitemVM>
    {
        public String WidthHeading { get { return SCW; } }
        public String WidthDetail { get { return SCW; } }

        internal DetailListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new DetailLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return DetailLVitemVM.NumCols_; } }
    }


    abstract class ClonesLVitemVM_Base : ListViewItemVM
    {
        public String Folders { get { return datum.Text; } }

        public Brush Foreground { get { return SDLWPF._ClrToBrush(datum.ForeColor); } }
        public Brush Background { get { return SDLWPF._ClrToBrush(datum.BackColor); } }
        public FontStyle Fontstyle { get { return ((datum.Font != null) && (datum.Font.Style == System.Drawing.FontStyle.Bold)) ? FontStyles.Oblique : FontStyles.Normal;} }
        internal ClonesLVitemVM_Base(ListViewVM LV, SDL_ListViewItem datum_in)
            : base(LV, datum_in) { }
    }

    class SolitaryLVitemVM : ClonesLVitemVM_Base
    {
        readonly static String[] marrPropName = new String[] { "Folders" };
        internal const int NumCols_ = 1;

        internal SolitaryLVitemVM(ListViewVM LV, SDL_ListViewItem datum_in)
            : base(LV, datum_in) { }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
    }

    class SolitaryListViewVM : ListViewVM_Generic<SolitaryLVitemVM>
    {
        public String WidthFolders { get { return SCW; } }

        internal SolitaryListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(SDL_ListViewItem datum_in, bool bQuiet = false) { Add(new SolitaryLVitemVM(this, datum_in), bQuiet); }
        internal override int NumCols { get { return SolitaryLVitemVM.NumCols_; } }
    }


    // Used for two listviewers
    class ClonesLVitemVM : ClonesLVitemVM_Base
    {
        public String Occurrences { get { return datum.SubItems[1].Text; } }
        readonly static String[] marrPropName = new String[] { "Folders", "Occurrences" };
        internal const int NumCols_ = 2;

        internal ClonesLVitemVM(ListViewVM LV, SDL_ListViewItem datum_in)
            : base(LV, datum_in) { }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
    }

    class ClonesListViewVM : ListViewVM_Generic<ClonesLVitemVM>
    {
        public String WidthFolders { get { return SCW; } }
        public String WidthOccurrences { get { return SCW; } }

        internal ClonesListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(SDL_ListViewItem datum_in, bool bQuiet = false) { Add(new ClonesLVitemVM(this, datum_in), bQuiet); }
        internal override int NumCols { get { return ClonesLVitemVM.NumCols_; } }
    }
}
