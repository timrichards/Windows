using DoubleFile;

namespace DoubleFile
{
    class RootNodeDatum : NodeDatum
    {
        internal TabledString
            ListingFile { get; private set; }
        internal TabledString
            VolumeGroup { get; set; }
        internal TabledString
            Root { get; private set; }

        internal TabledString
            RootPath { get; private set; }

        internal bool
            VolumeView { get; set; }

        internal ulong
            VolumeFree { get; private set; }
        internal ulong
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
