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
        }

        internal bool IsEmpty { get { return null == _DictFiles; } }
        internal void ResetAbortFlag() { IsAborted = false; }

        internal IEnumerable<DuplicateStruct> GetDuplicates(string[] asFileLine)
        {
            if (null == _DictFiles)
                return null;

            if (10 >= asFileLine.Length)
                return null;

            var key = new FileKeyTuple(asFileLine[10], ulong.Parse(asFileLine[7]));
            IEnumerable<int> lsDupes = null;

            if (false == _DictFiles.TryGetValue(key, out lsDupes))
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
            _thread = new Thread(Go) { IsBackground = true };
            _thread.Start();
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
                    .OrderBy(lvItem => lvItem.SourcePath))
            {
                _DictLVtoItemNumber.Add(lvItem, nLVitems);
                _DictItemNumberToLV.Add(nLVitems, lvItem);
                ++nLVitems;
            }

            var nLVitems_A = 0;

            using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                .Subscribe(x =>
            {
                if (nLVitems == nLVitems_A)
                    StatusCallback(nProgress: _nFilesProgress/(double) _nFilesTotal);
            }))
            {
                var dictFiles = new ConcurrentDictionary<FileKeyTuple, List<int>>();

                Parallel.ForEach(
                    _LVprojectVM.ItemsCast
                    .Where(lvItem => lvItem.CanLoad), lvItem =>
                {
                    if (IsAborted)
                        return;

                    var arrTuples =
                        File
                        .ReadLines(lvItem.ListingFile)
                        .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                        .Select(strLine => strLine.Split('\t'))
                        .Where(asLine => 10 < asLine.Length)
                        .Select(asLine => Tuple.Create(int.Parse(asLine[1]), ulong.Parse(asLine[7]), asLine[10]))
                        .ToArray();

                    var nLVitem = _DictLVtoItemNumber[lvItem];

                    Interlocked.Add(ref _nFilesTotal, arrTuples.Length);
                    Interlocked.Increment(ref nLVitems_A);

                    foreach (var tuple in arrTuples)
                    {
                        if (IsAborted)
                            return;

                        Interlocked.Increment(ref _nFilesProgress);

                        var key = new FileKeyTuple(tuple.Item3, tuple.Item2);
                        var lookup = 0;

                        SetLVitemProjectVM(ref lookup, nLVitem);
                        SetLineNumber(ref lookup, tuple.Item1);
#if (DEBUG)
                        MBoxStatic.Assert(99907, _DictItemNumberToLV[GetLVitemProjectVM(lookup)] == lvItem);
                        MBoxStatic.Assert(99908, GetLineNumber(lookup) == tuple.Item1);
#endif
                        List<int> ls = null;

                        if (dictFiles.TryGetValue(key, out ls))
                        {
                            lock (ls)                      // jic sorting downstream too at A
                            {
                                ls.Insert(ls.TakeWhile(nLookup => lookup >= nLookup).Count(),
                                    lookup);
                            }
                        }
                        else
                        {
                            dictFiles[key] = new List<int> { lookup };
                        }
                    }
                });

                if (IsAborted)
                    return;

                _DictFiles =
                    dictFiles
                    .Where(kvp => kvp.Value.Count > 1)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable());
            }

             StatusCallback(bDone: true);
             _callbackWR = null;
            _LVprojectVM = null;
            _thread = null;
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

                while ((strLine = reader.ReadLine()) != null)
                {
                    var asLine = strLine.Split('\t');
                    var asKey = asLine[0].Split(' ');

                    _DictFiles[new FileKeyTuple(asKey[0], ulong.Parse(asKey[1]))] =
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

        readonly Dictionary<LVitem_ProjectVM, int>
            _DictLVtoItemNumber = new Dictionary<LVitem_ProjectVM, int>();
        readonly Dictionary<int, LVitem_ProjectVM>
            _DictItemNumberToLV = new Dictionary<int, LVitem_ProjectVM>();
        Dictionary<FileKeyTuple, IEnumerable<int>>
            _DictFiles = null;

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
