namespace DoubleFile
{
    class LocalLV
    {
        internal LocalLVitem
            TopItem { get; set; }
        internal LocalLVitem[]
            Items { get; set; }
        
        internal void Invalidate() { }
    }
}
