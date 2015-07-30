namespace DoubleFile
{
    class LV_ClonesVM : ListViewVM_Base<LVitem_ClonesVM>
    {
        public string WidthFolder => SCW;                   // franken all NaN
        public string WidthClones => SCW;                   // franken all NaN
        public string WidthClonePaths => SCW;               // franken all NaN

        internal override int NumCols => LVitem_ClonesVM.NumCols_;

        internal LVitem_ClonesVM
            TopItem;
    }
}
