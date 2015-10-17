using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System;
using System.Reactive.Linq;
using System.Diagnostics;

namespace DoubleFile
{
    interface ICreateDupeFileDictStatus
    {
        void Callback(bool bDone = false, double nProgress = double.NaN);
    }
    
    partial class DupeFileDictionary : IDisposable
    {
        internal DupeFileDictionary()
        {
            // ProjectFile.OnSavingProject += Serialize;
            // ProjectFile.OnOpenedProject += Deserialize;
        }

        public void Dispose()
        {
            // ProjectFile.OnSavingProject -= Serialize;
            // ProjectFile.OnOpenedProject -= Deserialize;
        }

        internal bool
            IsEmpty => null == _dictDuplicateFiles;

        internal int
            HashColumn => AllListingsHashV2
            ? 11
            : 10;

        internal bool
            AllListingsHashV2
        {
            get
            {
                if (null != _allListingsHashV2)
                    return _allListingsHashV2.Value;

                if (null == _LVprojectVM)
                    return false;

                _allListingsHashV2 =
                    _LVprojectVM.ItemsCast
                    .Where(lvItem => lvItem.CanLoad)
                    .Aggregate(true, (current, lvItem) => current && lvItem.HashV2);

                return _allListingsHashV2.Value;
            }
        }
        bool? _allListingsHashV2 = null;

        internal bool IsDuplicate(int nFileID) => null != _dictDuplicateFiles.TryGetValue(nFileID);

        internal IReadOnlyList<DuplicateStruct>
            GetDuplicates(string[] asFileLine)
        {
            var nHashColumn = HashColumn;

            if (asFileLine.Length <= nHashColumn)
                return null;

            var lsLookup =
                _dictDuplicateFiles?.TryGetValue(HashTuple.FileIndexedIDfromString(
                asFileLine[nHashColumn],
                ("" + asFileLine[7]).ToUlong()));

            if (null == lsLookup)
                return null;

            return
                lsLookup.AsParallel()
                .Select(lookup => new DuplicateStruct
            {
                LVitemProjectVM = _dictItemNumberToLV[GetLVitemProjectVM(lookup)],
                LineNumber = GetLineNumber(lookup)
            })
                .ToList();
        }

        internal DupeFileDictionary
            DoThreadFactory(LV_ProjectVM lvProjectVM, WeakReference<ICreateDupeFileDictStatus> callbackWR)
        {
            _LVprojectVM = lvProjectVM;
            _callbackWR = callbackWR;
            _dictDuplicateFiles = null;
            IsAborted = false;
            _thread = Util.ThreadMake(Go);
            return this;
        }

        internal void
            Abort()
        {
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
            var dictLVtoItemNumber = new Dictionary<LVitemProject_Explorer, int>();
            var dictItemNumberToLV = new Dictionary<int, LVitemProject_Explorer>();

            foreach (var lvItem
                in _LVprojectVM.Items.Cast<LVitemProject_Explorer>()
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
            var dict = new ConcurrentDictionary<int, List<Tuple<int, ulong>>> { };
            var nFolderCount = 1;
            var nHashColumn = HashColumn;

            using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .LocalSubscribe(99757, x => StatusCallback(nProgress: nProgress/(double) nLVitems)))
            Util.ParallelForEach(99655,
                _LVprojectVM.Items.Cast<LVitemProject_Explorer>()
                .Where(lvItem => lvItem.CanLoad), new ParallelOptions{ CancellationToken = cts.Token }, lvItem =>
            {
                if (IsAborted)
                {
                    cts.Cancel();
                    return;     // from inner lambda
                }

                var nLVitem = _dictLVtoItemNumber[lvItem];

                foreach (var tuple in
                    lvItem.ListingFile
                    .ReadLines(99649)
                    .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                    .Select(strLine => strLine.Split('\t'))
                    .Where(asLine => nHashColumn < asLine.Length)

                    .Select(asLine => Tuple.Create(
                        ("" + asLine[1]).ToInt(),                           // Item1 - line number
                        ("" + asLine[FileParse.knColLength]).ToUlong(),     // Item2 - file length
                        asLine[nHashColumn]))                               // Item3 - hash

                    .OrderBy(tuple => tuple.Item2))
                {
                    if (IsAborted)
                    {
                        cts.Cancel();
                        return;     // from inner lambda
                    }

                    var lookup = 0;

                    SetLVitemProjectVM(ref lookup, nLVitem);
                    SetLineNumber(ref lookup, tuple.Item1);
#if (DEBUG)
                    Util.Assert(99907, _dictItemNumberToLV[GetLVitemProjectVM(lookup)] == lvItem);
                    Util.Assert(99908, GetLineNumber(lookup) == tuple.Item1);
#endif
                    var tupleB = Tuple.Create(lookup, tuple.Item2);

                    dict.AddOrUpdate(HashTuple.FileIndexedIDfromString(tuple.Item3, tuple.Item2),
                        x => new List<Tuple<int, ulong>> { tupleB },
                        (x, ls) =>
                    {
                        lock (ls)
                        {
                            if (1 == ls.Count)
                                Interlocked.Increment(ref nFolderCount);

                            // jic sorting downstream too at A
                            ls.Insert(
                                ls.TakeWhile(tupleA => lookup >= tupleA.Item1).Count(),
                                tupleB);
                        }

                        return ls;      // from lambda
                    });
                }

                Interlocked.Increment(ref nProgress);
            });

            if (IsAborted)
            {
                cts.Cancel();
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            foreach (var kvp in dict)
            {
                var nLength = kvp.Value.First().Item2;

                Util.Assert(99958, kvp.Value.All(tuple => tuple.Item2 == nLength)); 
            }

            _dictDuplicateFiles =
                dict
                .Where(kvp => 1 < kvp.Value.Count)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(tuple => tuple.Item1));

            Util.Assert(99896, _dictDuplicateFiles.Count == nFolderCount - 1, bIfDefDebug: true);
            stopwatch.Stop();
            Util.WriteLine("_DictFiles " + stopwatch.ElapsedMilliseconds + " ms");   // 650 ms 
            StatusCallback(bDone: true);
            _callbackWR = null;
            _LVprojectVM = null;
            _thread = null;
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

        static int GetLVitemProjectVM(int n) => (n & (-1 -_knItemVMmask)) >> _kLookupShift;
        static int SetLVitemProjectVM(ref int n, int v) => n = GetLineNumber(n) + (v << _kLookupShift);
        static int GetLineNumber(int n) => n & _knItemVMmask;
        static int SetLineNumber(ref int n, int v) => n = (n & (-1 - _knItemVMmask)) + v;
        const int _knItemVMmask = ((1 << _kLookupShift) - 1);
        const int _kLookupShift = 24;

        IReadOnlyDictionary<LVitemProject_Explorer, int>
            _dictLVtoItemNumber = null;
        IReadOnlyDictionary<int, LVitemProject_Explorer>
            _dictItemNumberToLV = null;
        IReadOnlyDictionary<int, IEnumerable<int>>
            _dictDuplicateFiles = null;

        WeakReference<ICreateDupeFileDictStatus>
            _callbackWR = null;
        Thread
            _thread = null;

        LV_ProjectVM
            _LVprojectVM = null;
    }
}
