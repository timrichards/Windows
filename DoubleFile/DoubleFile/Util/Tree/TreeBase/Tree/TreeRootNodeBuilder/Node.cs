using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    partial class Tree
    {
        internal class FolderDetails
        {
            internal uint
                PrevLineNo = 0;
            internal uint
                LineNo = 0;
            internal ulong
                Length = 0;
            internal IReadOnlyList<int>
                FilesHereHashes = null;
            internal bool
                IsHashComplete = false;
            internal DateTime
                Created = DateTime.MinValue;
            internal DateTime
                Modified = DateTime.MinValue;
            internal FileAttributes
                Attributes = 0;
        }

        partial class TreeRootNodeBuilder
        {
            // can't be struct because of object ==
            class Node
            {
                internal
                    Node(string strPath, string[] asLine, IReadOnlyList<int> lsFilesHereHashes, bool isHashComplete, RootNode rootNode)
                    : this(strPath, (uint)("" + asLine[1]).ToInt(), ("" + asLine[knColLength]).ToUlong(), lsFilesHereHashes, isHashComplete, rootNode)
                {
                    _detailsDatum.Created = ("" + asLine[knColCreated]).ToDateTime(bEmptyStringOK: true);
                    _detailsDatum.Modified = ("" + asLine[knColModified]).ToDateTime(bEmptyStringOK: true);
                    _detailsDatum.Attributes = (FileAttributes)Convert.ToInt32("0" + asLine[knColAttributes], /* from base */ 16);
                }

                internal
                    Node(string strPath, uint nLineNo, ulong nLength, IReadOnlyList<int> lsFilesHereHashes, bool isHashComplete, RootNode rootNode)
                {
                    if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                        return;

                    _rootNode = rootNode;
                    _detailsDatum.PrevLineNo = _rootNode.PrevLineNo;
                    _rootNode.PrevLineNo = nLineNo;

                    _detailsDatum.LineNo = nLineNo;
                    _detailsDatum.Length = nLength;
                    _detailsDatum.FilesHereHashes = lsFilesHereHashes ?? new int[0];
                    _detailsDatum.IsHashComplete = isHashComplete;

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
                        nodeParent = new Node(strParent, _detailsDatum.LineNo, 0, null, _detailsDatum.IsHashComplete, _rootNode);
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
                                    new RootNodeDatum(new NodeDatum(subNode._detailsDatum), subNode._strPath);
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

                    treeNode.NodeDatum = new NodeDatum(_detailsDatum);  // this is almost but not quite always newly assigned here.
                    return treeNode;
                }

                FolderDetails
                    _detailsDatum = new FolderDetails { };
                RootNode
                    _rootNode = null;
                bool
                    _bUseShortPath = true;
                readonly IDictionary<string, Node>
                    _subNodes = new SortedDictionary<string, Node>();
                readonly string 
                    _strPath = null;
            }
        }
    }
}
