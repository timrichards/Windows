using System.Collections.Generic;

namespace DoubleFile
{
    class NodeDatum : DetailsDatum
    {
        internal bool
            IsSolitary => null == Clones;
        internal bool
            IsAllOnOneVolume => UtilColorcode.AllOnOneVolume == Clones?[0].ColorcodeFG;

        internal List<LocalTreeNode>
            Clones;
        internal LVitem_ClonesVM
            LVitem;
        internal LocalTreeNode
            TreemapFiles;

        internal NodeDatum() { }
        internal NodeDatum(DetailsDatum datum)
            : base(datum) { }
    }
}
