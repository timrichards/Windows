using System.Collections.Generic;
using System;

namespace DoubleFile
{
    class TreeBase : FileParse
    {
        protected readonly IDictionary<string, string>
            _dictDriveInfo = null;
        protected readonly WeakReference<ITreeStatus>
            _callbackWR = null;

        internal TreeBase(
            IDictionary<string, string> dictDriveInfo,
            WeakReference<ITreeStatus> callbackWR)
        {
            _dictDriveInfo = dictDriveInfo;
            _callbackWR = callbackWR;
        }

        protected TreeBase(TreeBase base_in)
        {
            _dictDriveInfo = base_in._dictDriveInfo;
            _callbackWR = base_in._callbackWR;
        }
    }
}
