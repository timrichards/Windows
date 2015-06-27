using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
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

        internal void Clear()
        {
            _DictFiles = null;
            _bListingFileWithOnlyHashV1pt0 = false;
        }

        internal bool IsEmpty { get { return null == _DictFiles; } }
        internal void ResetAbortFlag() { IsAborted = false; }

        internal bool AllListingsHashV2
        {
            get
            {
                if (null != _allListingsHashV2)
                    return _allListingsHashV2.Value;

                if (null == _LVprojectVM)
                {
                    MBoxStatic.Assert(99959, false);
                    Util.Block(1000);

                    if (null == _LVprojectVM)
                    {
                        MBoxStatic.Assert(99938, false);
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

        internal IEnumerable<DuplicateStruct> GetDuplicates(string[] asFileLine)
        {
            var nHashColumn =
                _bListingFileWithOnlyHashV1pt0
                ? 10
                : 11;

            if (null == _DictFiles)
                return null;

            if (nHashColumn >= asFileLine.Length)
                return null;

            var lsDupes = _DictFiles.TryGetValue(FileKeyTuple.FactoryCreate(
                asFileLine[nHashColumn],
                ("" + asFileLine[7]).ToUlong()));

            if (null == lsDupes)
                return null;

            return
                lsDupes.AsParallel()
                .Select(lookup => new DuplicateStruct
            {
                LVitemProjectVM = _DictItemNumberToLV[GetLVitemProjectVM(lookup)],
                LineNumber = GetLineNumber(lookup)
            });
        }

        internal FileDictionary DoThreadFactory(LV_ProjectVM lvProjectVM, WeakReference<ICreateFileDictStatus> callbackWR)
        {
            _LVprojectVM = lvProjectVM;
            _callbackWR = callbackWR;
            _DictFiles = null;
            IsAborted = false;
            _thread = Util.ThreadMake(Go);
            return this;
        }

        internal void Abort()
        {
            if (IsAborted)
                return;

            IsAborted = true;

            if (null != _thread)
                _thread.Abort();
        }

        internal bool IsAborted { get; private set; }

        void Go()
        {
            if (null == _LVprojectVM)
                return;

            var nLVitems = 0;

            _DictLVtoItemNumber.Clear();
            _DictItemNumberToLV.Clear();

            foreach (var lvItem
                in _LVprojectVM.ItemsCast
                .Where(lvItem => lvItem.CanLoad)
                .OrderBy(lvItem => lvItem.SourcePath))
            {
                _DictLVtoItemNumber.Add(lvItem, nLVitems);
                _DictItemNumberToLV.Add(nLVitems, lvItem);
                ++nLVitems;
            }

            var nLVitems_A = 0;
            var cts = new CancellationTokenSource();

            using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .Subscribe(x =>
            {
                if (nLVitems == nLVitems_A)
                    StatusCallback(nProgress: _nFilesProgress/(double) _nFilesTotal);
            }))
            {
                var dictV1pt0 = new ConcurrentDictionary<FileKeyTuple, List<int>> { };
                var dictV2 = new ConcurrentDictionary<FileKeyTuple, List<int>> { };

                Util.ParallelForEach(
                    _LVprojectVM.ItemsCast
                    .Where(lvItem => lvItem.CanLoad), new ParallelOptions{ CancellationToken = cts.Token }, lvItem =>
                {
                    if (IsAborted)
                    {
                        cts.Cancel();
                        return;     // from inner lambda
                    }

                    var arrTuples =
                        File
                        .ReadLines(lvItem.ListingFile)
                        .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                        .Select(strLine => strLine.Split('\t'))
                        .Where(asLine => 10 < asLine.Length)
                        .Select(asLine => Tuple.Create(("" + asLine[1]).ToInt(), ("" + asLine[FileParse.knColLength]).ToUlong(), asLine[10],
                            (11 < asLine.Length) ? asLine[11] : null))
                        .ToArray();

                    var nLVitem = _DictLVtoItemNumber[lvItem];
                    var bOnlyHashV1pt0 = true;

                    Interlocked.Add(ref _nFilesTotal, arrTuples.Length);
                    Interlocked.Increment(ref nLVitems_A);

                    foreach (var tuple in arrTuples)
                    {
                        if (IsAborted)
                        {
                            cts.Cancel();
                            return;     // from inner lambda
                        }

                        Interlocked.Increment(ref _nFilesProgress);

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
                        MBoxStatic.Assert(99907, _DictItemNumberToLV[GetLVitemProjectVM(lookup)] == lvItem);
                        MBoxStatic.Assert(99908, GetLineNumber(lookup) == tuple.Item1);
#endif
                        Insert(dictV1pt0, keyv1pt0, lookup);

                        if (null != keyV2)
                            Insert(dictV2, keyV2, lookup);
                    }

                    if (bOnlyHashV1pt0)
                        _bListingFileWithOnlyHashV1pt0 = true;
                });

                if (IsAborted)
                {
                    cts.Cancel();
                    return;     // from inner lambda
                }

                _DictFiles =
                    (_bListingFileWithOnlyHashV1pt0 ? dictV1pt0 : dictV2)
                    .Where(kvp => kvp.Value.Count > 1)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable());

                // Skip enumerating AllListingsHashV2 when possible: not important, but it'd be a small extra step
                // Otherwise note that _LVprojectVM gets nulled six lines down so the value has to be set by now.
                if (null == _allListingsHashV2)
                    _allListingsHashV2 = (false == _bListingFileWithOnlyHashV1pt0);
                else
                    MBoxStatic.Assert(99958, _bListingFileWithOnlyHashV1pt0 != AllListingsHashV2);
            }

             StatusCallback(bDone: true);
             _callbackWR = null;
            _LVprojectVM = null;
            _thread = null;
        }

        void Insert(IDictionary<FileKeyTuple, List<int>> dictionary, FileKeyTuple key, int lookup)
        {
            var ls = dictionary.TryGetValue(key);

            if (null != ls)
            {
                lock (ls)                      // jic sorting downstream too at A
                {
                    ls.Insert(ls.TakeWhile(nLookup => lookup >= nLookup).Count(),
                        lookup);
                }
            }
            else
            {
                dictionary[key] = new List<int> { lookup };
            }
        }

        void StatusCallback(bool bDone = false, double nProgress = double.NaN)
        {
            if (null == _callbackWR)
            {
                MBoxStatic.Assert(99869, false);
                return;
            }

            ICreateFileDictStatus createFileDictStatus = null;

            _callbackWR.TryGetTarget(out createFileDictStatus);

            if (null == createFileDictStatus)
            {
                MBoxStatic.Assert(99868, false);
                return;
            }

            createFileDictStatus.Callback(bDone, nProgress);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        string Serialize()
        {
            using (var writer = new StreamWriter(_ksSerializeFile, false))
            {
                foreach (var kvp in _DictFiles)
                {
                    writer.Write(kvp.Key.Item1 + " " + kvp.Key.Item2);

                    foreach (var ls in kvp.Value)
                        writer.Write("\t" + ls);

                    writer.WriteLine();
                }
            }

            return _ksSerializeFile;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        void Deserialize()
        {
            if (false == File.Exists(_ksSerializeFile))
                return;

            using (var reader = new StreamReader(_ksSerializeFile, false))
            {
                string strLine = null;

                _DictFiles = new Dictionary<FileKeyTuple, IEnumerable<int>>();

                while (null != (strLine = reader.ReadLine()))
                {
                    var asLine = strLine.Split('\t');
                    var asKey = asLine[0].Split(' ');

                    _DictFiles[FileKeyTuple.FactoryCreate(asKey[0], ("" + asKey[1]).ToUlong())] =
                        asLine
                        .Skip(1)
                        .Select(s => Convert.ToInt32(s));
                }
            }
        }

        static int GetLVitemProjectVM(int n) { return (int)(n & _knItemVMmask) >> 24; }
        static int SetLVitemProjectVM(ref int n, int v) { return n = GetLineNumber(n) + (v << 24); }
        static int GetLineNumber(int n) { return n & 0x00FFFFFF; }
        static int SetLineNumber(ref int n, int v) { return n = (int)(n & _knItemVMmask) + v; }

        const uint
            _knItemVMmask = 0xFF000000;

        readonly string
            _ksSerializeFile = ProjectFile.TempPath + "_DuplicateFiles._";

        readonly IDictionary<LVitem_ProjectVM, int>
            _DictLVtoItemNumber = new Dictionary<LVitem_ProjectVM, int> { };
        readonly IDictionary<int, LVitem_ProjectVM>
            _DictItemNumberToLV = new Dictionary<int, LVitem_ProjectVM> { };
        IDictionary<FileKeyTuple, IEnumerable<int>>
            _DictFiles = null;
        bool
            _bListingFileWithOnlyHashV1pt0 = false;

        WeakReference<ICreateFileDictStatus>
            _callbackWR = null;
        Thread
            _thread = null;
        LV_ProjectVM
            _LVprojectVM = null;
        long
            _nFilesTotal = 0;
        long
            _nFilesProgress = 0;
    }
}
