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

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 WriteProcessMemory(IntPtr HProcessess, IntPtr lpBaseAddress, byte[] buffer, uint size, out uint lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        private static void ShowError(string error, int errorCode)
        {
            MessageBox.Show(error + "\nError code: " + errorCode, "Patcher Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        Process RemoteProcess { get; set; }
        IntPtr HProcess { get; set; }

        public Remote(Process remoteProcess)
        {
            RemoteProcess = remoteProcess;
            HProcess = IntPtr.Zero;
            HInvokeMethod = IntPtr.Zero;
        }

        private bool OpenRemoteProcess()
        {
            if(HProcess == IntPtr.Zero)
            {
                HProcess = OpenProcess(
                    ProcessAccessFlags.CreateThread |
                    ProcessAccessFlags.VMOperation |
                    ProcessAccessFlags.VMRead |
                    ProcessAccessFlags.VMWrite |
                    ProcessAccessFlags.QueryInformation,
                    false,
                    RemoteProcess.Id);
            }

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

            if (RemoteAlloc((uint)dllPath.Length + 1, out IntPtr pMemory))
            {
                if (RemoteWrite(pMemory, dllPath))
                {
                    return Invoke("kernel32.dll", "LoadLibraryA", pMemory);
                }
            }

            return false;
        }

        public bool Invoke(string moduleName, string methodName, IntPtr parameter)
        {
            Console.WriteLine("remote invoke {0}::{1}({2:X}) synchronously", moduleName, methodName, (uint)parameter);
            return RemoteInvoke(moduleName, methodName, parameter, true);
        }
        public bool InvokeAsync(string moduleName, string methodName, IntPtr parameter)
        {
            Console.WriteLine("remote invoke {0}::{1}({2:X}) asynchronously", moduleName, methodName, (uint)parameter);
            return RemoteInvoke(moduleName, methodName, parameter, false);
        }

        private bool RemoteInvoke(string moduleName, string methodName, IntPtr parameter, bool synchronize)
        {
            if (OpenRemoteProcess() == false)
            {
                ShowError("Unable to open process.", Marshal.GetLastWin32Error());
                return false;
            }

            //IntPtr pMethod = GetProcAddress(GetModuleHandle(moduleName), methodName);
            //if (pMethod == IntPtr.Zero)
            //{
            //    ShowError(string.Format("Unable to find address of \"{0}::{1}\".", moduleName, methodName), Marshal.GetLastWin32Error());
            //    return false;
            //}

            if(RemoteAllocInvokeMethod(out IntPtr pInvokeMethod))
            {
                if (RemoteAlloc(4 * 3, out IntPtr pMemory))
                {
                    if (RemoteAlloc((uint)moduleName.Length + 1, out IntPtr pModuleName)
                        && RemoteAlloc((uint)methodName.Length + 1, out IntPtr pMethodName))
                    {
                        if ((RemoteWrite(pModuleName, moduleName) && RemoteWrite(pMethodName, methodName)) == false)
                        {
                            return false;
                        }
                        byte[] buffer = new byte[4 * 3];
                        BitConverter.GetBytes((uint)pModuleName).CopyTo(buffer, 0);
                        BitConverter.GetBytes((uint)pMethodName).CopyTo(buffer, 4);
                        BitConverter.GetBytes((uint)parameter).CopyTo(buffer, 8);
                        if (RemoteWrite(pMemory, buffer))
                        {
                            IntPtr hThread = CreateRemoteThread(HProcess, IntPtr.Zero, 0, pInvokeMethod, pMemory, 0, IntPtr.Zero);
                            if (hThread == IntPtr.Zero)
                            {
                                ShowError("Unable to CreateRemoteThread.", Marshal.GetLastWin32Error());
                                return false;
                            }

                            if (synchronize)
                            {
                                const uint INFINITE = 0xFFFFFFFF;
                                const uint WAIT_FAILED = 0xFFFFFFFF;
                                if (WaitForSingleObject(hThread, INFINITE) == WAIT_FAILED)
                                {
                                    ShowError("Unable to WaitForSingleObject.", Marshal.GetLastWin32Error());
                                    return false;
                                }
                            }

                            return true;
                        }

                    }
                }
            }

            return false;
        }

        IntPtr HInvokeMethod { get; set; }
        private bool RemoteAllocInvokeMethod(out IntPtr pInvokeMethod)
        {
            if (HInvokeMethod == IntPtr.Zero)
            {
                HInvokeMethod = IntPtr.Zero;
                byte[] buffer = {
                        0x55, // PUSH EBP
                        0x8B, 0xEC, // MOV EBP, ESP
                        0x8B, 0x45, 0x08, // MOV EAX, [EBP + 8]
                        0xFF, 0x70, 0x00, // PUSH [EAX + 0]
                        0xB8, 0x00, 0x00, 0x00, 0x00, // MOV EAX, GetModuleHandle
                        0xFF, 0xD0, // CALL EAX
                        0x8B, 0xD8, // MOV EBX, EAX
                        0x8B, 0x45, 0x08, // MOV EAX, [EBP + 8]
                        0xFF, 0x70, 0x04, // PUSH [EAX + 4]
                        0x53, // PUSH EBX
                        0xB8, 0x00, 0x00, 0x00, 0x00,// MOV EAX, GetProcAddress
                        0xFF, 0xD0, // CALL EAX
                        0x8B, 0xD8, // MOV EBX, EAX
                        0x8B, 0x45, 0x08, // MOV EAX, [EBP + 8]
                        0xFF, 0x70, 0x08, // PUSH [EAX + 8]
                        0xFF, 0xD3, // CALL EBX
                        0x8B, 0xE5, // MOV ESP, EBP
                        0x5D, // POP EBP
                        0xC3 // RET
                     };

                IntPtr pGetModuleHandle = GetProcAddress(GetModuleHandle("kernel32.dll"), "GetModuleHandleA");
                IntPtr pGetProcAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "GetProcAddress");

                BitConverter.GetBytes((uint)pGetModuleHandle).CopyTo(buffer, 10);
                BitConverter.GetBytes((uint)pGetProcAddress).CopyTo(buffer, 26);

                if (RemoteAlloc((uint)buffer.Length, out IntPtr pMemory))
                {
                    if (RemoteWrite(pMemory, buffer))
                    {
                        HInvokeMethod = pMemory;
                    }
                }
            }

            pInvokeMethod = HInvokeMethod;
            return pInvokeMethod != IntPtr.Zero;
        }

        private bool RemoteAlloc(uint length, out IntPtr pMemory)
        {
            pMemory = VirtualAllocEx(HProcess, IntPtr.Zero, length, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);
            if (pMemory == IntPtr.Zero)
            {
                ShowError("Unable to allocate memory to target process.", Marshal.GetLastWin32Error());
                return false;
            }
            Console.WriteLine("remote alloc 0x{0:X} ({1})", (uint)pMemory, length);
            return true;
        }

        private bool RemoteWrite(IntPtr pMemory, byte[] buffer)
        {
            if (WriteProcessMemory(HProcess, pMemory, buffer, (uint)buffer.Length, out uint written) == 0)
            {
                ShowError("Unable to write memory to process.", Marshal.GetLastWin32Error());
                return false;
            }
            Console.WriteLine("remote write 0x{0:X} [{1}]", (uint)pMemory, string.Join(", ", buffer));
            return true;
        }

        private bool RemoteWrite(IntPtr pMemory, string buffer)
        {
            if (WriteProcessMemory(HProcess, pMemory, buffer, (uint)buffer.Length + 1, out uint written) == 0)
            {
                ShowError("Unable to write memory to process.", Marshal.GetLastWin32Error());
                return false;
            }
            Console.WriteLine("remote write 0x{0:X} [{1}]", (uint)pMemory, buffer);
            return true;
        }
    }
}