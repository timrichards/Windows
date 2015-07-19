namespace DoubleFile
{
    class LocalLVVM : ListViewVM_Base<LocalLVitemVM>
    {
        internal LocalLVitemVM
            TopItem { get; set; }
        
        public string WidthText { get { return SCW; } }                     // franken all NaN
        public string WidthSubItem { get { return SCW; } }                  // franken all NaN

        internal override int NumCols { get { return LocalLVitemVM.NumCols_; } }
    }
}
