using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System;
using System.Reactive.Linq;

namespace DoubleFile
{
    interface ICreateFileDictStatus
    {
        void Callback(bool bDone = false, double nProgress = double.NaN);
    }
    
    partial class FileDictionary : IDisposable
    {
        internal FileDictionary()
        {
            // ProjectFile.OnOpenedProject += Deserialize;
            // ProjectFile.OnSavingProject += Serialize;
        }

        public void Dispose()
        {
            // ProjectFile.OnOpenedProject -= Deserialize;
            // ProjectFile.OnSavingProject -= Serialize;
        }

        internal void
            Clear()
        {
            _dictFiles = null;
            _bListingFileWithOnlyHashV1pt0 = false;
        }

        internal bool
            IsEmpty => null == _dictFiles;

        internal bool
            AllListingsHashV2
        {
            get
            {
                if (null != _allListingsHashV2)
                    return _allListingsHashV2.Value;

                // 7/1/15 DoThreadFactory() is now synchronous so TreeRootNodeBuilder can use folder scorer
                Util.Assert(99906, false);

                if (null == _LVprojectVM)
                {
                    Util.Assert(99959, false);
                    Util.Block(1000);

                    if (null == _LVprojectVM)
                    {
                        Util.Assert(99938, false);
                        return false;
                    }
                }

                _allListingsHashV2 =
                    _LVprojectVM.ItemsCast
                    .Where(lvItem => lvItem.CanLoad)
                    .Aggregate(true, (current, lvItem) => current && lvItem.HashV2);

                return _allListingsHashV2.Value;
            }
        }
        bool? _allListingsHashV2 = null;

        internal IReadOnlyList<uint>
            GetFolderScorer(FileKeyTuple fileKeyTuple)
        {
            var tuple = _dictFiles.TryGetValue(fileKeyTuple);

            return
                (null != tuple)
                ? tuple.Item1
                : (0 < fileKeyTuple.Item2)
                ? new[] { 1U, 1U, 1U }      // no duplicates for this file so just account for its existence
                : new[] { 0U, 0U, 0U };     // empty file
        }

        internal IReadOnlyList<DuplicateStruct>
            GetDuplicates(string[] asFileLine)
        {
            var nHashColumn =
                _bListingFileWithOnlyHashV1pt0
                ? 10
                : 11;

            if (asFileLine.Length <= nHashColumn)
                return null;

            var tuple = _dictFiles?.TryGetValue(FileKeyTuple.FactoryCreate(
                asFileLine[nHashColumn],
                ("" + asFileLine[7]).ToUlong()));

            if (null == tuple)
                return null;

            return
                tuple.Item2.AsParallel()
                .Select(lookup => new DuplicateStruct
            {
                LVitemProjectVM = _dictItemNumberToLV[GetLVitemProjectVM(lookup)],
                LineNumber = GetLineNumber(lookup)
            })
                .ToList();
        }

        internal FileDictionary
            DoThreadFactory(LV_ProjectVM lvProjectVM, WeakReference<ICreateFileDictStatus> callbackWR)
        {
            _LVprojectVM = lvProjectVM;
            _callbackWR = callbackWR;
            _dictFiles = null;
            IsAborted = false;

            _thread = Util.ThreadMake(() => { Go(); _blockingFrame.Continue = false; });
            _blockingFrame.PushFrameToTrue();
            return this;
        }

        internal void
            Abort()
        {
            _blockingFrame.Continue = false;
            IsAborted = true;
            _thread?.Abort();
        }
        internal bool
            IsAborted { get; private set; }
        internal void
            ResetAbortFlag() => IsAborted = false;

        void Go()
        {
            if (null == _LVprojectVM)
                return;

            var nLVitems = 0;

            var dictLVtoItemNumber = new Dictionary<LVitem_ProjectVM, int>();
            var dictItemNumberToLV = new Dictionary<int, LVitem_ProjectVM>();

            foreach (var lvItem
                in _LVprojectVM.ItemsCast
                .Where(lvItem => lvItem.CanLoad)
                .OrderBy(lvItem => lvItem.SourcePath))
            {
                dictLVtoItemNumber.Add(lvItem, nLVitems);
                dictItemNumberToLV.Add(nLVitems, lvItem);
                ++nLVitems;
            }

            _dictLVtoItemNumber = dictLVtoItemNumber;
            _dictItemNumberToLV = dictItemNumberToLV;

            var cts = new CancellationTokenSource();
            var nProgress = 0;
            var dictV1pt0 = new ConcurrentDictionary<FileKeyTuple, List<int>> { };
            var dictV2 = new ConcurrentDictionary<FileKeyTuple, List<int>> { };
            var nFolderCount1pt0 = 1;
            var nFolderCount2 = 1;

            using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .LocalSubscribe(99757, x => StatusCallback(nProgress: nProgress/(double) nLVitems)))
            Util.ParallelForEach(
                _LVprojectVM.ItemsCast
                .Where(lvItem => lvItem.CanLoad), new ParallelOptions{ CancellationToken = cts.Token }, lvItem =>
            {
                if (IsAborted)
                {
                    cts.Cancel();
                    return;     // from inner lambda
                }

                var nLVitem = _dictLVtoItemNumber[lvItem];
                var bOnlyHashV1pt0 = true;

                foreach (var tuple in
                    lvItem.ListingFile
                    .ReadLines()
                    .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                    .Select(strLine => strLine.Split('\t'))
                    .Where(asLine => 10 < asLine.Length)

                    .Select(asLine => Tuple.Create(
                        ("" + asLine[1]).ToInt(),                           // Item1 - line number
                        ("" + asLine[FileParse.knColLength]).ToUlong(),     // Item2 - file length
                        asLine[10],                                         // Item3 - 4K hash
                        (11 < asLine.Length) ? asLine[11] : null))          // Item4 - 1M hash

                    .OrderBy(tuple => tuple.Item2))
                {
                    if (IsAborted)
                    {
                        cts.Cancel();
                        return;     // from inner lambda
                    }

                    var keyv1pt0 = FileKeyTuple.FactoryCreate(tuple.Item3, tuple.Item2);

                    if (null == keyv1pt0)
                    {
                        IsAborted = true;
                        cts.Cancel();
                        return;     // from inner lambda
                    }

                    var lookup = 0;
                    FileKeyTuple keyV2 = null;

                    if ((false == _bListingFileWithOnlyHashV1pt0) &&
                        (null != tuple.Item4))
                    {
                        keyV2 = FileKeyTuple.FactoryCreate(tuple.Item4, tuple.Item2);

                        if (null == keyV2)
                        {
                            IsAborted = true;
                            cts.Cancel();
                            return;     // from inner lambda
                        }

                        bOnlyHashV1pt0 = false;
                    }

                    SetLVitemProjectVM(ref lookup, nLVitem);
                    SetLineNumber(ref lookup, tuple.Item1);
#if (DEBUG)
                    Util.Assert(99907, _dictItemNumberToLV[GetLVitemProjectVM(lookup)] == lvItem);
                    Util.Assert(99908, GetLineNumber(lookup) == tuple.Item1);
#endif
                    Insert(dictV1pt0, keyv1pt0, lookup, ref nFolderCount1pt0);

                    if (null != keyV2)
                        Insert(dictV2, keyV2, lookup, ref nFolderCount2);
                }

                if (bOnlyHashV1pt0)
                    _bListingFileWithOnlyHashV1pt0 = true;

                Interlocked.Increment(ref nProgress);
            });

            if (IsAborted)
            {
                cts.Cancel();
                return;     // from inner lambda
            }

            var dt = DateTime.Now;
            var nFolderScorer = 0U;
            var nFolderCount = (uint)(_bListingFileWithOnlyHashV1pt0 ? nFolderCount1pt0 : nFolderCount2);

            _dictFiles =
                (_bListingFileWithOnlyHashV1pt0 ? dictV1pt0 : dictV2)
                .Where(kvp => 1 < kvp.Value.Count)
                .OrderBy(kvp => kvp.Key.Item2)
                .ToDictionary(kvp => kvp.Key, kvp => Tuple.Create(
                    (IReadOnlyList<uint>)new[] { ++nFolderScorer, nFolderCount - nFolderScorer },
                    kvp.Value.AsEnumerable()));

            Util.Assert(99895, 1 == nFolderCount - nFolderScorer, bTraceOnly: true);
            Util.Assert(99896, _dictFiles.Count == nFolderCount - 1, bTraceOnly: true);
            Util.WriteLine("_DictFiles " + (DateTime.Now - dt).TotalMilliseconds + " ms");   // 650 ms 

            // Skip enumerating AllListingsHashV2 when possible: not important, but it'd be a small extra step
            // Otherwise note that _LVprojectVM gets nulled six lines down so the value has to be set by now.
            if (null == _allListingsHashV2)
                _allListingsHashV2 = (false == _bListingFileWithOnlyHashV1pt0);
            else
                Util.Assert(99958, _bListingFileWithOnlyHashV1pt0 != AllListingsHashV2);

            StatusCallback(bDone: true);
            _callbackWR = null;
            _LVprojectVM = null;
            _thread = null;
        }

        void Insert(IDictionary<FileKeyTuple, List<int>> dictionary, FileKeyTuple key, int lookup,
            ref int nFolderCount)
        {
            var ls = dictionary.TryGetValue(key);

            if (null == ls)
            {
                dictionary[key] = new List<int> { lookup };
                return;
            }

            lock (ls)
            {
                if (1 == ls.Count)
                    Interlocked.Increment(ref nFolderCount);

                // jic sorting downstream too at A
                ls.Insert(
                    ls.TakeWhile(nLookup => lookup >= nLookup).Count(),
                    lookup);
            }
        }

        void StatusCallback(bool bDone = false, double nProgress = double.NaN)
        {
            var createFileDictStatus = _callbackWR?.Get(w => w);

            if (null == createFileDictStatus)
            {
                Util.Assert(99868, false);
                return;
            }

            createFileDictStatus.Callback(bDone, nProgress);
        }

        static int GetLVitemProjectVM(int n) => (int)(n & _knItemVMmask) >> 24;
        static int SetLVitemProjectVM(ref int n, int v) => n = GetLineNumber(n) + (v << 24);
        static int GetLineNumber(int n) => n & 0x00FFFFFF;
        static int SetLineNumber(ref int n, int v) => n = (int)(n & _knItemVMmask) + v;

        const uint
            _knItemVMmask = 0xFF000000;

        IReadOnlyDictionary<LVitem_ProjectVM, int>
            _dictLVtoItemNumber = null;
        IReadOnlyDictionary<int, LVitem_ProjectVM>
            _dictItemNumberToLV = null;
        IReadOnlyDictionary<FileKeyTuple, Tuple<IReadOnlyList<uint>, IEnumerable<int>>>
            _dictFiles = null;
        bool
            _bListingFileWithOnlyHashV1pt0 = false;

        WeakReference<ICreateFileDictStatus>
            _callbackWR = null;
        Thread
            _thread = null;

        // 7/1/15 Make this synchronous so NodeDatum can use folder scorer
        LocalDispatcherFrame 
            _blockingFrame = new LocalDispatcherFrame(99881);

        LV_ProjectVM
            _LVprojectVM = null;
    }
}
