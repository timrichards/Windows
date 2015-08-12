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

        internal LVitem_ProjectExplorer
            SetCulledPath(string strCulledPath)
        {
            Util.Assert(99869, null == _strCulledPath);
            _strCulledPath = strCulledPath;
            return this;
        }
        internal string
            CulledPath => _strCulledPath;
        string _strCulledPath;

        internal string
            RootText
        {
            get
            {
                string culledPath = _strCulledPath ?? SourcePath;

                var strRet = (string.IsNullOrWhiteSpace(Nickname))
                    ? culledPath
                    : (Nickname +
                    ((Nickname.EndsWith(culledPath)) ? "" : " (" + culledPath.TrimEnd('\\') + ")"))
                    .Replace('\\', '/');

                if (null != _strCulledPath)
                {
                    // prime the string table for Search UX
                    TabledString<TabledStringType_Folders> strSearchUX_throwaway = null;

                    if (TabledString<TabledStringType_Folders>.IsGenerating)    // useful if statement; confuse any optimizer
                        strSearchUX_throwaway = (TabledString<TabledStringType_Folders>)strRet;
                }

                return strRet;
            }
        }
    }
}
