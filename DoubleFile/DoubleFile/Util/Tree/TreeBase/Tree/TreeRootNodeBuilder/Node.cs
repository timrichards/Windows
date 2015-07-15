using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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
                    uint[] folderScore,
                    RootNode rootNode)
                {
                    if ((null == Application.Current) || Application.Current.Dispatcher.HasShutdownStarted)
                        return;

                    MBoxStatic.Assert(1301.2303m, nLineNo != 0);
                    _rootNode = rootNode;

                    if (in_str.EndsWith(@":\") == false)
                    {
                        MBoxStatic.Assert(1301.2304m, in_str.Trim().EndsWith(@"\") == false);
                    }

                    _strPath = in_str;
                    _nPrevLineNo = _rootNode.FirstLineNo;
                    _rootNode.FirstLineNo = _nLineNo = nLineNo;
                    _nLength = nLength;
                    _folderScore = folderScore;

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
                        nodeParent = new Node(strParent, _rootNode.FirstLineNo, 0, _folderScore, _rootNode);
                        _rootNode.Nodes.Add(strParent, nodeParent);
                    }

                    if (null == nodeParent._subNodes.TryGetValue(_strPath))
                    {
                        nodeParent._subNodes.Add(_strPath, this);
                    }
                }

                internal LocalTreeNode AddToTree(string strNickname, out string strRootPath)
                {
                    var rootNode = AddToTree();

                    // out parameter
                    strRootPath = rootNode.Text;

                    if (string.IsNullOrWhiteSpace(strNickname))
                        return rootNode;

                    rootNode.Text = LVitem_ProjectVM.RootText(strNickname, strRootPath);
                    return rootNode;
                }

                LocalTreeNode AddToTree()
                {
                    if ((null == Application.Current) || Application.Current.Dispatcher.HasShutdownStarted)
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
                            _folderScore = subNode._folderScore;
                        }
                        else
                        {
                            treeNode = new LocalTreeNode(strShortPath, new List<LocalTreeNode> { subNode.AddToTree() });
                        }
                    }
                    else if (1 < _subNodes.Count)
                    {
                        var treeList = new List<LocalTreeNode>();

                        foreach (var node in _subNodes.Values)
                            treeList.Add(node.AddToTree());

                        treeNode = new LocalTreeNode(strShortPath, treeList);
                    }
                    else    // Zero
                    {
                        treeNode = new LocalTreeNode(strShortPath);
                    }

                    //Utilities.Assert(1301.2305, treeNode.Text == strShortPath, "\"" + treeNode.Text + "\" != \"" + strShortPath + "\""); not true for non-root
                    MBoxStatic.Assert(1301.2306m, treeNode.SelectedImageIndex == -1);     // sets the bitmap size
                    treeNode.SelectedImageIndex = -1;
                    treeNode.NodeDatum = new NodeDatum(_nPrevLineNo, _nLineNo, _nLength, _folderScore);  // this is almost but not quite always newly assigned here.
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
                uint[]
                    _folderScore = new[] { 0U, 0U, 0U };  // Weighted folder scores: HashParity (random); largest; smallest
            }
        }
    }
}
