using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            // can't be struct because of object ==
            class Node
            {
                internal
                    Node(string strPath, uint nLineNo, ulong nLength, IReadOnlyList<int> lsFilesHereHashes, bool isHashComplete, RootNode rootNode)
                {
                    if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                        return;

                    _rootNode = rootNode;
                    _nPrevLineNo = _rootNode.PrevLineNo;
                    _rootNode.PrevLineNo = nLineNo;

                    _nLineNo = nLineNo;
                    _nLength = nLength;
                    _lsFilesHereHashes = lsFilesHereHashes ?? new int[0];
                    _isHashComplete = isHashComplete;

                    // Path.GetDirectoryName() does not preserve filesystem root

                    _strPath = strPath;

                    var nIndex = _strPath.LastIndexOf('\\');

                    if (0 > nIndex)
                    {
                        Util.Assert(99633, 2 == _strPath.Length);
                        return;
                    }

                    Util.Assert(99629, 3 < _strPath.Length);

                    var strParent = _strPath.Remove(nIndex);
                    var nodeParent = _rootNode.Nodes.TryGetValue(strParent);

                    if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                        return;

                    if (null == nodeParent)
                    {
                        nodeParent = new Node(strParent, _nLineNo, 0, null, _isHashComplete, _rootNode);
                        _rootNode.Nodes.Add(strParent, nodeParent);
                    }

                    if (null == nodeParent._subNodes.TryGetValue(_strPath))
                        nodeParent._subNodes.Add(_strPath, this);
                }

                internal LocalTreeNode
                    AddToTree()
                {
                    if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                        return new LocalTreeNode();

                    var strShortPath = _bUseShortPath ? _strPath.Substring(_strPath.LastIndexOf('\\') + 1) : _strPath;
                    LocalTreeNode treeNode = null;

                    if (1 == _subNodes.Count)
                    {
                        var subNode = _subNodes.Values.First();

                        if (ReferenceEquals(this, _rootNode.Nodes.Values.First()))
                        {
                            Util.WriteLine(_strPath + " cull all root node single-chains");
                            _rootNode.Nodes = _subNodes;
                            subNode._bUseShortPath = false;
                            treeNode = subNode.AddToTree();

                            if (false == treeNode.NodeDatum is RootNodeDatum)
                            {
                                // pass the culled path back to TreeRootNodeBuilder; ultimately to LVitem_ProjectExplorer
                                treeNode.NodeDatum =
                                    new RootNodeDatum(new NodeDatum(
                                    subNode._nPrevLineNo, subNode._nLineNo, subNode._nLength, subNode._lsFilesHereHashes, subNode._isHashComplete),
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

                    treeNode.NodeDatum = new NodeDatum(
                        _nPrevLineNo, _nLineNo, _nLength, _lsFilesHereHashes, _isHashComplete);  // this is almost but not quite always newly assigned here.

                    return treeNode;
                }

                RootNode
                    _rootNode = null;
                bool
                    _bUseShortPath = true;
                readonly IDictionary<string, Node>
                    _subNodes = new SortedDictionary<string, Node>();
                readonly string 
                    _strPath = null;
                readonly uint
                    _nPrevLineNo = 0;
                readonly uint
                    _nLineNo = 0;
                readonly ulong
                    _nLength = 0;
                readonly IReadOnlyList<int>
                    _lsFilesHereHashes = null;
                readonly bool
                    _isHashComplete = false;
            }
        }
    }
}
