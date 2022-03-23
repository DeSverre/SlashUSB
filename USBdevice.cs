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
        private enum Status
        {
            Klar,
            Jobber,
            Ferdig,
            Feil
        };

        private readonly string[] statusTekster = { "Klar", "Jobber", "Ferdig", "Feil!" };

        private char? driveLetter;
        char? DriveLetter { get { return driveLetter; } 
            set 
            {
                driveLetter = value;
                UpdateDisk();
            }
        }
        private string? diskName;
        string? DiskName
        {
            get { return diskName; }
            set
            {
                if(value != null && value.Length > 0)
                    diskName = value;
                UpdateDisk();
            }
        }

        private void UpdateDisk()
        {
            string _drive = "";
            if(driveLetter != null) _drive = driveLetter.ToString()+":";
 
            if(mItem != null)
            {
                mItem.SubItems[1].Text = _drive;
                mItem.SubItems[2].Text = DiskName;
            }
        }

        private void UpdateStatus(Status nyStatus)
        {
            if (mItem != null)
            {
                mItem.UseItemStyleForSubItems = false;
                var subIt = mItem.SubItems[0];
                subIt.Text = statusTekster[(int)nyStatus];

                switch (nyStatus)
                {
                    case Status.Klar:
                        subIt.BackColor = Color.Blue;
                        subIt.ForeColor = Color.White;
                        break;
                    case Status.Jobber:
                        subIt.BackColor = Color.Yellow;
                        subIt.ForeColor = Color.Black;
                        break;
                    case Status.Ferdig:
                        subIt.BackColor = Color.Green;
                        subIt.ForeColor = Color.White;
                        break;
                    case Status.Feil:
                        subIt.BackColor = Color.Red;
                        subIt.ForeColor = Color.Black;
                        break;
                }
            }
        }

        UInt64 size;
        readonly string hub;
        readonly string? lok;
        readonly string deviceName;
        string serial;
        string volumeName;

        readonly ListView myView;

        ListViewItem? mItem = null;

        bool Valid() { return true; }

        public USBdevice(ListView view, USB_EventInfo info)
        {
            DriveLetter = info.DriveLetter;
            DiskName = info.DiskName;
            size = info.Size;
            hub = info.Hub;
            lok = info.Lokasjon;
            deviceName = info.DeviceName;
            serial = info.Serial;
            volumeName = info.VolumeName;

            myView = view;

            var existing = ExistingDevice(deviceName);
            if (existing != null)
            {
                existing.DriveLetter = info.DriveLetter;
                existing.DiskName = info.DiskName;
            }
            else
                Add2View(GetListViewItem());
        }

        private ListViewItem GetListViewItem()
        {
            var result = new ListViewItem(new[] { "", DriveLetter != null && DriveLetter != null ? DriveLetter + ":" : "", DiskName, deviceName, Utils.SizeSuffix(size), lok, hub })
            {
                Name = deviceName,
                UseItemStyleForSubItems = false
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
            UpdateStatus(Status.Klar);
        }

        public void Remove()
        {
            if(mItem != null)
                mItem.Remove();
        }

        public bool CleanDisk()
        {
            bool result = false;
            var diskC = WQL.QueryMi("root\\Microsoft\\Windows\\Storage", @"SELECT * FROM MSFT_Disk WHERE SerialNumber = '" +serial+"'");
            if(diskC != null)
                foreach(ManagementObject disk in diskC)
                {
                    ManagementBaseObject inParams = disk.GetMethodParameters("Clear");
                    inParams["RemoveData"] = true;
                    inParams["RemoveOEM"] = true;
                    inParams["ZeroOutEntireDisk"] = false;

                    ManagementBaseObject outParams = disk.InvokeMethod("Clear", inParams, null);
                    UInt32 returnValue = (UInt32)outParams["ReturnValue"];
                    if (returnValue == 0)
                    {
                        inParams = disk.GetMethodParameters("CreatePartition");
                        inParams["AssignDriveLetter"] = true;
                        inParams["UseMaximumSize"] = true;
                        outParams = disk.InvokeMethod("CreatePartition", inParams, null);
                        returnValue = (UInt32)outParams["ReturnValue"];
                        ManagementBaseObject? part = outParams["CreatedPartition"] as ManagementBaseObject;
                        char nydrive = (char)part["DriveLetter"];
                        driveLetter = nydrive;
                        diskName = "";
                        UpdateDisk();
                        result = true;
                    }
                    break;
                }

            return result;
        }
        public bool Format(bool sizeLabel, string fs = "ExFAT")
        {
#if false
            bool result = false;
            var volumeC = WQL.QueryMi(@"SELECT * FROM Win32_Volume WHERE DeviceID = '" + volumeName.Replace(@"\", @"\\") + "'");
            if (volumeC != null)
                foreach (ManagementObject volume in volumeC)
                {
                    ManagementBaseObject inParams = volume.GetMethodParameters("Format");
                    string label = sizeLabel ? Utils.SizeSuffix(size,0) : (DiskName != null ? DiskName : "Ny disk");
                    inParams["FileSystem"] = "FAT32";
                    inParams["Label"] = label;
                    inParams["QuickFormat"] = true;

                    ManagementBaseObject outParams = volume.InvokeMethod("Format", inParams, null);
                    UInt32 returnValue = (UInt32)outParams["ReturnValue"];

                    if(returnValue == 0)
                    {
                        DiskName = label;
                        UpdateDisk();
                        result = true;
                    }
                    break;
                }
            return result;
#else
            bool result = false;
            var volumeC = WQL.QueryMi("root\\Microsoft\\Windows\\Storage", @"SELECT * FROM MSFT_Volume WHERE Path = '" + volumeName.Replace(@"\", @"\\") + "'");
            if (volumeC != null)
                foreach (ManagementObject volume in volumeC)
                {
                    ManagementBaseObject inParams = volume.GetMethodParameters("Format");
                    string label = sizeLabel ? Utils.SizeSuffix(size, 0) : (DiskName != null ? DiskName : "Ny disk");
                    inParams["FileSystem"] = fs;
                    inParams["FileSystemLabel"] = label;
                    inParams["Full"] = false;
                    inParams["Force"] = true;

                    ManagementBaseObject outParams = volume.InvokeMethod("Format", inParams, null);
                    UInt32 returnValue = (UInt32)outParams["ReturnValue"];

                    if (returnValue == 0)
                    {
                        DiskName = label;
                        UpdateDisk();
                        result = true;
                    }
                    break;
                }
            return result;
#endif
        }

        public void KjørJobb(bool clean,bool format,bool sizeLabel,string fs)
        {
            UpdateStatus(Status.Jobber);
            bool ok = true;
            if (clean) ok &= CleanDisk();
            if (format) ok &= Format(sizeLabel, fs);

            if (ok)
                UpdateStatus(Status.Ferdig);
            else
                UpdateStatus(Status.Feil);
        }

        internal USBdevice? ExistingDevice(string deviceName)
        {
            ListViewItem[] found = myView.Items.Find(deviceName, false);

            return found.Length > 0 ? (USBdevice)found[0].Tag : null;
        }
    }
}
