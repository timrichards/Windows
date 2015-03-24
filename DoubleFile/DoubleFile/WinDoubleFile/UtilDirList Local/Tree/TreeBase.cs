using System.Collections.Generic;
using System.Windows.Forms;
using DoubleFile;
using System.Collections.Concurrent;

namespace DoubleFile
{
    abstract class TreeBase : FileParse
    {
        protected readonly ConcurrentDictionary<FolderKeyTuple, KeyList<LocalTreeNode>> m_dictNodes = null;
        protected readonly Dictionary<string, string> m_dictDriveInfo = null;
        protected readonly TreeStatusDelegate m_statusCallback = null;

        protected TreeBase(
            ConcurrentDictionary<FolderKeyTuple, KeyList<LocalTreeNode>> dictNodes,
            Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback)
        {
            m_dictNodes = dictNodes;
            m_dictDriveInfo = dictDriveInfo;
            m_statusCallback = statusCallback;
        }

        protected TreeBase(TreeBase base_in)
        {
            m_dictNodes = base_in.m_dictNodes;
            m_dictDriveInfo = base_in.m_dictDriveInfo;
            m_statusCallback = base_in.m_statusCallback;
        }
    }
}
