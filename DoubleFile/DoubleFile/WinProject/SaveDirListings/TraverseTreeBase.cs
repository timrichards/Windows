using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DoubleFile
{
    partial class SaveDirListings
    {
        abstract class TraverseTreeBase : FileParse
        {
            protected ulong
                LengthRead { get; private set; }

            protected List<string>
                ErrorList { get { return _errorList; } }
            readonly List<string> _errorList = new List<string>();

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
            /// <param name="fs">If omitted then params are ignored and it returns the file list.</param>
            /// <param name="dictHash"></param>
            /// <param name="dictException_FileRead"></param>
            /// <returns>File list if first pass</returns>
            IReadOnlyList<Tuple<string, ulong>> ImplementationDetails(
                TextWriter fs = null,
                Tuple<IReadOnlyDictionary<string, Tuple<HashTuple, HashTuple>>, IReadOnlyDictionary<string, string>> tuple = null)
            {
                var stackDirs = new Stack<NativeMethods.DATUM>(64);
                var winRoot = default(NativeMethods.DATUM);

                Win32FindFileStatic.FileData.WinFile(LVitemProjectVM.SourcePath, out winRoot);
                stackDirs.Push(winRoot);

                var listFilePaths = new List<Tuple<string, ulong>>();

                MBoxStatic.Assert(99939, LengthRead == 0);
                LengthRead = 0;

                while (false == stackDirs.IsEmpty())
                {
                    if (App.LocalExit || _bThreadAbort)
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
                        if (fs != null)
                        {
                            ErrorList.Add(FormatString(strDir: strFullPath,
                                strError1: strWin32Error,
                                strError2: strError2_Dir));
                        }

                        continue;
                    }

                    var nDirLength = 0UL;
                    var bHasLength = false;

                    foreach (var winFile in ieFiles.OrderBy(winData => winData.strAltFileName))
                    {
                        if (App.LocalExit || _bThreadAbort)
                        {
                            return null;
                        }

                        var fi = new Win32FindFileStatic.FileData(winFile);
                        var strFile = winFile.strFileName;
                        var strError2_File = "" + CheckNTFS_chars(ref strFile, bFile: true);

                        if (fi.IsValid == false)
                        {
                            if (fs != null)
                            {
                                var strErrorDir = winFile.strAltFileName.Substring(0,
                                    winFile.strAltFileName.LastIndexOf('\\'));

                                CheckNTFS_chars(ref strErrorDir);

                                ErrorList.Add(FormatString(strFile: strFile, strDir: strErrorDir,
                                    strError2: strError2_File));
                            }

                            continue;
                        }

                        MBoxStatic.Assert(1306.7307m, fi.Size >= 0);

                        if (fs == null)
                        {
                            if (fi.Size > 0)
                            {
                                listFilePaths.Add(Tuple.Create(winFile.strAltFileName, fi.Size));
                            }
                        }
                        else
                        {
                            if (fi.Size > 0)
                            {
                                bHasLength = true;
                                LengthRead += fi.Size;
                                nDirLength += fi.Size;
                            }

                            string strHashV1pt0 = null;
                            string strHashV2 = null;

                            if (null != tuple)
                            {
                                var tupleA = tuple.Item1.TryGetValue(winFile.strAltFileName);

                                if (null != tupleA)
                                {
                                    strHashV1pt0 = "" + tupleA.Item1;
                                    strHashV2 = "" + tupleA.Item2;
                                }
                            }

                            string strError1 = null;

                            if (winFile.strAltFileName.Length > 260)
                            {
                                strError1 = "Path Length: " + winFile.strAltFileName.Length;
                            }

                            if (null != tuple)
                            {
                                var str = tuple.Item2.TryGetValue(strFile);

                                if (false == string.IsNullOrEmpty(str))
                                {
                                    strError2_File += " " + str;
                                    strError2_File = strError2_File.TrimStart();
                                }
                            }

                            fs.WriteLine(FormatString(strFile: strFile, dtCreated: fi.CreationTime,
                                strAttributes: ((int)fi.Attributes).ToString("X"), dtModified: fi.LastWriteTime,
                                nLength: fi.Size, strError1: strError1, strError2: strError2_File,
                                strHashV1pt0: strHashV1pt0, strHashV2: strHashV2));
                        }
                    }

                    if (fs != null)
                    {
                        string strError1 = null;

                        if (strFullPath.Length > 240)
                        {
                            strError1 = "Path Length: " + strFullPath.Length;
                        }

                        MBoxStatic.Assert(1306.7308m, bHasLength == (nDirLength > 0));
                        MBoxStatic.Assert(1306.7301m, nDirLength >= 0);

                        var di = new Win32FindFileStatic.FileData(winDir);

                        if (strFullPath.EndsWith(@":\"))                            // root directory
                        {
                            MBoxStatic.Assert(1306.7302m, di.IsValid == false);      // yes, yes...
                            MBoxStatic.Assert(1306.7303m, strFullPath.Length == 3);
                            fs.WriteLine(FormatString(strDir: strFullPath, nLength: nDirLength,
                                strError1: strError1, strError2: strError2_Dir));
                        }
                        else
                        {
                            MBoxStatic.Assert(1306.7304m, di.IsValid);
                            fs.WriteLine(FormatString(strDir: strFullPath, dtCreated: di.CreationTime,
                                strAttributes: ((int)di.Attributes).ToString("X"),
                                dtModified: di.LastWriteTime, nLength: nDirLength,
                                strError1: strError1, strError2: strError2_Dir));
                        }
                    }

                    foreach (var winData in ieSubDirs.OrderBy(winData => winData.strAltFileName))
                    {
                        stackDirs.Push(winData);
                    }
                }

                return listFilePaths;
            }
        }
    }
}
