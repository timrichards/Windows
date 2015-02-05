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
            ProjectFile.OnOpenedProject += OpenedProject;
            //ProjectFile.OnSavingProject += Serialize;
        }

        public void Dispose()
        {
            ProjectFile.OnOpenedProject -= OpenedProject;
           // ProjectFile.OnSavingProject -= Serialize;
        }

        internal void Clear() { m_DictFiles.Clear(); }
        internal bool IsEmpty { get { return m_DictFiles.Count == 0; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strLine"></param>
        /// <param name="strListingFile">Only used to prevent adding queried item to the returned list.</param>
        /// <returns>null if bad strLine. Otherwise always non-null, even if empty.</returns>
        internal IEnumerable<DuplicateStruct> GetDuplicates(string strLine, string strListingFile = null)
        {
            if (false == strLine.StartsWith(FileParse.ksLineType_File))
            {
                MBox.Assert(0, false);
                return null;
            }

            var asLine = strLine.Split('\t');
            var key = new FileKeyStruct(asLine[10], asLine[7]);
            var lsRet = new List<DuplicateStruct>();

            if (false == m_DictFiles.ContainsKey(key))
            {
                return lsRet;
            }

            var lsDupes = m_DictFiles[key];
            var nLine = int.Parse(asLine[1]);

            foreach (var lookup in lsDupes)
            {
                DuplicateStruct dupe = new DuplicateStruct();

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
            var dictFiles = new ConcurrentDictionary<FileKeyStruct, List<int>>();
            var tsStatus = new TimeSpan(0, 0, 0, 0, 200);
            var tmrStatus = new Timer(new TimerCallback((Object state) =>
            {
                if (nLVitems == nLVitems_A)
                {
                    m_statusCallback(nProgress: m_nFilesProgress / (double)m_nFilesTotal);
                }
            }), null, tsStatus, tsStatus);

            Parallel.ForEach(LVprojectVM.ItemsCast, (lvItem =>
            {
                var iesLines = File.ReadLines(lvItem.ListingFile)
                     .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File))
                     .Select(strLine => strLine.Split('\t'))
                     .Where(asLine => asLine.Length > 10);

                var nLVitem = DictLVtoItemNumber[lvItem];

                Interlocked.Add(ref m_nFilesTotal, iesLines.Count());
                Interlocked.Increment(ref nLVitems_A);

                foreach (var asLine in iesLines)
                {
                    Interlocked.Increment(ref m_nFilesProgress);

                    var key = new FileKeyStruct(asLine[10], asLine[7]);
                    int lookup = 0;

                    SetLVitemProjectVM(ref lookup, nLVitem);
                    SetLineNumber(ref lookup, int.Parse(asLine[1]));

                    if (false == dictFiles.ContainsKey(key))
                    {
                        var listFiles = new List<int>();

                        listFiles.Add(lookup);
                        dictFiles[key] = listFiles;
                    }
                    else
                    {
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
                }
            }));

            m_DictFiles = dictFiles
                .Where(kvp => kvp.Value.Count > 1)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable());

            m_statusCallback(bDone: true);
        }

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

        void Deserialize()
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

                    m_DictFiles[new FileKeyStruct(asLine[0])] =
                        asLine.Skip(1).Select(s => Convert.ToInt32(s)).ToList();
                }
            }
        }

        void OpenedProject()
        {
            m_DictFiles.Clear();
        }

        readonly string ksSerializeFile = ProjectFile.TempPath + "_DuplicateFiles_";

        Dictionary<LVitem_ProjectVM, int> DictLVtoItemNumber = new Dictionary<LVitem_ProjectVM, int>();
        Dictionary<int, LVitem_ProjectVM> DictItemNumberToLV = new Dictionary<int, LVitem_ProjectVM>();
        Dictionary<FileKeyStruct, IEnumerable<int>> m_DictFiles = new Dictionary<FileKeyStruct, IEnumerable<int>>();

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
