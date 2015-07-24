using System;
using System.Collections.Generic;
using System.IO;

namespace DoubleFile
{
    static class Win32FindFileStatic
    {
        [Serializable]
        internal class FileData
        {
            internal readonly FileAttributes Attributes;
            internal readonly DateTime CreationTimeUtc;
            internal readonly DateTime LastAccessTimeUtc;
            internal readonly DateTime LastWriteTimeUtc;

            internal ulong
                Size
            { get { MBoxStatic.Assert(99928, 0 <= _Size); return (ulong)_Size; } }
            readonly long _Size;

            internal readonly bool IsValid = false;

            static internal NativeMethods.DATUM WinFile(string strFile)
            {
                var winFindData = default(NativeMethods.DATUM);

                NativeMethods.FindFirstFileExW(@"\\?\" + strFile, NativeMethods.IndexInfoLevels.FindExInfoBasic,
                    out winFindData, NativeMethods.IndexSearchOps.FindExSearchNameMatch, IntPtr.Zero,
                    NativeMethods.FIND_FIRST_EX_LARGE_FETCH);

                winFindData.strAltFileName = strFile.Replace(@"\\", @"\");          // 8.3 not used
                return winFindData;
            }

            internal FileData(NativeMethods.DATUM findData)
            {
                Attributes = findData.fileAttributes;
                CreationTimeUtc = ConvertDateTime(findData.ftCreationTimeHigh, findData.ftCreationTimeLow);
                LastAccessTimeUtc = ConvertDateTime(findData.ftLastAccessTimeHigh, findData.ftLastAccessTimeLow);
                LastWriteTimeUtc = ConvertDateTime(findData.ftLastWriteTimeHigh, findData.ftLastWriteTimeLow);
                _Size = CombineHighLowInts(findData.nFileSizeHigh, findData.nFileSizeLow);
                IsValid = (findData.ftCreationTimeHigh | findData.ftCreationTimeLow) != 0;
            }

            internal DateTime CreationTime => CreationTimeUtc.ToLocalTime();
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal DateTime LastAccessTime => LastAccessTimeUtc.ToLocalTime();
            internal DateTime LastWriteTime => LastWriteTimeUtc.ToLocalTime();
            static long CombineHighLowInts(uint high, uint low) => (((long)high) << 0x20) | low;
            static DateTime ConvertDateTime(uint high, uint low) => DateTime.FromFileTimeUtc(CombineHighLowInts(high, low));
        }

        static internal bool GetDirectory(string strDir, out IEnumerable<NativeMethods.DATUM> ieDirs, out IEnumerable<NativeMethods.DATUM> ieFiles,
            out string strWin32Error)
        {
            strWin32Error = null;

            var winFindData = default(NativeMethods.DATUM);

            var handle = NativeMethods.FindFirstFileExW(@"\\?\" + strDir + @"\*", NativeMethods.IndexInfoLevels.FindExInfoBasic,
                out winFindData, NativeMethods.IndexSearchOps.FindExSearchNameMatch, IntPtr.Zero,
                NativeMethods.FIND_FIRST_EX_LARGE_FETCH);

            if (handle == NativeMethods.InvalidHandleValue)
            {
                strWin32Error = new System.ComponentModel.Win32Exception(
                        System.Runtime.InteropServices.Marshal.GetLastWin32Error()).Message;

                ieDirs = null;
                ieFiles = null;
                return false;
            }

            var dictDirs = new SortedDictionary<NativeMethods.DATUM, bool>();
            var dictFiles = new SortedDictionary<NativeMethods.DATUM, bool>();

            do
            {
                if ("..".Contains(winFindData.strFileName))
                    continue;

                winFindData.strAltFileName = (strDir + '\\' +
                    winFindData.strFileName).Replace(@"\\", @"\");    // 8.3 not used

                if ((winFindData.fileAttributes & FileAttributes.Directory) != 0)
                {
                    if ((winFindData.fileAttributes & FileAttributes.ReparsePoint) != 0)
                    {
                        const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
                        const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;

                        // stay on source volume. Treat mount points and symlinks as files.
                        if (((winFindData.dwReserved0 & IO_REPARSE_TAG_MOUNT_POINT) != 0) ||
                            ((winFindData.dwReserved0 & IO_REPARSE_TAG_SYMLINK) != 0))
                        {
                            dictFiles.Add(winFindData, false);
                            continue;
                        }
                    }

                    dictDirs.Add(winFindData, false);
                }
                else
                {
                    dictFiles.Add(winFindData, false);
                }
            }
            while (NativeMethods.FindNextFileW(handle, out winFindData));

            NativeMethods.FindClose(handle);
            ieDirs = dictDirs.Keys;
            ieFiles = dictFiles.Keys;
            return true;
        }
    }
}
