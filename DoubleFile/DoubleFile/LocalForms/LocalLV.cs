namespace DoubleFile
{
    class LocalLV
    {
        internal LocalLVitem
            TopItem { get; set; }
        internal LocalLVitemCollection
            Items { get; set; }
        
        internal LocalLV() { Items = new LocalLVitemCollection(this); }
        internal void Invalidate() { }
    }
}
