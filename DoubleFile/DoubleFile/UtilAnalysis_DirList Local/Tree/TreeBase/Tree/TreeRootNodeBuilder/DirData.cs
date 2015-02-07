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
                RootNode m_rootNode = null;

                internal DirData(GlobalData_Base gd_in,
                    RootNode rootNode)
                {
                    gd = gd_in;
                    m_rootNode = rootNode;
                }

                internal void AddToTree(string str_in, uint nLineNo, ulong nLength)
                {
                    if (m_rootNode.Nodes.ContainsKey(str_in))
                    {
                        Node node = m_rootNode.Nodes[str_in];

                        MBoxStatic.Assert(1301.2302, false);
                    }

                    string str = str_in.TrimEnd('\\');

                    m_rootNode.Nodes.Add(str, new Node(gd, str, nLineNo, nLength, m_rootNode));
                }

                internal LocalTreeNode AddToTree(string strVolumeName)
                {
                    var nodes = m_rootNode.Nodes.Values;

                    if (nodes.Count <= 0)
                    {
                        return null;
                    }

                    return m_rootNode.Nodes.Values.First().AddToTree(strVolumeName);
                }
            }
        }
    }
}
