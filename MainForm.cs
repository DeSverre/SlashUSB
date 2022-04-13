using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace USkummelB
{
    public partial class MainForm : Form
    {
        private const string ActivatedGroupName = "listViewGroupAktivert";
        private const string FoundGroupName = "listViewGroupFunnet";

        readonly USBDetect usbdetector = new();
        readonly List<string> hubList = new();
        readonly List<string> activatedHubList = new();

        bool mDeactivate = false;

        public MainForm()
        {
            InitializeComponent();
            usbdetector.USBinserted += C_USBInserted;
            usbdetector.USBremoved += C_USBRemoved;

            versionLabel.Text = "Versjon: " + typeof(MainForm).Assembly.GetName().Version?.ToString() ?? "--";
        }

        private UInt32 queryCancelAutoPlay = 0;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (queryCancelAutoPlay == 0)
            {
                queryCancelAutoPlay = Pinvoke.RegisterWindowMessage("QueryCancelAutoPlay");

                // Open up filter for this message when in administrator mode (and we are)
                Pinvoke.ChangeWindowMessageFilter(queryCancelAutoPlay,Pinvoke.MSGFLT_ADD);  
            }

            //if the window message id equals the QueryCancelAutoPlay message id
            if (m.Msg == queryCancelAutoPlay)
            {
                /* only needed if your application is using a dialog box and needs to
                * respond to a "QueryCancelAutoPlay" message, it cannot simply return TRUE or FALSE.
                SetWindowLong(this.Handle, 0, 1);
                */
                Pinvoke.SetWindowLongPtr(this.Handle, 0, new IntPtr(1));
                m.Result = (IntPtr)1;
            }
        }

        private void AktivertCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox? activated = sender as CheckBox;
            if (activated != null)
            {
                activated.Text = activated.Checked ? "Deaktiver" : "Aktiver";
                UpdateEmergencyLight();
            }
        }

        private void UpdateEmergencyLight()
        {
            var actGrp = usbListView.Groups[ActivatedGroupName];
            emergencyLight.Visible = activatedCB.Checked && actGrp.Items.Count>0;
        }

        private void C_USBInserted(object? sender, EventArgs e)
        {
            USB_EventInfo? usbInfo = e as USB_EventInfo;
            if (usbInfo != null)
                this.Invoke((MethodInvoker)delegate
                {
                    InsertDevice(usbInfo);
                });
        }

        private void C_USBRemoved(object? sender, USB_RemovedEvent e)
        {
            USB_RemovedEvent? usbInfo = e as USB_RemovedEvent;
            if (usbInfo != null)
                this.Invoke((MethodInvoker)delegate
                {
                    RemoveDevice(usbInfo.DeviceName);
                });
        }

        private void InsertDevice(USB_EventInfo usbInfo)
        {
            DetermineHubIndex(usbInfo);
            bool aktivert = (activatedHubList.FindIndex(x => x == usbInfo.HubFriendlyName) != -1);

            var usb = new USBmemoryDevice(usbListView, usbInfo, aktivert ? ActivatedGroupName : FoundGroupName);
            if (activatedCB.Checked && aktivert && usb.InstanceAdded)
                RunJob(usb);
        }

        private void DetermineHubIndex(USB_EventInfo usbInfo)
        {
            if(usbInfo.HubID == null ) return;
            var index = hubList.FindIndex(x => x == usbInfo.HubID);
            if (index == -1)
            {
                hubList.Add(usbInfo.HubID);
                index = hubList.Count - 1;
            }
            usbInfo.HubFriendlyName = index.ToString();
        }

        private void RemoveDevice(string? deviceName)
        {
            if (deviceName == null)
                return;

            var result = usbListView.Items.Find(deviceName, false);
            if (result.Length > 0)
            {
                foreach (var item in result)
                {
                    USBmemoryDevice? device = item.Tag as USBmemoryDevice;
                    if (device != null)
                        device.Remove();
                }
            }
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem s in usbListView.SelectedItems)
            {
                USBmemoryDevice usb = (USBmemoryDevice)s.Tag;
                s.Selected = false;
                RunJob(usb, true);
            }
        }

        bool CleanOn { get { return cleanChecked.Checked; } }
        bool CleanEnabled { get { return CleanOn && activatedCB.Checked; } }
        bool FormatOn { get { return formatChecked.Checked; } }
        bool FormatEnabled { get { return FormatOn && activatedCB.Checked; } }

        private void RunJob(USBmemoryDevice usb, bool force = false)
        {
            string fs = "FAT32";
            if (ntfsSelect.Checked) fs = "NTFS";
            if (ExFATselect.Checked) fs = "ExFAT";
            if (force)
                new Thread(() => { usb.RunJob(CleanOn, FormatOn, merkelappCheckBox.Checked, fs, roundUpSizeCB.Checked); }).Start();
            else if(activatedCB.Checked)
                new Thread(() => { usb.RunJob(CleanEnabled, FormatEnabled, merkelappCheckBox.Checked, fs, roundUpSizeCB.Checked); }).Start();
        }

        private void ActHubButClick(object sender, EventArgs e)
        {
            if (mDeactivate)
                foreach (ListViewItem s in usbListView.SelectedItems)
                {
                    s.Selected = false;
                    USBmemoryDevice usb = (USBmemoryDevice)s.Tag;
                    var hub = usb.HubFriendlyName;
                    RemoveHubFromActivated(hub);
                }
            else
            {
                foreach (ListViewItem s in usbListView.SelectedItems)
                {
                    s.Selected = false;
                    USBmemoryDevice usb = (USBmemoryDevice)s.Tag;
                    var hub = usb.HubFriendlyName;
                    AddHub2Activated(hub);

                    string message = String.Format("Alle minnepinner som settes inn i hub {0}, vil nå bli initialisert på spesifisert måte", hub);
                    MessageBox.Show(this, message, "HUB aktivert");
                }
            }
            UpdateAndRunActivated();
            UpdateEmergencyLight();
        }

        private void RemoveHubFromActivated(string? hub)
        {
            if (hub == null) return;

            var index = activatedHubList.FindIndex(x => x == hub);
            if (index != -1)
                activatedHubList.RemoveAt(index);
        }

        private void ListViewSelect_Changed(object sender, EventArgs e)
        {
            bool enabled = usbListView.SelectedItems.Count > 0;
            activateHUBbt.Enabled = enabled;
            testButton.Enabled = enabled;

            bool deactivated = false;
            var funGrp = usbListView.Groups[FoundGroupName];
            foreach (ListViewItem s in usbListView.SelectedItems)
            {
                var group = s.Group;
                if (group == funGrp) deactivated = true;
            }
            if (!deactivated)
            {
                activateHUBbt.Text = "Deaktiver hub";
                mDeactivate = true;
            }
            else
            {
                activateHUBbt.Text = "Aktiver hub";
                mDeactivate = false;
            }
        }

        private void UpdateAndRunActivated()
        {
            foreach (ListViewItem s in usbListView.Items)
            {
                USBmemoryDevice usb = (USBmemoryDevice)s.Tag;
                if (activatedHubList.FindIndex(x => usb.HubFriendlyName == x) != -1)
                {
                    usb.ChangeGroup(ActivatedGroupName);
                    RunJob(usb);
                }
                else
                    usb.ChangeGroup(FoundGroupName);
            }
        }

        private void AddHub2Activated(string? hub)
        {
            if (hub == null) return;
            if (activatedHubList.FindIndex(x => x == hub) == -1)
                activatedHubList.Add(hub);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }

    internal class Utils
    {
        static readonly string[] SizeSuffixes =
                          { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        static public string SizeSuffix(UInt64 value, int decimalPlaces = 1, bool roundUpToLog2 = false)
        {
            int divisor = 1000;
            if (roundUpToLog2)
            {
                var log2 = BitOperations.Log2(value);
                var temp = (UInt64)1 << (log2 - 1);
                log2 = BitOperations.Log2(temp+value);
                value = (UInt64)1 << log2;
                divisor = 1024;
                decimalPlaces = 0;
            }
            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue, decimalPlaces) >= divisor)
            {
                dValue /= divisor;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
        }
    }
}
