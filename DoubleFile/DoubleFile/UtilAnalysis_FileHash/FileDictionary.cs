﻿using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using System;

namespace DoubleFile
{
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strLine"></param>
        /// <param name="strListingFile">Only used to prevent adding queried item to the returned list.</param>
        /// <returns>null if bad strLine. Otherwise always non-null, even if empty.</returns>
        internal IReadOnlyList<DuplicateStruct> GetDuplicates(
            string strLine,
            out string strFilename,
            string strListingFile = null)
        {
            strFilename = null;

            var lsRet = new List<DuplicateStruct>();

            if (false == strLine.StartsWith(FileParse.ksLineType_File))
            {
                MBoxStatic.Assert(99956, false);
                return lsRet;
            }

            var asLine = strLine.Split('\t');

            strFilename = asLine[3];

            if (10 >= asLine.Length)
                return lsRet;

            var key = new FileKeyStruct(asLine[10], asLine[7]);

            if (null == _DictFiles)
            {
                return lsRet;
            }

            IEnumerable<int> lsDupes = null;

            if (false == _DictFiles.TryGetValue(key, out lsDupes))
            {
                return lsRet;
            }

            var nLine = int.Parse(asLine[1]);

            lsRet.AddRange(lsDupes.Select(lookup => new DuplicateStruct
            {
                LVitemProjectVM = _DictItemNumberToLV[GetLVitemProjectVM(lookup)],
                LineNumber = GetLineNumber(lookup)
            }).Where(dupe => (dupe.LVitemProjectVM.ListingFile != strListingFile) ||    // exactly once every query
                             (dupe.LineNumber != nLine)));

            return lsRet;
        }

        internal FileDictionary DoThreadFactory(LV_ProjectVM lvProjectVM,
            CreateFileDictStatusDelegate statusCallback)
        {
            _LVprojectVM = lvProjectVM;
            _statusCallback = statusCallback;
            _DictFiles = null;
            IsAborted = false;
            _thread = new Thread(Go) { IsBackground = true };
            _thread.Start();
            return this;
        }

        internal void Abort()
        {
            IsAborted = true;
            _thread.Abort();
        }

        internal bool IsAborted { get; private set; }

        void Go()
        {
            if (_LVprojectVM == null)
            {
                return;
            }

            var nLVitems = 0;

            _DictLVtoItemNumber.Clear();
            _DictItemNumberToLV.Clear();

            foreach (var lvItem in _LVprojectVM.ItemsCast.OrderBy(lvItem => lvItem.SourcePath))
            {
                _DictLVtoItemNumber.Add(lvItem, nLVitems);
                _DictItemNumberToLV.Add(nLVitems, lvItem);
                ++nLVitems;
            }

            var nLVitems_A = 0;

            using (new SDL_Timer(() =>
            {
                if (nLVitems == nLVitems_A)
                {
                    _statusCallback(nProgress: _nFilesProgress/(double) _nFilesTotal);
                }
            }).Start())
            {
                var dictFiles = new ConcurrentDictionary<FileKeyStruct, List<int>>();

                Parallel.ForEach(_LVprojectVM.ItemsCast, lvItem =>
                {
                    if (IsAborted)
                        return;

                    var iesLines =
                        File
                        .ReadLines(lvItem.ListingFile)
                        .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                        .Select(strLine => strLine.Split('\t'))
                        .Where(asLine => asLine.Length > 10);

                    var nLVitem = _DictLVtoItemNumber[lvItem];

                    Interlocked.Add(ref _nFilesTotal, iesLines.Count());
                    Interlocked.Increment(ref nLVitems_A);

                    foreach (var asLine in iesLines)
                    {
                        if (IsAborted)
                            return;

                        Interlocked.Increment(ref _nFilesProgress);

                        var key = new FileKeyStruct(asLine[10], asLine[7]);
                        var lookup = 0;

                        SetLVitemProjectVM(ref lookup, nLVitem);
                        SetLineNumber(ref lookup, int.Parse(asLine[1]));

                        List<int> ls = null;

                        if (false == dictFiles.TryGetValue(key, out ls))
                        {
                            dictFiles[key] = new List<int> {lookup};
                        }
                        else
                        {
                            lock (ls)
                            {
                                ls.Insert(ls.TakeWhile(nLookup => lookup >= nLookup).Count(),
                                    lookup);
                            }
                        }
                    }
                });

                if (IsAborted)
                {
                    return;
                }

                _DictFiles =
                    dictFiles
                    .Where(kvp => kvp.Value.Count > 1)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable());
            }

            _statusCallback(bDone: true);
            _statusCallback = null;
            _LVprojectVM = null;
            _thread = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        string Serialize()
        {
            using (var writer = new StreamWriter(_ksSerializeFile, false))
            {
                foreach (var kvp in _DictFiles)
                {
                    writer.Write(kvp.Key);

                    foreach (var ls in kvp.Value)
                    {
                        writer.Write("\t" + ls);
                    }

                    writer.WriteLine();
                }
            }

            return _ksSerializeFile;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        void Deserialize()
        {
            if (false == File.Exists(_ksSerializeFile))
            {
                return;
            }

            using (var reader = new StreamReader(_ksSerializeFile, false))
            {
                string strLine = null;
                _DictFiles = new Dictionary<FileKeyStruct,IEnumerable<int>>();

                while ((strLine = reader.ReadLine()) != null)
                {
                    var asLine = strLine.Split('\t');

                    _DictFiles[new FileKeyStruct(asLine[0])] =
                        asLine
                        .Skip(1)
                        .Select(s => Convert.ToInt32(s));
                }
            }
        }

        static int GetLVitemProjectVM(int n) { return (int)(n & _knItemVMmask) >> 24; }
        static int SetLVitemProjectVM(ref int n, int v) { return n = GetLineNumber(n) + (v << 24); }
        static int GetLineNumber(int n) { return n & 0xFFFFFF; }
        static int SetLineNumber(ref int n, int v) { return n = (int)(n & _knItemVMmask) + v; }

        const uint
            _knItemVMmask = 0xFF000000;

        readonly string
            _ksSerializeFile = ProjectFile.TempPath + "_DuplicateFiles._";

        readonly Dictionary<LVitem_ProjectVM, int>
            _DictLVtoItemNumber = new Dictionary<LVitem_ProjectVM, int>();
        readonly Dictionary<int, LVitem_ProjectVM>
            _DictItemNumberToLV = new Dictionary<int, LVitem_ProjectVM>();
        Dictionary<FileKeyStruct, IEnumerable<int>>
            _DictFiles = null;

        CreateFileDictStatusDelegate
            _statusCallback = null;
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
