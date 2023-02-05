using System.Runtime.InteropServices;
using System.Text;

namespace G2D_Monitor.Game
{
    internal class Memory
    {
        public static IntPtr ReadIntPtr(HandleRef hProc, IntPtr address) => new(BitConverter.ToInt64(Read(hProc, address, sizeof(long))));

        public static string ReadString(HandleRef hProc, IntPtr address)
        {
            var len = BitConverter.ToInt32(Read(hProc, address + 0x10, sizeof(int)));
            if (len <= 0 || len >= 128) return string.Empty;
            var data = Read(hProc, address + 0x14, len * 2);
            return Encoding.UTF8.GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, data));
        }

        public static T Read<T>(HandleRef hProc, IntPtr address) where T : unmanaged
        {
            var length = Marshal.SizeOf<T>();
            var buffer = Marshal.AllocHGlobal(length);
            ReadProcessMemory(hProc, address, buffer, new IntPtr(length), out _);
            var result = Marshal.PtrToStructure<T>(buffer);
            Marshal.FreeHGlobal(buffer);
            return result;
        }

        public static byte[] Read(HandleRef hProc, IntPtr address, int length)
        {
            var data = new byte[length];
            if (address == IntPtr.Zero) return data;
            ReadProcessMemory(hProc, address, data, new IntPtr(data.Length), out _);
            return data;
        }

        public static void Write(HandleRef hProc, IntPtr address, byte[] data) => WriteProcessMemory(hProc, address, data, new IntPtr(data.Length), out _);

        [DllImport("Kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(HandleRef hProcess, IntPtr lpBaseAddress, [In, Out] byte[] lpBuffer, [MarshalAs(UnmanagedType.SysInt)] IntPtr nSize, [Out, Optional, MarshalAs(UnmanagedType.SysInt)] out IntPtr lpNumberOfBytesRead);

        [DllImport("Kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(HandleRef hProcess, IntPtr lpBaseAddress, [In, Out] IntPtr lpBuffer, [MarshalAs(UnmanagedType.SysInt)] IntPtr nSize, [Out, Optional, MarshalAs(UnmanagedType.SysInt)] out IntPtr lpNumberOfBytesRead);

        [DllImport("Kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteProcessMemory(HandleRef hProcess, IntPtr lpBaseAddress, [In, Out] byte[] lpBuffer, [MarshalAs(UnmanagedType.SysInt)] IntPtr nSize, [Out, Optional, MarshalAs(UnmanagedType.SysInt)] out IntPtr lpNumberOfBytesWritten);

    }
}
