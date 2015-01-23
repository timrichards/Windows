using System.Collections.Generic;
using System.Windows.Forms;

namespace DoubleFile
{
    abstract class TreeBase : UtilAnalysis_DirList
    {
        protected readonly GlobalDataBase gd = null;
        protected readonly SortedDictionary<Correlate, UList<TreeNode>> m_dictNodes = null;
        protected readonly Dictionary<string, string> m_dictDriveInfo = null;
        protected static TreeStatusDelegate m_statusCallback = null;

        protected TreeBase(GlobalDataBase gd_in, 
            SortedDictionary<Correlate, UList<TreeNode>> dictNodes,
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
            MBox.Assert(1301.2301, m_statusCallback != null);
        }
    }
}
