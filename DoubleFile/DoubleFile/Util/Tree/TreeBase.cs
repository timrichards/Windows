using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

namespace DoubleFile
{
    class TreeBase : FileParse
    {
        protected readonly ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>>
            _dictNodes = null;
        protected readonly IDictionary<string, string>
            _dictDriveInfo = null;
        protected readonly WeakReference<ITreeStatus>
            _callbackWR = null;

        internal TreeBase(
            ConcurrentDictionary<FolderKeyTuple, List<LocalTreeNode>> dictNodes,
            IDictionary<string, string> dictDriveInfo,
            WeakReference<ITreeStatus> callbackWR)
        {
            _dictNodes = dictNodes;
            _dictDriveInfo = dictDriveInfo;
            _callbackWR = callbackWR;
        }

        protected TreeBase(TreeBase base_in)
        {
            _dictNodes = base_in._dictNodes;
            _dictDriveInfo = base_in._dictDriveInfo;
            _callbackWR = base_in._callbackWR;
        }
    }
}
