using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SlashUSB
{
    internal class Pinvoke
    {
        internal const uint IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
        internal const uint IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS = 0x560000;

        internal const uint ANYSIZE_ARRAY = 1;

        [StructLayout(LayoutKind.Sequential)]
        internal struct DISK_EXTENT
        {
            internal int DiskNumber;
            internal long StartingOffset;
            internal long ExtentLength;
        }

        internal const uint FILE_SHARE_READ = 0x00000001;
        internal const uint FILE_SHARE_WRITE = 0x00000002;
        internal const uint FILE_SHARE_DELETE = 0x00000004;

        internal const uint OPEN_EXISTING = 3;

        internal const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        internal const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;

        internal const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;

        [DllImport("user32.dll")]
        internal static extern UInt32 RegisterWindowMessage(String strMessage);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        internal const UInt32 MSGFLT_ADD = 1;
        internal const UInt32 MSGFLT_REMOVE = 2;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ChangeWindowMessageFilter(uint message, UInt32 dwFlag);

        [DllImport("setupapi.dll")]
        internal static extern int CM_Get_Parent(out IntPtr pdnDevInst, IntPtr dnDevInst, int ulFlags);

        internal const uint CM_LOCATE_DEVNODE_NORMAL = 0;

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, EntryPoint = "CM_Locate_DevNodeW")]
        internal static extern int CM_Locate_DevNode(out IntPtr pdnDevInst, string pDeviceID, uint ulFlags);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, EntryPoint = "CM_Get_DevNode_Registry_PropertyW")]
        internal static extern int
           CM_Get_DevNode_Registry_Property(
               IntPtr deviceHandle,
               uint property,
               IntPtr regDataType,
               IntPtr outBuffer,
               ref uint size,
               int flags);

        internal enum DevRegProperty : uint
        {
            DeviceDescription = 1,
            HardwareId = 2,
            CompatibleIds = 3,
            Unused0 = 4,
            Service = 5,
            Unused1 = 6,
            Unused2 = 7,
            Class = 8,
            ClassGuid = 9,
            Driver = 0x0a,
            ConfigFlags = 0x0b,
            Mfg = 0x0c,
            FriendlyName = 0x0d,
            LocationInfo = 0x0e,
            PhysicalDeviceObjectName = 0x0f,
            Capabilities = 0x10,
            UiNumber = 0x11,
            UpperFilters = 0x12,
            LowerFilters = 0x13,
            BusTypeGuid = 0x014,
            LegacyBusType = 0x15,
            BusNumber = 0x16,
            EnumeratorName = 0x17,
        }

        internal static string? GetDeviceProperties(IntPtr devHandle, DevRegProperty propertyIndex)
        {
            uint bufsize = 2048;

            IntPtr buffer =
                Marshal.AllocHGlobal((int)bufsize);

            var result = CM_Get_DevNode_Registry_Property(
                    devHandle,
                    (uint)propertyIndex,
                    IntPtr.Zero,
                    buffer,
                    ref bufsize,
                    0);

            string? propertyString = result == 0
                ? Marshal.PtrToStringUni(buffer)
                : string.Empty;

            Marshal.FreeHGlobal(buffer);

            return propertyString;
        }
    }
}
