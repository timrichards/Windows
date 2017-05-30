using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using DokanNet;
using System.Linq;
using FileAccess = DokanNet.FileAccess;
using System.Runtime.InteropServices;

namespace DoubleFile
{
    partial class LocalTreeNode : IDokanOperations
    {
        public void Cleanup(string fileName, DokanFileInfo info)
        {
        }

        public void CloseFile(string fileName, DokanFileInfo info)
        {
        }

        public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
        {
            NtStatus result = NtStatus.Success;
            var filePath = GetPath(fileName);

            if (info.IsDirectory)
            {
                try
                {
                    switch (mode)
                    {
                        case FileMode.Open:
                        {
                            if (!Dir ectory.Exists(filePath))
                            {
                                try
                                {
                                    if (!Fi le.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
                                        return NtStatus.NotADirectory;
                                }
                                catch (Exception)
                                {
                                    return DokanResult.FileNotFound;
                                }

                                return DokanResult.PathNotFound;
                            }

                            break;
                        }

                        case FileMode.CreateNew:
                        {
                            if (Dire ctory.Exists(filePath))
                                return DokanResult.FileExists;

                            try
                            {
                                Fi le.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
                                return DokanResult.AlreadyExists;
                            }
                            catch (IOException)
                            {
                            }

                            Dir ectory.CreateDirectory(GetPath(fileName));
                            break;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return DokanResult.AccessDenied;
                }
            }
            else
            {
                var pathExists = true;
                var pathIsDirectory = false;

                const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                                      FileAccess.Execute |
                                                      FileAccess.GenericExecute | FileAccess.GenericWrite |
                                                      FileAccess.GenericRead;

                const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                           FileAccess.Delete |
                                                           FileAccess.GenericWrite;

                var readWriteAttributes = (access & DataAccess) == 0;
                var readAccess = (access & DataWriteAccess) == 0;

                try
                {
                    pathExists = (Directory.Exists(filePath) || File.Exists(filePath));
                    pathIsDirectory = File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
                }
                catch (IOException)
                {
                }

                switch (mode)
                {
                    case FileMode.Open:

                        if (pathExists)
                        {
                            if (readWriteAttributes || pathIsDirectory)
                                // check if driver only wants to read attributes, security info, or open directory
                            {
                                if (pathIsDirectory && (access & FileAccess.Delete) == FileAccess.Delete
                                    && (access & FileAccess.Synchronize) != FileAccess.Synchronize)
                                    //It is a DeleteFile request on a directory
                                    return DokanResult.AccessDenied;

                                info.IsDirectory = pathIsDirectory;
                                info.Context = new object();
                                // must set it to someting if you return DokanError.Success

                                return DokanResult.Success;
                            }
                        }
                        else
                        {
                            return DokanResult.FileNotFound;
                        }
                        break;

                    case FileMode.CreateNew:
                        if (pathExists)
                            return DokanResult.FileExists;
                        break;

                    case FileMode.Truncate:
                        if (!pathExists)
                            return DokanResult.FileNotFound;
                        break;
                }

                try
                {
                    info.Context = new FileStream(filePath, mode,
                        readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite, share, 4096, options);

                    if (pathExists && (mode == FileMode.OpenOrCreate
                        || mode == FileMode.Create))
                        result = DokanResult.AlreadyExists;

                    if (mode == FileMode.CreateNew || mode == FileMode.Create) //Files are always created as Archive
                        attributes |= FileAttributes.Archive;
                    File.SetAttributes(filePath, attributes);
                }
                catch (UnauthorizedAccessException) // don't have access rights
                {
                    return DokanResult.AccessDenied;
                }
                catch (DirectoryNotFoundException)
                {
                    return DokanResult.PathNotFound;
                }
                catch (Exception ex)
                {
                    var hr = (uint) Marshal.GetHRForException(ex);
                    switch (hr)
                    {
                        case 0x80070020: //Sharing violation
                            return DokanResult.SharingViolation;
                        default:
                            throw;
                    }
                }
            }

            return result;
        }

        public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus DeleteFile(string fileName, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName,       // not in interface
            out long streamSize, DokanFileInfo info)
        {
            streamName = string.Empty;
            streamSize = 0;
            return DokanResult.NotImplemented;
        }

        void FindFiles(out IList<FileInformation> files)
        {
            var outFiles = files = new List<FileInformation> { };

            Nodes?.ForEach(node =>
            {
                var fi = new FileInformation { FileName = node.PathShort };

                node.GetFileInformation(ref fi);
                outFiles.Add(fi);
            });

            if (0 < NodeDatum.FileCountHere)
            {
                foreach (var strFileLine in GetFileList())
                {
                    var lvItem = new LVitem_FilesVM { FileLine = strFileLine
                        .Split('\t')
                        .Skip(3)                            // makes this an LV line: knColLengthLV
                        .ToArray() };

                    var fi = new FileInformation
                    {
                        FileName = lvItem.Filename,
                        Length = (long)lvItem.LengthRaw,
                        CreationTime = lvItem.CreatedRaw,
                        LastWriteTime = lvItem.ModifiedRaw,
                        Attributes = (FileAttributes)Convert.ToInt32(lvItem.SubItems[3], /* from base */ 16)
                    };

                    files.Add(fi);
                }
            }
        }

        string ConvertRootPath(string fileName = null)
        {
            if (null != fileName)
            {
                string ret = fileName.TrimStart('\\');

                LVitem_ProjectVM.InvalidNicknamePathCharException.translateInvalidChars
                    .ForEach(kvp => ret = ret.Replace(kvp.Value, kvp.Key));

                return ret.Replace(@"\\", @"\");
            }
            else
            {
                var strRoot = Root.PathFull;
                var ret = strRoot;

                LVitem_ProjectVM.InvalidNicknamePathCharException.translateInvalidChars
                    .ForEach(kvp => ret = ret.Replace(kvp.Key, kvp.Value));

                return PathFull.Replace(strRoot, ret);
            }
        }

        LocalTreeNode GetOneNodeByRootPath(string fileName)
        {
            return LocalTV.GetOneNodeByRootPath(ConvertRootPath(fileName), null);
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            fileName = fileName.TrimStart('\\');
            files = new List<FileInformation> { };      // C# doesn't see the default case when assigning the out parameter

            switch (fileName)
            {
                case "":
                {
                    files.Add(new FileInformation { FileName = kSink, Attributes = FileAttributes.Directory });

                    foreach (var rootNode in LocalTV.RootNodes)
                    {
                        var fi = new FileInformation { FileName = rootNode.ConvertRootPath() };

                        rootNode.GetFileInformation(ref fi);
                        files.Add(fi);
                    }

                    break;
                }

                case kSink:
                {
                    files.Add(new FileInformation { FileName = "`In the Sink" });
                    break;
                }

                default:
                {
                    var treeNode = GetOneNodeByRootPath(fileName);

                    if (null == treeNode)
                    {
                        files = new FileInformation[0];

                        if (false == fileName.EndsWith("desktop.ini"))
                        {
                            Util.WriteLine($"{fileName}");
                        }

                        return DokanResult.Error;
                    }

                    treeNode.FindFiles(out files);
                    break;
                }
            }

            return DokanResult.Success;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, DokanFileInfo info)
        {
            files = new FileInformation[0];
            return DokanResult.NotImplemented;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info)
        {
            streams = new FileInformation[0];
            return DokanResult.NotImplemented;
        }

        public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info)
        {
            var rootNodeDatum = RootNodeDatum;
            var nVolumeLength = rootNodeDatum.VolumeLength;

            // Code copied from UC_TreeMapVM.cs RecurseDrawGraph()
            var nUnreadLength =
                (long)nVolumeLength -
                (long)rootNodeDatum.VolumeFree -
                (long)rootNodeDatum.LengthTotal;

            if (nUnreadLength < 0)
                nUnreadLength = 0;

            freeBytesAvailable = (long)rootNodeDatum.VolumeFree - nUnreadLength;
            totalNumberOfBytes = (long)nVolumeLength;
            totalNumberOfFreeBytes = (long)rootNodeDatum.VolumeFree;
            return DokanResult.Success;
        }

        void GetFileInformation(ref FileInformation fileInfo)
        {
            //  FileName -- set by caller
            fileInfo.Attributes = NodeDatum.FolderDetails.Attributes | FileAttributes.Directory;
            fileInfo.CreationTime = NodeDatum.FolderDetails.Created;
            fileInfo.LastWriteTime = NodeDatum.FolderDetails.Modified;
            fileInfo.Length = (long)NodeDatum.LengthTotal;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            fileInfo = new FileInformation { FileName = fileName };
            fileName = fileName.TrimStart('\\');

            switch (fileName)
            {
                case "":
                {
                    fileInfo.Attributes = FileAttributes.Directory;
                    fileInfo.LastAccessTime = DateTime.Now;
                    fileInfo.LastWriteTime = null;
                    fileInfo.CreationTime = null;
                    return DokanResult.Success;
                }

                case kSink:
                {
                    fileInfo.Attributes = FileAttributes.Directory;
                    break;
                }

                default:
                {
                    var treeNode = GetOneNodeByRootPath(fileName);

                    if (null == treeNode)
                    {
                        return DokanResult.Error;
                    }

                    treeNode.GetFileInformation(ref fileInfo);
                    break;
                }
            }

            return DokanResult.Success;
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            security = null;
            return DokanResult.Error;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = "test";
            features = FileSystemFeatures.None;
            fileSystemName = string.Empty;
            return DokanResult.Success;
        }

        public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus Mounted(DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            bytesRead = buffer.Length;
            return DokanResult.Success;
        }

        public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus Unmounted(DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
        {
            bytesWritten = 0;
            return DokanResult.Error;
        }

        const string kSink = "`Sink";
    }
}
