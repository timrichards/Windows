namespace DoubleFile
{
    class RootNodeDatum : NodeDatum
    {
        internal LVitem_ProjectVM LVitemProjectVM { get; }
        internal readonly bool VolumeView;
        internal readonly ulong VolumeFree;
        internal readonly ulong VolumeLength;

        internal TabledString<TabledStringType_Folders>
            RootText;

        internal
            RootNodeDatum(NodeDatum node, LVitem_ProjectVM lvItemProjectVM,
            ulong nVolumeFree, ulong nVolumeLength)
            : base(node)
        {
            VolumeView = true;
            LVitemProjectVM = lvItemProjectVM;
            VolumeLength = nVolumeLength;
            VolumeFree = nVolumeFree;
            RootText = (TabledString<TabledStringType_Folders>)LVitemProjectVM.RootText;
        }
    }
}
