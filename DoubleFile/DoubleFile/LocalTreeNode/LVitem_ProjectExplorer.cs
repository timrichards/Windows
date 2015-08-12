namespace DoubleFile
{
    class LVitem_ProjectExplorer : LVitem_ProjectVM
    {
        internal LVitem_ProjectExplorer(LVitem_ProjectVM lvItemTemp = null)
            : base(lvItemTemp)
        {
        }

        internal string
            Volume =>
            (string.IsNullOrWhiteSpace(VolumeGroup))
            ? RootText
            : VolumeGroup.Replace('\\', '/');

        internal string CulledPath;

        internal string
            RootText
        {
            get
            {
                string culledPath = CulledPath ?? SourcePath;

                var strRet = (string.IsNullOrWhiteSpace(Nickname))
                    ? culledPath
                    : (Nickname +
                    ((Nickname.EndsWith(culledPath)) ? "" : " (" + culledPath.TrimEnd('\\') + ")"))
                    .Replace('\\', '/');

                if (null != CulledPath)
                {
                    // prime the string table for Search UX
                    TabledString<TabledStringType_Folders> strSearchUX_throwaway = null;

                    if (TabledString<TabledStringType_Folders>.IsGenerating)
                        strSearchUX_throwaway = (TabledString<TabledStringType_Folders>)strRet;
                }

                return strRet;
            }
        }
    }
}
