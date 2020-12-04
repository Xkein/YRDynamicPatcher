using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    public class MemoryHandle : IDisposable
    {
        public int Memory { get; set; }
        public int Size { get; private set; }

        private bool disposedValue;
        public MemoryHandle(int size)
        {
            var memory = MemoryHelper.AllocMemory(size);
            if(memory == (int)IntPtr.Zero)
            {
                throw new OutOfMemoryException("MemoryHandle Alloc fail.");
            }
            Memory = memory;
            Size = size;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                MemoryHelper.FreeMemory(Memory);
                disposedValue = true;
            }
        }
        ~MemoryHandle()
        {
            Dispose(disposing: false);
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class MemoryHelper
    {
        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        static public bool Write(int address, byte[] buffer, int length)
        {
            int tmp = 0;
            return WriteProcessMemory(Utilities.GetProcessHandle(), address, buffer, length, ref tmp);
        }

        static public bool Read(int address, byte[] buffer, int length)
        {
            int tmp = 0;
            return ReadProcessMemory(Utilities.GetProcessHandle(), address, buffer, length, ref tmp);
        }

        static public bool Write<T>(int address, T obj) where T : unmanaged
        {
            int size = Marshal.SizeOf(obj);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, buffer, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(buffer, bytes, 0, size);
            Marshal.FreeHGlobal(buffer);

            return Write(address, bytes, bytes.Length);
        }
        static public bool Read<T>(int address, T obj) where T : unmanaged
        {
            int size = Marshal.SizeOf(obj);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            byte[] bytes = new byte[size];

            bool ret = Read(address, bytes, bytes.Length);

            Marshal.Copy(bytes, 0, buffer, size);
            Marshal.PtrToStructure(buffer, obj);
            Marshal.FreeHGlobal(buffer);

            return ret;
        }

        enum AllocationType
        {
            MEM_RESERVE = 0x00002000,
            MEM_COMMIT = 0x00001000
        }
        enum Protect
        {
            PAGE_EXECUTE_READWRITE = 0x40
        }
        [DllImport("kernel32.dll")]
        static extern int VirtualAllocEx(IntPtr hProcess, int lpBaseAddress, int dwSize, AllocationType flAllocationType, Protect flProtect);
        static public int AllocMemory(int size)
        {
            return VirtualAllocEx(Utilities.GetProcessHandle(), 0, size, AllocationType.MEM_RESERVE | AllocationType.MEM_COMMIT, Protect.PAGE_EXECUTE_READWRITE);
        }

        enum FreeType
        {
            MEM_RELEASE = 0x00008000
        }
        [DllImport("kernel32.dll")]
        static extern bool VirtualFreeEx(IntPtr hProcess, int lpBaseAddress, int dwSize, FreeType flFreeType);
        static public bool FreeMemory(int address)
        {
            return VirtualFreeEx(Utilities.GetProcessHandle(), address, 0, FreeType.MEM_RELEASE);
        }
    }

    static class ASM
    {
        static public readonly byte INIT = 0x00;
        static public readonly byte INT3 = 0xCC;
        static public readonly byte NOP = 0x90;

        static public readonly byte[] Jmp = { 0xE9, INIT, INIT, INIT, INIT };
        static public readonly byte[] Call = { 0xE8, INIT, INIT, INIT, INIT };
    }

    struct JumpStruct
    {
        public int From { get; set; }
        public int To { get; set; }
        public JumpStruct(int from, int to)
        {
            From = from;
            To = to;
        }

        public int Offset
        {
            get => To - From - 5;
        }
	}

    class ASMWriter
    {
        [DllImport("kernel32.dll")]
        static extern bool FlushInstructionCache(IntPtr hProcess, int lpBaseAddress, int dwSize);
        static public bool FlushInstructionCache(int lpBaseAddress, int dwSize)
        {
            return FlushInstructionCache(Utilities.GetProcessHandle(), lpBaseAddress, dwSize);
        }

        static public void WriteJump(JumpStruct jump)
        {
            byte[] buffer = ASM.Jmp.Clone() as byte[];
            var stream = new MemoryStream(buffer);
            using (var writer = new BinaryWriter(stream))
            {
                stream.Seek(1, SeekOrigin.Begin);
                writer.Write(jump.Offset);

                MemoryHelper.Write(jump.From, buffer, buffer.Length);
            }
        }
        static public void WriteCall(JumpStruct jump)
        {
            byte[] buffer = ASM.Call.Clone() as byte[];
            var stream = new MemoryStream(buffer);
            using (var writer = new BinaryWriter(stream))
            {
                stream.Seek(1, SeekOrigin.Begin);
                writer.Write(jump.Offset);

                MemoryHelper.Write(jump.From, buffer, buffer.Length);
            }
        }
    }

    class Utilities
    {
        private static IntPtr yrHandle = IntPtr.Zero;

        static IntPtr YRHandle {
            get
            {
                if(yrHandle == IntPtr.Zero)
                {
                    yrHandle = FindYRProcessHandle();
                }
                return yrHandle;
            }
            set => yrHandle = value;
        }
        static private IntPtr FindYRProcessHandle()
        {

            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName.Contains("gamemd"))
                {
                    var targetProcess = Process.GetCurrentProcess();
                    try
                    {
                        Logger.Log("find YR process: {0} ({1})", targetProcess.MainWindowTitle, targetProcess.Id);
                        targetProcess.EnableRaisingEvents = true;
                        targetProcess.Exited += (object sender, EventArgs e) => {
                            YRHandle = IntPtr.Zero;
                            Logger.Log("{0} ({1}) exited.", targetProcess.MainWindowTitle, targetProcess.Id);
                            };
                        return targetProcess.Handle;
                    }
                    catch (Exception e)
                    {
                        Logger.Log("get process handle error: " + e.Message);
                        throw e;
                    }
                }
            }

            throw new InvalidOperationException("could not find yr handle");
        }

        static public IntPtr GetProcessHandle()
        {
            return YRHandle;
        }
    }
};


