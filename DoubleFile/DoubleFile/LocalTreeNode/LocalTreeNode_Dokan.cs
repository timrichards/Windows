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

        internal NtStatus FindFiles(out IList<FileInformation> files)
        {
            Util.WriteLine("internal FindFiles");
            files = new List<FileInformation> { };

            foreach (var node in Nodes)
            {
                FileInformation fi;

                node.GetFileInformation(out fi);
                files.Add(fi);
            }

            if (0 < NodeDatum.LengthHere)
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
                        Length = (long)lvItem.Length.ToUlong(),
                        CreationTime = lvItem.CreatedRaw,
                        LastWriteTime = lvItem.ModifiedRaw,
                        Attributes = (FileAttributes)Convert.ToInt32(lvItem.Attributes, /* from base */ 16)
                    };

                    files.Add(fi);
                }
            }

            return DokanResult.Success;
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            Util.WriteLine("public FindFiles");
            info.IsDirectory = true;

            if ("\\" == fileName)
            {
                files = new List<FileInformation> { };

                foreach (var rootNode in LocalTV.RootNodes)
                {
                    FileInformation fi;

                    rootNode.GetFileInformation(out fi);
                    files.Add(fi);
                }

                return DokanResult.Success;
            }
            else
            {
                var treeNode = LocalTV.GetOneNodeByRootPathA(fileName, null);

                if (null == treeNode)
                {
                    files = new List<FileInformation> { };
                    return DokanResult.Error;
                }

                return treeNode.FindFiles(out files);
            }
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

        internal NtStatus GetFileInformation(out FileInformation fileInfo)
        {
            Util.WriteLine("internal GetFileInformation");
            fileInfo = new FileInformation
            {
                FileName = PathShort,
                Attributes = NodeDatum.FolderDetails.Attributes,
                CreationTime = NodeDatum.FolderDetails.Created,
                LastWriteTime = NodeDatum.FolderDetails.Modified,
                Length = (long)NodeDatum.LengthTotal                
            };

            Util.WriteLine(PathShort);
            return DokanResult.Success;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            Util.WriteLine("public GetFileInformation");
            Util.WriteLine(fileName);
            if (fileName == "\\")
            {
                fileInfo = new FileInformation {FileName = fileName};

                fileInfo.Attributes = FileAttributes.Directory;
                fileInfo.LastAccessTime = DateTime.Now;
                fileInfo.LastWriteTime = null;
                fileInfo.CreationTime = null;

                return DokanResult.Success;
            }

            var treeNode = LocalTV.GetOneNodeByRootPathA(fileName, null);

            if (null == treeNode)
            {
                fileInfo = new FileInformation { };
                return DokanResult.Error;
            }

            info.IsDirectory = true;
            return treeNode.GetFileInformation(out fileInfo);
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            security = null;
            return DokanResult.Error;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
        {
            Util.WriteLine("public GetVolumeInformation");
            volumeLabel = "test";
            features = FileSystemFeatures.None;
            fileSystemName = string.Empty;
   //         info.IsDirectory = true;
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
