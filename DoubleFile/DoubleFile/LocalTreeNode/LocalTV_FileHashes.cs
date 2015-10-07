using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DoubleFile
{
    partial class LocalTV
    {
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

            _cts = cts ?? new CancellationTokenSource();
            Setup_AllFileHashes_Scratch(RootNodes);
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
            Util.Assert(99614, 0 < _nAllFileHashes_refCount);

            if (0 < --_nAllFileHashes_refCount)
                return;

            Cleanup_AllFileHashes_Scratch(RootNodes);
        }

        static IReadOnlyList<int>
            Setup_AllFileHashes_Scratch(IReadOnlyList<LocalTreeNode> nodes, bool bStart = true)
        {
            var lsAllFilesHashes = new List<int> { };

            foreach (var treeNode in nodes)
            {
                if (_cts.IsCancellationRequested)
                    return new int[0];

                if (false == bStart)
                    lsAllFilesHashes.AddRange(treeNode.NodeDatum.Hashes_FilesHere);

                if (0 == (treeNode.Nodes?.Count ?? 0))
                {
                    treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch = new int[0];
                    continue;
                }

                treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch =
                    Setup_AllFileHashes_Scratch(treeNode.Nodes, bStart: false)              // recurse
                    .OrderBy(n => n)
                    .Distinct()
                    .ToList();

                if (false == bStart)
                    lsAllFilesHashes.AddRange(treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch);
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
            Cleanup_AllFileHashes_Scratch(IReadOnlyList<LocalTreeNode> nodes)
        {
            if (null == nodes)
                return;

            foreach (var treeNode in nodes)
            {
                treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch = null;
                Cleanup_AllFileHashes_Scratch(treeNode.Nodes);                 // recurse
            }
        }

        //static ConcurrentDictionary<int, IReadOnlyList<int>>
        //    _dictClones = null;
        static CancellationTokenSource
            _cts = null;
        static int
            _nAllFileHashes_refCount = 0;
    }
}
