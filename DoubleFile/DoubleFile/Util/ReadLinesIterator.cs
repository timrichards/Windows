using System.Collections.Concurrent;
using System.IO;

namespace DoubleFile
{
    internal class ReadLinesIterator : Iterator<string>
    {
        private StreamReader _reader;

        private ReadLinesIterator(StreamReader reader)
        {
            _reader = reader;
            nID = ++static_nID;
        }

        public override bool MoveNext()
        {
            if (_reader != null)
            {
                current = _reader.ReadLine();
                if (current != null)
                {
                    return true;
                }
				base.Dispose(); // ------------------------- remove if using chaser iterator code
            }
            return false;
        }

        protected override Iterator<string> Clone()
        {
            return CreateIterator(_reader);
        }

        internal ReadLinesIterator
            StayOpen()
        {
            _stayOpen = true;
            return this;
        }
        internal ReadLinesIterator
            Close()
        {
            _stayOpen = false;
            Dispose();
            return this;
        }
        static bool _stayOpen = false;

        protected override void Dispose(bool disposing)
        {
            if (_stayOpen)
                return;

            if (0 < --nRefCount)
                return;

            ReadLinesIterator foobar = null;
            _dictFilesOpen.TryRemove(_strFile, out foobar);
            //Util.WriteLine("Disposing " + nID);

            try
            {
                if (disposing && _reader != null)
                {
                    _reader.Dispose();
                }
            }
            finally
            {
                _reader = null;
                base.Dispose(disposing);
            }
        }

        static ReadLinesIterator CreateIterator(StreamReader reader)
        {
            return new ReadLinesIterator(reader);
        }

        static internal ReadLinesIterator CreateIterator(string strFile)
        {
            if (_stayOpen)
                Util.Assert(0, false);

            return CreateIterator(new StreamReader(strFile.OpenFile(FileMode.Open)));
        }

        static internal ReadLinesIterator CreateIterator_(string strFile)
        {
            ReadLinesIterator holder = null;

            lock (holderLock)
            {
                holder = _dictFilesOpen.GetOrAdd(strFile, x => CreateIterator(new StreamReader(strFile.OpenFile(FileMode.Open))));
                ++holder.nRefCount;
                holder._strFile = strFile;
            }

            return holder;
        }

        int nRefCount = 0;
        string _strFile = "";

        static object holderLock = new object();

        static ConcurrentDictionary<string, ReadLinesIterator>
            _dictFilesOpen = new ConcurrentDictionary<string, ReadLinesIterator>();

        static int static_nID = 0; 
        int nID = 0; 
    }
}
