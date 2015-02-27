using System.Threading;
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
            m_DictFiles = null;
        }

        internal bool IsEmpty { get { return null == m_DictFiles; } }
        internal void ResetAbortFlag() { IsAborted = false; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strLine"></param>
        /// <param name="strListingFile">Only used to prevent adding queried item to the returned list.</param>
        /// <returns>null if bad strLine. Otherwise always non-null, even if empty.</returns>
        internal IReadOnlyList<DuplicateStruct> GetDuplicates(string strLine, string strListingFile = null)
        {
            if (false == strLine.StartsWith(FileParse.ksLineType_File))
            {
                MBoxStatic.Assert(99956, false);
                return null;
            }

            var asLine = strLine.Split('\t');

            if (10 >= asLine.Length)
                return null;

            var key = new FileKeyStruct(asLine[10], asLine[7]);
            var lsRet = new List<DuplicateStruct>();

            if (null == m_DictFiles)
            {
                return lsRet;
            }

            IEnumerable<int> lsDupes = null;

            if (false == m_DictFiles.TryGetValue(key, out lsDupes))
            {
                return lsRet;
            }

            var nLine = int.Parse(asLine[1]);

            lsRet.AddRange(lsDupes.Select(lookup => new DuplicateStruct
            {
                LVitemProjectVM = DictItemNumberToLV[GetLVitemProjectVM(lookup)],
                LineNumber = GetLineNumber(lookup)
            }).Where(dupe => (dupe.LVitemProjectVM.ListingFile != strListingFile) ||    // exactly once every query
                             (dupe.LineNumber != nLine)));

            return lsRet;
        }

        internal FileDictionary DoThreadFactory(LV_ProjectVM lvProjectVM,
            CreateFileDictStatusDelegate statusCallback)
        {
            LVprojectVM = lvProjectVM;
            m_statusCallback = statusCallback;
            m_DictFiles = null;
            IsAborted = false;
            m_thread = new Thread(Go) { IsBackground = true };
            m_thread.Start();
            return this;
        }

        internal void Abort()
        {
            IsAborted = true;
            m_thread.Abort();
        }

        internal bool IsAborted { get; private set; }

        void Go()
        {
            if (LVprojectVM == null)
            {
                return;
            }

            var nLVitems = 0;

            DictLVtoItemNumber.Clear();
            DictItemNumberToLV.Clear();

            foreach (var lvItem in LVprojectVM.ItemsCast.OrderBy(lvItem => lvItem.SourcePath))
            {
                DictLVtoItemNumber.Add(lvItem, nLVitems);
                DictItemNumberToLV.Add(nLVitems, lvItem);
                ++nLVitems;
            }

            var nLVitems_A = 0;

            using (new SDL_Timer(() =>
            {
                if (nLVitems == nLVitems_A)
                {
                    m_statusCallback(nProgress: m_nFilesProgress/(double) m_nFilesTotal);
                }
            }).Start())
            {
                var dictFiles = new ConcurrentDictionary<FileKeyStruct, List<int>>();

                Parallel.ForEach(LVprojectVM.ItemsCast, lvItem =>
                {
                    if (IsAborted)
                        return;

                    var iesLines =
                        File
                        .ReadLines(lvItem.ListingFile)
                        .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                        .Select(strLine => strLine.Split('\t'))
                        .Where(asLine => asLine.Length > 10);

                    var nLVitem = DictLVtoItemNumber[lvItem];

                    Interlocked.Add(ref m_nFilesTotal, iesLines.Count());
                    Interlocked.Increment(ref nLVitems_A);

                    foreach (var asLine in iesLines)
                    {
                        if (IsAborted)
                            return;

                        Interlocked.Increment(ref m_nFilesProgress);

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

                m_DictFiles =
                    dictFiles
                    .Where(kvp => kvp.Value.Count > 1)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable());
            }

            m_statusCallback(bDone: true);
            m_statusCallback = null;
            LVprojectVM = null;
            m_thread = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        string Serialize()
        {
            using (var writer = new StreamWriter(ksSerializeFile, false))
            {
                foreach (var kvp in m_DictFiles)
                {
                    writer.Write(kvp.Key);

                    foreach (var ls in kvp.Value)
                    {
                        writer.Write("\t" + ls);
                    }

                    writer.WriteLine();
                }
            }

            return ksSerializeFile;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        void Deserialize()
        {
            if (false == File.Exists(ksSerializeFile))
            {
                return;
            }

            using (var reader = new StreamReader(ksSerializeFile, false))
            {
                string strLine = null;
                m_DictFiles = new Dictionary<FileKeyStruct,IEnumerable<int>>();

                while ((strLine = reader.ReadLine()) != null)
                {
                    var asLine = strLine.Split('\t');

                    m_DictFiles[new FileKeyStruct(asLine[0])] =
                        asLine
                        .Skip(1)
                        .Select(s => Convert.ToInt32(s));
                }
            }
        }

        readonly string ksSerializeFile = ProjectFile.TempPath + "_DuplicateFiles._";

        readonly Dictionary<LVitem_ProjectVM, int> DictLVtoItemNumber = new Dictionary<LVitem_ProjectVM, int>();
        readonly Dictionary<int, LVitem_ProjectVM> DictItemNumberToLV = new Dictionary<int, LVitem_ProjectVM>();
        Dictionary<FileKeyStruct, IEnumerable<int>> m_DictFiles = null;

        const uint knItemVMmask = 0xFF000000;
        static int GetLVitemProjectVM(int n) { return (int)(n & knItemVMmask) >> 24; }
        static int SetLVitemProjectVM(ref int n, int v) { return n = GetLineNumber(n) + (v << 24); }
        static int GetLineNumber(int n) { return n & 0xFFFFFF; }
        static int SetLineNumber(ref int n, int v) { return n = (int)(n & knItemVMmask) + v; }

        CreateFileDictStatusDelegate m_statusCallback = null;
        Thread m_thread = null;
        LV_ProjectVM LVprojectVM = null;
        long m_nFilesTotal = 0;
        long m_nFilesProgress = 0;
    }
}
