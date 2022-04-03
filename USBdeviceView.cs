using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USkummelB
{
    internal class USBdeviceView
    {
        readonly USBdevice mDevice;
        readonly ListView myView;
        ListViewItem? mItem = null;

        public ListView.ListViewItemCollection Items { get { return myView.Items; } }
        public ListView View { get { return myView; } }

        public USBdeviceView(USBdevice device, ListView view)
        {
            mDevice = device;
            myView = view;
        }

        internal void UpdateDisk(string drive, string? diskName)
        {
            if (mItem != null)
            {
                mItem.SubItems[1].Text = drive;
                mItem.SubItems[2].Text = diskName;
            }
        }

        internal void UpdateColorActionItem(Color backColor, Color foreColor, string nyText)
        {
            if (mItem != null)
            {
                mItem.UseItemStyleForSubItems = false;
                var subIt = mItem.SubItems[0];
                subIt.Text = nyText;

                subIt.BackColor = backColor;
                subIt.ForeColor = foreColor;
            }
        }

        internal void Add2View(ListViewItem listViewItem, string gruppeNavn)
        {
            mItem = listViewItem;
            mItem.Tag = mDevice;
            ListViewGroup gruppe = myView.Groups[gruppeNavn];
            mItem.Group = gruppe;
            myView.Items.Add(mItem);
            if (gruppe.Name == "listViewGroupAktivert")
                mItem.SubItems[mItem.SubItems.Count - 1].ForeColor = Color.Red;
            else
                mItem.SubItems[mItem.SubItems.Count - 1].ForeColor = Color.Black;
        }

        internal void Remove()
        {
            if (mItem != null)
            {
                mItem.Remove();
                mItem = null;
            }
        }

        internal void ChangeGroup(string gruppe)
        {
            if (mItem != null)
            {
                mItem.Group = myView.Groups[gruppe];

                if (gruppe == "listViewGroupAktivert")
                    mItem.SubItems[mItem.SubItems.Count - 1].ForeColor = Color.Red;
                else
                    mItem.SubItems[mItem.SubItems.Count - 1].ForeColor = Color.Black;
            }
        }
    }
}
