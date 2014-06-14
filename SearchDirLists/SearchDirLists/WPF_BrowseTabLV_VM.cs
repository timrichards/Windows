﻿using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;

namespace SearchDirLists
{
    // In order of appearance on the form
    class CopyScratchpadLVitemVM : ListViewItemVM
    {
        public String Folders { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Path { get { return marr[1]; } set { SetProperty(1, value); } }

        readonly static String[] marrPropName = new String[] { "Folders", "Path" };

        internal CopyScratchpadLVitemVM(CopyScratchpadListViewVM LV, String[] arrStr)
            : base(LV)
        {
            mPropChanged_LV = LV.RaisePropertyChanged;
            CopyInArray(arrStr);
        }

        internal const int NumCols_ = 2;
        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
        protected override RaisePropertyChangedDelegate LV_RaisePropertyChanged { get { return mPropChanged_LV; } } RaisePropertyChangedDelegate mPropChanged_LV = null;
        protected override int SearchCol { get { return 0; } }
    }

    class CopyScratchpadListViewVM : ListViewVM
    {
        public String WidthFolders { get { return SCW; } }
        public String WidthPath { get { return SCW; } }

        internal CopyScratchpadListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new CopyScratchpadLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return CopyScratchpadLVitemVM.NumCols_; } }

        internal IEnumerable<CopyScratchpadLVitemVM> ItemsCast { get { return m_items.Cast<CopyScratchpadLVitemVM>(); } }
        internal IEnumerable<CopyScratchpadLVitemVM> Selected { get { return m_lv.SelectedItems.Cast<CopyScratchpadLVitemVM>(); } }
    }


    class IgnoreLVitemVM : ListViewItemVM
    {
        public String Folders { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Level { get { return marr[1]; } set { SetProperty(1, value); } }

        readonly static String[] marrPropName = new String[] { "Folders", "Level" };

        internal IgnoreLVitemVM(IgnoreListViewVM LV, String[] arrStr)
            : base(LV)
        {
            mPropChanged_LV = LV.RaisePropertyChanged;
            CopyInArray(arrStr);
        }

        internal const int NumCols_ = 2;
        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
        protected override RaisePropertyChangedDelegate LV_RaisePropertyChanged { get { return mPropChanged_LV; } } RaisePropertyChangedDelegate mPropChanged_LV = null;
        protected override int SearchCol { get { return 0; } }
    }

    class IgnoreListViewVM : ListViewVM
    {
        public String WidthFolders { get { return SCW; } }
        public String WidthLevel { get { return SCW; } }

        internal IgnoreListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new IgnoreLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return IgnoreLVitemVM.NumCols_; } }

        internal IEnumerable<IgnoreLVitemVM> ItemsCast { get { return m_items.Cast<IgnoreLVitemVM>(); } }
        internal IEnumerable<IgnoreLVitemVM> Selected { get { return m_lv.SelectedItems.Cast<IgnoreLVitemVM>(); } }
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

        internal FilesLVitemVM(FilesListViewVM LV, String[] arrStr)
            : base(LV)
        {
            mPropChanged_LV = LV.RaisePropertyChanged;
            CopyInArray(arrStr);
        }

        internal const int NumCols_ = 7;
        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
        protected override RaisePropertyChangedDelegate LV_RaisePropertyChanged { get { return mPropChanged_LV; } } RaisePropertyChangedDelegate mPropChanged_LV = null;
        protected override int SearchCol { get { return 0; } }
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

        internal FilesListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new FilesLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return FilesLVitemVM.NumCols_; } }

        internal IEnumerable<FilesLVitemVM> ItemsCast { get { return m_items.Cast<FilesLVitemVM>(); } }
        internal IEnumerable<FilesLVitemVM> Selected { get { return m_lv.SelectedItems.Cast<FilesLVitemVM>(); } }
    }


    // Used for two listviewers
    class DetailLVitemVM : ListViewItemVM
    {
        public String Heading { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Detail { get { return marr[1]; } set { SetProperty(1, value); } }

        readonly static String[] marrPropName = new String[] { "Heading", "Detail" };

        internal DetailLVitemVM(DetailListViewVM LV, String[] arrStr)
            : base(LV)
        {
            mPropChanged_LV = LV.RaisePropertyChanged;
            CopyInArray(arrStr);
        }

        internal const int NumCols_ = 2;
        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
        protected override RaisePropertyChangedDelegate LV_RaisePropertyChanged { get { return mPropChanged_LV; } } RaisePropertyChangedDelegate mPropChanged_LV = null;
        protected override int SearchCol { get { return 0; } }
    }

    class DetailListViewVM : ListViewVM
    {
        public String WidthHeading { get { return SCW; } }
        public String WidthDetail { get { return SCW; } }

        internal DetailListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new DetailLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return DetailLVitemVM.NumCols_; } }

        internal IEnumerable<DetailLVitemVM> ItemsCast { get { return m_items.Cast<DetailLVitemVM>(); } }
        internal IEnumerable<DetailLVitemVM> Selected { get { return m_lv.SelectedItems.Cast<DetailLVitemVM>(); } }
    }


    class SolitaryLVitemVM : ListViewItemVM
    {
        public String Folders { get { return marr[0]; } set { SetProperty(0, value); } }

        readonly static String[] marrPropName = new String[] { "Folders" };

        internal SolitaryLVitemVM(SolitaryListViewVM LV, String[] arrStr)
            : base(LV)
        {
            mPropChanged_LV = LV.RaisePropertyChanged;
            CopyInArray(arrStr);
        }

        internal const int NumCols_ = 1;
        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
        protected override RaisePropertyChangedDelegate LV_RaisePropertyChanged { get { return mPropChanged_LV; } } RaisePropertyChangedDelegate mPropChanged_LV = null;
        protected override int SearchCol { get { return 0; } }
    }

    class SolitaryListViewVM : ListViewVM
    {
        public String WidthFolders { get { return SCW; } }

        internal SolitaryListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new SolitaryLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return SolitaryLVitemVM.NumCols_; } }

        internal IEnumerable<SolitaryLVitemVM> ItemsCast { get { return m_items.Cast<SolitaryLVitemVM>(); } }
        internal IEnumerable<SolitaryLVitemVM> Selected { get { return m_lv.SelectedItems.Cast<SolitaryLVitemVM>(); } }
    }


    // Used for two listviewers
    class ClonesLVitemVM : ListViewItemVM
    {
        public String Folders { get { return marr[0]; } set { SetProperty(0, value); } }
        public String Occurrences { get { return marr[1]; } set { SetProperty(1, value); } }

        readonly static String[] marrPropName = new String[] { "Folders", "Occurrences" };

        internal ClonesLVitemVM(ClonesListViewVM LV, String[] arrStr)
            : base(LV)
        {
            mPropChanged_LV = LV.RaisePropertyChanged;
            CopyInArray(arrStr);
        }

        internal const int NumCols_ = 2;
        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }
        protected override RaisePropertyChangedDelegate LV_RaisePropertyChanged { get { return mPropChanged_LV; } } RaisePropertyChangedDelegate mPropChanged_LV = null;
        protected override int SearchCol { get { return 0; } }
    }

    class ClonesListViewVM : ListViewVM
    {
        public String WidthFolders { get { return SCW; } }
        public String WidthOccurrences { get { return SCW; } }

        internal ClonesListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(String[] arrStr) { Add(new ClonesLVitemVM(this, arrStr)); }
        internal override int NumCols { get { return ClonesLVitemVM.NumCols_; } }

        internal IEnumerable<ClonesLVitemVM> ItemsCast { get { return m_items.Cast<ClonesLVitemVM>(); } }
        internal IEnumerable<ClonesLVitemVM> Selected { get { return m_lv.SelectedItems.Cast<ClonesLVitemVM>(); } }
    }
}
