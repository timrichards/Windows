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
            ProjectFile.OnOpenedProject += Deserialize;
            ProjectFile.OnSavingProject += Serialize;
        }

        public void Dispose()
        {
            ProjectFile.OnOpenedProject -= Deserialize;
            ProjectFile.OnSavingProject -= Serialize;
        }

        internal void Clear() { m_DictFiles.Clear(); }
        internal bool IsEmpty { get { return m_DictFiles.Count == 0; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strLine"></param>
        /// <param name="strListingFile">Only used to prevent adding queried item to the returned list.</param>
        /// <returns>null if bad strLine. Otherwise always non-null, even if empty.</returns>
        internal IEnumerable<Duplicate> GetDuplicates(string strLine, string strListingFile = null)
        {
            if (false == strLine.StartsWith(FileParse.ksLineType_File))
            {
                MBox.Assert(0, false);
                return null;
            }

            var asLine = strLine.Split('\t');
            var key = new FileKey(asLine[10], asLine[7]);
            var lsRet = new List<Duplicate>();

            if (false == m_DictFiles.ContainsKey(key))
            {
                return lsRet;
            }

            var lsDupes = m_DictFiles[key];
            var nLine = int.Parse(asLine[1]);

            foreach (var lookup in lsDupes)
            {
                Duplicate dupe = new Duplicate();

                dupe.LVitemProjectVM = DictItemNumberToLV[GetLVitemProjectVM(lookup)];
                dupe.LineNumber = GetLineNumber(lookup);

                if ((dupe.LVitemProjectVM.ListingFile != strListingFile) ||     // exactly once every query
                    (dupe.LineNumber != nLine))
                {
                    lsRet.Add(dupe);
                }
            }

            return lsRet;
        }

        internal string Serialize()
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

        internal void Deserialize()
        {
            if (false == File.Exists(ksSerializeFile))
            {
                return;
            }

            using (var reader = new StreamReader(ksSerializeFile, false))
            {
                string strLine = null;
                m_DictFiles.Clear();

                while ((strLine = reader.ReadLine()) != null)
                {
                    var asLine = strLine.Split('\t');

                    m_DictFiles[new FileKey(asLine[0])] =
                        asLine.Skip(1).Select(s => Convert.ToInt32(s)).ToList();
                }
            }
        }

        internal FileDictionary DoThreadFactory(LV_ProjectVM lvProjectVM, CreateFileDictStatusDelegate statusCallback)
        {
            LVprojectVM = lvProjectVM;
            m_statusCallback = statusCallback;
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
            return this;
        }

        internal void Join()
        {
            m_thread.Join();
        }

        internal void Abort()
        {
            m_bThreadAbort = true;
            m_thread.Abort();
        }

        internal bool IsAborted { get { return m_bThreadAbort; } }

        void Go()
        {
            if (LVprojectVM == null)
            {
                return;
            }

            int nLVitems = 0;

            DictLVtoItemNumber.Clear();
            DictItemNumberToLV.Clear();

            foreach (var lvItem in LVprojectVM.ItemsCast.OrderBy(lvItem => lvItem.SourcePath))
            {
                DictLVtoItemNumber.Add(lvItem, nLVitems);
                DictItemNumberToLV.Add(nLVitems, lvItem);
                ++nLVitems;
            }

            int nLVitems_A = 0;
            ConcurrentDictionary<FileKey, List<int>> dictFiles = new ConcurrentDictionary<FileKey, List<int>>();

            bool bLinq = false;

            if (bLinq)
            {
                var lsieKVP = new List<IEnumerable<KeyValuePair<FileKey, int>>>();

                Parallel.ForEach(LVprojectVM.ItemsCast, (lvItem =>
                {
                    var nLVitem = DictLVtoItemNumber[lvItem];

                    var iesLines = File.ReadLines(lvItem.ListingFile)
                         .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                         .Select(strLine => strLine.Split('\t'))
                         .Where(asLine => asLine.Length > 10)
                         .Select(asLine =>
                         {
                             Interlocked.Increment(ref m_nFilesTotal);

                             int lookup = 0;

                             SetLVitemProjectVM(ref lookup, nLVitem);
                             SetLineNumber(ref lookup, int.Parse(asLine[1]));

                             return new KeyValuePair<FileKey, int>
                             (
                                  new FileKey(asLine[10], asLine[7]),
                                  lookup
                             );
                         });
                    //                     .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    lsieKVP.Add(iesLines);
                }));

                m_DictFiles = lsieKVP
                    .SelectMany(g => g)
                    .GroupBy(kvp => kvp.Key)
                    .Select(g =>
                    {
                        Interlocked.Increment(ref m_nFilesProgress);

                        return new KeyValuePair<FileKey, IEnumerable<int>>
                        (
                            g.Key,
                            g.Select(kvp => kvp.Value)
                        );
                    })
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            var tsStatus = new TimeSpan(0, 0, 0, 0, 200);
            var tmrStatus = new Timer(new TimerCallback((Object state) =>
            {
                if (nLVitems == nLVitems_A)
                {
                    m_statusCallback(nProgress: m_nFilesProgress / (double)m_nFilesTotal);
                }
            }), null, tsStatus, tsStatus);

            TimeSpan[] aTimeSpan = new TimeSpan[8];

            Parallel.ForEach(LVprojectVM.ItemsCast, (lvItem =>
            {
                // very low-cost block, even with the Count() iterator
                var iesLines = File.ReadLines(lvItem.ListingFile)
                     .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                     .Select(strLine => strLine.Split('\t'))
                     .Where(asLine => asLine.Length > 10);

                var nLVitem = DictLVtoItemNumber[lvItem];

                Interlocked.Add(ref m_nFilesTotal, iesLines.Count());
                Interlocked.Increment(ref nLVitems_A);

                iesLines.Select(asLine =>
                {
                    DateTime dt0 = DateTime.Now;
                    Interlocked.Increment(ref m_nFilesProgress);
                    aTimeSpan[0] += DateTime.Now - dt0;

                    DateTime dt1 = DateTime.Now;
                    var key = new FileKey(asLine[10], asLine[7]);
                    aTimeSpan[1] += DateTime.Now - dt1;
                    int lookup = 0;

                    DateTime dt2 = DateTime.Now;
                    SetLVitemProjectVM(ref lookup, nLVitem);
                    aTimeSpan[2] += DateTime.Now - dt2;
                    DateTime dt3 = DateTime.Now;
                    SetLineNumber(ref lookup, int.Parse(asLine[1]));
                    aTimeSpan[3] += DateTime.Now - dt3;
                    DateTime dt4 = DateTime.Now;
                    var bContains = dictFiles.ContainsKey(key);             // this takes the most time by far
                    aTimeSpan[4] += DateTime.Now - dt4;

                    if (false == bContains)
                    {
                        DateTime dt5 = DateTime.Now;
                        var listFiles = new List<int>();
                        aTimeSpan[5] += DateTime.Now - dt5;

                        DateTime dt6 = DateTime.Now;
                        listFiles.Add(lookup);
                        aTimeSpan[6] += DateTime.Now - dt6;
                        DateTime dt7 = DateTime.Now;
                        dictFiles[key] = listFiles;                         // this takes the most time by far
                        aTimeSpan[7] += DateTime.Now - dt7;
                    }
                    else
                    {
                        // This section takes no time at all: sure beats sorting the list afterward
                        var ls = dictFiles[key];

                        lock (ls)
                        {
                            var nIndex = 0;

                            foreach (var nLookup in ls)
                            {
                                if (lookup < nLookup)
                                    break;

                                ++nIndex;
                            }

                            ls.Insert(nIndex, lookup);
                        }
                    }

                    return false;   // must return something from select lambda
                }).Count();         // must kick it in the butt
            }));

            m_nFilesProgress = 0;
            m_nFilesTotal = dictFiles.Count;
            m_statusCallback(nProgress: m_nFilesProgress++ / (double)m_nFilesTotal);

            m_DictFiles = dictFiles
                .Where(kvp => kvp.Value.Count > 1)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable());

            m_statusCallback(bDone: true);

            foreach (var ts in aTimeSpan)
            {
                UtilProject.WriteLine(ts.ToString());
            }
        }

        readonly string ksSerializeFile = ProjectFile.TempPath + "_DuplicateFiles_";

        Dictionary<LVitem_ProjectVM, int> DictLVtoItemNumber = new Dictionary<LVitem_ProjectVM, int>();
        Dictionary<int, LVitem_ProjectVM> DictItemNumberToLV = new Dictionary<int, LVitem_ProjectVM>();
        Dictionary<FileKey, IEnumerable<int>> m_DictFiles = new Dictionary<FileKey, IEnumerable<int>>();

        const uint knItemVMmask = 0xFF000000;
        int GetLVitemProjectVM(int n) { return (int)(n & knItemVMmask) >> 24; }
        int SetLVitemProjectVM(ref int n, int v) { return n = n + (v << 24); }
        int GetLineNumber(int n) { return n & 0xFFFFFF; }
        int SetLineNumber(ref int n, int v) { return n = (int)(n & knItemVMmask) + v; }

        CreateFileDictStatusDelegate m_statusCallback = null;
        Thread m_thread = null;
        protected bool m_bThreadAbort = false;
        LV_ProjectVM LVprojectVM = null;
        long m_nFilesTotal = 0;
        long m_nFilesProgress = 0;
    }
}
