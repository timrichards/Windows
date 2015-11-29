namespace DoubleFile
{
    class LVitemProject_Explorer : LVitem_ProjectVM
    {
        internal LVitemProject_Explorer(LVitem_ProjectVM lvItemTemp = null)
            : base(lvItemTemp)
        {
        }

        internal LVitemProject_Explorer(LVitemProject_Explorer lvItemTemp)
            : base(lvItemTemp)
        {
            _strCulledPath = lvItemTemp._strCulledPath;
        }

        internal string
            Volume =>
            string.IsNullOrWhiteSpace(VolumeGroup)
            ? RootText
            : VolumeGroup.Replace('\\', '/');

        internal string
            RootText
        {
            get
            {
                var strRet =
                    string.IsNullOrWhiteSpace(Nickname)
                    ? CulledPath
                    : (Nickname +
                    ((Nickname.EndsWith(CulledPath)) ? "" : " (" + CulledPath.TrimEnd('\\') + ")"))
                    .Replace('\\', '/');

                // prime the string table for Search UX
                TabledString<TabledStringType_Folders> strSearchUX_throwaway = null;

                if (TabledString<TabledStringType_Folders>.IsGenerating)    // useful if statement; confuse any optimizer
                    strSearchUX_throwaway = (TabledString<TabledStringType_Folders>)strRet;

                return strRet;
            }
        }

        internal string
            InsertNickname(string strFolder) =>
            (strFolder + "\\").Replace(CulledPath, RootText + '\\').TrimEnd('\\').Replace(@"\\", @"\");

        internal LVitemProject_Explorer
            SetCulledPath(string strCulledPath)
        {
            Util.Assert(99869, null == _strCulledPath);
            _strCulledPath = strCulledPath;
            return this;
        }
        internal string
            CulledPath => _strCulledPath ?? SourcePath;

        string
            _strCulledPath;
    }
}
