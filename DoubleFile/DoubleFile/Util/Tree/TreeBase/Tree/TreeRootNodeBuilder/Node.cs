using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            class Node
            {
                // can't be struct because of object ==
                internal Node(
                    string in_str,
                    uint nLineNo,
                    ulong nLength,
                    int nHashParity,
                    RootNode rootNode)
                {
                    if (App.LocalExit)
                        return;

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

                    var nodeParent = _rootNode.Nodes.TryGetValue(strParent);

                    if (null == nodeParent)
                    {
                        nodeParent = new Node(strParent, _rootNode.FirstLineNo, 0, _nHashParity, _rootNode);
                        _rootNode.Nodes.Add(strParent, nodeParent);
                    }

                    if (null == nodeParent._subNodes.TryGetValue(_strPath))
                    {
                        nodeParent._subNodes.Add(_strPath, this);
                    }
                }

                internal LocalTreeNode AddToTree(string strVolumeName, out string strRootPath)
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
                        (strVolumeName.EndsWith("" + rootNode.Text)
                            ? ""
                            : " (" + rootNode.Text + ")"
                        );

                    return rootNode;
                }

                LocalTreeNode AddToTree()
                {
                    if (App.LocalExit)
                        return new LocalTreeNode();

                    var nIndex = _strPath.LastIndexOf('\\');
                    var strShortPath = _bUseShortPath ? _strPath.Substring(nIndex + 1) : _strPath;
                    LocalTreeNode treeNode = null;

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
                            _nHashParity = subNode._nHashParity;
                        }
                        else
                        {
                            treeNode = new LocalTreeNode(strShortPath, new[] { subNode.AddToTree() });
                        }
                    }
                    else if (1 < _subNodes.Count)
                    {
                        var treeList = new List<LocalTreeNode>();

                        foreach (var node in _subNodes.Values)
                            treeList.Add(node.AddToTree());

                        treeNode = new LocalTreeNode(strShortPath, treeList);
                    }
                    else
                    {
                        treeNode = new LocalTreeNode(strShortPath);
                    }

                    //Utilities.Assert(1301.2305, treeNode.Text == strShortPath, "\"" + treeNode.Text + "\" != \"" + strShortPath + "\""); not true for non-root
                    MBoxStatic.Assert(1301.2306, treeNode.SelectedImageIndex == -1);     // sets the bitmap size
                    treeNode.SelectedImageIndex = -1;
                    treeNode.NodeDatum = new NodeDatum(_nPrevLineNo, _nLineNo, _nLength, _nHashParity);  // this is almost but not quite always newly assigned here.
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
            }
        }
    }
}
