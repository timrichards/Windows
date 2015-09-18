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
                    Node(string in_str, uint nLineNo, ulong nLength, int nAllFilesHash, IReadOnlyList<int> lsFilesHereHashes,
                    IDictionary<string, Node> nodes, uint nPrevLineNo)
                {
                    if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                        return;

                    Util.Assert(1301.2303m, 0 < nLineNo);
                    _nodes = nodes;
                    _nPrevLineNo = nPrevLineNo;

                    if (false == in_str.EndsWith(@":\"))
                        Util.Assert(1301.2304m, in_str.Trim().EndsWith(@"\") == false);

                    _strPath = in_str;
                    _nLineNo = nLineNo;
                    _nLength = nLength;
                    _nAllFilesHash = nAllFilesHash;
                    _lsFilesHereHashes = lsFilesHereHashes ?? new int[0];

                    // Path.GetDirectoryName() does not preserve filesystem root

                    var nIndex = _strPath.LastIndexOf('\\');

                    if (0 > nIndex)
                    {
                        Util.Assert(99633, 2 == _strPath.Length);
                        Util.Assert(99632, 'A' <= _strPath[0]);
                        Util.Assert(99631, 'Z' >= _strPath[0]);
                        Util.Assert(99630, ':' == _strPath[1]);
                        return;
                    }

                    Util.Assert(99629, 3 < _strPath.Length);

                    var strParent = _strPath.Remove(nIndex);
                    var nodeParent = _nodes.TryGetValue(strParent);

                    if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                        return;

                    if (null == nodeParent)
                    {
                        nodeParent = new Node(strParent, _nPrevLineNo, 0, 0, null, _nodes, _nPrevLineNo);
                        _nodes.Add(strParent, nodeParent);
                    }

                    if (null == nodeParent._subNodes.TryGetValue(_strPath))
                        nodeParent._subNodes.Add(_strPath, this);
                }

                internal LocalTreeNode
                    AddToTree()
                {
                    if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                        return new LocalTreeNode();

                    var nIndex = _strPath.LastIndexOf('\\');
                    var strShortPath = _bUseShortPath ? _strPath.Substring(nIndex + 1) : _strPath;
                    LocalTreeNode treeNode = null;

                    if (1 == _subNodes.Count)
                    {
                        var subNode = _subNodes.Values.First();

                        if (this == _nodes.Values.First())
                        {
                            Util.WriteLine(_strPath + " cull all root node single-chains");
                            _nodes = _subNodes;
                            subNode._bUseShortPath = false;
                            treeNode = subNode.AddToTree();

                            if (false == treeNode.NodeDatum is RootNodeDatum)
                            {
                                // pass the culled path back to TreeRootNodeBuilder; ultimately to LVitem_ProjectExplorer
                                treeNode.NodeDatum =
                                    new RootNodeDatum(new NodeDatum(new DetailsDatum(
                                    subNode._nPrevLineNo, subNode._nLineNo, subNode._nLength, subNode._nAllFilesHash, subNode._lsFilesHereHashes)),
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

                    treeNode.NodeDatum = new NodeDatum(new DetailsDatum(
                        _nPrevLineNo, _nLineNo, _nLength, _nAllFilesHash, _lsFilesHereHashes));  // this is almost but not quite always newly assigned here.

                    return treeNode;
                }

                IDictionary<string, Node>
                    _nodes = null;
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
                readonly int
                    _nAllFilesHash = 0;
                readonly IReadOnlyList<int>
                    _lsFilesHereHashes = null;
            }
        }
    }
}
