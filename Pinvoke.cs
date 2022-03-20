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
        public const uint IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS = 0x560000;

        public const uint ANYSIZE_ARRAY = 1;

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
        public static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, IntPtr lpInBuffer,
            uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        public string[]? QueryDosDevice(string path)
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

        public static string GetVolumeName(string MountPoint)
        {
            const int MaxVolumeNameLength = 100;
            StringBuilder sb = new StringBuilder(MaxVolumeNameLength);
            if (!GetVolumeNameForVolumeMountPoint(MountPoint, sb, (uint)MaxVolumeNameLength))
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            return sb.ToString();
        }

        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        public const uint FILE_SHARE_DELETE = 0x00000004;

        public const uint OPEN_EXISTING = 3;

        public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        public const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;

        public const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW")]
        public static extern SafeFileHandle CreateFile(string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode, IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    }
}
