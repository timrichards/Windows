using System.Collections.Generic;
using System.Windows.Forms;

namespace DoubleFile
{
    abstract class TreeBase : FileParse
    {
        protected readonly GlobalData_Base gd = null;
        protected readonly SortedDictionary<FolderKeyTuple, UList<TreeNode>> m_dictNodes = null;
        protected readonly Dictionary<string, string> m_dictDriveInfo = null;
        protected readonly TreeStatusDelegate m_statusCallback = null;

        protected TreeBase(GlobalData_Base gd_in, 
            SortedDictionary<FolderKeyTuple, UList<TreeNode>> dictNodes,
            Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback)
        {
            gd = gd_in;
            m_dictNodes = dictNodes;
            m_dictDriveInfo = dictDriveInfo;
            m_statusCallback = statusCallback;
        }

        protected TreeBase(TreeBase base_in)
        {
            gd = base_in.gd;
            m_dictNodes = base_in.m_dictNodes;
            m_dictDriveInfo = base_in.m_dictDriveInfo;
            m_statusCallback = base_in.m_statusCallback;
        }
    }
}
