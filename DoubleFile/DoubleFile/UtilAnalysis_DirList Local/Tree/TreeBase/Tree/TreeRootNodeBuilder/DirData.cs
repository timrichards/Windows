using System.Linq;
using System.Windows.Forms;
using DoubleFile;

namespace Local
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            // can't be struct because of null
            class DirData
            {
                readonly GlobalData_Base gd = null;
                readonly RootNode m_rootNode = null;

                internal DirData(GlobalData_Base gd_in, RootNode rootNode)
                {
                    gd = gd_in;
                    m_rootNode = rootNode;
                }

                internal void AddToTree(string str_in, uint nLineNo, ulong nLength)
                {
                    var str = str_in.TrimEnd('\\');

                    m_rootNode.Nodes.Add(str, new Node(gd, str, nLineNo, nLength, m_rootNode));
                }

                internal LocalTreeNode AddToTree(string strVolumeName)
                {
                    var nodes = m_rootNode.Nodes.Values;

                    return (nodes.IsEmpty()) ? null :
                        m_rootNode.Nodes.Values.First().AddToTree(strVolumeName);
                }
            }
        }
    }
}
