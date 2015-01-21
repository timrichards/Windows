using System.Drawing;
using System.Windows.Forms;

namespace DoubleFile
{
    class NodeDatum : DetailsDatum
    {
        internal readonly uint nPrevLineNo = 0;
        internal readonly uint nLineNo = 0;
        internal readonly ulong nLength = 0;

        internal Correlate Key
        {
            get
            {
                return new Correlate((ulong)nTotalLength, nFilesInSubdirs, nDirsWithFiles);
            }
        }

        internal UList<TreeNode> m_listClones = new UList<TreeNode>();

        internal bool m_bDifferentVols = false;

        internal NodeDatum() { }
        internal NodeDatum(uint nPrevLineNo_in, uint nLineNo_in, ulong nLength_in)
        { nPrevLineNo = nPrevLineNo_in; nLineNo = nLineNo_in; nLength = nLength_in; }

        protected NodeDatum(NodeDatum node)
            : base(node)
        {
            nPrevLineNo = node.nPrevLineNo;
            nLineNo = node.nLineNo;
            nLength = node.nLength;
        }

        internal Rectangle TreeMapRect = Rectangle.Empty;
        internal TreeNode TreeMapFiles = null;
    }

}
