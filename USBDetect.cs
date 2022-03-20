using Microsoft.Win32;
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
            Drive = "";
            Lokasjon = "";
            Hub = "";
            VolumeName = "";
            DeviceName = "";
            Serial = "";
        }

        public string Drive { get; set; }
        public UInt64 Size { get; set; }
        public string Lokasjon { get; set; }
        public string Hub { get; set; }
        public string VolumeName { get; set; }
        public string DeviceName { get; set; }
        public string Serial { get; set; }
        public string DiskName { get; set; }
    }

    class USBDetect
    {
        public USBDetect()
        {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(VolumeChangedEvent);
            insertWatcher.Start();

            //WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'MSFT_Disk' AND TargetInstance.BusType = 7");

            //WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            //ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            //removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            //removeWatcher.Start();
        }

        private void VolumeChangedEvent(object sender, EventArrivedEventArgs e)
        {
            string drive = (string)e.NewEvent["DriveName"];
            string pnp_deviceID = "";
            string location = "";
            string hubID = "";
            string diskName;
            string deviceName = "";
            uint diskIndex;
            UInt64 size = 0;
            ManagementObjectCollection ldc = WQL.QueryMi(@"SELECT * FROM Win32_LogicalDisk WHERE DeviceID='" + drive + "'");
            foreach (ManagementObject ld in ldc)
            {
                diskName = (string)ld["VolumeName"];
                var dpc = ld.GetRelated("Win32_DiskPartition");
                string serial = "";
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
                    location = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\" + usb["PNPDeviceID"], "LocationInformation", "");
                }

                string volumeName = Pinvoke.GetVolumeName(drive+"\\");

                var args = new USB_EventInfo();
                args.Drive = drive[..1];
                args.Size = size;
                args.Hub = hubID;
                args.Lokasjon = location;
                args.DiskName = diskName;
                args.VolumeName = volumeName;
                args.DeviceName = deviceName;
                args.Serial = serial;
                OnUSBFound(args);
            }
        }

        protected virtual void OnUSBFound(USB_EventInfo e)
        {
            EventHandler<USB_EventInfo> handler = USBInserted;
            handler(this, e);
        }

        public event EventHandler<USB_EventInfo> USBInserted;
    }
}
