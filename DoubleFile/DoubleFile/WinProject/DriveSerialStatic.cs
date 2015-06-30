using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;

namespace DoubleFile
{
    static internal class DriveSerialStatic
    {
        internal const string WMI = "WMI: ";

        static internal ulong? Get(string strSourcePath,
            ref string strDriveModel_ref, ref string strDriveSerial_ref)
        {
            var strDriveLetter = strSourcePath.Substring(0, 2);
            string strDriveModel = null;
            string strDriveSerial = null;
            ulong? nSize = null;
            var bWMI = false;

            try
            {
                bWMI = (ServiceControllerStatus.Running == new ServiceController("Winmgmt").Status);
            }
            catch (InvalidOperationException) { }

            if (bWMI)
                new ManagementObjectSearcher
            (
                new ManagementScope(@"\\.\ROOT\cimv2"),
                new ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DeviceID='" + strDriveLetter + "'")
            )
                .Get().Cast<ManagementObject>().FirstOnlyAssert(logicalDisk =>
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
                        try { nSize = (ulong?)diskDrive["Size"]; }
                        catch (ManagementException) { }
                        try { strDriveModel = diskDrive["DriveModel"].ToPrintString(); }
                        catch (ManagementException) { }

                        if (string.IsNullOrWhiteSpace(strDriveModel))
                        {
                            try { strDriveModel = diskDrive["Caption"].ToPrintString(); }
                            catch (ManagementException) { }
                        }

                        try { strDriveSerial = diskDrive["SerialNumber"].ToPrintString(); }
                        catch (ManagementException) { }

                        if (string.IsNullOrWhiteSpace(strDriveSerial))
                        {
                            diskDrive
                                .GetRelated("Win32_PhysicalMedia")
                                .Cast<ManagementObject>()
                                .FirstOnlyAssert(diskMedia =>
                            {
                                try { strDriveSerial = diskMedia["SerialNumber"].ToPrintString(); }
                                catch (ManagementException) { }
                            });
                        }
                    });
                });
            });

            //if (string.IsNullOrEmpty(strDriveModel))
            //{
            //    var listDrives = GetAll();


            //}

            bool? bAskedYesOverwrite = null;

            Func<bool> AskOverwrite = () =>
            {
                if (null != bAskedYesOverwrite)
                    return bAskedYesOverwrite.Value;

                bAskedYesOverwrite =
                    ((System.Windows.MessageBoxResult.Yes ==
                    MBoxStatic.ShowDialog("Overwrite user-entered drive model and/or serial # for " +
                    strDriveLetter + @"\ ?", "Save Directory Listings",
                    System.Windows.MessageBoxButton.YesNo)));

                return bAskedYesOverwrite.Value;
            };

            CheckValues(strDriveModel, ref strDriveModel_ref, AskOverwrite);
            CheckValues(strDriveSerial, ref strDriveSerial_ref, AskOverwrite);
            return nSize;
        }

        static void CheckValues(string strValue, ref string strValue_ref, Func<bool> AskOverwrite)
        {
            if (null != strValue)
                strValue = WMI + strValue;

            var bAsk = ((false == string.IsNullOrWhiteSpace(strValue)) &&
                (false == string.IsNullOrWhiteSpace(strValue_ref)) &&
                (false == strValue_ref.StartsWith(WMI)) &&
                (strValue != strValue_ref));

            // separating these allows one user value to substitute blank robo-get, while keeping the other one
            // here overwriting robo-get values with the ones the user entered
            // because the user said No.

            if (bAsk && (false == AskOverwrite()))
                strValue = strValue_ref;

            if (false == string.IsNullOrWhiteSpace(strValue))
                strValue_ref = strValue;
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

                try { nSize = (ulong?)diskDrive["Size"]; } catch (ManagementException) {}
                try { strDriveModel = diskDrive["DriveModel"].ToPrintString(); } catch (ManagementException) {}

                if (string.IsNullOrWhiteSpace(strDriveModel))
                {
                    try { strDriveModel = diskDrive["Caption"].ToPrintString(); } catch (ManagementException) {}
                }

                try { strDriveSerial = diskDrive["SerialNumber"].ToPrintString(); } catch (ManagementException) {}

                if (string.IsNullOrWhiteSpace(strDriveSerial))
                {
                    diskDrive
                        .GetRelated("Win32_PhysicalMedia")
                        .Cast<ManagementObject>()
                        .FirstOnlyAssert(diskMedia =>
                    {
                        try { strDriveSerial = diskMedia["SerialNumber"].ToPrintString(); } catch (ManagementException) {}
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
