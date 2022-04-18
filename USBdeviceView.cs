using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlashUSB
{
    internal class USBdeviceView
    {
        private const string activatedGroupName = "listViewGroupAktivert";
        readonly USBmemoryDevice mDevice;
        readonly ListView myView;
        ListViewItem? mItem = null;

        public ListView.ListViewItemCollection Items { get { return myView.Items; } }
        public ListView View { get { return myView; } }

        public USBdeviceView(USBmemoryDevice device, ListView view)
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

        internal void UpdateColorActionItem(Color backColor, Color foreColor, string newText)
        {
            if (mItem != null)
            {
                mItem.UseItemStyleForSubItems = false;
                var subIt = mItem.SubItems[0];
                subIt.Text = newText;

                subIt.BackColor = backColor;
                subIt.ForeColor = foreColor;
            }
        }

        internal void Add2View(ListViewItem listViewItem, string groupNavn, string toolTipText="")
        {
            mItem = listViewItem;
            mItem.Tag = mDevice;
            ListViewGroup group = myView.Groups[groupNavn];
            mItem.Group = group;
            myView.Items.Add(mItem);
            SetHubColor(mItem.SubItems[^1], group.Name == activatedGroupName);

            mItem.ToolTipText = toolTipText;    // Doesn't seem to work
        }

        private static void SetHubColor(ListViewItem.ListViewSubItem hubItem, bool activated)
        {
            hubItem.BackColor = activated ? Color.Red : Color.White;
        }

        internal void Remove()
        {
            if (mItem != null)
            {
                mItem.Remove();
                mItem = null;
            }
        }

        internal void ChangeGroup(string group)
        {
            if (mItem != null)
            {
                mItem.Group = myView.Groups[group];

                SetHubColor(mItem.SubItems[^1], group == activatedGroupName);
            }
        }
    }
}
