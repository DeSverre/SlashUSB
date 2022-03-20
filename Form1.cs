using System.Runtime.InteropServices;

namespace USkummelB
{
    public partial class Form1 : Form
    {
        private UInt32 queryCancelAutoPlay = 0;

        USBDetect usbdetector;

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
                     new USBdevice(usbListView, usbInfo);
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

        private void RemoveDevice(string deviceName)
        {
            var result = usbListView.Items.Find(deviceName, false);
            if(result.Length > 0)
            {
                foreach (var item in result)
                {
                    var device = item.Tag as USBdevice;
                    device.Remove();
                }
            }
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            foreach(ListViewItem s in usbListView.SelectedItems)
            {
                USBdevice usb = (USBdevice)s.Tag;
                usb.CleanDisk();
            }
        }
    }

    class Utils
    {
        static readonly string[] SizeSuffixes =
                          { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        static public string SizeSuffix(UInt64 value, int decimalPlaces = 1)
        {
            int i = 0;
            int divisor = 1024;
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