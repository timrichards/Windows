using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace DoubleFile
{
    delegate void CreateFileDictStatusDelegate(bool bDone = false, double nProgress = double.NaN);

    class CreateFileDictionary : FileParse
    {
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

        void Go()
        {
            Parallel.ForEach(LVprojectVM.ItemsCast, (lvItem => 
            {
                var ieLines = File.ReadLines(lvItem.ListingFile).Skip(4)
                    .Where(strLine => strLine.StartsWith(ksLineType_File));

                Interlocked.Add(ref m_nFilesTotal, ieLines.Count());

                ieLines.ForEach(strLine =>
                {
                    Interlocked.Increment(ref m_nFilesProgress);

                    var arrLine = strLine.Split('\t');

                    if (arrLine.Length > 10)
                    {
                        var strKey = arrLine[10] + arrLine[7];                      // key is hash concat length
                        var strLookup = lvItem.ListingFile + '\t' + strLine[1];     // lookup is ListingFile tab linenumber

                        if (false == m_dictFiles.ContainsKey(strKey))
                        {
                            var listFiles = new UList<string>();

                            listFiles.Add(strLookup);
                            m_dictFiles.Add(strKey, listFiles);
                        }
                        else
                        {
                            m_dictFiles[strKey].Add(strLookup);
                        }
                    }

                    m_statusCallback(nProgress: m_nFilesProgress / (double)m_nFilesTotal);
                });
            }));
        }

        readonly CreateFileDictStatusDelegate m_statusCallback = null;
        Thread m_thread = null;
        protected bool m_bThreadAbort = false;
        LV_ProjectVM LVprojectVM = null;
        Dictionary<string, UList<string>> m_dictFiles = new Dictionary<string, UList<string>>();
        int m_nFilesTotal = 0;
        int m_nFilesProgress = 0;
    }
}
