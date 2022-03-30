using Shell32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace USkummelB
{
    internal class USBdevice
    {
        UInt64 size;
        public readonly string Hub;
        readonly string? lok;
        readonly string deviceName;
        string serial;
        string volumeName;
        Jobb? jobb = null;
        Status status = Status.Klar;

        readonly USBdeviceView? mDeviceView = null;

        private enum Status
        {
            Klar,
            Rensing,
            Format,
            Eject,
            Feil
        };

        private readonly string[] statusTekster = { "Klar", "Renser", "Formaterer", "OK - løser ut", "Feil!" };

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
        private TaskCompletionSource<UInt32>? formatResult;
        private TaskCompletionSource<ManagementStatus>? formatCompletedStatus;
        private ManagementObject? formatStorageJob = null;

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

        public bool InstanceAdded { get { return mDeviceView != null; } }

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
                case Status.Eject:
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

        public bool CleanDisk()
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
                        if (returnValue == 0)
                        {
                            ManagementBaseObject? part = outParams["CreatedPartition"] as ManagementBaseObject;
                            char? nydrive = null;
                            if (part != null)
                            {
                                nydrive = (char)part["DriveLetter"];
                                if (nydrive == '\0') nydrive = null;
                            }
                            driveLetter = nydrive;
                            diskName = "";
                            UpdateDisk();
                        }
                    }
                    break;
                }

            if(returnValue != 0) UpdateStatus(Status.Feil, "Rens feilet");

            return returnValue == 0;
        }

        internal void ByttGruppe(string gruppe)
        {
            if (mDeviceView != null)
                mDeviceView.View.Invoke((MethodInvoker)delegate
                {
                    mDeviceView.ChangeGroup(gruppe);
                });
        }

        public bool Format(bool sizeLabel, string fs = "FAT32")
        {
            bool result = false;
            var volumeC = WQL.QueryMi("root\\Microsoft\\Windows\\Storage", @"SELECT * FROM MSFT_Volume WHERE Path = '" + volumeName.Replace(@"\", @"\\") + "'");
            if (volumeC != null)
                foreach (ManagementObject volume in volumeC)
                {
                    ManagementBaseObject inParams = volume.GetMethodParameters("Format");
                    string newLabel = sizeLabel ? Utils.SizeSuffix(size, 0) : (DiskName != null ? DiskName : "PiratSoft(tm)");
                    inParams["FileSystem"] = fs;
                    inParams["FileSystemLabel"] = newLabel;
                    inParams["Full"] = false;
#if false
                    ManagementBaseObject outParams = volume.InvokeMethod("Format", inParams, null);
                    UInt32 returnValue = (UInt32)outParams["ReturnValue"];

                    if (returnValue == 0)
                    {
                        DiskName = newLabel;
                        result = true;
                    }
#else
                    // No need to do this async, but it works
                    ManagementOperationObserver results = new();
                    results.Completed += new CompletedEventHandler(this.FormatCompleted);
                    results.ObjectReady += new ObjectReadyEventHandler(this.FormatObjectReady);
                    // results.Progress += new ProgressEventHandler(this.FormatProgress); // I have not been able to get progressbar to work

                    InvokeMethodOptions formatOptions = new InvokeMethodOptions();
                    formatOptions.Timeout = TimeSpan.Zero;// new TimeSpan(0, 0, 5);

                    formatResult = new();
                    formatCompletedStatus = new();
                    /*ManagementBaseObject outParams = */
                    volume.InvokeMethod(results, "Format", inParams, formatOptions);

                    var resultValue = formatResult.Task.Result;  // Wait for event
                    if (resultValue == 0)
                    {
                        DiskName = newLabel;
                        result = true;
                    }
                    else
                        UpdateStatus(Status.Feil, "Format feilet");

                    var status = formatCompletedStatus.Task.Result;
#endif
                    break;
                }

            Task.Delay(1000).Wait();    // Disk still active if ejected immediately

            return result;
        }

        private void FormatProgress(object sender, ProgressEventArgs e)
        {
        }

        private void FormatObjectReady(object sender, ObjectReadyEventArgs e)
        {
            formatStorageJob = e.NewObject["CreatedStorageJob"] as ManagementObject;
            formatResult?.TrySetResult((UInt32)e.NewObject["ReturnValue"]);
        }

        private void FormatCompleted(object sender, CompletedEventArgs e)
        {
            formatCompletedStatus?.TrySetResult(e.Status);
        }

        public void KjørJobb(bool clean, bool format, bool sizeLabel, string fs)
        {
            jobb = new Jobb(clean, format, sizeLabel, fs);
            while (status != Status.Eject && status != Status.Feil)
            {
                NextState();
                RunState();
            }
        }

        private void RunState()
        {
            switch (status)
            {
                case Status.Klar: break;
                case Status.Rensing:
                    if (jobb != null && jobb.Clean) CleanDisk();
                    break;
                case Status.Format:
                    if (jobb != null && jobb.Format) Format(jobb.SizeLabel, jobb.FileSystem);
                    break;
                case Status.Eject:
                    Eject();
                    jobb = null;
                    break;
                case Status.Feil:
                    break;
            }
        }

        static internal USBdevice? ExistingDevice(ListView view, string deviceName)
        {
            ListViewItem[] found = view.Items.Find(deviceName, false);

            return found.Length > 0 ? (USBdevice)found[0].Tag : null;
        }

        private void NextState()
        {
            switch (status)
            {
                case Status.Klar:
                    UpdateStatus(Status.Rensing);
                    break;
                case Status.Rensing:
                    UpdateStatus(Status.Format);
                    break;
                case Status.Format:
                    UpdateStatus(Status.Eject);
                    break;
                case Status.Eject:
                case Status.Feil:
                    break;
            }
        }

        private void Eject()
        {
#if true
            static void EjectDriveShell(object? param)
            {
                if (param == null) return;

                var ssfDRIVES = 0x11;

                var driveName = param.ToString();

                var shell = new Shell();
                shell.NameSpace(ssfDRIVES).ParseName(driveName).InvokeVerb("Eject");
            }

            if (driveLetter != null)
            {
                string drive = driveLetter + ":\\";
                var staThread = new Thread(new ParameterizedThreadStart(EjectDriveShell));
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start(drive);
                staThread.Join();
            }
            else
            {
                var volumeC = WQL.QueryMi(@"SELECT * FROM Win32_Volume WHERE DeviceID = '" + volumeName.Replace(@"\", @"\\") + "'");
                if (volumeC != null)
                    foreach (ManagementObject volume in volumeC)
                    {
                        ManagementBaseObject inParams = volume.GetMethodParameters("Dismount");
                        inParams["Force"] = true;
                        inParams["Permanent"] = true;

                        ManagementBaseObject outParams = volume.InvokeMethod("Dismount", inParams, null);
                        UInt32 returnValue = (UInt32)outParams["ReturnValue"];
                        // just fail silently
                        break;
                    }
            }
#elif false
             if(driveLetter!=null)
            {
                string drive = driveLetter + ":\\";
                var result = Pinvoke.DeleteVolumeMountPoint(drive);
                if(result == false)
                {
                    var lastError = Marshal.GetHRForLastWin32Error();
                }
            }
#else
            using (var handle = Pinvoke.CreateFile(deviceName, FileAccess.Read, FileShare.Write | FileShare.Read | FileShare.Delete, IntPtr.Zero, FileMode.Open, Pinvoke.FILE_ATTRIBUTE_SYSTEM | Pinvoke.FILE_FLAG_SEQUENTIAL_SCAN, IntPtr.Zero))
            {
                uint bytesReturned;
                Pinvoke.DeviceIoControl(handle, Pinvoke.IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);
            }
#endif
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
