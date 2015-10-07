using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

namespace DoubleFile
{
    static class LocalIsoStore
    {
        static internal readonly string
            TempDir = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "\\";

        static internal void
            InitFromStatics(IsolatedStorageFile isoStore)
        {
            Util.Assert(99883, null == _isoStore);
            _isoStore = isoStore;
        }
        static IsolatedStorageFile _isoStore = null;

        static internal bool
            DirectoryExists(string path) => _isoStore.DirectoryExists(path);
        static internal IReadOnlyList<string>
            GetFileNames(string searchPattern) => _isoStore.GetFileNames(searchPattern);

        static internal IsolatedStorageFileStream
            LockFile(string path) => WriteLockA(() => _isoStore.OpenFile(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None));

        static internal void
            CopyFile(string sourceFileName, string destinationFileName) => WriteLock(() => _isoStore.CopyFile(sourceFileName, destinationFileName));
        static internal void
            CreateDirectory(string dir) => WriteLock(() => _isoStore.CreateDirectory(dir));
        static internal IsolatedStorageFileStream
            CreateFile(string path) => WriteLockA(() => _isoStore.CreateFile(path));
        static internal void
            DeleteDirectory(string dir) => WriteLock(() => _isoStore.DeleteDirectory(dir));
        static internal void
            DeleteFile(string file) => WriteLock(() => _isoStore.DeleteFile(file));
        static internal void
            MoveFile(string sourceFileName, string destinationFileName) => WriteLock(() => _isoStore.MoveFile(sourceFileName, destinationFileName));

        static readonly string _strLockFile = Path.GetTempPath() + Statics.Namespace + "_IsoLock";

        static void
            WriteLock(Action doSomething) => WriteLockA(() => { doSomething(); return false; });

        static T
            WriteLockA<T>(Func<T> doSomething)
        {
            Func<IDisposable> checkLockFile = () =>
            {
                try { return File.Open(_strLockFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None); }
                catch (IOException) { return null; }
            };

            IDisposable lockFile = null;

            while (null == (lockFile = checkLockFile()))
                Util.Block(100);

            try
            {
                return doSomething();
            }
            finally
            {
                lockFile.Dispose();
            }
        }

        // extension methods

        static internal bool
            FileExists(this string strFile)
        {
            if (string.IsNullOrWhiteSpace(strFile))
                return false;

            return
                Path.IsPathRooted(strFile)
                ? File.Exists(strFile)
                : _isoStore.FileExists(strFile);
        }

        static internal string
            FileMoveToIso(this string strSource, string strIsoDest)
        {
            if (string.IsNullOrWhiteSpace(strSource))
                return null;

            if (Path.IsPathRooted(strSource))
            {
                using (var sr = File.OpenText(strSource))
                using (var sw = new StreamWriter(CreateFile(TempDir + Path.GetFileName(strIsoDest))))
                    Util.CopyStream(sr, sw);

                File.Delete(strSource);
            }
            else
            {
                MoveFile(strSource, strIsoDest);
            }

            return strIsoDest;
        }

        static internal FileStream
            OpenFile(this string strFile, FileMode mode)
        {
            if (string.IsNullOrWhiteSpace(strFile))
                return null;

            if (Path.IsPathRooted(strFile))
            {
                return File.Open(strFile, mode);
            }
            else
            {
                if (FileMode.Open == mode)
                    return _isoStore.OpenFile(strFile, FileMode.Open, FileAccess.Read);

                return WriteLockA(() => _isoStore.OpenFile(strFile, mode));
            }
        }

        static internal IEnumerable<string>
            ReadLinesWait(this string strFile, decimal nLocation)
        {
            IOException eThrow = null;

            for (int i = 0; i < 10; ++i)
            {
                try
                {
                    return strFile.ReadLines(nLocation);
                }
                catch (IOException e)
                {
                    eThrow = e;
                    Util.WriteLine("ReadLinesWait " + strFile);
                    Util.Block(50);
                }
            }

            throw eThrow;
        }

        static internal IEnumerable<string>
            ReadLines(this string strFile, decimal nLocation)
        {
            if (string.IsNullOrWhiteSpace(strFile))
                return new string[0];

            //var a = new StreamReader(OpenFile(strFile, FileMode.Open));
            //var b = ReadLinesIterator.CreateIterator(a, true);
            //var c = ReadLinesIterator.CreateIterator(a, true);
            //b.MoveNext();
            //c.MoveNext();
            //b.MoveNext();
            //Util.WriteLine(b.current);
            //Util.WriteLine(c.current);

            //b.DeferredDispose();
            //c.DeferredDispose();

            return (Path.IsPathRooted(strFile))
                ? File.ReadLines(strFile)
                : ReadLinesIterator.CreateIterator(strFile);
        }
    }
}
