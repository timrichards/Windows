using System.Drawing;
using System.Windows.Forms;

namespace SearchDirLists
{
    class NodeDatum : DetailsDatum
    {
        internal readonly uint nPrevLineNo = 0;
        internal readonly uint nLineNo = 0;
        internal readonly ulong nLength = 0;

        class NodeDatumLVitemHolder     // this was a way of setting the listview item in a different node after processing the first. Not used.
        {
            internal ListViewItem m_lvItem = null;
        }

        internal Correlate Key
        {
            get
            {
                return new Correlate((ulong)nTotalLength, nFilesInSubdirs, nDirsWithFiles);
            }
        }

        internal UList<TreeNode> m_listClones = new UList<TreeNode>();

        internal void SetLVitemHolder(NodeDatum holder) { m_lvItem_ = (holder != null) ? holder.m_lvItem_ : null; }
        NodeDatumLVitemHolder m_lvItem_ = new NodeDatumLVitemHolder();
        internal ListViewItem m_lvItem
        {
            get { return (m_lvItem_ != null) ? m_lvItem_.m_lvItem : null; }
            set { if (m_lvItem_ != null) m_lvItem_.m_lvItem = value; }
        }

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
