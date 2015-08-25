using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace DoubleFile
{
    partial class SaveDirListings
    {
        abstract class TraverseTreeBase : FileParse
        {
            protected ulong
                LengthRead { get; private set; }

            protected IList<string>
                ErrorList => _errorList;
            readonly List<string> _errorList = new List<string> { };

            protected bool
                _bThreadAbort = false;

            protected readonly LVitem_ProjectVM
                LVitemProjectVM = null;

            protected TraverseTreeBase(LVitem_ProjectVM lvProjectVM)
            {
                LVitemProjectVM = lvProjectVM;
            }

            protected IReadOnlyList<Tuple<string, ulong>> GetFileList()
            {
                return ImplementationDetails();
            }

            protected void WriteDirectoryListing(TextWriter fs,
                Tuple<IReadOnlyDictionary<string, Tuple<HashTuple, HashTuple>>, IReadOnlyDictionary<string, string>> tuple)
            {
                ImplementationDetails(fs, tuple);
            }

            /// <summary>
            /// Two-pass private implementation. First to get file list; second to write to file.
            /// </summary>
            /// <param name="fs">If omitted then tuple is ignored (no params) and it returns the file list.</param>
            /// <param name="tuple">Item 1: hash dictionary; Item2: read errors dictionary</param>
            /// <returns>File list if first pass</returns>
            IReadOnlyList<Tuple<string, ulong>> ImplementationDetails(
                TextWriter fs = null,
                Tuple<IReadOnlyDictionary<string, Tuple<HashTuple, HashTuple>>, IReadOnlyDictionary<string, string>> tuple = null)
            {
                var stackDirs = new Stack<NativeMethods.DATUM>(64);

                stackDirs.Push(Win32FindFileStatic.FileData.WinFile(LVitemProjectVM.SourcePath));

                var listFilePaths = new List<Tuple<string, ulong>> { };

                Util.Assert(99939, 0 == LengthRead);
                LengthRead = 0;

                while (0 < stackDirs.Count)
                {
                    if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                        _bThreadAbort)
                    {
                        return null;
                    }

                    var winDir = stackDirs.Pop();
                    var strFullPath = winDir.strAltFileName;
                    var strError2_Dir = CheckNTFS_chars(ref strFullPath);
                    IEnumerable<NativeMethods.DATUM> ieSubDirs = null;
                    IEnumerable<NativeMethods.DATUM> ieFiles = null;
                    string strWin32Error = null;

                    if (false == Win32FindFileStatic.GetDirectory(strFullPath, out ieSubDirs, out ieFiles, out strWin32Error))
                    {
                        if (null != fs)             // second pass
                        {
                            ErrorList.Add(FormatString(strDir: strFullPath,
                                strError1: strWin32Error,
                                strError2: strError2_Dir));
                        }

                        continue;
                    }

                    var nDirLength = 0UL;
                    var bHasLength = false;

                    foreach (var winFile in ieFiles)
                    {
                        if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                            _bThreadAbort)
                        {
                            return null;
                        }

                        var fi = new Win32FindFileStatic.FileData(winFile);
                        var strFile = winFile.strFileName;
                        var strError2_File = "" + CheckNTFS_chars(ref strFile, bFile: true);

                        if (false == fi.IsValid)
                        {
                            if (null != fs)             // second pass
                            {
                                var strErrorDir = winFile.strAltFileName.Substring(0,
                                    winFile.strAltFileName.LastIndexOf('\\'));

                                CheckNTFS_chars(ref strErrorDir);

                                ErrorList.Add(FormatString(strFile: strFile,
                                    strDir: strErrorDir,
                                    strError2: strError2_File));
                            }

                            continue;
                        }

                        if (null == fs)
                        {
                            if (0 < fi.Size)
                                listFilePaths.Add(Tuple.Create(winFile.strAltFileName, fi.Size));
                        }
                        else                            // second pass
                        {
                            if (0 < fi.Size)
                            {
                                bHasLength = true;
                                LengthRead += fi.Size;
                                nDirLength += fi.Size;
                            }

                            string strError1 =
                                (260 < winFile.strAltFileName.Length)
                                ? "Path Length: " + winFile.strAltFileName.Length
                                : null;

                            string strHashV1pt0 = null;
                            string strHashV2 = null;

                            if (null != tuple)
                            {
                                var tupleA = tuple.Item1.TryGetValue(winFile.strAltFileName);

                                strHashV1pt0 = "" + tupleA?.Item1;
                                strHashV2 = "" + tupleA?.Item2;

                                var strError = tuple.Item2.TryGetValue(strFile);

                                if (false == string.IsNullOrWhiteSpace(strError))
                                {
                                    strError2_File += " " + strError;
                                    strError2_File = strError2_File.TrimStart();
                                }
                            }

                            fs.WriteLine(FormatString(strFile: strFile, dtCreated: fi.CreationTime,
                                strAttributes: ((int)fi.Attributes).ToString("X"), dtModified: fi.LastWriteTime,
                                nLength: fi.Size, strError1: strError1, strError2: strError2_File,
                                strHashV1pt0: strHashV1pt0, strHashV2: strHashV2));
                        }
                    }

                    if (null != fs)                     // second pass
                    {
                        string strError1 =
                            (240 < strFullPath.Length)
                            ? "Path Length: " + strFullPath.Length
                            : null;

                        Util.Assert(1306.7308m, bHasLength == (0 < nDirLength));

                        var di = new Win32FindFileStatic.FileData(winDir);

                        if (strFullPath.EndsWith(@":\"))                            // root directory
                        {
                            Util.Assert(1306.7302m, false == di.IsValid);     // yes, yes...
                            Util.Assert(1306.7303m, 3 == strFullPath.Length);

                            fs.WriteLine(FormatString(strDir: strFullPath, nLength: nDirLength,
                                strError1: strError1, strError2: strError2_Dir));
                        }
                        else
                        {
                            Util.Assert(1306.7304m, di.IsValid);

                            fs.WriteLine(FormatString(strDir: strFullPath, dtCreated: di.CreationTime,
                                strAttributes: ((int)di.Attributes).ToString("X"),
                                dtModified: di.LastWriteTime, nLength: nDirLength,
                                strError1: strError1, strError2: strError2_Dir));
                        }
                    }

                    foreach (var winData in ieSubDirs)
                        stackDirs.Push(winData);
                }

                return listFilePaths;
            }
        }
    }
}
