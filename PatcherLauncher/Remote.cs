using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatcherLauncher
{
    class Remote
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }
        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }
        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr HProcessess, IntPtr lpThreadAttributes, uint dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr HProcessess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 WriteProcessMemory(IntPtr HProcessess, IntPtr lpBaseAddress, string buffer, uint size, out uint lpNumberOfBytesWritten);

        private static void ShowError(string error, int errorCode)
        {
            MessageBox.Show(error + "\nError code: " + errorCode, "Patcher Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        Process RemoteProcess { get; set; }
        IntPtr HProcess { get; set; }

        public Remote(Process remoteProcess)
        {
            RemoteProcess = remoteProcess;
        }

        private bool OpenRemoteProcess()
        {
            IntPtr HProcess = OpenProcess(
                ProcessAccessFlags.CreateThread |
                ProcessAccessFlags.VMOperation |
                ProcessAccessFlags.VMRead |
                ProcessAccessFlags.VMWrite |
                ProcessAccessFlags.QueryInformation,
                false,
                RemoteProcess.Id);

            return HProcess != IntPtr.Zero;
        }

        public bool LoadLibrary(string dllPath)
        {
            if (!RemoteLoadLibrary(dllPath))
            {
                if (HProcess != IntPtr.Zero)
                    CloseHandle(HProcess);
                return false;
            }
            return true;
        }

        private bool RemoteLoadLibrary(string dllPath)
        {
            if (OpenRemoteProcess() == false)
            {
                ShowError("Unable to open process.", Marshal.GetLastWin32Error());
                return false;
            }

            IntPtr pLoadLibrary = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (pLoadLibrary == IntPtr.Zero)
            {
                ShowError("Unable to find address of \"LoadLibraryA\".", Marshal.GetLastWin32Error());
                return false;
            }

            IntPtr pMemory = VirtualAllocEx(HProcess, IntPtr.Zero, (uint)dllPath.Length + 1, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);
            if (pMemory == IntPtr.Zero)
            {
                ShowError("Unable to allocate memory to target process.", Marshal.GetLastWin32Error());
                return false;

            }

            WriteProcessMemory(HProcess, pMemory, dllPath, (uint)dllPath.Length + 1, out uint written);
            if (Marshal.GetLastWin32Error() != 0)
            {
                ShowError("Unable to write memory to process.", Marshal.GetLastWin32Error());
                return false;
            }

            IntPtr hThread = CreateRemoteThread(HProcess, IntPtr.Zero, 0, pLoadLibrary, pMemory, 0, IntPtr.Zero);
            if (hThread == IntPtr.Zero)
            {
                ShowError("Unable to load dll into memory.", Marshal.GetLastWin32Error());
                return false;
            }

            return true;
        }
    }
}