using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace USkummelB
{
    public class USB_EventInfo : EventArgs
    {
        public USB_EventInfo()
        {
            DriveLetter = '\0';
            Lokasjon = "";
            HubID = "";
            HubFriendlyName = "";
            VolumePath = "";
            DeviceName = "";
            Serial = "";
            DiskName = "";
        }

        public char DriveLetter { get; set; }
        public UInt64 Size { get; set; }
        public string? Lokasjon { get; set; }
        public string? HubID { get; set; }
        public string? HubFriendlyName { get; set; }
        public string VolumePath { get; set; }
        public string DeviceName { get; set; }
        public string Serial { get; set; }
        public string DiskName { get; set; }

        public bool IsValid() { return VolumePath.Length > 0 && DeviceName.Length > 0; }
    }

    public class USB_RemovedEvent : EventArgs
    {
        public string? DeviceName { get; set;}
    }

    class USBDetect
    {
        public USBDetect()
        {
            WqlEventQuery diskQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'MSFT_Disk' AND TargetInstance.BusType = 7");
            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(new ManagementScope(@"\root\Microsoft\Windows\Storage"),diskQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(NewDiskEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'MSFT_Disk' AND TargetInstance.BusType = 7");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(new ManagementScope(@"\root\Microsoft\Windows\Storage"), removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DiskRemovedEvent);
            removeWatcher.Start();
        }

        private void DiskRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject? instance = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            if (instance != null)
            {
                uint diskIndex = (uint)instance["Number"];
                var args = new USB_RemovedEvent
                {
                    DeviceName = string.Format("\\\\.\\PHYSICALDRIVE{0}", diskIndex)
                };
                OnUSBRemoved(args);
            }
        }

        private void NewDiskEvent(object sender, EventArrivedEventArgs e)
        {
            string pnp_deviceID = "";
            string deviceName = "";
            UInt64 size = 0;
            string pnp_serial = "";

            if (e.NewEvent["TargetInstance"] is not ManagementBaseObject instance)
                return;

            uint diskIndex = (uint)instance["Number"];  // Index like in \\.\PHYSICALDRIVE{Index}

            using ManagementObjectCollection ddc = WQL.QueryMi(@"SELECT * FROM Win32_DiskDrive WHERE Index=" + diskIndex);
            foreach (ManagementObject dd in ddc)
            {
                pnp_deviceID = (string)dd["PnPDeviceID"];
                size = (UInt64)dd.GetPropertyValue("Size");
                deviceName = (string)dd["DeviceID"];
                pnp_serial = (string)dd["SerialNumber"];
                break;  // Only one
            }

            string diskName = "";
            string? location = "";
            string? hubID = "";
            string PDOname;
            using var pdoc = WQL.QueryMi(@"SELECT * FROM Win32_PnPSignedDriver WHERE DeviceID='" + pnp_deviceID.Replace(@"\", @"\\") + "'");
            foreach(ManagementObject pdo in pdoc)
            {
                PDOname = (string)pdo["PDO"];
            }

            // This is the device with name
            if (Pinvoke.CM_Locate_DevNode(out IntPtr pdnDevInst, pnp_deviceID, Pinvoke.CM_LOCATE_DEVNODE_NORMAL) == 0)
            {
                // The generic device
                if (Pinvoke.CM_Get_Parent(out IntPtr classNode, pdnDevInst, 0) == 0)
                {
                    location = Pinvoke.GetDeviceProperties(classNode, Pinvoke.DevRegProperty.LocationInfo);
                    // The connected hub
                    if (Pinvoke.CM_Get_Parent(out IntPtr hubNode, classNode, 0) == 0)
                    {
                        hubID = Pinvoke.GetDeviceProperties(hubNode, Pinvoke.DevRegProperty.HardwareId);
                    }
                }
            }

            string volumeName = "";
            char driveLetter = '\0';
            using var diskC = WQL.QueryMi("root\\Microsoft\\Windows\\Storage", @"SELECT * FROM MSFT_Disk WHERE SerialNumber = '" + pnp_serial + "'");
            foreach (ManagementObject disk in diskC)
            {
                using var partC = disk.GetRelated("MSFT_Partition");
                foreach (ManagementObject part in partC)
                {
                    driveLetter = (char)part["DriveLetter"];
                    using var volC = part.GetRelated("MSFT_Volume");
                    foreach (var vol in volC)
                        volumeName = (string)vol["Path"];
                }
            }

            var args = new USB_EventInfo();
            args.DriveLetter = driveLetter;
            args.Size = size;
            args.HubID = hubID;
            args.Lokasjon = location;
            args.DiskName = diskName;
            args.VolumePath = volumeName;
            args.DeviceName = deviceName;
            args.Serial = pnp_serial;
            
            if(args.IsValid())
                OnUSBFound(args);
        }

        private static void FindDeviceNameFromVolume(string cfVolName)
        {
            using (SafeFileHandle? handle = Pinvoke.CreateFile(cfVolName, FileAccess.Read, FileShare.Write | FileShare.Read | FileShare.Delete, IntPtr.Zero, FileMode.Open, Pinvoke.FILE_ATTRIBUTE_SYSTEM | Pinvoke.FILE_FLAG_SEQUENTIAL_SCAN, IntPtr.Zero))
            {
                if (!handle.IsInvalid)
                {
                    int bsize = 0x400;//some big size
                    IntPtr buffer = Marshal.AllocHGlobal(bsize);
                    uint bytesReturned = 0;

                    Pinvoke.DeviceIoControl(handle, Pinvoke.IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS, IntPtr.Zero, 0, buffer, (uint)bsize, out bytesReturned, IntPtr.Zero);
                    if (bytesReturned > 0)
                    {
                        Pinvoke._VOLUME_DISK_EXTENTS extents = Marshal.PtrToStructure<Pinvoke._VOLUME_DISK_EXTENTS>(buffer);
                        var numberOfDiskExtents = extents.NumberOfDiskExtents;
                        if (numberOfDiskExtents == 1)
                        {
                            string result = string.Format("\\\\.\\PHYSICALDRIVE{0}", extents.Extents.DiskNumber);
                            Console.WriteLine(result);
                        }
                    }
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        protected virtual void OnUSBFound(USB_EventInfo e)
        {
            USBinserted?.Invoke(this, e);
        }

        private void OnUSBRemoved(USB_RemovedEvent e)
        {
            USBremoved?.Invoke(this,e);
        }

        public event EventHandler<USB_EventInfo>? USBinserted;
        public event EventHandler<USB_RemovedEvent>? USBremoved;
    }
}
