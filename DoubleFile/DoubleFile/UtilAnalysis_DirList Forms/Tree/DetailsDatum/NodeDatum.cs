using System.Drawing;
using System.Windows.Forms;

namespace DoubleFile
{
    class NodeDatum : DetailsDatum
    {
        internal uint
            PrevLineNo { get; private set; }
        internal uint
            LineNo { get; private set; }
        internal ulong
            Length { get; private set; }

        internal UList<TreeNode>
            Clones = new UList<TreeNode>();
        internal ListViewItem 
            LVitem { get; set; }
        internal bool 
            SeparateVols { get; set; }

        internal Rectangle
            TreeMapRect { get; set; }
        internal TreeNode
            TreeMapFiles { get; set; }

        internal FolderKeyStruct Key
        {
            get
            {
                return new FolderKeyStruct(TotalLength, FilesInSubdirs, DirsWithFiles, HashParity);
            }
        }

        internal NodeDatum() { }
        internal NodeDatum(uint nPrevLineNo, uint nLineNo, ulong nLength)
        {
            PrevLineNo = nPrevLineNo;
            LineNo = nLineNo;
            Length = nLength;
        }

        protected NodeDatum(NodeDatum node)
            : base(node)
        {
            PrevLineNo = node.PrevLineNo;
            LineNo = node.LineNo;
            Length = node.Length;
        }
    }
}
