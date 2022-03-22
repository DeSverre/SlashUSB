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
            DriveLetter = null;
            Lokasjon = "";
            Hub = "";
            VolumeName = "";
            DeviceName = "";
            Serial = "";
            DiskName = "";
        }

        public char? DriveLetter { get; set; }
        public UInt64 Size { get; set; }
        public string? Lokasjon { get; set; }
        public string Hub { get; set; }
        public string VolumeName { get; set; }
        public string DeviceName { get; set; }
        public string Serial { get; set; }
        public string DiskName { get; set; }
    }

    public class USB_RemovedEvent : EventArgs
    {
        public string? DeviceName { get; set;}
    }

    class USBDetect
    {
        public USBDetect()
        {
            WqlEventQuery vcQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");

            ManagementEventWatcher vcWatcher = new ManagementEventWatcher(vcQuery);
            vcWatcher.EventArrived += new EventArrivedEventHandler(VolumeChangedEvent);
            vcWatcher.Start();

            WqlEventQuery diskQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'MSFT_Disk' AND TargetInstance.BusType = 7");
            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(new ManagementScope(@"\root\Microsoft\Windows\Storage"),diskQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(NewDiskEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'MSFT_Disk' AND TargetInstance.BusType = 7");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(new ManagementScope(@"\root\Microsoft\Windows\Storage"), removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
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
            string? location = "";
            string hubID = "";
            string deviceName = "";
            UInt64 size = 0;
            string serial = "";

            ManagementBaseObject? instance = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            if (instance != null)
            {
                uint diskIndex = (uint)instance["Number"];

                ManagementObjectCollection ddc = WQL.QueryMi(@"SELECT * FROM Win32_DiskDrive WHERE Index=" + diskIndex);
                foreach (ManagementObject dd in ddc)
                {
                    pnp_deviceID = (string)dd["PnPDeviceID"];
                    size = (UInt64)dd.GetPropertyValue("Size");
                    deviceName = (string)dd["DeviceID"];
                    serial = (string)dd["SerialNumber"];
                }
            }
            CommonSendEvent("", pnp_deviceID, ref location, ref hubID, "", deviceName, size, serial);
        }

        private void VolumeChangedEvent(object sender, EventArrivedEventArgs e)
        {
            string drive = (string)e.NewEvent["DriveName"];
            string pnp_deviceID = "";
            string? location = "";
            string hubID = "";
            string diskName = "";
            string deviceName = "";
            uint diskIndex;
            UInt64 size = 0;
            string serial = "";
            ManagementObjectCollection ldc = WQL.QueryMi(@"SELECT * FROM Win32_LogicalDisk WHERE DeviceID='" + drive + "'");
            foreach (ManagementObject ld in ldc)
            {
                diskName = (string)ld["VolumeName"];
                var dpc = ld.GetRelated("Win32_DiskPartition");
                foreach (ManagementObject dp in dpc)
                {
                    var ddc = dp.GetRelated("Win32_DiskDrive");
                    foreach (ManagementObject dd in ddc)
                    {
                        pnp_deviceID = (string)dd["PnPDeviceID"];
                        diskIndex = (uint)dd.GetPropertyValue("Index");
                        size = (UInt64)dd.GetPropertyValue("Size");
                        deviceName = (string)dd["DeviceID"];
                        serial = (string)dd["SerialNumber"];
                    }
                }
            }
            CommonSendEvent(drive, pnp_deviceID, ref location, ref hubID, diskName, deviceName, size, serial);
        }

        private void CommonSendEvent(string drive, string pnp_deviceID, ref string? location, ref string hubID, string diskName, string deviceName, ulong size, string serial)
        {
            ManagementObjectCollection usbc = WQL.QueryMi(@"SELECT * FROM Win32_PnPEntity WHERE PnPDeviceID='" + pnp_deviceID.Replace(@"\", @"\\") + "'");
            foreach (ManagementObject usb in usbc)
            {
                var hubc = usb.GetRelated("Win32_USBController");
                foreach (var hub in hubc)
                {
                    hubID = (string)hub.GetPropertyValue("PnPDeviceID");
                }
            }

            usbc = WQL.QueryMi(@"SELECT * FROM Win32_PnPEntity WHERE PnPDeviceID LIKE 'USB\\%" + serial + "'");
            foreach (ManagementObject usb in usbc)
            {
                location = (string?)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\" + usb["PNPDeviceID"], "LocationInformation", "");
            }

            string volumeName = Pinvoke.GetVolumeName(drive + "\\");

            var args = new USB_EventInfo();
            args.DriveLetter = drive.Length > 0 ? drive[0] : null;
            args.Size = size;
            args.Hub = hubID;
            args.Lokasjon = location;
            args.DiskName = diskName;
            args.VolumeName = volumeName;
            args.DeviceName = deviceName;
            args.Serial = serial;
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
