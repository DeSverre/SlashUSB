using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace USkummelB
{
    internal class USBdevice
    {
        private string drive;
        string Drive { get { return drive; } 
            set 
            {
                if (value.Length > 0)
                    drive = value;
                UpdateDisk();
            }
        }
        private string diskName;
        string DiskName
        {
            get { return diskName; }
            set
            {
                if(value.Length > 0)
                    diskName = value;
                UpdateDisk();
            }
        }

        private void UpdateDisk()
        {
            if(mItem != null)
            {
                mItem.SubItems[0].Text = Drive;
                mItem.SubItems[1].Text = DiskName;
            }
        }


        UInt64 size;
        readonly string hub;
        readonly string lok;
        readonly string deviceName;
        string serial;
        string volumeName;

        readonly ListView myView;

        ListViewItem? mItem = null;

        bool Valid() { return true; }

        public USBdevice(ListView view, USB_EventInfo info)
        {
            Drive = info.Drive;
            size = info.Size;
            hub = info.Hub;
            lok = info.Lokasjon;
            deviceName = info.DeviceName;
            serial = info.Serial;
            DiskName = info.DiskName;
            volumeName = info.VolumeName;

            myView = view;

            var existing = ExistingDevice(deviceName);
            if (existing != null)
            {
                existing.Drive = info.Drive;
                existing.DiskName = info.DiskName;
            }
            else
                Add2View(GetListViewItem());
        }

        private ListViewItem GetListViewItem()
        {
            var result = new ListViewItem(new[] { Drive!=null&& Drive.Length > 0 ? Drive + ":" : "", DiskName, deviceName, Utils.SizeSuffix(size), lok, hub })
            {
                Name = deviceName
            };
            return result;
        }

        private void Add2View(ListViewItem listViewItem)
        {
            mItem = listViewItem;
            mItem.Tag = this;
            var funnetGruppe = myView.Groups["listViewGroupFunnet"];
            mItem.Group = funnetGruppe;
            myView.Items.Add(mItem);
        }

        public void Remove()
        {
            if(mItem != null)
                mItem.Remove();
        }

        public void CleanDisk()
        {
            var diskC = WQL.QueryMi("root\\Microsoft\\Windows\\Storage", @"SELECT * FROM MSFT_Disk WHERE SerialNumber = '" +serial+"'");
            if(diskC != null)
                foreach(ManagementObject disk in diskC)
                {
                    ManagementBaseObject inParams = disk.GetMethodParameters("Clear");
                    inParams["RemoveData"] = true;
                    inParams["RemoveOEM"] = true;
                    inParams["ZeroOutEntireDisk"] = false;

                    ManagementBaseObject outParams = disk.InvokeMethod("Clear", inParams, null);
                    Console.WriteLine("Out parameters:");
                    string line;
                    line = "CreatedStorageJob: " + outParams["CreatedStorageJob"];
                    Console.WriteLine(line);
                    line = "ReturnValue: " + outParams["ReturnValue"];
                    Console.WriteLine("The ExtendedStatus out-parameter contains an object.");
                    Console.WriteLine(line);
                    break;
                }
        }
        public void Format()
        {
            var volumeC = WQL.QueryMi(@"SELECT * FROM Win32_Volume WHERE DriveLetter = '" + Drive + "'");
            if (volumeC != null)
                foreach (ManagementObject volume in volumeC)
                {
                    ManagementBaseObject inParams = volume.GetMethodParameters("Format");
                    inParams["FileSystem"] = "FAT32";
                    inParams["Label"] = DiskName;
                    inParams["QuickFormat"] = true;

                    ManagementBaseObject outParams = volume.InvokeMethod("Format", inParams, null);
                    Console.WriteLine("Out parameters:");
                    string line = "ReturnValue: " + outParams["ReturnValue"];
                    Console.WriteLine("The ExtendedStatus out-parameter contains an object.");
                    Console.WriteLine(line);
                    break;
                }
        }
        internal USBdevice? ExistingDevice(string deviceName)
        {
            ListViewItem[] found = myView.Items.Find(deviceName, false);

            return found.Length > 0 ? (USBdevice)found[0].Tag : null;
        }
    }
}
