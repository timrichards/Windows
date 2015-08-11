namespace DoubleFile
{
    class RootNodeDatum : NodeDatum
    {
        internal LVitem_ProjectExplorer LVitemProjectVM { get; }
        internal readonly bool VolumeView;
        internal readonly ulong VolumeFree;
        internal readonly ulong VolumeLength;

        internal TabledString<TabledStringType_Folders>
            CulledPath;

        internal                                            // CulledPath
            RootNodeDatum(NodeDatum node)                   // using this
            : base(node)                                    // constructor
        {
        }

        internal
            RootNodeDatum(NodeDatum node, LVitem_ProjectExplorer lvItemProjectVM, ulong nVolumeFree, ulong nVolumeLength)
            : base(node)
        {
            VolumeView = true;
            LVitemProjectVM = lvItemProjectVM;
            VolumeLength = nVolumeLength;
            VolumeFree = nVolumeFree;
        }
    }
}
