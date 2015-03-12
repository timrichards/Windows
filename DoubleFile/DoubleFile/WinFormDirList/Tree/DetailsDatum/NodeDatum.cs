using System.Windows.Forms;

namespace DoubleFile
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

        internal KeyList<TreeNode>
            Clones = new KeyList<TreeNode>();
        internal ListViewItem 
            LVitem { get; set; }
        internal TreeNode
            TreeMapFiles { get; set; }
    }
}
