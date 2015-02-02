using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System;
using System.Collections.Concurrent;

namespace DoubleFile
{
    internal struct FileDictLookup
    {
        internal int nLVitemProjectVM;
        internal ulong nLVitem;

        internal FileDictLookup(int nLVitemProjectVM_in, ulong nLVitem_in)
        {
            nLVitemProjectVM = nLVitemProjectVM_in;
            nLVitem = nLVitem_in;
        }
    }

    class CreateFileDictionary : FileParse
    {
        internal Dictionary<LVitem_ProjectVM, int> DictLVitemNumber = new Dictionary<LVitem_ProjectVM, int>();
        internal Dictionary<FileKey, List<FileDictLookup>> DictFiles = new Dictionary<FileKey, List<FileDictLookup>>();
        
        internal CreateFileDictionary(LV_ProjectVM lvProjectVM, CreateFileDictStatusDelegate statusCallback)
        {
            LVprojectVM = lvProjectVM;
            m_statusCallback = statusCallback;
        }

        internal CreateFileDictionary DoThreadFactory()
        {
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
                DictLVitemNumber.Add(lvItem, nLVitems++);
            }

            int nLVitems_A = 0;
            ConcurrentDictionary<FileKey, List<FileDictLookup>> dictFiles = new ConcurrentDictionary<FileKey, List<FileDictLookup>>();

            Parallel.ForEach(LVprojectVM.ItemsCast, (lvItem => 
            {
                var ieLines = File.ReadLines(lvItem.ListingFile)
                    .Where(strLine => strLine.StartsWith(ksLineType_File));
                var nLVitem = DictLVitemNumber[lvItem];

                Interlocked.Add(ref m_nFilesTotal, ieLines.Count());
                Interlocked.Increment(ref nLVitems_A);

                ieLines.ForEach(strLine =>
                {
                    Interlocked.Increment(ref m_nFilesProgress);

                    var arrLine = strLine.Split('\t');

                    if (arrLine.Length > 10)
                    {
                        var key = new FileKey(arrLine[10], arrLine[7]);
                        var lookup = new FileDictLookup(nLVitem, ulong.Parse(arrLine[1]));

                        if (false == dictFiles.ContainsKey(key))
                        {
                            var listFiles = new List<FileDictLookup>();

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

            foreach (var keyValuePair in dictFiles)
            {
                if (keyValuePair.Value.Count > 1)
                {
                    DictFiles[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            m_statusCallback(bDone: true);
        }

        readonly CreateFileDictStatusDelegate m_statusCallback = null;
        Thread m_thread = null;
        protected bool m_bThreadAbort = false;
        LV_ProjectVM LVprojectVM = null;
        long m_nFilesTotal = 0;
        long m_nFilesProgress = 0;
    }
}
