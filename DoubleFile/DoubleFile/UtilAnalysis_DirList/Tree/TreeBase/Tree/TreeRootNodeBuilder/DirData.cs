using DoubleFile;
using System.Linq;
using System.Windows.Forms;

namespace SearchDirLists
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            class DirData
            {
                RootNode m_rootNode = null;

                internal DirData(RootNode rootNode)
                {
                    m_rootNode = rootNode;
                }

                internal void AddToTree(string str_in, uint nLineNo, ulong nLength)
                {
                    if (m_rootNode.Nodes.ContainsKey(str_in))
                    {
                        Node node = m_rootNode.Nodes[str_in];

                        MBox.Assert(1301.2302, false);
                    }

                    string str = str_in.TrimEnd('\\');

                    m_rootNode.Nodes.Add(str, new Node(str, nLineNo, nLength, m_rootNode));
                }

                internal TreeNode AddToTree(string strVolumeName)
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
