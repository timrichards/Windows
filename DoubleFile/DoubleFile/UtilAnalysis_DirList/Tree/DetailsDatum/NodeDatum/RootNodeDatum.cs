
namespace SearchDirLists
{
    class RootNodeDatum : NodeDatum
    {
        internal string StrVolumeGroup = null;
        internal bool VolumeView = true;

        internal readonly string StrFile = null;
        internal readonly ulong VolumeFree = 0;
        internal readonly ulong VolumeLength = 0;

        internal RootNodeDatum(NodeDatum node, string strFile_in, string strVolGroup_in,
            ulong nVolumeFree_in, ulong nVolumeLength_in)
            : base(node)
        {
            StrFile = strFile_in;
            StrVolumeGroup = strVolGroup_in;
            VolumeLength = nVolumeLength_in;
            VolumeFree = nVolumeFree_in;
        }

        internal RootNodeDatum(NodeDatum node, RootNodeDatum rootNode)
            : base(node)
        {
            StrFile = rootNode.StrFile;
            StrVolumeGroup = rootNode.StrVolumeGroup;
            VolumeLength = rootNode.VolumeLength;
            VolumeFree = rootNode.VolumeFree;
        }
    }
}
