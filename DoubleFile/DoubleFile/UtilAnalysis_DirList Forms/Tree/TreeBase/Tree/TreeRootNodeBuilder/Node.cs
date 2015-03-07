using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            class Node
            {
                // can't be struct because of object ==
                internal Node(GlobalData_Base gd,
                    string in_str,
                    uint nLineNo,
                    ulong nLength,
                    int nHashParity,
                    RootNode rootNode)
                {
                    _gd = gd;

                    if (_gd.WindowClosed)
                    {
                        return;
                    }

                    MBoxStatic.Assert(1301.2303, nLineNo != 0);
                    _rootNode = rootNode;

                    if (in_str.EndsWith(@":\") == false)
                    {
                        MBoxStatic.Assert(1301.2304, in_str.Trim().EndsWith(@"\") == false);
                    }

                    _strPath = in_str;
                    _nPrevLineNo = _rootNode.FirstLineNo;
                    _rootNode.FirstLineNo = _nLineNo = nLineNo;
                    _nLength = nLength;
                    _nHashParity = nHashParity;

                    // Path.GetDirectoryName() does not preserve filesystem root

                    var strParent = _strPath;
                    var nIndex = strParent.LastIndexOf('\\');

                    if (nIndex < 0)
                    {
                        return;
                    }

                    strParent = strParent.Remove(nIndex).TrimEnd('\\');

                    Node nodeParent = null;

                    if (false == _rootNode.Nodes.TryGetValue(strParent, out nodeParent))
                    {
                        nodeParent = new Node(_gd, strParent, _rootNode.FirstLineNo, 0, nHashParity, _rootNode);
                        _rootNode.Nodes.Add(strParent, nodeParent);
                    }

                    if (nodeParent._subNodes.ContainsKeyA(_strPath) == false)
                    {
                        nodeParent._subNodes.Add(_strPath, this);
                    }
                }

                internal TreeNode AddToTree(string strVolumeName, out string strRootPath)
                {
                    var rootNode = AddToTree();

                    strRootPath = rootNode.Text;

                    //treeNode.Name = treeNode.Text;    Name is now a get accessor for Text in LocalTreeNode 2/22/15

                    if (string.IsNullOrWhiteSpace(strVolumeName))
                    {
                        return rootNode;
                    }

                    rootNode.Text =
                        strVolumeName +
                        (strVolumeName.EndsWith(rootNode.Text)
                            ? ""
                            : " (" + rootNode.Text + ")"
                        );

                    return rootNode;
                }

                TreeNode AddToTree()
                {
                    if (_gd.WindowClosed)
                    {
                        return new TreeNode();
                    }

                    var nIndex = _strPath.LastIndexOf('\\');
                    var strShortPath = _bUseShortPath ? _strPath.Substring(nIndex + 1) : _strPath;
                    TreeNode treeNode = null;

                    if (1 == _subNodes.Count)
                    {
                        var subNode = _subNodes.Values.First();

                        if (this == _rootNode.Nodes.Values.First())
                        {
                            // cull all root node single-chains.
                            _rootNode.Nodes = _subNodes;
                            subNode._bUseShortPath = false;
                            treeNode = subNode.AddToTree();

                            // further down at new NodeDatum...
                            _nPrevLineNo = subNode._nPrevLineNo;
                            _nLength = subNode._nLength;
                            _nLineNo = subNode._nLineNo;
                        }
                        else
                        {
                            treeNode = new TreeNode(strShortPath, new[] { subNode.AddToTree() });
                        }
                    }
                    else if (1 < _subNodes.Count)
                    {
                        var treeList = new UList<TreeNode>();

                        foreach (var node in _subNodes.Values)
                        {
                            treeList.Add(node.AddToTree());
                        }

                        treeNode = new TreeNode(strShortPath, treeList.ToArray());
                    }
                    else
                    {
                        treeNode = new TreeNode(strShortPath);
                    }

                    //Utilities.Assert(1301.2305, treeNode.Text == strShortPath, "\"" + treeNode.Text + "\" != \"" + strShortPath + "\""); not true for non-root
                    MBoxStatic.Assert(1301.2306, treeNode.SelectedImageIndex == -1);     // sets the bitmap size
                    treeNode.SelectedImageIndex = -1;
                    treeNode.Tag = new NodeDatum(_nPrevLineNo, _nLineNo, _nLength);  // this is almost but not quite always newly assigned here.
                    return treeNode;
                }

                readonly RootNode
                    _rootNode = null;
                readonly SortedDictionary<string, Node>
                    _subNodes = new SortedDictionary<string, Node>();
                readonly string 
                    _strPath = null;
                uint
                    _nPrevLineNo = 0;
                uint
                    _nLineNo = 0;
                ulong
                    _nLength = 0;
                bool
                    _bUseShortPath = true;
                int
                    _nHashParity = 0;
                readonly GlobalData_Base
                    _gd = null;
            }
        }
    }
}
