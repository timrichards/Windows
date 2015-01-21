using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace SearchDirLists
{
    class DriveSerial
    {
        // winddk.h
        const int FILE_DEVICE_MASS_STORAGE = 0x0000002d;
        const int METHOD_BUFFERED = 0;
        const int FILE_ANY_ACCESS = 0x00000000;
        static int CTL_CODE(int DeviceType, int Function, int Method, int Access)
        {
            return ((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method);
        }

        // ntddstor.h
        const int IOCTL_STORAGE_BASE = FILE_DEVICE_MASS_STORAGE;
        static int IOCTL_STORAGE_QUERY_PROPERTY = CTL_CODE(IOCTL_STORAGE_BASE, 0x0500, METHOD_BUFFERED, FILE_ANY_ACCESS);
        static int IOCTL_STORAGE_GET_MEDIA_SERIAL_NUMBER = CTL_CODE(IOCTL_STORAGE_BASE, 0x0304, METHOD_BUFFERED, FILE_ANY_ACCESS);

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        internal struct STORAGE_DESCRIPTOR_HEADER
        {
            public Int32 Version;
            public Int32 Size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        internal struct STORAGE_DEVICE_DESCRIPTOR_
        {
            public Int32 Version;
            public Int32 Size;
            public byte DeviceType;
            public byte DeviceTypeModifier;
            public byte RemovableMedia;
            public byte CommandQueueing;
            public Int32 VendorIdOffset;
            public Int32 ProductIdOffset;
            public Int32 ProductRevisionOffset;
            public Int32 SerialNumberOffset;
            public byte BusType;
            public Int32 RawPropertiesLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10240)]
            public byte[] RawDeviceProperties;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        internal struct STORAGE_DEVICE_DESCRIPTOR
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10240)]
            public byte[] data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        internal struct STORAGE_PROPERTY_QUERY
        {
            public Int32 PropertyId;
            public Int32 QueryType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public byte[] AdditionalParameters;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        internal struct MEDIA_SERIAL_NUMBER_DATA
        {
            public Int32 SerialNumberLength;
            public Int32 Result;
            public Int32 Reserved1;
            public Int32 Reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] AdditionalParameters;
        }

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern bool DeviceIoControl(
            SafeHandle hDevice,
            int IoControlCode,
            ref STORAGE_PROPERTY_QUERY InBuffer,
            int nInBufferSize,
            out STORAGE_DESCRIPTOR_HEADER OutBuffer,
            int nOutBufferSize,
            out int pBytesReturned,
            IntPtr Overlapped
        );

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool DeviceIoControl(
            SafeHandle hDevice,
            int IoControlCode,
            ref STORAGE_PROPERTY_QUERY InBuffer,
            int nInBufferSize,
            out STORAGE_DEVICE_DESCRIPTOR OutBuffer,
            int nOutBufferSize,
            out int pBytesReturned,
            IntPtr Overlapped
        );

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool DeviceIoControl(
            SafeHandle hDevice,
            int IoControlCode,
            ref IntPtr InBuffer,
            int nInBufferSize,
            out MEDIA_SERIAL_NUMBER_DATA outBuffer,
            int nOutBufferSize,
            out int pBytesReturned,
            IntPtr Overlapped
        );

        [DllImport("Kernel32.dll", SetLastError = false, CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControl(
            SafeHandle hDevice,
            int IoControlCode,

            [MarshalAs(UnmanagedType.AsAny)]
                [In] object InBuffer,
                uint nInBufferSize,

                [MarshalAs(UnmanagedType.AsAny)]
                [Out] object OutBuffer,
                uint nOutBufferSize,

                out uint pBytesReturned,
                [In] IntPtr Overlapped
                );

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

        internal static void Get(string strPath, out string strModel_out, out string strSerialNo_out, out int? nSize_out)
        {
            var letter = strPath.Substring(0, 2);

            string strModel = null;
            string strSerialNo = null;
            object objSize = null;

            //var query = new STORAGE_PROPERTY_QUERY { PropertyId = 0, QueryType = 0 };
            //var qsize = Marshal.SizeOf(query);
            //int written;

            //using (SafeHandle handle = CreateFile(@"\\.\" + letter, 0, FileShare.ReadWrite, IntPtr.Zero, 3, 0, IntPtr.Zero))
            //{
            //    STORAGE_DESCRIPTOR_HEADER header = new STORAGE_DESCRIPTOR_HEADER();
            //    int rsize = Marshal.SizeOf(header);

            //    bool ok = DeviceIoControl(handle, IOCTL_STORAGE_QUERY_PROPERTY, ref query, qsize, out header,
            //        Marshal.SizeOf(header), out written, IntPtr.Zero);

            //    Utilities.Assert(0, ok);
            //    Utilities.Assert(0, written == rsize);

            //    STORAGE_DEVICE_DESCRIPTOR data = new STORAGE_DEVICE_DESCRIPTOR();

            //    ok = DeviceIoControl(handle, IOCTL_STORAGE_QUERY_PROPERTY, ref query, qsize, out data,
            //        header.Size, out written, IntPtr.Zero);

            //    Utilities.Assert(0, ok);
            //    Utilities.Assert(0, written == rsize);
            //    Utilities.Assert(0, written == header.Size);

            //    //MEDIA_SERIAL_NUMBER_DATA data = new MEDIA_SERIAL_NUMBER_DATA();
            //    //int rsize = Marshal.SizeOf(data);

            //    //IntPtr foo = IntPtr.Zero;
            //    //bool ok = DeviceIoControl(handle, IOCTL_STORAGE_GET_MEDIA_SERIAL_NUMBER, ref foo, 0, out data,
            //    //    rsize, out written, IntPtr.Zero);

            //    //Utilities.Assert(0, ok);
            //    //Utilities.Assert(0, written == rsize);
            //}

            bool bWMI = false;

            try
            {
                bWMI = (new System.ServiceProcess.ServiceController("Winmgmt").Status == System.ServiceProcess.ServiceControllerStatus.Running);
            }
            catch (InvalidOperationException) { }

            if (bWMI)
                new ManagementObjectSearcher(
                    new ManagementScope(@"\\.\ROOT\cimv2"),
                    new ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DeviceID='" + letter + "'")
            ).Get().Cast<ManagementObject>().FirstOnlyAssert(new Action<ManagementObject>((logicalDisk) =>
            {
                logicalDisk.GetRelated("Win32_DiskPartition").Cast<ManagementObject>().FirstOnlyAssert(new Action<ManagementObject>((partition) =>
                {
                    partition.GetRelated("Win32_DiskDrive").Cast<ManagementObject>().FirstOnlyAssert(new Action<ManagementObject>((diskDrive) =>
                    {
                        foreach (var prop in diskDrive.Properties)
                        {
                            Utilities.WriteLine(prop.Name + "\t\t\t" + prop.Value);
                        }

                        objSize = diskDrive["Size"];
                        strModel = diskDrive["Model"].ToPrintString();

                        if ((strModel == null) || (strModel.Trim().Length == 0))
                        {
                            strModel = diskDrive["Caption"].ToPrintString();
                        }

                        strSerialNo = diskDrive["SerialNumber"].ToPrintString();

                        if ((strSerialNo == null) || (strSerialNo.Trim().Length == 0))
                        {
                            diskDrive.GetRelated("Win32_PhysicalMedia").Cast<ManagementObject>().FirstOnlyAssert(new Action<ManagementObject>((diskMedia) =>
                            {
                                foreach (var prop in diskMedia.Properties)
                                {
                                    Utilities.WriteLine(prop.Name + "\t\t\t" + prop.Value);
                                }

                                strSerialNo = diskMedia["SerialNumber"].ToPrintString();
                            }));
                        }
                    }));
                }));
            }));

            // out parameters can't be in lambda
            nSize_out = null;

            strModel_out = strModel;
            strSerialNo_out = strSerialNo;

            if ((objSize as string) != null)
            {
                int n = 0;

                int.TryParse(objSize as string, out n);
                nSize_out = n;
            }
        }
    }
}
