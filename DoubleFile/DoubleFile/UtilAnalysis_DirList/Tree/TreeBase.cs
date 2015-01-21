using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoubleFile
{
    delegate void TreeStatusDelegate(LVitem_ProjectVM volStrings, TreeNode rootNode = null, bool bError = false);

    class TreeBase : FileParse
    {
        protected readonly SortedDictionary<Correlate, UList<TreeNode>> m_dictNodes = null;
        protected readonly Dictionary<string, string> m_dictDriveInfo = null;
        protected static TreeStatusDelegate m_statusCallback = null;

        internal TreeBase(SortedDictionary<Correlate, UList<TreeNode>> dictNodes, Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback)
        {
            m_dictNodes = dictNodes;
            m_dictDriveInfo = dictDriveInfo;
            m_statusCallback = statusCallback;
        }

        internal TreeBase(TreeBase base_in)
        {
            m_dictNodes = base_in.m_dictNodes;
            m_dictDriveInfo = base_in.m_dictDriveInfo;
            MBox.Assert(1301.2301, m_statusCallback != null);
        }
    }

}
