using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace DoubleFile
{
    delegate void CreateFileDictStatusDelegate(bool bDone = false, double nProgress = double.NaN);

    struct FileDictLookup
    {
        internal int nLVitemProjectVM;
        internal ulong nLine;

        internal FileDictLookup(int nLVitemProjectVM_in, ulong nLine_in)
        {
            nLVitemProjectVM = nLVitemProjectVM_in;
            nLine = nLine_in;
        }
    }

    class CreateFileDictionary : FileParse
    {
        internal Dictionary<LVitem_ProjectVM, int> DictLVitemNumber = new Dictionary<LVitem_ProjectVM, int>();
        internal Dictionary<string, UList<FileDictLookup>> DictFiles = new Dictionary<string, UList<FileDictLookup>>();
        
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
            {
                int nLineNumber = 0;

                foreach (var lvItem in LVprojectVM.ItemsCast)
                {
                    DictLVitemNumber.Add(lvItem, nLineNumber++);
                }
            }

            Parallel.ForEach(LVprojectVM.ItemsCast, (lvItem => 
            {
                var ieLines = File.ReadLines(lvItem.ListingFile).Skip(4)
                    .Where(strLine => strLine.StartsWith(ksLineType_File));
                var nLVitem = DictLVitemNumber[lvItem];

                Interlocked.Add(ref m_nFilesTotal, ieLines.Count());

                ieLines.ForEach(strLine =>
                {
                    Interlocked.Increment(ref m_nFilesProgress);

                    var arrLine = strLine.Split('\t');

                    if (arrLine.Length > 10)
                    {
                        var strKey = arrLine[10] + arrLine[7];                      // key is hash concat length
                        var lookup = new FileDictLookup(nLVitem, ulong.Parse(arrLine[1]));

                        if (false == DictFiles.ContainsKey(strKey))
                        {
                            var listFiles = new UList<FileDictLookup>();

                            listFiles.Add(lookup);
                            DictFiles.Add(strKey, listFiles);
                        }
                        else
                        {
                            DictFiles[strKey].Add(lookup);
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
        long m_nFilesTotal = 0;
        long m_nFilesProgress = 0;
    }
}
