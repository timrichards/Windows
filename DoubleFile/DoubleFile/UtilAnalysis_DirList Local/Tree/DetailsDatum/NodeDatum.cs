using DoubleFile;

namespace Local
{
    class NodeDatum : DetailsDatum
    {
        internal NodeDatum() { }
        internal NodeDatum(DetailsDatum datum)
            : base(datum) { }
        internal NodeDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, int nHashParity)
            : base(nPrevLineNo, nLineNo, nLength, nHashParity)
        {
        }

        internal UList<LocalTreeNode>
            Clones { get; set; }
        internal LocalLVitem 
            LVitem { get; set; }
        internal LocalTreeNode
            TreeMapFiles { get; set; }
    }
}
