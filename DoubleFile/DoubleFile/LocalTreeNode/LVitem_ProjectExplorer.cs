
using System.Linq;

namespace DoubleFile
{
    class LVitem_ProjectExplorer : LVitem_ProjectVM
    {
        internal LVitem_ProjectExplorer(LVitem_ProjectVM lvItemTemp)
            : base(lvItemTemp)
        {
        }

        internal string
            Volume =>
            (false == string.IsNullOrWhiteSpace(VolumeGroup))
            ? VolumeGroup.Replace('\\', '/')
            : RootText;

        string _culledPath => CulledPath?.IfGenerated ?? SourcePath;

        internal string
            RootText =>
            (string.IsNullOrWhiteSpace(Nickname))
            ? _culledPath
            : (Nickname +
            ((Nickname.EndsWith(_culledPath)) ? "" : " (" + _culledPath.TrimEnd('\\') + ")"))
            .Replace('\\', '/');

        internal TabledString<TabledStringType_Folders>
            CulledPath { set; private get; }
    }
}
