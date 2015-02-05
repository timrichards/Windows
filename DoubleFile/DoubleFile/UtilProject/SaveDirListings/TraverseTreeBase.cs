using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace DoubleFile
{
    partial class SaveDirListings
    {
        abstract class TraverseTreeBase
        {
            protected TraverseTreeBase(GlobalData_Base gd_in,
                LVitem_ProjectVM lvProjectVM)
            {
                gd = gd_in;
                LVitemProjectVM = lvProjectVM;
            }

            protected IEnumerable<string> GetFileList()
            {
                return ImplementationDetails();
            }

            protected void WriteDirectoryListing(TextWriter fs, ConcurrentDictionary<string, HashStruct> dictHash, Dictionary<string, string> dictException_FileRead)
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
            private IEnumerable<string> ImplementationDetails(
                TextWriter fs = null,
                ConcurrentDictionary<string, HashStruct> dictHash = null,
                Dictionary<string, string> dictException_FileRead = null)
            {
                var stackDirs = new Stack<Win32FindFileStatic.DATUM>(64);
                Win32FindFileStatic.DATUM winRoot;

                Win32FindFileStatic.FileData.WinFile(LVitemProjectVM.SourcePath, out winRoot);
                stackDirs.Push(winRoot);

                var listFilePaths = new List<string>();

                MBox.Assert(0, LengthRead == 0);
                MBox.Assert(0, FilesRead == 0);
                LengthRead = 0;
                FilesRead = 0;

                while (stackDirs.Count > 0)
                {
                    if (m_bThreadAbort || gd.WindowClosed)
                    {
                        return null;
                    }

                    var winDir = stackDirs.Pop();
                    var strFullPath = winDir.strAltFileName;
                    var strError2_Dir = CheckNTFS_chars(ref strFullPath);
                    var listSubDirs = new List<Win32FindFileStatic.DATUM>();
                    var listFiles = new List<Win32FindFileStatic.DATUM>();

                    if (Win32FindFileStatic.GetDirectory(strFullPath, ref listSubDirs, ref listFiles) == false)
                    {
                        if (fs != null)
                        {
                            m_listErrors.Add(FormatString(strDir: strFullPath,
                                strError1: new System.ComponentModel.Win32Exception(
                                    System.Runtime.InteropServices.Marshal.GetLastWin32Error()).Message,
                                strError2: strError2_Dir));
                        }

                        continue;
                    }

                    long nDirLength = 0;
                    var bHasLength = false;

                    foreach (var winFile in listFiles)
                    {
                        if (m_bThreadAbort || gd.WindowClosed)
                        {
                            return null;
                        }

                        var fi = new Win32FindFileStatic.FileData(winFile);
                        var strFile = winFile.strFileName;
                        var strError2_File = CheckNTFS_chars(ref strFile, bFile: true) ?? "";

                        if (fi.IsValid == false)
                        {
                            if (fs != null)
                            {
                                var strErrorDir = winFile.strAltFileName.Substring(0, winFile.strAltFileName.LastIndexOf('\\'));

                                CheckNTFS_chars(ref strErrorDir);
                                m_listErrors.Add(FormatString(strFile: strFile, strDir: strErrorDir, strError2: strError2_File));
                            }

                            continue;
                        }

                        MBox.Assert(1306.7307, fi.Size >= 0);

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
                                ++FilesRead;
                            }

                            string strHash = null;

                            if (dictHash.ContainsKey(winFile.strAltFileName))
                            {
                                strHash = dictHash[winFile.strAltFileName].ToString();
                            }

                            string strError1 = null;

                            if (winFile.strAltFileName.Length > 260)
                            {
                                strError1 = "Path Length: " + winFile.strAltFileName.Length.ToString();
                            }

                            if (dictException_FileRead.ContainsKey(strFile))
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
                            strError1 = "Path Length: " + strFullPath.Length.ToString();
                        }

                        MBox.Assert(1306.7308, bHasLength == (nDirLength > 0));
                        MBox.Assert(1306.7301, nDirLength >= 0);

                        var di = new Win32FindFileStatic.FileData(winDir);

                        if (strFullPath.EndsWith(@":\"))                        // root directory
                        {
                            MBox.Assert(1306.7302, di.IsValid == false);        // yes, yes...
                            MBox.Assert(1306.7303, strFullPath.Length == 3);
                            fs.WriteLine(FormatString(strDir: strFullPath, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
                        }
                        else
                        {
                            MBox.Assert(1306.7304, di.IsValid);
                            fs.WriteLine(FormatString(strDir: strFullPath, dtCreated: di.CreationTime, strAttributes: ((int)di.Attributes).ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
                        }
                    }

                    foreach (Win32FindFileStatic.DATUM winData in listSubDirs)
                    {
                        stackDirs.Push(winData);
                    }
                }

                return listFilePaths;
            }

            protected readonly GlobalData_Base gd = null;
            protected long LengthRead { get; private set; }
            protected long FilesRead { get; private set; }

            List<string> m_listErrors = new List<string>();
            protected IEnumerable<string> ErrorList { get { return m_listErrors; } }

            protected bool m_bThreadAbort = false;
            protected readonly LVitem_ProjectVM LVitemProjectVM = null;
        }
    }
}
