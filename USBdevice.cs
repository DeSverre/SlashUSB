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
        private string? newLabel = null;
        UInt64 size;
        public readonly string Hub;
        readonly string? lok;
        readonly string deviceName;
        string serial;
        string volumeName;

        readonly ListView myView;

        ListViewItem? mItem = null;


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
                if (value != null)
                {
                    driveLetter = value;
                    UpdateDisk();
                }
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

        private void UpdateStatus(Status nyStatus, string? text=null)
        {
            if (mItem != null)
            {
                mItem.UseItemStyleForSubItems = false;
                var subIt = mItem.SubItems[0];
                subIt.Text = text ?? statusTekster[(int)nyStatus];

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

        bool Valid() { return true; }

        public USBdevice(ListView view, USB_EventInfo info, string gruppeNavn)
        {
            DriveLetter = info.DriveLetter;
            DiskName = info.DiskName;
            size = info.Size;
            Hub = info.Hub;
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
                Add2View(GetListViewItem(),gruppeNavn);
        }

        private ListViewItem GetListViewItem()
        {
            var result = new ListViewItem(new[] { "", DriveLetter != null && DriveLetter != null ? DriveLetter + ":" : "", DiskName, deviceName, Utils.SizeSuffix(size), lok, Hub })
            {
                Name = deviceName,
                UseItemStyleForSubItems = false
            };
            return result;
        }

        private void Add2View(ListViewItem listViewItem, string gruppeNavn)
        {
            mItem = listViewItem;
            mItem.Tag = this;
            ListViewGroup gruppe = myView.Groups[gruppeNavn];
            mItem.Group = gruppe;
            myView.Items.Add(mItem);
            UpdateStatus(Status.Klar);
        }

        public void Remove()
        {
            if (mItem != null)
            {
                mItem.Remove();
                mItem = null;
            }
        }

        public void CleanDisk()
        {
            UpdateStatus(Status.Jobber, "Renser");
            UInt32 returnValue = 1;
            var diskC = WQL.QueryMi("root\\Microsoft\\Windows\\Storage", @"SELECT * FROM MSFT_Disk WHERE SerialNumber = '" +serial+"'");
            if(diskC != null)
                foreach(ManagementObject disk in diskC)
                {
                    ManagementBaseObject inParams = disk.GetMethodParameters("Clear");
                    inParams["RemoveData"] = true;
                    inParams["RemoveOEM"] = true;
                    inParams["ZeroOutEntireDisk"] = false;

                    ManagementBaseObject outParams = disk.InvokeMethod("Clear", inParams, null);
                    returnValue = (UInt32)outParams["ReturnValue"];
                    if (returnValue == 0)
                    {
                        inParams = disk.GetMethodParameters("CreatePartition");
                        inParams["AssignDriveLetter"] = true;
                        inParams["UseMaximumSize"] = true;
                        outParams = disk.InvokeMethod("CreatePartition", inParams, null);
                        returnValue = (UInt32)outParams["ReturnValue"];
                        ManagementBaseObject? part = outParams["CreatedPartition"] as ManagementBaseObject;
                        char? nydrive = (char)part["DriveLetter"];
                        driveLetter = nydrive;
                        diskName = "";
                        UpdateDisk();
                    }
                    break;
                }
            UpdateStatus(returnValue == 0?Status.Ferdig:Status.Feil);
        }

        internal void ByttGruppe(string gruppe)
        {
            if(mItem!=null)
                mItem.Group = myView.Groups[gruppe];
        }

        public void Format(bool sizeLabel, string fs = "FAT32")
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
            UpdateStatus(Status.Jobber, "Formaterer");
            var volumeC = WQL.QueryMi("root\\Microsoft\\Windows\\Storage", @"SELECT * FROM MSFT_Volume WHERE Path = '" + volumeName.Replace(@"\", @"\\") + "'");
            if (volumeC != null)
                foreach (ManagementObject volume in volumeC)
                {
                    ManagementBaseObject inParams = volume.GetMethodParameters("Format");
                    newLabel = sizeLabel ? Utils.SizeSuffix(size, 0) : (DiskName != null ? DiskName : "Ny disk");
                    inParams["FileSystem"] = fs;
                    inParams["FileSystemLabel"] = newLabel;
                    inParams["Full"] = false;
//                    inParams["Force"] = true;
                    inParams["RunAsJob"] = false;

                    ManagementOperationObserver results = new();
                    results.Progress += new ProgressEventHandler(this.ProgressHandler);
                    results.ObjectReady += new ObjectReadyEventHandler(this.FormatObjRdy);
                    results.Completed += new CompletedEventHandler(this.FormatCompleted);
                    results.ObjectPut += new ObjectPutEventHandler(this.ObjPut);

                    /*ManagementBaseObject outParams = */
                    volume.InvokeMethod(results, "Format", inParams, null);
                    break;
                }
#endif
        }

        private void ObjPut(object sender, ObjectPutEventArgs e)
        {
            var context = e.Context;
            var path = e.Path;
        }

        private void FormatObjRdy(object sender, ObjectReadyEventArgs e)
        {
            var context = e.Context;
            var newObj = e.NewObject;
        }

        private void FormatCompleted(object sender, CompletedEventArgs e)
        {
            if (e.Status == ManagementStatus.NoError)
            {
                myView.Invoke((MethodInvoker)delegate
                 {
                     DiskName = newLabel;
                     UpdateStatus(Status.Ferdig);
                 });
            }
            else
                myView.Invoke((MethodInvoker)delegate
                {
                    UpdateStatus(Status.Feil);
                });
        }

        private void ProgressHandler(object sender, ProgressEventArgs e)
        {
            
        }

        public void KjørJobb(bool clean,bool format,bool sizeLabel,string fs)
        {
            if (clean) CleanDisk();
            if (format) Format(sizeLabel, fs);
        }

        internal USBdevice? ExistingDevice(string deviceName)
        {
            ListViewItem[] found = myView.Items.Find(deviceName, false);

            return found.Length > 0 ? (USBdevice)found[0].Tag : null;
        }
    }
}
