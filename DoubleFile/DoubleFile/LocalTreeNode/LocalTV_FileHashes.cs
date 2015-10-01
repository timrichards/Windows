using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DoubleFile
{
    partial class LocalTV
    {
        internal static void
            AllFileHashes_AddRef(CancellationTokenSource cts = null)
        {
            if (0 < _nAllFileHashes_refCount++)
                return;

            var stopwatch = Stopwatch.StartNew();

            _dictClones = new Dictionary<int, IReadOnlyList<int>>();
            _cts = cts ?? new CancellationTokenSource();
            Setup_AllFileHashes_Scratch(RootNodes);
            _dictClones = null;
            _cts = null;
            stopwatch.Stop();
            Util.WriteLine("Setup_AllFileHashes_Scratch " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
        }

        internal static void
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
            if (null == nodes)
                return new int[0];

            var lsAllFilesHashes = new List<int> { };

            foreach (var treeNode in nodes)
            {
                if (_cts.IsCancellationRequested)
                    return new int[0];

                var lsAllFileHashes_childNodes = new List<int> { };

                if (null != treeNode.Nodes)
                    lsAllFileHashes_childNodes.AddRange(Setup_AllFileHashes_Scratch(treeNode.Nodes, bStart: false));    // recurse

                IReadOnlyList<int> lsClone = null;

                if (_dictClones.TryGetValue(treeNode.NodeDatum.Hash_AllFiles, out lsClone))
                {
                    treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch = lsClone;
                }
                else
                {
                    treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch = lsAllFileHashes_childNodes.OrderBy(n => n).Distinct().ToList();
                    _dictClones.Add(treeNode.NodeDatum.Hash_AllFiles, treeNode.NodeDatum.Hashes_SubnodeFiles_Scratch);
                }

                if (false == bStart)
                {
                    lsAllFilesHashes.AddRange(treeNode.NodeDatum.Hashes_FilesHere);
                    lsAllFilesHashes.AddRange(lsAllFileHashes_childNodes);
                }
            }

            return lsAllFilesHashes;
        }

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

        static Dictionary<int, IReadOnlyList<int>>
            _dictClones = null;
        static CancellationTokenSource
            _cts = null;
        static int
            _nAllFileHashes_refCount = 0;
    }
}
