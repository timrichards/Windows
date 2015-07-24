namespace DoubleFile
{
    class RootNodeDatum : NodeDatum
    {
        internal readonly string ListingFile;
        internal readonly string VolumeGroup;

        internal readonly string RootPath;

        internal readonly bool VolumeView;

        internal readonly ulong VolumeFree;
        internal readonly ulong VolumeLength;

        internal
            RootNodeDatum(NodeDatum node, string listingFile, string strVolGroup, ulong nVolumeFree, ulong nVolumeLength, string strRootPath)
            : base(node)
        {
            VolumeView = true;

            ListingFile = listingFile;
            VolumeGroup = strVolGroup;
            VolumeLength = nVolumeLength;
            VolumeFree = nVolumeFree;
            RootPath = strRootPath;
        }
    }
}
