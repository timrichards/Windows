namespace DoubleFile
{
    class LocalLVVM : ListViewVM_Base<LocalLVitemVM>
    {
        public string WidthFolder => SCW;                  // franken all NaN
        public string WidthClones => SCW;                  // franken all NaN

        internal override int NumCols => LocalLVitemVM.NumCols_;

        internal LocalLVitemVM
            TopItem;
    }
}
