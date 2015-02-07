using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DoubleFile;

namespace Local
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            class Node
            {
                // can't be struct because of object ==
                internal Node(GlobalData_Base gd_in,
                    string in_str,
                    uint nLineNo,
                    ulong nLength,
                    RootNode rootNode)
                {
                    gd = gd_in;

                    if (gd.WindowClosed)
                    {
                        return;
                    }

                    MBoxStatic.Assert(1301.2303, nLineNo != 0);
                    m_rootNode = rootNode;

                    if (in_str.EndsWith(@":\") == false)
                    {
                        MBoxStatic.Assert(1301.2304, in_str.Trim().EndsWith(@"\") == false);
                    }

                    m_strPath = in_str;
                    m_nPrevLineNo = m_rootNode.FirstLineNo;
                    m_rootNode.FirstLineNo = m_nLineNo = nLineNo;
                    m_nLength = nLength;

                    // Path.GetDirectoryName() does not preserve filesystem root

                    string strParent = m_strPath;
                    int nIndex = strParent.LastIndexOf('\\');

                    if (nIndex < 0)
                    {
                        return;
                    }

                    strParent = strParent.Remove(nIndex).TrimEnd('\\');

                    if (m_rootNode.Nodes.ContainsKey(strParent) == false)
                    {
                        m_rootNode.Nodes.Add(strParent, new Node(gd, strParent, m_rootNode.FirstLineNo, 0, m_rootNode));
                    }

                    if (m_rootNode.Nodes[strParent].subNodes.ContainsKey(m_strPath) == false)
                    {
                        m_rootNode.Nodes[strParent].subNodes.Add(m_strPath, this);
                    }
                }

                internal LocalTreeNode AddToTree(string strVolumeName = null)
                {
                    if (gd.WindowClosed)
                    {
                        return new LocalTreeNode();
                    }

                    int nIndex = m_strPath.LastIndexOf('\\');
                    string strShortPath = bUseShortPath ? m_strPath.Substring(nIndex + 1) : m_strPath;
                    LocalTreeNode treeNode = null;

                    if (subNodes.Count == 1)
                    {
                        Node subNode = subNodes.Values.First();

                        if (this == m_rootNode.Nodes.Values.First())
                        {
                            // cull all root node single-chains.
                            m_rootNode.Nodes = subNodes;
                            subNode.m_strPath.Insert(0, m_strPath + '\\');
                            subNode.bUseShortPath = false;
                            treeNode = subNode.AddToTree(strVolumeName);

                            // further down at new NodeDatum...
                            m_nPrevLineNo = subNode.m_nPrevLineNo;
                            m_nLength = subNode.m_nLength;
                            m_nLineNo = subNode.m_nLineNo;
                        }
                        else
                        {
                            treeNode = new LocalTreeNode(strShortPath, new LocalTreeNode[] { subNode.AddToTree() });
                        }
                    }
                    else if (subNodes.Count > 1)
                    {
                        UList<LocalTreeNode> treeList = new UList<LocalTreeNode>();

                        foreach (Node node in subNodes.Values)
                        {
                            treeList.Add(node.AddToTree());
                        }

                        treeNode = new LocalTreeNode(strShortPath, treeList.ToArray());
                    }
                    else
                    {
                        treeNode = new LocalTreeNode(strShortPath);
                    }

                    //Utilities.Assert(1301.2305, treeNode.Text == strShortPath, "\"" + treeNode.Text + "\" != \"" + strShortPath + "\""); not true for non-root
                    MBoxStatic.Assert(1301.2306, treeNode.SelectedImageIndex == -1);     // sets the bitmap size
                    treeNode.SelectedImageIndex = -1;
                    treeNode.Tag = new NodeDatum(m_nPrevLineNo, m_nLineNo, m_nLength);  // this is almost but not quite always newly assigned here.

                    if (this == m_rootNode.Nodes.Values.First())
                    {
                        treeNode.Name = treeNode.Text;

                        if (false == string.IsNullOrWhiteSpace(strVolumeName))
                        {
                            if (strVolumeName.EndsWith(treeNode.Text))
                            {
                                treeNode.Text = strVolumeName;
                            }
                            else
                            {
                                treeNode.Text = strVolumeName + " (" + treeNode.Text + ")";
                            }
                        }
                    }

                    return treeNode;
                }

                readonly GlobalData_Base gd = null;
                readonly RootNode m_rootNode = null;
                readonly SortedDictionary<string, Node> subNodes = new SortedDictionary<string, Node>();
                readonly string m_strPath = null;
                uint m_nPrevLineNo = 0;
                uint m_nLineNo = 0;
                ulong m_nLength = 0;
                bool bUseShortPath = true;
            }
        }
    }
}
