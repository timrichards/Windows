using System.Drawing;
using System.Windows.Forms;
using DoubleFile;

namespace Local
{
    class NodeDatum : DetailsDatum
    {
        internal readonly uint nPrevLineNo = 0;
        internal readonly uint nLineNo = 0;
        internal readonly ulong nLength = 0;

        internal UList<LocalTreeNode> m_listClones = new UList<LocalTreeNode>();
        internal LocalLVitem m_lvItem = null;
        internal bool m_bDifferentVols = false;

        internal Rectangle TreeMapRect = Rectangle.Empty;
        internal LocalTreeNode TreeMapFiles = null;

        internal FolderKeyStruct Key
        {
            get
            {
                return new FolderKeyStruct(nTotalLength, nFilesInSubdirs, nDirsWithFiles);
            }
        }

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
    }
}
