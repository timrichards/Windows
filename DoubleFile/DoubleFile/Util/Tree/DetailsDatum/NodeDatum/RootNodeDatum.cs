namespace DoubleFile
{
    class RootNodeDatum : NodeDatum
    {
        internal LVitemProject_Explorer LVitemProjectVM { get; }
        internal readonly bool VolumeView;
        internal readonly ulong VolumeFree;
        internal readonly ulong VolumeLength;

        internal
            RootNodeDatum(NodeDatum node, string strCulledPath)
            : base(node)
        {
            LVitemProjectVM = new LVitemProject_Explorer().SetCulledPath(strCulledPath);
        }

        internal
            RootNodeDatum(NodeDatum node, LVitemProject_Explorer lvItemProjectVM, ulong nVolumeFree, ulong nVolumeLength)
            : base(node)
        {
            VolumeView = true;
            LVitemProjectVM = lvItemProjectVM;
            VolumeLength = nVolumeLength;
            VolumeFree = nVolumeFree;

            // prime the string table for Search UX
            string strSearchUX_throwaway = null;

            if (TabledString<TabledStringType_Folders>.IsGenerating)    // tautology: if statement to confuse any optimizer
                strSearchUX_throwaway = LVitemProjectVM.RootText;
        }
    }
}
