namespace DoubleFile
{
    class RootNodeDatum : NodeDatum
    {
        internal LVitem_ProjectVM LVitemProjectVM { get; }
        internal readonly bool VolumeView;
        internal readonly ulong VolumeFree;
        internal readonly ulong VolumeLength;

        internal string
            RootText(string strSourcePath) => RootText(LVitemProjectVM.Nickname, strSourcePath);
        static internal string
            RootText(string strNickname, string strSourcePath)
        {
            if (string.IsNullOrWhiteSpace(strNickname))
                return strSourcePath;

            if (("" + strNickname).EndsWith(strSourcePath))
                return strNickname;

            return strNickname + " (" + strSourcePath + ")";
        }

        internal
            RootNodeDatum(NodeDatum node, LVitem_ProjectVM lvItemProjectVM,
            ulong nVolumeFree, ulong nVolumeLength)
            : base(node)
        {
            VolumeView = true;
            LVitemProjectVM = lvItemProjectVM;
            VolumeLength = nVolumeLength;
            VolumeFree = nVolumeFree;
        }
    }
}
