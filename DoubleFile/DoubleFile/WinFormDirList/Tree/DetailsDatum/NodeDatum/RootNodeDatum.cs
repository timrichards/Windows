namespace DoubleFile
{
    class RootNodeDatum : NodeDatum, IRootNodeDatum
    {
        internal string
            ListingFile { get; private set; }
        internal string
            VolumeGroup { get; set; }
        internal string
            Root { get; private set; }

        internal string
            RootPath { get; private set; }

        public bool
            VolumeView { get; set; }

        public ulong
            VolumeFree { get; private set; }
        public ulong
            VolumeLength { get; private set; }

        internal RootNodeDatum(DetailsDatum node, string listingFile, string strVolGroup,
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
