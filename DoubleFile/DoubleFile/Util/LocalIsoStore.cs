using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

namespace DoubleFile
{
    static class LocalIsoStore
    {
        internal static readonly string
            TempDir = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "\\";

        internal static void
            InitFromStatics(IsolatedStorageFile isoStore)
        {
            Util.Assert(99883, null == _isoStore);
            _isoStore = isoStore;
        }
        static IsolatedStorageFile _isoStore = null;

        static internal bool
            DirectoryExists(string path) => _isoStore.DirectoryExists(path);
        static internal string[]
            GetFileNames() => _isoStore.GetFileNames();

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
            if (null == strFile)
                return false;

            return File.Exists(strFile) ||
                _isoStore.FileExists(strFile);
        }

        static internal string
            FileMoveToIso(this string strSource, string strIsoDest)
        {
            if (null == strSource)
                return null;

            if (File.Exists(strSource))
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
            if (File.Exists(strFile))
            {
                return File.Open(strFile, mode);
            }
            else
            {
                if (FileMode.Open == mode)
                    return _isoStore.OpenFile(strFile, FileMode.Open);

                return WriteLockA(() => _isoStore.OpenFile(strFile, mode));
            }
        }

        static internal IEnumerable<string>
            ReadLines(this string strFile)
        {
            if (null == strFile)
                return new string[0];

            return (File.Exists(strFile))
                ? File.ReadLines(strFile)
                : ReadLinesIterator.CreateIterator(new StreamReader(OpenFile(strFile, FileMode.Open)));
        }
    }
}
