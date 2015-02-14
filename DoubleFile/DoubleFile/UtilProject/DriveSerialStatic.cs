using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace DoubleFile
{
    internal static class DriveSerialStatic
    {
        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(
            string lpFileName,
            FileAccess dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        internal static void Get(string strPath, out string strDriveModel_out,
            out string strDriveSerial_out, out ulong? nSize_out)
        {
            var letter = strPath.Substring(0, 2);

            string strDriveModel = null;
            string strDriveSerial = null;
            ulong? nSize = null;

            var bWMI = false;

            try
            {
                bWMI = (new System.ServiceProcess.ServiceController("Winmgmt").Status ==
                    System.ServiceProcess.ServiceControllerStatus.Running);
            }
            catch (InvalidOperationException) { }

            if (bWMI)
                new ManagementObjectSearcher(
                    new ManagementScope(@"\\.\ROOT\cimv2"),
                    new ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DeviceID='" + letter + "'")
            ).Get().Cast<ManagementObject>().FirstOnlyAssert(logicalDisk =>
            {
                logicalDisk
                    .GetRelated("Win32_DiskPartition")
                    .Cast<ManagementObject>()
                    .FirstOnlyAssert(partition =>
                {
                    partition
                        .GetRelated("Win32_DiskDrive")
                        .Cast<ManagementObject>()
                        .FirstOnlyAssert(diskDrive =>
                    {
                        try { nSize = (ulong?)diskDrive["Size"]; } catch { }
                        try { strDriveModel = diskDrive["DriveModel"].ToPrintString(); } catch { }

                        if (string.IsNullOrWhiteSpace(strDriveModel))
                        {
                            try { strDriveModel = diskDrive["Caption"].ToPrintString(); } catch { }
                        }

                        try { strDriveSerial = diskDrive["SerialNumber"].ToPrintString(); } catch { }

                        if (string.IsNullOrWhiteSpace(strDriveSerial))
                        {
                            diskDrive
                                .GetRelated("Win32_PhysicalMedia")
                                .Cast<ManagementObject>()
                                .FirstOnlyAssert(diskMedia =>
                            {
                                try { strDriveSerial = diskMedia["SerialNumber"].ToPrintString(); } catch { }
                            });
                        }
                    });
                });
            });

            if (string.IsNullOrEmpty(strDriveModel))
            {
                var listDrives = GetAll();


            }

            strDriveModel_out = strDriveModel;
            strDriveSerial_out = strDriveSerial;
            nSize_out = nSize;
        }

        static IEnumerable<string> GetAll()
        {
            var listDrives = new List<string>();

            new ManagementObjectSearcher(
                new ManagementScope(@"\\.\ROOT\cimv2"),
                new ObjectQuery("SELECT * FROM Win32_DiskDrive")
            )
                .Get()
                .Cast<ManagementObject>()
                .ForEach(diskDrive =>
            {
                string strDriveModel = null;
                string strDriveSerial = null;
                ulong? nSize = null;

                try { nSize = (ulong?)diskDrive["Size"]; } catch { }
                try { strDriveModel = diskDrive["DriveModel"].ToPrintString(); } catch { }

                if (string.IsNullOrWhiteSpace(strDriveModel))
                {
                    try { strDriveModel = diskDrive["Caption"].ToPrintString(); } catch { }
                }

                try { strDriveSerial = diskDrive["SerialNumber"].ToPrintString(); } catch { }

                if (string.IsNullOrWhiteSpace(strDriveSerial))
                {
                    diskDrive
                        .GetRelated("Win32_PhysicalMedia")
                        .Cast<ManagementObject>()
                        .FirstOnlyAssert(diskMedia =>
                    {
                        try { strDriveSerial = diskMedia["SerialNumber"].ToPrintString(); } catch { }
                    });
                }

                var strOut = "\t\t\t" + strDriveModel + "\t\t\t" + strDriveSerial + "\t\t\t" + nSize;

                if (false == string.IsNullOrWhiteSpace(strOut))
                {
                    listDrives.Add(strOut);
                }
            });

            return listDrives;
        }
    }
}
