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
                internal
                    Node(string in_str, uint nLineNo, ulong nLength, IReadOnlyList<uint> folderScore, RootNode rootNode)
                {
                    if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                        return;

                    Util.Assert(1301.2303m, nLineNo != 0);
                    _rootNode = rootNode;

                    if (false == in_str.EndsWith(@":\"))
                        Util.Assert(1301.2304m, in_str.Trim().EndsWith(@"\") == false);

                    _strPath = in_str;
                    _nPrevLineNo = _rootNode.FirstLineNo;
                    _rootNode.FirstLineNo = _nLineNo = nLineNo;
                    _nLength = nLength;
                    _folderScore = folderScore;

                    // Path.GetDirectoryName() does not preserve filesystem root

                    var strParent = _strPath;
                    var nIndex = strParent.LastIndexOf('\\');

                    if (0 > nIndex)
                        return;

                    strParent = strParent.Remove(nIndex).TrimEnd('\\');

                    var nodeParent = _rootNode.Nodes.TryGetValue(strParent);

                    if (null == nodeParent)
                    {
                        nodeParent = new Node(strParent, _rootNode.FirstLineNo, 0, _folderScore, _rootNode);
                        _rootNode.Nodes.Add(strParent, nodeParent);
                    }

                    if (null == nodeParent._subNodes.TryGetValue(_strPath))
                        nodeParent._subNodes.Add(_strPath, this);
                }

                internal LocalTreeNode
                    AddToTree()
                {
                    if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                        return new LocalTreeNode();

                    var nIndex = _strPath.LastIndexOf('\\');
                    var strShortPath = _bUseShortPath ? _strPath.Substring(nIndex + 1) : _strPath;
                    LocalTreeNode treeNode = null;

                    if (1 == _subNodes.Count)
                    {
                        var subNode = _subNodes.Values.First();

                        if (this == _rootNode.Nodes.Values.First())
                        {
                            Util.WriteLine(_strPath + " cull all root node single-chains");
                            _rootNode.Nodes = _subNodes;
                            subNode._bUseShortPath = false;
                            treeNode = subNode.AddToTree();

                            if (false == treeNode.NodeDatum is RootNodeDatum)
                            {
                                // pass the culled path back to TreeRootNodeBuilder; ultimately to LVitem_ProjectExplorer
                                treeNode.NodeDatum =
                                    new RootNodeDatum(new NodeDatum(new DetailsDatum(
                                    subNode._nPrevLineNo, subNode._nLineNo, subNode._nLength, subNode._folderScore)),
                                    subNode._strPath);
                            }

                            return treeNode;
                        }

                        treeNode = new LocalTreeNode(strShortPath, new List<LocalTreeNode> { subNode.AddToTree() });
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

                    treeNode.NodeDatum = new NodeDatum(new DetailsDatum(_nPrevLineNo, _nLineNo, _nLength, _folderScore));  // this is almost but not quite always newly assigned here.
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
                IReadOnlyList<uint>
                    _folderScore = new[] { 0U, 0U, 0U };  // Weighted folder scores: HashParity (random); largest; smallest
            }
        }
    }
}
