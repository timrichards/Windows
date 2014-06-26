﻿using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace SearchDirLists
{
    // In order of appearance on the form
    // Clones LVs are in ClonesLV_VM.cs
    // Solitary lvvm is here.
    public class CopyScratchpadLVitemVM : ListViewItemVM
    {
        public String Folders { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Path { get { return marr[1]; } set { SetProperty(1, value); } }
        readonly static String[] marrPropName = new String[] { "Folders", "Path" };
        internal const int NumCols_ = 2;

        internal CopyScratchpadLVitemVM(CopyScratchpadListViewVM lvvm, String[] arrStr)
            : base(lvvm, arrStr) { }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }

        internal SDL_TreeNode treeNode = null;
    }

    public class CopyScratchpadListViewVM : ListViewVM_Generic<CopyScratchpadLVitemVM>
    {
        public String WidthFolders { get { return SCW; } }
        public String WidthPath { get { return SCW; } }

        internal CopyScratchpadListViewVM(ListView lvfe) : base(lvfe) { }
        internal override void NewItem(String[] arrStr) { Add(new CopyScratchpadLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return CopyScratchpadLVitemVM.NumCols_; } }
    }


    public class IgnoreLVitemVM : ListViewItemVM
    {
        public String Folders { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Level { get { return marr[1]; } set { SetProperty(1, value); } }
        readonly static String[] marrPropName = new String[] { "Folders", "Level" };
        internal const int NumCols_ = 2;

        internal IgnoreLVitemVM(IgnoreListViewVM lvvm, String[] arrStr)
            : base(lvvm, arrStr) { }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
    }

    public class IgnoreListViewVM : ListViewVM_Generic<IgnoreLVitemVM>
    {
        public String WidthFolders { get { return SCW; } }
        public String WidthLevel { get { return SCW; } }

        internal IgnoreListViewVM(ListView lvfe) : base(lvfe) { }
        internal override void NewItem(String[] arrStr) { Add(new IgnoreLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return IgnoreLVitemVM.NumCols_; } }
    }


    // Used for two listviewers
    public class FilesLVitemVM : ListViewItemVM
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

        internal FilesLVitemVM(FilesListViewVM lvvm, String[] arrStr)
            : base(lvvm, arrStr) { }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
    }

    public class FilesListViewVM : ListViewVM_Generic<FilesLVitemVM>
    {
        public String WidthFilename { get { return SCW; } }
        public String WidthCreated { get { return SCW; } }
        public String WidthModified { get { return SCW; } }
        public String WidthAttributes { get { return SCW; } }
        public String WidthLength { get { return SCW; } }
        public String WidthError1 { get { return SCW; } }
        public String WidthError2 { get { return SCW; } }

        internal FilesListViewVM(ListView lvfe) : base(lvfe) { }
        internal override void NewItem(String[] arrStr) { Add(new FilesLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return FilesLVitemVM.NumCols_; } }
    }


    // Used for two listviewers
    public class DetailLVitemVM : ListViewItemVM
    {
        public String Heading { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Detail { get { return marr[1]; } set { SetProperty(1, value); } }
        readonly static String[] marrPropName = new String[] { "Heading", "Detail" };
        internal const int NumCols_ = 2;

        internal DetailLVitemVM(DetailListViewVM lvvm, String[] arrStr)
            : base(lvvm, arrStr) { }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
    }

    public class DetailListViewVM : ListViewVM_Generic<DetailLVitemVM>
    {
        public String WidthHeading { get { return SCW; } }
        public String WidthDetail { get { return SCW; } }

        internal DetailListViewVM(ListView lvfe) : base(lvfe) { }
        internal override void NewItem(String[] arrStr) { Add(new DetailLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return DetailLVitemVM.NumCols_; } }
    }


    public abstract class ClonesLVitemVM_Base : ListViewItemVM
    {
        public String Folders { get { return datum.Text; } }
#if (WPF)
        public Brush Foreground { get { return SDLWPF._ForeClrToBrush(datum.ForeColor); } }
        public Brush Background { get { return SDLWPF._BackClrToBrush(datum.BackColor); } }
        public FontWeight FontWeight { get { return datum.FontWeight; } }
#endif
        internal ClonesLVitemVM_Base(ListViewVM lvvm, SDL_ListViewItem datum_in)
            : base(lvvm, datum_in) { }
    }

    public class SolitaryLVitemVM : ClonesLVitemVM_Base
    {
        readonly static String[] marrPropName = new String[] { "Folders" };
        internal const int NumCols_ = 1;

        internal SolitaryLVitemVM(ListViewVM lvvm, SDL_ListViewItem datum_in)
            : base(lvvm, datum_in)
        {
            SDL_TreeNode treeNode = ((SDL_TreeNode)datum.Tag);

            if (treeNode == null)
            {
                return;     // marker item
            }

            treeNode.LVIVM = this;
        }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }

        protected override void ActOnDirectSelChange()
        {
            SDL_TreeNode treeNode = ((SDL_TreeNode)datum.Tag);

            if (treeNode == null)
            {
                return;     // marker item
            }

            if (Utilities.Assert(0, treeNode.TVIVM != null))
            {
                treeNode.TVIVM.SelectProgrammatic(m_bSelected);
            }
        }
    }

    public class SolitaryListViewVM : ListViewVM_Generic<SolitaryLVitemVM>
    {
        public String WidthFolders { get { return SCW; } }          // not used

        internal SolitaryListViewVM(ListView lvfe) : base(lvfe) { }
        internal override void NewItem(SDL_ListViewItem datum_in, bool bQuiet = false) { Add(new SolitaryLVitemVM(this, datum_in), bQuiet); }
        internal override int NumCols { get { return SolitaryLVitemVM.NumCols_; } }
    }
}