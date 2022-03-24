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
        Jobb? jobb = null;
        Status status = Status.Klar;
        TaskCompletionSource<bool>? formatComplete = null;

        readonly USBdeviceView? mDeviceView = null;

        private enum Status
        {
            Klar,
            Rensing,
            Format,
            Ferdig,
            Feil
        };

        private readonly string[] statusTekster = { "Klar", "Renser", "Formaterer", "Ferdig", "Feil!" };

        private char? driveLetter;
        char? DriveLetter
        {
            get { return driveLetter; }
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
                if (value != null && value.Length > 0)
                    diskName = value;
                UpdateDisk();
            }
        }

        private void UpdateDisk()
        {
            string _drive = "";
            if (driveLetter != null) _drive = driveLetter.ToString() + ":";
            if (mDeviceView != null)
                mDeviceView.View.Invoke((MethodInvoker)delegate
                {
                    mDeviceView.UpdateDisk(_drive, DiskName);
                });
        }

        private void UpdateStatus(Status nyStatus, string? text = null)
        {
            status = nyStatus;

            string nyText = text ?? statusTekster[(int)nyStatus];

            Color backColor;
            Color foreColor;

            switch (nyStatus)
            {
                case Status.Klar:
                    backColor = Color.Blue;
                    foreColor = Color.White;
                    break;
                case Status.Rensing:
                case Status.Format:
                    backColor = Color.Yellow;
                    foreColor = Color.Black;
                    break;
                case Status.Ferdig:
                    backColor = Color.Green;
                    foreColor = Color.White;
                    break;
                case Status.Feil:
                    backColor = Color.Red;
                    foreColor = Color.Black;
                    break;
                default:
                    backColor = Color.White;
                    foreColor = Color.Black;
                    break;
            }

            if (mDeviceView != null)
                mDeviceView.View.Invoke((MethodInvoker)delegate
                {
                    mDeviceView.UpdateColorActionItem(backColor, foreColor, nyText);
                });
        }

        bool Valid() { return true; }

        public USBdevice(ListView view, USB_EventInfo info, string gruppeNavn)
        {
            driveLetter = info.DriveLetter;
            diskName = info.DiskName;
            size = info.Size;
            Hub = info.Hub;
            lok = info.Lokasjon;
            deviceName = info.DeviceName;
            serial = info.Serial;
            volumeName = info.VolumeName;


            var existing = ExistingDevice(view, deviceName);
            if (existing != null)
            {
                existing.DriveLetter = info.DriveLetter;
                existing.DiskName = info.DiskName;
                status = Status.Feil;
            }
            else
            {
                mDeviceView = new(this, view);
                Add2View(GetListViewItem(), gruppeNavn);
            }
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
            if (mDeviceView != null)
                mDeviceView.View.Invoke((MethodInvoker)delegate
                {
                    mDeviceView.Add2View(listViewItem, gruppeNavn);
                });
            UpdateStatus(Status.Klar);
        }

        public void Remove()
        {
            if(mDeviceView!=null)
                mDeviceView.Remove();
        }

        public void CleanDisk()
        {
            UInt32 returnValue = 1;
            var diskC = WQL.QueryMi("root\\Microsoft\\Windows\\Storage", @"SELECT * FROM MSFT_Disk WHERE SerialNumber = '" + serial + "'");
            if (diskC != null)
                foreach (ManagementObject disk in diskC)
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
                        char? nydrive = part != null ? (char)part["DriveLetter"] : null;
                        driveLetter = nydrive;
                        diskName = "";
                        UpdateDisk();
                    }
                    break;
                }
        }

        internal void ByttGruppe(string gruppe)
        {
            if (mDeviceView != null)
                mDeviceView.View.Invoke((MethodInvoker)delegate
                {
                    mDeviceView.ChangeGroup(gruppe);
                });
        }

        public void Format(bool sizeLabel, string fs = "FAT32")
        {
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
#if false
                    ManagementBaseObject outParams = volume.InvokeMethod("Format", inParams, null);
                    UInt32 returnValue = (UInt32)outParams["ReturnValue"];

                    if (returnValue == 0)
                    {
                        DiskName = label;
                        UpdateDisk();
                        result = true;
                    }
#else
                    ManagementOperationObserver results = new();
                    results.Completed += new CompletedEventHandler(this.FormatCompleted);

                    /*ManagementBaseObject outParams = */
                    volume.InvokeMethod(results, "Format", inParams, null);
#endif
                    break;
                }
        }

        private void FormatCompleted(object sender, CompletedEventArgs e)
        {
            if (e.Status == ManagementStatus.NoError)
            {
                DiskName = newLabel;
            }
            formatComplete?.TrySetResult(true);
        }

        public void KjørJobb(bool clean, bool format, bool sizeLabel, string fs)
        {
            jobb = new Jobb(clean, format, sizeLabel, fs);
            while (status != Status.Ferdig && status != Status.Feil) NextState();
        }

        static internal USBdevice? ExistingDevice(ListView view, string deviceName)
        {
            ListViewItem[] found = view.Items.Find(deviceName, false);

            return found.Length > 0 ? (USBdevice)found[0].Tag : null;
        }

        private async void NextState()
        {
            switch (status)
            {
                case Status.Klar:
                    UpdateStatus(Status.Rensing);
                    if (jobb != null && jobb.Clean)
                        CleanDisk();
                    break;
                case Status.Rensing:
                    UpdateStatus(Status.Format);
                    formatComplete = new();
                    if (jobb != null && jobb.Format)
                    {
                        Format(jobb.SizeLabel, jobb.FileSystem);
                        await formatComplete.Task;
                    }
                    break;
                case Status.Format:
                    UpdateStatus(Status.Ferdig);
                    jobb = null;
                    break;
                case Status.Ferdig:
                case Status.Feil:
                    break;
            }
        }

        private class Jobb
        {
            public readonly bool Clean;
            public readonly bool Format;
            public readonly bool SizeLabel;
            public readonly string FileSystem;

            public Jobb(bool clean, bool format, bool sizeLabel, string fs)
            {
                this.Clean = clean;
                this.Format = format;
                this.SizeLabel = sizeLabel;
                this.FileSystem = fs;
            }
        }
    }
}
