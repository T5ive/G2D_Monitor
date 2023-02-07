using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace G2D_Monitor.Game
{
    internal class MutexKiller
    {
        private const string TYPE_STRING = "Mutant";
        private const string NAME_SUBSTRING = "SingleInstanceMutex";

        private const int TOLERANCE_SIZE = 2 << 20;
        private const int HANDLE_SIZE = 40;
        private const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private const int SYNCHRONIZE = 0x00100000;
        private const int PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xfff;

        private const int PROCESS_DUP_HANDLE = 0x0040;
        private const int PROCESS_SUSPEND_RESUME = 0x0800;
        private const int DUPLICATE_CLOSE_SOURCE = 0x00000001;
        private const int DUPLICATE_SAME_ACCESS = 0x00000002;

        private struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Buffer;
        }

        private static readonly HashSet<int> ProcIds = new();
        private static string? ProcPath = null;

        public static void Run()
        {
            foreach (var process in Process.GetProcessesByName(MainForm.PROCESS_NAME))
            {
                if (ProcPath == null)
                {
                    var info = process.MainModule;
                    if (info != null) ProcPath = info.FileName;
                }
                if (ProcIds.Contains(process.Id)) continue;
                KillMutex(process.Id);
                ProcIds.Add(process.Id);
            }
            if (ProcPath == null || !File.Exists(ProcPath))
            {
                var dial = new OpenFileDialog();
                dial.Title = "Please Select '" + MainForm.PROCESS_NAME + ".exe'";
                dial.Filter = "Executable File|*.exe";
                dial.FileName = MainForm.PROCESS_NAME + ".exe";
                if (dial.ShowDialog() == DialogResult.OK)
                    Process.Start(ProcPath = dial.FileName);
            }
            else Process.Start(ProcPath);
        }

        private static bool KillMutex(int procId)
        {
            var current = GetCurrentProcess();
            byte[] data = EnumHandles();
            var count = BitConverter.ToInt64(data);
            var i = 16 - HANDLE_SIZE;
            var sourceHandle = OpenProcess(PROCESS_ALL_ACCESS | PROCESS_DUP_HANDLE | PROCESS_SUSPEND_RESUME, false, procId);
            while (count-- > 0)
            {
                i += HANDLE_SIZE;
                var id = BitConverter.ToInt64(data, i + 8);
                if (id != procId) continue;
                var handleValue = new IntPtr(BitConverter.ToInt64(data, i + 16));
                //IntPtr h = new();
                var state = NtDuplicateObject(sourceHandle, handleValue, current, out var h, 0, 0, DUPLICATE_SAME_ACCESS);
                if (state == 0)
                {
                    if (TYPE_STRING.Equals(GetObjectInfo(h, 2)))
                    {
                        var name = GetObjectInfo(h, 1);
                        if (name != null && name.Contains(NAME_SUBSTRING))
                        {
                            NtClose(h);
                            CloseHandle(sourceHandle);
                            var closeHandle = OpenProcess(PROCESS_DUP_HANDLE, false, procId);
                            NtDuplicateObject(closeHandle, handleValue, IntPtr.Zero, out _, 0, 0, DUPLICATE_CLOSE_SOURCE);
                            return true;
                        }
                    }
                    NtClose(h);
                }
            }
            CloseHandle(sourceHandle);
            return false;
        }

        private static string? GetObjectInfo(IntPtr h, ulong flag)
        {
            var allocMem = Marshal.AllocHGlobal(260);
            if (NtQueryObject(h, flag, allocMem, 260, 0) == 0)
            {
                var str = Marshal.PtrToStructure<UNICODE_STRING>(allocMem);
                if (str.Length > 0)
                {
                    var s = ConvertToMutiByte(str.Buffer);
                    Marshal.FreeHGlobal(allocMem);
                    return s;
                }
            }
            Marshal.FreeHGlobal(allocMem);
            return null;
        }

        private static string ConvertToMutiByte(string str, uint codepage = 0)
        {
            var i = WideCharToMultiByte(codepage, 0, str, -1, new(), 0, IntPtr.Zero, IntPtr.Zero);
            StringBuilder MultiByte = new(i);
            WideCharToMultiByte(codepage, 0, str, -1, MultiByte, MultiByte.Capacity, IntPtr.Zero, IntPtr.Zero);
            string s = MultiByte.ToString();
            return s;
        }

        private static byte[] EnumHandles(int size = 16)
        {
            byte[] data = new byte[size];
            if (NtQuerySystemInformation(64, data, size, out var len) == 0) return data;
            return EnumHandles(len + TOLERANCE_SIZE);
        }

        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern long NtQuerySystemInformation(ulong objectInformationClass, byte[] data, int len, [Out] out int length);

        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern long NtQueryObject(IntPtr hWnd, ulong objectInformationClass, IntPtr data, int len, int arg);
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern long NtDuplicateObject(IntPtr sourceProcessHwnd, IntPtr sourceHwnd, IntPtr currentProcess, out IntPtr handle, ulong f1, ulong f2, int f3);

        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern long NtClose(IntPtr hwnd);
        [DllImport("kernel32.dll")]
        private static extern int WideCharToMultiByte(uint CodePage,
            uint dwFlags, [In, MarshalAs(UnmanagedType.LPWStr)] string lpWideCharStr,
            int cchWideChar, [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder lpMultiByteStr,
            int cbMultiByte, IntPtr lpDefaultChar, IntPtr lpUsedDefaultChar);
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hProcess);
    }
}
