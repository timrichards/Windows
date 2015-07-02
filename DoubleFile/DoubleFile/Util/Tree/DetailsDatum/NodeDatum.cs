using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class NodeDatum : DetailsDatum
    {
        internal NodeDatum() { }
        internal NodeDatum(DetailsDatum datum)
            : base(datum) { }
        internal NodeDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, uint[] folderScore)
            : base(nPrevLineNo, nLineNo, nLength, folderScore)
        {
        }

        internal List<LocalTreeNode>
            Clones { get; set; }
        internal LocalLVitem 
            LVitem { get; set; }
        internal LocalTreeNode
            TreeMapFiles { get; set; }
    }
}
