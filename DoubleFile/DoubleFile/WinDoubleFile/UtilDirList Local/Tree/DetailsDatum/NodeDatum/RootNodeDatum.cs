using DoubleFile;

namespace Local
{
    class RootNodeDatum : NodeDatum, IRootNodeDatum
    {
        internal TabledString
            ListingFile { get; private set; }
        internal TabledString
            VolumeGroup { get; set; }
        internal TabledString
            Root { get; private set; }

        internal TabledString
            RootPath { get; private set; }

        public bool
            VolumeView { get; set; }

        public ulong
            VolumeFree { get; private set; }
        public ulong
            VolumeLength { get; private set; }

        internal RootNodeDatum(NodeDatum node, string listingFile, string strVolGroup,
            ulong nVolumeFree, ulong nVolumeLength, string strRootPath)
            : base(node)
        {
            VolumeView = true;

            ListingFile = listingFile;
            VolumeGroup = strVolGroup;
            VolumeLength = nVolumeLength;
            VolumeFree = nVolumeFree;
            RootPath = strRootPath;
        }

        internal RootNodeDatum(NodeDatum node, RootNodeDatum rootNode)
            : this(node, rootNode.ListingFile, rootNode.VolumeGroup, rootNode.VolumeLength, rootNode.VolumeFree,
            rootNode.RootPath)
        {
        }
    }
}
