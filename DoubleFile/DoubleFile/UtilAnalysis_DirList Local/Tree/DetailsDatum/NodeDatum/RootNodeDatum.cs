
using DoubleFile;
namespace Local
{
    class RootNodeDatum : NodeDatum
    {
        internal UString ListingFile { get; private set; }
        internal UString VolumeGroup { get; set; }
        internal UString Root { get; private set; }

        internal UString RootPath { get; private set; }

        internal bool VolumeView = true;

        internal readonly ulong VolumeFree = 0;
        internal readonly ulong VolumeLength = 0;

        internal RootNodeDatum(NodeDatum node, string listingFile, string strVolGroup,
            ulong nVolumeFree, ulong nVolumeLength, string strRootPath)
            : base(node)
        {
            ListingFile = listingFile;
            VolumeGroup = strVolGroup;
            VolumeLength = nVolumeLength;
            VolumeFree = nVolumeFree;
            RootPath = strRootPath;
        }

        internal RootNodeDatum(NodeDatum node, RootNodeDatum rootNode)
            : base(node)
        {
            ListingFile = rootNode.ListingFile;
            VolumeGroup = rootNode.VolumeGroup;
            VolumeLength = rootNode.VolumeLength;
            VolumeFree = rootNode.VolumeFree;
        }
    }
}
