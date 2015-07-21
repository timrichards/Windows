namespace DoubleFile
{
    class LocalLVVM : ListViewVM_Base<LocalLVitemVM>
    {
        internal LocalLVitemVM
            TopItem { get; set; }
        
        public string WidthFolder { get { return SCW; } }                   // franken all NaN
        public string WidthClones { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LocalLVitemVM.NumCols_; } }
    }
}
