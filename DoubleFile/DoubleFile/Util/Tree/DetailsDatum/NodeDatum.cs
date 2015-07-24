using System.Collections.Generic;

namespace DoubleFile
{
    class NodeDatum : DetailsDatum
    {
        internal List<LocalTreeNode>
            Clones;
        internal LocalLVitemVM
            LVitem;
        internal LocalTreeNode
            TreeMapFiles;

        internal NodeDatum() { }
        internal NodeDatum(DetailsDatum datum)
            : base(datum) { }
    }
}
