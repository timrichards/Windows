﻿using System;
using System.Linq;

namespace DoubleFile
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder
        {
            // can't be struct because of null
            class DirData
            {
                internal DirData(int nFirstLineNo)
                {
                    _rootNode.FirstLineNo = (uint)nFirstLineNo;
                }

                internal void AddToTree(string str_in, uint nLineNo, ulong nLength, Tuple<int, uint> folderScoreTuple)
                {
                    var str = str_in.TrimEnd('\\');

                    _rootNode.Nodes.Add(str, new Node(str, nLineNo, nLength, folderScoreTuple, _rootNode));
                }

                internal LocalTreeNode AddToTree(string strNickname, out string strRootPath)
                {
                    string strRootPath_out = null;

                    var rootTreeNode = 
                        _rootNode.Nodes.Values
                        .Select(rootNode => rootNode.AddToTree(strNickname, out strRootPath_out))
                        .FirstOrDefault();

                    strRootPath = strRootPath_out;
                    return rootTreeNode;
                }

                readonly RootNode
                    _rootNode = new RootNode { };
            }
        }
    }
}
