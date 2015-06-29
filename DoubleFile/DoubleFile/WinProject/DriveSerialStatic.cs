using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;

namespace DoubleFile
{
    static internal class DriveSerialStatic
    {
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

            var bAsk_DriveModel = ((false == string.IsNullOrWhiteSpace(strDriveModel)) &&
                ((false == string.IsNullOrWhiteSpace(strDriveModel_ref)) &&
                (strDriveModel != strDriveModel_ref)));

            var bAsk_DriveSerial = ((false == string.IsNullOrWhiteSpace(strDriveSerial)) &&
                ((false == string.IsNullOrWhiteSpace(strDriveSerial_ref)) &&
                (strDriveSerial != strDriveSerial_ref)));

            if ((bAsk_DriveModel || bAsk_DriveSerial) &&
                ((System.Windows.MessageBoxResult.No == 
                MBoxStatic.ShowDialog("Overwrite user-entered drive model and/or serial # for " +
                strDriveLetter + @"\ ?", "Save Directory Listings",
                System.Windows.MessageBoxButton.YesNo))))
            {
                // separating these allows one user value to substitute blank robo-get, while keeping the other one
                // here overwriting robo-get values with the ones the user entered
                // because the user said No.

                if (bAsk_DriveModel)
                    strDriveModel = strDriveModel_ref;

                if (bAsk_DriveSerial)
                    strDriveSerial = strDriveSerial_ref;
            }

            if (false == string.IsNullOrWhiteSpace(strDriveModel))
                strDriveModel_ref = strDriveModel;

            if (false == string.IsNullOrWhiteSpace(strDriveSerial))
                strDriveSerial_ref = strDriveSerial;

            return nSize;
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
