using System.Collections.Generic;
using System.Windows.Forms;
using DoubleFile;

namespace WPF
{
    abstract class TreeBase : FileParse
    {
        protected readonly GlobalData_Base gd = null;
        protected readonly SortedDictionary<FolderKeyStruct, UList<SDL_TreeNode>> m_dictNodes = null;
        protected readonly Dictionary<string, string> m_dictDriveInfo = null;
        protected static TreeStatusDelegate m_statusCallback = null;

        protected TreeBase(GlobalData_Base gd_in, 
            SortedDictionary<FolderKeyStruct, UList<SDL_TreeNode>> dictNodes,
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
            MBoxStatic.Assert(1301.2301, m_statusCallback != null);
        }
    }
}
