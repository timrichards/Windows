using System.Collections.Generic;
using System.Windows.Forms;
using DoubleFile;
using System.Collections.Concurrent;

namespace DoubleFile
{
    abstract class TreeBase : FileParse
    {
        protected readonly ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>
            _dictNodes = null;
        protected readonly Dictionary<string, string>
            _dictDriveInfo = null;
        protected readonly TreeStatusDelegate
            _statusCallback = null;

        protected TreeBase(
            ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>> dictNodes,
            Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback)
        {
            _dictNodes = dictNodes;
            _dictDriveInfo = dictDriveInfo;
            _statusCallback = statusCallback;
        }

        protected TreeBase(TreeBase base_in)
        {
            _dictNodes = base_in._dictNodes;
            _dictDriveInfo = base_in._dictDriveInfo;
            _statusCallback = base_in._statusCallback;
        }
    }
}
