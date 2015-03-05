
namespace Local
{
    class RootNodeDatum : NodeDatum
    {
        internal string ListingFile { get; private set; }
        internal string VolumeGroup { get; set; }
        internal string Root { get; set; }

        internal bool VolumeView = true;

        internal readonly ulong VolumeFree = 0;
        internal readonly ulong VolumeLength = 0;

        internal RootNodeDatum(NodeDatum node, string listingFile, string strVolGroup_in,
            ulong nVolumeFree_in, ulong nVolumeLength_in)
            : base(node)
        {
            ListingFile = listingFile;
            VolumeGroup = strVolGroup_in;
            VolumeLength = nVolumeLength_in;
            VolumeFree = nVolumeFree_in;
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
