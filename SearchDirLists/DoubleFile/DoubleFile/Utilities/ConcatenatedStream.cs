using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleFile
{
    class ConcatenatedStream : Stream
    {
        private IEnumerable<FileStream> fileStreams;
        private FileStream currentStream;
        private int currentStreamIndex;
        private long length=-1;
        private long position;
        private bool endReached;

        internal static char FileEndMarker { get { return '\0'; } }

        internal ConcatenatedStream(IEnumerable<string> files)
        {
            fileStreams = files.Select(file => File.Open(file, FileMode.Open, FileAccess.Read)).ToList();
            currentStreamIndex = 0;
            currentStream = fileStreams.ElementAt(currentStreamIndex);
        }
 
        public override void Flush()
        {
            if (currentStream != null)
                currentStream.Flush();
        }
 
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new System.InvalidOperationException("Stream is not seekable.");
        }
 
        public override void SetLength(long value)
        {
            this.length = value;
        }
 
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (endReached)
            {
                for (int i = offset; i < offset + count; i++)
                    buffer[i] = 0;
 
                return 0;
            }
 
            int result = 0;
            int buffPostion = offset;
 
            while (count > 0)
            {
                int bytesRead = currentStream.Read(buffer, buffPostion, count);
                result += bytesRead;
                buffPostion += bytesRead;
                position += bytesRead;
 
                if (bytesRead <= count)
                {
                    count -= bytesRead;
                }
 
                if (count > 0)
                {
                    var stream = fileStreams.ElementAt(currentStreamIndex);

                    if (currentStreamIndex >= fileStreams.Count() - 1)
                    {
                        stream.Close();
                        endReached = true;
                        break;
                    }

                    buffer[buffPostion] = (byte)FileEndMarker;
                    buffPostion++;
                    count--;
                    result++;

                    stream.Close();
                    currentStream = fileStreams.ElementAt(++currentStreamIndex);
                }
            }
 
            return result;
        }
 
        public override long Length
        {
            get
            {
                if (length == -1)
                {
                    length = 0;
                    foreach (var stream in fileStreams)
                    {
                        length += stream.Length;
                    }
                }
 
                return length;
            }
        }
 
        public override long Position
        {
            get { return this.position; }
            set { throw new System.NotImplementedException(); }
        }
 
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.InvalidOperationException("Stream is not writable");
        }
 
        public override bool CanRead
        {
            get { return true; }
        }
 
        public override bool CanSeek
        {
            get { return false; }
        }
 
        public override bool CanWrite
        {
            get { return false; }
        }
    }
}
