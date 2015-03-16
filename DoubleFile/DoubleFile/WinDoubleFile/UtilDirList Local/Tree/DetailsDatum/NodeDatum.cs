using DoubleFile;

namespace Local
{
    class NodeDatum : DetailsDatum, INodeDatum
    {
        internal NodeDatum() { }
        internal NodeDatum(DetailsDatum datum)
            : base(datum) { }
        internal NodeDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, int nHashParity)
            : base(nPrevLineNo, nLineNo, nLength, nHashParity)
        {
        }

        internal KeyList<LocalTreeNode>
            Clones { get; set; }
        internal LocalLVitem 
            LVitem { get; set; }
        public TreeNodeProxy
            TreeMapFiles { get; set; }
    }
}
