using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Text;

namespace G2D_Monitor.Game
{
    internal sealed class Mono : IDisposable
    {
        private const int OFFSET_FUNC_STATIC_FIELD = 0x23D;
        private const int OFFSET_ARG_CLASS_NAME = 0x3C8;
        private const int OFFSET_ARG_NAME_SPACE = 0x408;
        private const int OFFSET_RET = 0x3C0;

        private readonly HandleRef hProc;
        private readonly bool injected;
        private readonly IntPtr allocAddr;

        public Mono(HandleRef hProc)
        {
            this.hProc = hProc;
            allocAddr = VirtualAllocEx(hProc, IntPtr.Zero, new IntPtr(1096), MEM_COMMIT, EXECUTE_READ_WRITE); // Memory alloc returns error 5 (Access Denied) even when launched with admin right so assistant tool not works anymore
            if (allocAddr == IntPtr.Zero) return;
            Memory.Write(hProc, allocAddr, Properties.Resources.g2d);
            if (!(injected = Call(0))) VirtualFreeEx(hProc, allocAddr, IntPtr.Zero, MEM_RELEASE);
        }

        public IntPtr GetStaticFields(string nameSapce, string className)
        {
            if (!injected) return IntPtr.Zero;
            WriteBytes(OFFSET_ARG_CLASS_NAME, className);
            WriteBytes(OFFSET_ARG_NAME_SPACE, nameSapce);
            Call(OFFSET_FUNC_STATIC_FIELD);
            return Memory.ReadIntPtr(hProc, allocAddr + OFFSET_RET);
        }

        private void WriteBytes(int offset, string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return;
            if (!str.EndsWith('\0')) str += '\0';
            var data = new byte[128];
            Encoding.ASCII.GetBytes(str, 0, str.Length, data, 0);
            Memory.Write(hProc, allocAddr + offset, data);
        }

        private bool Call(int offset)
        {
            var hThr = CreateRemoteThread(hProc, IntPtr.Zero, IntPtr.Zero, allocAddr + offset, IntPtr.Zero, 0, out _);
            return !hThr.IsInvalid && WaitForSingleObject(hThr, -1) == 0;
        }

        public void Dispose()
        {
            if (injected) VirtualFreeEx(hProc, allocAddr, IntPtr.Zero, MEM_RELEASE);
        }

        #region DllImport

        private const uint EXECUTE_READ_WRITE = 0x40;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint MEM_RELEASE = 0x8000;

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr VirtualAllocEx(HandleRef hProcess, IntPtr lpAddress, [MarshalAs(UnmanagedType.SysInt)] IntPtr nSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = false, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualFreeEx(HandleRef hProcess, IntPtr lpAddress, [MarshalAs(UnmanagedType.SysInt)] IntPtr nSize, uint dwFreeType);
        [DllImport("kernel32.dll")]
        private static extern SafeWaitHandle CreateRemoteThread(HandleRef hProcess,
            IntPtr lpThreadAttributes, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out int lpThreadId);
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public static extern uint WaitForSingleObject([In] SafeWaitHandle hHandle, [In] int dwMilliseconds);

        #endregion
    }
}
