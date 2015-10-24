using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace DoubleFile
{
    partial class LocalTV
    {
        static LocalTV()
        {
            LV_ProjectVM.Modified.LocalSubscribe(99778, x => Cleanup_AllFileHashes_Scratch());
        }

        static internal void
            AllFileHashes_AddRef(CancellationTokenSource cts = null)
        {
            if (0 < _nAllFileHashes_refCount++)
            {
                while (null != _cts)    // hold the caller while building so as not to signal built
                    Util.Block(50);

                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var isHashComplete_throwaway = false;

            _cts = cts ?? new CancellationTokenSource();
            Setup_AllFileHashes_Scratch(RootNodes, out isHashComplete_throwaway);
            stopwatch.Stop();
            Util.WriteLine("Setup_AllFileHashes_Scratch " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
#if (false)
            stopwatch.Reset();
            _dictClones = new ConcurrentDictionary<int, IReadOnlyList<int>>();
            Check_AllFileHashes_Scratch(RootNodes);
            _dictClones = null;
            stopwatch.Stop();
            Util.WriteLine("Check_AllFileHashes_Scratch " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
#endif
            _cts = null;
        }

        static internal void
            AllFileHashes_DropRef()
        {
            if (0 < --_nAllFileHashes_refCount)
                return;

            Cleanup_AllFileHashes_Scratch();
        }

        static IReadOnlyList<int>
            Setup_AllFileHashes_Scratch(IReadOnlyList<LocalTreeNode> nodes, out bool isHashComplete, bool bStart = true)
        {
            var lsAllFilesHashes = new List<int> { };

            isHashComplete = true;

            foreach (var treeNode in nodes)
            {
                ++_progress;

                var nodeDatum = treeNode.NodeDatum;

                if (_cts.IsCancellationRequested)
                    return new int[0];

                if (false == nodeDatum.Hashes_FilesHere_IsComplete)
                    isHashComplete = false;

                if (false == bStart)
                    lsAllFilesHashes.AddRange(nodeDatum.Hashes_FilesHere);

                if (null == treeNode.Nodes)
                {
                    nodeDatum.Hashes_SubnodeFiles_Scratch = new int[0];
                    nodeDatum.Hashes_SubnodeFiles_IsComplete = true;
                    continue;
                }

                nodeDatum.Hashes_SubnodeFiles_Scratch =
                    Setup_AllFileHashes_Scratch(treeNode.Nodes, out nodeDatum.Hashes_SubnodeFiles_IsComplete, bStart: false)   // recurse
                    .OrderBy(n => n)
                    .Distinct()
                    .ToList();

                if (false == nodeDatum.Hashes_SubnodeFiles_IsComplete)
                    isHashComplete = false;

                if (false == bStart)
                    lsAllFilesHashes.AddRange(nodeDatum.Hashes_SubnodeFiles_Scratch);
            }

            return lsAllFilesHashes;
        }

        //static IReadOnlyList<int>
        //    Setup_AllFileHashes_Scratch(IReadOnlyList<LocalTreeNode> nodes, bool bStart = true)
        //{
        //    var lsAllFilesHashes = new List<int> { };

        //    foreach (var treeNode in nodes)
        //    {
        //        if (_cts.IsCancellationRequested)
        //            return new int[0];

        //        if (false == bStart)
        //            lsAllFilesHashes.AddRange(treeNode.NodeDatum.Hashes_FilesHere);

        //        if (0 == (treeNode.Nodes?.Count ?? 0))
        //        {
        //            treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch = new int[0];
        //            continue;
        //        }

        //            Setup_AllFileHashes_Scratch(treeNode.Nodes, bStart: false)              // recurse
        //            .OrderBy(n => n)
        //            .Distinct()
        //            .ToList();

        //        if (false == bStart)
        //            lsAllFilesHashes.AddRange(treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch);
        //    }

        //    return lsAllFilesHashes;
        //}

        //static IReadOnlyList<int>
        //    Check_AllFileHashes_Scratch(IReadOnlyList<LocalTreeNode> nodes, bool bStart = true)
        //{
        //    if (null == nodes)
        //        return new int[0];

        //    var lsAllFilesHashes = new List<int> { };

        //    foreach (var treeNode in nodes)
        //    {
        //        if (_cts.IsCancellationRequested)
        //            return new int[0];

        //        var lsAllFileHashes_childNodes = new List<int> { };

        //        if (null != treeNode.Nodes)
        //            lsAllFileHashes_childNodes.AddRange(Check_AllFileHashes_Scratch(treeNode.Nodes, bStart: false));    // recurse

        //        var lsCheck = _dictClones.GetOrAdd(treeNode.NodeDatum.Hash_AllFiles, x =>
        //            lsAllFileHashes_childNodes.OrderBy(n => n).Distinct().ToList());

        //        if (false == bStart)
        //        {
        //            lsAllFilesHashes.AddRange(treeNode.NodeDatum.Hashes_FilesHere);
        //            lsAllFilesHashes.AddRange(lsAllFileHashes_childNodes);
        //        }
        //    }

        //    return lsAllFilesHashes;
        //}

        static void
            Cleanup_AllFileHashes_Scratch()
        {
            _cts?.Cancel();

            while (null != _cts)
                Util.Block(50);

            Cleanup_AllFileHashes_ScratchA(RootNodes);
            _nAllFileHashes_refCount = 0;
            GC.Collect();
        }

        static void
            Cleanup_AllFileHashes_ScratchA(IReadOnlyList<LocalTreeNode> nodes)
        {
            if (null == nodes)
                return;

            foreach (var treeNode in nodes)
            {
                treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch = null;
                Cleanup_AllFileHashes_ScratchA(treeNode.Nodes);                 // recurse
            }
        }

        void SetAllFilesHashes(IEnumerable<LocalTreeNode> treeNodes)
        {
            foreach (var treeNode in treeNodes)
            {
                ++_progress;

                var ieHashes =
                    treeNode.NodeDatum.Hashes_FilesHere
                    .Union(treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch)
                    .OrderBy(n => n)
                    .Distinct();

                var nCount = 0;
                var nHash = 0;

                foreach (var nFileIndexedID in ieHashes)
                {
                    unchecked
                    {
                        nHash += nFileIndexedID;
                        nHash *= 37;
                    }

                    ++nCount;
                }

                unchecked
                {
                    nHash += nCount;
                    nHash *= 37;
                }

                ((ISetNodeDatum_Hash_AllFiles)treeNode.NodeDatum).Set(nHash);

                if (null != treeNode.Nodes)
                    SetAllFilesHashes(treeNode.Nodes);      // recurse
            }
        }

        void SetDictNodes(IDictionary<int, List<LocalTreeNode>> dictNodes, IEnumerable<LocalTreeNode> treeNodes)
        {
            foreach (var treeNode in treeNodes)
            {
                var nodeDatum = treeNode.NodeDatum;

                if (0 == nodeDatum.Hash_AllFiles)
                    continue;

                for (;;)
                {
                    var lsTreeNodes = dictNodes.TryGetValue(nodeDatum.Hash_AllFiles);

                    if (null != lsTreeNodes)
                    {
                        if (lsTreeNodes[0].NodeDatum.Hashes_SubnodeFiles_Scratch.SequenceEqual(nodeDatum.Hashes_SubnodeFiles_Scratch))
                        {
                            lsTreeNodes.Add(treeNode);
                        }
                        else
                        {
                            // collision: adjust. All its clones will pachenko into the same slot.

                            var nHash = nodeDatum.Hash_AllFiles;

                            unchecked
                            {
                                ++nHash;
                            }

                            if (0 == nHash)
                                ++nHash;

                            ((ISetNodeDatum_Hash_AllFiles)treeNode.NodeDatum).Set(nHash);
                            continue;       // for (;;)
                        }
                    }
                    else if (0 < nodeDatum.LengthTotal)
                    {
                        dictNodes[nodeDatum.Hash_AllFiles] = new List<LocalTreeNode> { treeNode };
                    }

                    break;      // for (;;)
                }

                if (null != treeNode.Nodes)
                    SetDictNodes(dictNodes, treeNode.Nodes);      // recurse
            }
        }

        //static ConcurrentDictionary<int, IReadOnlyList<int>>
        //    _dictClones = null;
        static CancellationTokenSource
            _cts = null;
        static int
            _nAllFileHashes_refCount = 0;
        static int
            _progress = 0;
    }
}
