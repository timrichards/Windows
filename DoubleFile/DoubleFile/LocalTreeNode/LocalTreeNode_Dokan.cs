using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using DokanNet;
using System.Linq;

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
            if (info.IsDirectory && mode == FileMode.CreateNew)
                return DokanResult.AccessDenied;
            return DokanResult.Success;
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
            const string colon = "`~";
            const string backslash = "~`";

            if (null != fileName)
            {
                return fileName.TrimStart('\\').Replace(colon, ":").Replace(backslash, @"\");
            }
            else
            {
                var strRoot = Root.PathFullGet(false);

                return PathFullGet(false).Replace(strRoot,
                    strRoot
                    .Replace(":", colon)
                    .Replace(@"\", backslash));
            }
        }

        LocalTreeNode GetOneNodeByRootPathA(string fileName)
        {
            return LocalTV.GetOneNodeByRootPathA(ConvertRootPath(fileName), null);
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            if (@"\" == fileName)
            {
                files = new List<FileInformation> { };

                foreach (var rootNode in LocalTV.RootNodes)
                {
                    var fi = new FileInformation { FileName = rootNode.ConvertRootPath() };

                    rootNode.GetFileInformation(ref fi);
                    files.Add(fi);
                }
            }
            else
            {
                var treeNode = GetOneNodeByRootPathA(fileName);

                if (null == treeNode)
                {
                    files = new FileInformation[0];
                    return DokanResult.Error;
                }

                treeNode.FindFiles(out files);
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
            fileInfo.Attributes = FileAttributes.Directory;
            fileInfo.CreationTime = NodeDatum.FolderDetails.Created;
            fileInfo.LastWriteTime = NodeDatum.FolderDetails.Modified;
            fileInfo.Length = (long)NodeDatum.LengthTotal;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            fileInfo = new FileInformation { FileName = fileName };

            if (fileName == @"\")
            {
                fileInfo.Attributes = FileAttributes.Directory;
                fileInfo.LastAccessTime = DateTime.Now;
                fileInfo.LastWriteTime = null;
                fileInfo.CreationTime = null;
                return DokanResult.Success;
            }

            var treeNode = GetOneNodeByRootPathA(fileName);

            if (null == treeNode)
            {
                return DokanResult.Error;
            }

            treeNode.GetFileInformation(ref fileInfo);
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
            return DokanResult.Error;
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
            bytesRead = 0;
            return DokanResult.NotImplemented;
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
    }
}
