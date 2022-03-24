using System.Runtime.InteropServices;

namespace USkummelB
{
    public partial class Form1 : Form
    {
        private UInt32 queryCancelAutoPlay = 0;

        USBDetect usbdetector;
        List<string> hubList = new();
        List<string> aktivertHubList = new();

        public Form1()
        {
            InitializeComponent();
            usbdetector = new USBDetect();
            usbdetector.USBinserted += C_USBInserted;
            usbdetector.USBremoved += C_USBRemoved;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (queryCancelAutoPlay == 0)
                queryCancelAutoPlay = Pinvoke.RegisterWindowMessage("QueryCancelAutoPlay");

            //if the window message id equals the QueryCancelAutoPlay message id
            if ((UInt32)m.Msg == queryCancelAutoPlay)
            {
                /* only needed if your application is using a dialog box and needs to
                * respond to a "QueryCancelAutoPlay" message, it cannot simply return TRUE or FALSE.
                SetWindowLong(this.Handle, 0, 1);
                */
                Pinvoke.SetWindowLong(this.Handle, 0, 1);
                m.Result = (IntPtr)1;
            }
        }

        private void AktivertCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox? aktivert = sender as CheckBox;
            if (aktivert != null)
            {
                aktivert.BackColor = aktivert.Checked ? Color.Red : Color.Empty;
                aktivert.Text = aktivert.Checked ? "Aktivert" : "Aktiver";
            }
        }

        private void C_USBInserted(object sender, EventArgs e)
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
            bool aktivert = (aktivertHubList.FindIndex(x => x == usbInfo.Hub) != -1);

            var usb = new USBdevice(usbListView, usbInfo, aktivert ? "listViewGroupAktivert" : "listViewGroupFunnet");
            if (aktivert)
                KjørJobb(usb);
        }

        private void DetermineHubIndex(USB_EventInfo usbInfo)
        {
            var index = hubList.FindIndex(x => x == usbInfo.Hub);
            if (index == -1)
            {
                hubList.Add(usbInfo.Hub);
                index = hubList.Count - 1;
            }
            usbInfo.Hub = index.ToString();
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
                    USBdevice? device = item.Tag as USBdevice;
                    if (device != null)
                        device.Remove();
                }
            }
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem s in usbListView.SelectedItems)
            {
                USBdevice usb = (USBdevice)s.Tag;
                s.Selected = false;
                KjørJobb(usb, true);
            }
        }

        bool CleanOn { get { return cleanChecked.Checked; } }
        bool CleanEnabled { get { return CleanOn && activatedCB.Checked; } }
        bool FormatOn { get { return formatChecked.Checked; } }
        bool FormatEnabled { get { return FormatOn && activatedCB.Checked; } }

        private void KjørJobb(USBdevice usb, bool force = false)
        {
            string fs = "FAT32";
            if (ntfsSelect.Checked) fs = "NTFS";
            if (ExFATselect.Checked) fs = "ExFAT";
            if (force)
                new Thread(() => { usb.KjørJobb(CleanOn, FormatOn, merkelappCheckBox.Checked, fs); }).Start();
            else
                new Thread(() => { usb.KjørJobb(CleanEnabled, FormatEnabled, merkelappCheckBox.Checked, fs); }).Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem s in usbListView.SelectedItems)
            {
                USBdevice usb = (USBdevice)s.Tag;
                var hub = usb.Hub;
                AddHub2Aktivert(hub);
            }
            OppdaterOgKjørAktiverte();
        }

        private void listViewSelect_Changed(object sender, EventArgs e)
        {
            bool enabled = usbListView.SelectedItems.Count > 0;
            activateHUBbt.Enabled = enabled;
            testButton.Enabled = enabled;
        }

        private void OppdaterOgKjørAktiverte()
        {
            foreach (ListViewItem s in usbListView.Items)
            {
                USBdevice usb = (USBdevice)s.Tag;
                if (aktivertHubList.FindIndex(x => usb.Hub == x) != -1)
                    usb.ByttGruppe("listViewGroupAktivert");
            }
        }

        private void AddHub2Aktivert(string hub)
        {
            if (aktivertHubList.FindIndex(x => x == hub) == -1)
                aktivertHubList.Add(hub);
        }
    }

    internal class Utils
    {
        static readonly string[] SizeSuffixes =
                          { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        static public string SizeSuffix(UInt64 value, int decimalPlaces = 1)
        {
            int i = 0;
            int divisor = 1000;
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
