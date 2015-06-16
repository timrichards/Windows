using System.Collections.Generic;
using System.IO;

namespace DoubleFile
{
    partial class SaveDirListings
    {
        abstract class TraverseTreeBase : FileParse
        {
            protected long
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

            protected IEnumerable<string> GetFileList()
            {
                return ImplementationDetails();
            }

            protected void WriteDirectoryListing(TextWriter fs,
                IDictionary<string, HashTuple> dictHash,
                IReadOnlyDictionary<string, string> dictException_FileRead)
            {
                ImplementationDetails(fs, dictHash, dictException_FileRead);
            }

            /// <summary>
            /// Two-pass private implementation. First to get file list; second to write to file.
            /// </summary>
            /// <param name="fs">If omitted then there can be no params and it returns the file list.</param>
            /// <param name="dictHash"></param>
            /// <param name="dictException_FileRead"></param>
            /// <returns>File list if first pass</returns>
            IEnumerable<string> ImplementationDetails(
                TextWriter fs = null,
                IDictionary<string, HashTuple> dictHash = null,
                IReadOnlyDictionary<string, string> dictException_FileRead = null)
            {
                var stackDirs = new Stack<NativeMethods.DATUM>(64);
                var winRoot = default(NativeMethods.DATUM);

                Win32FindFileStatic.FileData.WinFile(LVitemProjectVM.SourcePath, out winRoot);
                stackDirs.Push(winRoot);

                var listFilePaths = new List<string>();

                MBoxStatic.Assert(99939, LengthRead == 0);
                MBoxStatic.Assert(99938, m_nFilesRead == 0);
                LengthRead = 0;
                m_nFilesRead = 0;

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

                    long nDirLength = 0;
                    var bHasLength = false;

                    foreach (var winFile in ieFiles)
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

                        MBoxStatic.Assert(1306.7307, fi.Size >= 0);

                        if (fs == null)
                        {
                            if (fi.Size > 0)
                            {
                                listFilePaths.Add(winFile.strAltFileName);
                            }
                        }
                        else
                        {
                            if (fi.Size > 0)
                            {
                                bHasLength = true;
                                LengthRead += fi.Size;
                                nDirLength += fi.Size;
                                ++m_nFilesRead;
                            }

                            string strHash = null;
                            var hash = dictHash.TryGetValue(winFile.strAltFileName);

                            if ((null != dictHash) &&
                                (null != hash))
                            {
                                strHash = "" + hash;
                            }

                            string strError1 = null;

                            if (winFile.strAltFileName.Length > 260)
                            {
                                strError1 = "Path Length: " + winFile.strAltFileName.Length;
                            }

                            if ((null != dictException_FileRead) &&
                                (null != dictException_FileRead.TryGetValue(strFile)))
                            {
                                strError2_File += " " + dictException_FileRead[strFile];
                                strError2_File = strError2_File.TrimStart();
                            }

                            fs.WriteLine(FormatString(strFile: strFile, dtCreated: fi.CreationTime,
                                strAttributes: ((int)fi.Attributes).ToString("X"), dtModified: fi.LastWriteTime,
                                nLength: fi.Size, strError1: strError1, strError2: strError2_File, strHash: strHash));
                        }
                    }

                    if (fs != null)
                    {
                        string strError1 = null;

                        if (strFullPath.Length > 240)
                        {
                            strError1 = "Path Length: " + strFullPath.Length;
                        }

                        MBoxStatic.Assert(1306.7308, bHasLength == (nDirLength > 0));
                        MBoxStatic.Assert(1306.7301, nDirLength >= 0);

                        var di = new Win32FindFileStatic.FileData(winDir);

                        if (strFullPath.EndsWith(@":\"))                            // root directory
                        {
                            MBoxStatic.Assert(1306.7302, di.IsValid == false);      // yes, yes...
                            MBoxStatic.Assert(1306.7303, strFullPath.Length == 3);
                            fs.WriteLine(FormatString(strDir: strFullPath, nLength: nDirLength,
                                strError1: strError1, strError2: strError2_Dir));
                        }
                        else
                        {
                            MBoxStatic.Assert(1306.7304, di.IsValid);
                            fs.WriteLine(FormatString(strDir: strFullPath, dtCreated: di.CreationTime,
                                strAttributes: ((int)di.Attributes).ToString("X"),
                                dtModified: di.LastWriteTime, nLength: nDirLength,
                                strError1: strError1, strError2: strError2_Dir));
                        }
                    }

                    foreach (var winData in ieSubDirs)
                    {
                        stackDirs.Push(winData);
                    }
                }

                return listFilePaths;
            }

            long m_nFilesRead = 0;
        }
    }
}
