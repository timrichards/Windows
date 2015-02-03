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
                foreach (var keyValuePair in m_DictFiles)
                {
                    writer.Write(keyValuePair.Key);

                    foreach (var ls in keyValuePair.Value)
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

            foreach (var lvItem in LVprojectVM.ItemsCast)
            {
                DictLVtoItemNumber.Add(lvItem, nLVitems);
                DictItemNumberToLV.Add(nLVitems, lvItem);
                ++nLVitems;
            }

            int nLVitems_A = 0;
            ConcurrentDictionary<FileKey, List<int>> dictFiles = new ConcurrentDictionary<FileKey, List<int>>();

            Parallel.ForEach(LVprojectVM.ItemsCast, (lvItem => 
            {
                var ieLines = File.ReadLines(lvItem.ListingFile)
                    .Where(strLine => strLine.StartsWith(FileParse.ksLineType_File));
                var nLVitem = DictLVtoItemNumber[lvItem];

                Interlocked.Add(ref m_nFilesTotal, ieLines.Count());
                Interlocked.Increment(ref nLVitems_A);

                ieLines.ForEach(strLine =>
                {
                    Interlocked.Increment(ref m_nFilesProgress);

                    var asLine = strLine.Split('\t');

                    if (asLine.Length > 10)
                    {
                        var key = new FileKey(asLine[10], asLine[7]);
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
                            dictFiles[key].Add(lookup);
                        }
                    }

                    if (nLVitems == nLVitems_A)
                    {
                        m_statusCallback(nProgress: m_nFilesProgress / (double)m_nFilesTotal);
                    }
                });
            }));

            bool bCheck = false;

            if (bCheck)
            {
                m_nFilesProgress = 0;
                m_nFilesTotal = dictFiles.Count;
            }

            foreach (var keyValuePair in dictFiles)
            {
                var ls = keyValuePair.Value;
                var nLength = ls.Count;

                if (nLength > 1)
                {
                    m_DictFiles[keyValuePair.Key] = ls;

                    if (bCheck)
                    {
                        // check the dictionary. Extremely slow but it worked for the first 89.

                        var lookup = ls[0];
                        var asLine = File.ReadLines(DictItemNumberToLV[GetLVitemProjectVM(lookup)].ListingFile)
                            .Skip(GetLineNumber(lookup) - 1).Take(1).ToArray()[0]
                            .Split('\t');

                        for (int n = 1; n < nLength; ++n)
                        {
                            var lookup_A = ls[n];
                            var asLine_A = File.ReadLines(DictItemNumberToLV[GetLVitemProjectVM(lookup_A)].ListingFile)
                                .Skip(GetLineNumber(lookup_A) - 1).Take(1).ToArray()[0]
                                .Split('\t');
                            MBox.Assert(0, asLine_A[10] == asLine[10]);
                            MBox.Assert(0, asLine_A[7] == asLine[7]);
                        }
                    }
                }

                m_statusCallback(nProgress: m_nFilesProgress++ / (double)m_nFilesTotal);
            }

            m_statusCallback(bDone: true);
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
