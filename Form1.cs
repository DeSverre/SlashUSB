using System.Runtime.InteropServices;

namespace USkummelB
{
    public partial class Form1 : Form
    {
        private UInt32 queryCancelAutoPlay = 0;

        class Win32Call
        {
            [DllImport("user32.dll")]
            public static extern uint RegisterWindowMessage(String strMessage);

            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        }

        USBDetect usbdetector;

        public Form1()
        {
            InitializeComponent();
            usbdetector = new USBDetect();
            usbdetector.USBInserted += C_USBInserted;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (queryCancelAutoPlay == 0)
                queryCancelAutoPlay = Win32Call.RegisterWindowMessage("QueryCancelAutoPlay");

            //if the window message id equals the QueryCancelAutoPlay message id
            if ((UInt32)m.Msg == queryCancelAutoPlay)
            {
                /* only needed if your application is using a dialog box and needs to
                * respond to a "QueryCancelAutoPlay" message, it cannot simply return TRUE or FALSE.
                SetWindowLong(this.Handle, 0, 1);
                */
                Win32Call.SetWindowLong(this.Handle, 0, 1);
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
            var usbInfo = e as USB_EventInfo;
            if (usbInfo != null)
                this.Invoke((MethodInvoker)delegate
                 {
                     new USBdevice(usbListView, usbInfo);
                 });
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