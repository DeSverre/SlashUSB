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
    internal class USBmemoryDevice
    {
        private static Mutex mutex = new();
        readonly UInt64 size;
        public readonly string? HubFriendlyName;
        readonly string? HubID;
        readonly string? location;
        readonly string deviceName;
        readonly string pnp_serial;
        readonly string volumeName;
        Job? job = null;
        Status status = Status.Ready;

        readonly USBdeviceView? mDeviceView = null;

        private enum Status
        {
            Ready,
            Cleaning,
            Format,
            Eject,
            Error
        };

        private readonly string[] statusTekster = { "Klar", "Renser", "Formaterer", "OK - løser ut", "Feil!" };

        private char? driveLetterOrNull;
        char? DriveLetter
        {
            get { return driveLetterOrNull; }
            set
            {
                if (value != null)
                {
                    driveLetterOrNull = value;
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
            if (driveLetterOrNull != null) _drive = driveLetterOrNull.ToString() + ":";
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
                case Status.Ready:
                    backColor = Color.Blue;
                    foreColor = Color.White;
                    break;
                case Status.Cleaning:
                case Status.Format:
                    backColor = Color.Yellow;
                    foreColor = Color.Black;
                    break;
                case Status.Eject:
                    backColor = Color.Green;
                    foreColor = Color.White;
                    break;
                case Status.Error:
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

        public USBmemoryDevice(ListView view, USB_EventInfo info, string gruppeNavn)
        {
            driveLetterOrNull = info.DriveLetter;
            diskName = info.DiskName;
            size = info.Size;
            HubID = info.HubID;
            HubFriendlyName = info.HubFriendlyName;
            location = info.Location;
            deviceName = info.DeviceName;
            pnp_serial = info.PNPserial;
            volumeName = info.VolumePath;


            var existing = ExistingDevice(view, deviceName);
            if (existing != null)
            {
                status = Status.Error;
            }
            else
            {
                mDeviceView = new(this, view);
                Add2View(GetListViewItem(), gruppeNavn);
            }
        }

        private ListViewItem GetListViewItem()
        {
            var result = new ListViewItem(new[] { "", DriveLetter != null && DriveLetter != null ? DriveLetter + ":" : "", DiskName, deviceName, Utils.SizeSuffix(size), location, HubFriendlyName })
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
                    mDeviceView.Add2View(listViewItem, gruppeNavn, HubID ?? "");
                });
            UpdateStatus(Status.Ready);
        }

        public void Remove()
        {
            if (mDeviceView != null)
                mDeviceView.Remove();
        }

        private bool CleanDisk()
        {
            UInt32 returnValue = 1;
            string errorMessage = "Rens feilet";
            using var diskC = WQL.QueryMi("root\\Microsoft\\Windows\\Storage", @"SELECT * FROM MSFT_Disk WHERE SerialNumber = '" + pnp_serial + "'");
            if (diskC != null)
                foreach (ManagementObject msft_disk in diskC)
                {
                    SanityCheckDisk(msft_disk);
                    using ManagementBaseObject inParams = msft_disk.GetMethodParameters("Clear");
                    inParams["RemoveData"] = true;
                    inParams["RemoveOEM"] = true;
                    inParams["ZeroOutEntireDisk"] = false;

                    try
                    {
                        using ManagementBaseObject outParams = msft_disk.InvokeMethod("Clear", inParams, null);
                        returnValue = (UInt32)outParams["ReturnValue"];
                        if (returnValue == 0)
                            returnValue = CreatePartition(msft_disk);
                    }
                    catch (Exception ex)
                    {
                        errorMessage = "Rens feil: " + ex.Message;
                    }
                    break;
                }

            if (returnValue != 0) UpdateStatus(Status.Error, errorMessage);

            return returnValue == 0;
        }

        static private void SanityCheckDisk(ManagementObject msft_disk)
        {
            var busType = (UInt16)msft_disk["BusType"];
            if (busType != 7) // removable
                throw new Exception("Unexpected bus type");
        }

        private uint CreatePartition(ManagementObject msft_disk)
        {
            uint returnValue;
            {
                using var inParamsCP = msft_disk.GetMethodParameters("CreatePartition");
                inParamsCP["AssignDriveLetter"] = false;
                inParamsCP["UseMaximumSize"] = true;

                using var outParamsCP = msft_disk.InvokeMethod("CreatePartition", inParamsCP, null);
                returnValue = (UInt32)outParamsCP["ReturnValue"];
                if (returnValue == 0)
                {
                    var partition = outParamsCP["CreatedPartition"] as ManagementBaseObject;
                    char? nydrive = (char?)partition?[nameof(DriveLetter)];
                    driveLetterOrNull = nydrive == '\0' ? null : nydrive;
                    diskName = "";
                    UpdateDisk();
                }
            }

            return returnValue;
        }

        internal void ChangeGroup(string gruppe)
        {
            if (mDeviceView != null)
                mDeviceView.View.Invoke((MethodInvoker)delegate
                {
                    mDeviceView.ChangeGroup(gruppe);
                });
        }

        private bool Format(bool sizeLabel, string fs = "FAT32", bool round2MultipleOf2 = false)
        {
            bool result = false;
            using var volumeC = WQL.QueryMi("root\\Microsoft\\Windows\\Storage", @"SELECT * FROM MSFT_Volume WHERE Path = '" + volumeName.Replace(@"\", @"\\") + "'");
            if (volumeC != null)
                foreach (ManagementObject volume in volumeC)
                {
                    SanityCheckVolume(volume);

                    using ManagementBaseObject inParams = volume.GetMethodParameters("Format");
                    string newLabel = sizeLabel ? Utils.SizeSuffix(size, 0, round2MultipleOf2) : (DiskName ?? "PiratSoft");
                    inParams["FileSystem"] = fs;
                    inParams["FileSystemLabel"] = newLabel;
                    inParams["Full"] = false;

                    // No need to do this async, but it works
                    ManagementOperationObserver results = new();
                    results.Completed += new CompletedEventHandler(this.FormatCompleted);
                    results.ObjectReady += new ObjectReadyEventHandler(this.FormatObjectReady);
                    InvokeMethodOptions formatOptions = new()   // Not really doing anything atm...
                    {
                        Timeout = TimeSpan.Zero// new TimeSpan(0, 0, 5);
                    };

                    formatResult = new();
                    formatCompletedStatus = new();
                    volume.InvokeMethod(results, "Format", inParams, formatOptions);

                    var resultValue = formatResult.Task.Result;  // Wait for event
                    if (resultValue == 0)
                    {
                        DiskName = newLabel;
                        result = true;
                    }
                    else
                        UpdateStatus(Status.Error, "Format feilet");

                    var status = formatCompletedStatus.Task.Result;

                    if (driveLetterOrNull == null)
                        AssignDriveLetter(volume);
                    break;
                }

            return result;
        }

        static private void SanityCheckVolume(ManagementObject msft_volume)
        {
            var DriveType = (UInt32)msft_volume["DriveType"];
            if (DriveType != 2) // removable
                throw new Exception("Unexpected drive type");
        }

        private void AssignDriveLetter(ManagementObject volume)
        {
            using var partC = volume.GetRelated("MSFT_Partition");
            foreach (ManagementObject part in partC)
            {
                using var inParamsPart = part.GetMethodParameters("AddAccessPath");
                inParamsPart["AssignDriveLetter"] = true;
                using var outParams = part.InvokeMethod("AddAccessPath", inParamsPart, null);
                var returnValue = (UInt32)outParams["ReturnValue"];
                part.Get(); // Update data
                char? nydrive = (char?)part[nameof(DriveLetter)];
                driveLetterOrNull = nydrive == '\0' ? null : nydrive;
                UpdateDisk();
            }
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

        public void RunJob(bool clean, bool format, bool sizeLabel, string fs, bool round2multipleOf2)
        {
            if (mutex.WaitOne(0) == false)
                return; // If job already running

            job = new Job(clean, format, sizeLabel, fs, round2multipleOf2);
            while (status != Status.Eject && status != Status.Error)
            {
                NextState();
                RunState();
            }

            mutex.ReleaseMutex();
        }

        private void RunState()
        {
            switch (status)
            {
                case Status.Ready: break;
                case Status.Cleaning:
                    if (job != null && job.Clean) CleanDisk();
                    break;
                case Status.Format:
                    if (job != null && job.Format) Format(job.SizeLabel, job.FileSystem, job.Round2MultipleOf2);
                    break;
                case Status.Eject:
                    Eject();
                    job = null;
                    break;
                case Status.Error:
                    break;
            }
        }

        private void NextState()
        {
            switch (status)
            {
                case Status.Ready:
                    UpdateStatus(Status.Cleaning);
                    break;
                case Status.Cleaning:
                    UpdateStatus(Status.Format);
                    break;
                case Status.Format:
                    UpdateStatus(Status.Eject);
                    break;
                case Status.Eject:
                case Status.Error:
                    break;
            }
        }

        static internal USBmemoryDevice? ExistingDevice(ListView view, string deviceName)
        {
            ListViewItem[] found = view.Items.Find(deviceName, false);

            return found.Length > 0 ? (USBmemoryDevice)found[0].Tag : null;
        }

        private void Eject()
        {
            static void EjectDriveShell(object? param)
            {
                if (param == null) return;

                var ssfDRIVES = 0x11;
                var driveName = param.ToString();

                var shell = new Shell();
                shell.NameSpace(ssfDRIVES).ParseName(driveName).InvokeVerb("Eject");
            }

            Thread.Sleep(2500); // Try to avoid device busy message

            if (driveLetterOrNull != null)
            {
                string drive = driveLetterOrNull + ":\\";
                var staThread = new Thread(new ParameterizedThreadStart(EjectDriveShell));
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start(drive);
                staThread.Join();
            }
            else
            {
                using var volumeC = WQL.QueryMi(@"SELECT * FROM Win32_Volume WHERE DeviceID = '" + volumeName.Replace(@"\", @"\\") + "'");
                if (volumeC != null)
                    foreach (ManagementObject volume in volumeC)
                    {
                        using var inParams = volume.GetMethodParameters("Dismount");
                        inParams["Force"] = true;
                        inParams["Permanent"] = true;

                        using var outParams = volume.InvokeMethod("Dismount", inParams, null);
                        UInt32 returnValue = (UInt32)outParams["ReturnValue"];
                        // just fail silently
                        break;
                    }
            }
        }

        private class Job
        {
            public readonly bool Clean;
            public readonly bool Format;
            public readonly bool SizeLabel;
            public readonly string FileSystem;
            public readonly bool Round2MultipleOf2;

            public Job(bool clean, bool format, bool sizeLabel, string fs, bool round2multipleOf2)
            {
                Clean = clean;
                Format = format;
                SizeLabel = sizeLabel;
                FileSystem = fs;
                Round2MultipleOf2 = round2multipleOf2;
            }
        }
    }
}
