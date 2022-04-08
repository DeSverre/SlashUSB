using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace USkummelB
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

        [StructLayout(LayoutKind.Sequential)]
        internal struct _VOLUME_DISK_EXTENTS
        {
            internal long NumberOfDiskExtents;
            internal DISK_EXTENT Extents;
        }

            [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, IntPtr lpInBuffer,
            uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        internal string[]? QueryDosDevice(string path)
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern int QueryDosDevice([In] string lpDeviceName, IntPtr lpTargetPath, int ucchMax);

            const int ERROR_INSUFFICIENT_BUFFER = 122;

            // Allocate some memory to get a list of all system devices.
            // Start with a small size and dynamically give more space until we have enough room.
            int returnSize = 0;
            int maxSize = 100;
            string allDevices;
            IntPtr mem;
            string[]? retval = null;

            while (returnSize == 0)
            {
                mem = Marshal.AllocHGlobal(maxSize);
                if (mem != IntPtr.Zero)
                {
                    // mem points to memory that needs freeing
                    try
                    {
                        returnSize = QueryDosDevice(path, mem, maxSize);
                        if (returnSize != 0)
                        {
                            allDevices = Marshal.PtrToStringAnsi(mem, returnSize);
                            retval = allDevices.Split('\0');
                            break;    // not really needed, but makes it more clear...
                        }
                        else if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
                        //maybe better
                        //else if( Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
                        //ERROR_INSUFFICIENT_BUFFER = 122;
                        {
                            maxSize *= 10;
                        }
                        else
                        {
                            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                            break;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(mem);
                    }
                }
                else
                {
                    throw new OutOfMemoryException();
                }
            }
            return retval;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "GetVolumeNameForVolumeMountPointW")]
        static extern bool GetVolumeNameForVolumeMountPoint(string lpszVolumeMountPoint, [Out] StringBuilder lpszVolumeName, uint cchBufferLength);

        internal static string GetVolumeName(string MountPoint)
        {
            const int MaxVolumeNameLength = 100;
            StringBuilder sb = new StringBuilder(MaxVolumeNameLength);
            if (!GetVolumeNameForVolumeMountPoint(MountPoint, sb, (uint)MaxVolumeNameLength))
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            return sb.ToString();
        }

        internal const uint FILE_SHARE_READ = 0x00000001;
        internal const uint FILE_SHARE_WRITE = 0x00000002;
        internal const uint FILE_SHARE_DELETE = 0x00000004;

        internal const uint OPEN_EXISTING = 3;

        internal const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        internal const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;

        internal const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW")]
        internal static extern SafeFileHandle CreateFile(string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode, IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("user32.dll")]
        internal static extern UInt32 RegisterWindowMessage(String strMessage);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        internal const UInt32 MSGFLT_ADD = 1;
        internal const UInt32 MSGFLT_REMOVE = 2;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ChangeWindowMessageFilter(uint message, UInt32 dwFlag);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool DeleteVolumeMountPoint(string lpszVolumeMountPoint);

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
