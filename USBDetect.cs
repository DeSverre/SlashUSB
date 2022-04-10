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
            Location = "";
            HubID = "";
            HubFriendlyName = "";
            VolumePath = "";
            DeviceName = "";
            PNPserial = "";
            DiskName = "";
        }

        public char DriveLetter { get; set; }
        public UInt64 Size { get; set; }
        public string? Location { get; set; }
        public string? HubID { get; set; }
        public string? HubFriendlyName { get; set; }
        public string VolumePath { get; set; }
        public string DeviceName { get; set; }
        public string PNPserial { get; set; }
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

            var args = new USB_EventInfo
            {
                DriveLetter = driveLetter,
                Size = size,
                HubID = hubID,
                Location = location,
                DiskName = diskName,
                VolumePath = volumeName,
                DeviceName = deviceName,
                PNPserial = pnp_serial
            };

            if (args.IsValid())
                OnUSBFound(args);
        }

        private void OnUSBFound(USB_EventInfo e)
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
