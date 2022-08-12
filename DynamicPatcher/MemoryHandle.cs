using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xkein.Memory;

namespace DynamicPatcher
{
    /// <summary>Allocate memory on target process.</summary>
    public class MemoryHandle : CriticalFinalizerObject, IDisposable
    {
        /// <summary>The address of memory</summary>
        public int Memory { get; set; }
        /// <summary>The size of memory</summary>
        public int Size { get; private set; }

        private bool disposedValue;
        /// <summary>Allocate fixed size memory on target process.</summary>
        public MemoryHandle(int size)
        {
            var memory = MemoryHelper.AllocMemory(size);
            if (memory == (int)IntPtr.Zero)
            {
                throw new OutOfMemoryException("MemoryHandle Alloc fail.");
            }
            Memory = memory;
            Size = size;
        }

        /// <summary>Free memory.</summary>
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
        /// <summary>Free memory.</summary>
        ~MemoryHandle()
        {
            Dispose(disposing: false);
        }
        /// <summary>Free memory.</summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>The memory helper on target process.</summary>
    public unsafe static class MemoryHelper
    {
        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        /// <summary>Write byte[] to address.</summary>
        static public bool Write(int address, byte[] buffer, int length)
        {
            int tmp = 0;
            return WriteProcessMemory(Helpers.GetProcessHandle(), address, buffer, length, ref tmp);
        }

        /// <summary>Read byte[] from address.</summary>
        static public bool Read(int address, byte[] buffer, int length)
        {
            int tmp = 0;
            return ReadProcessMemory(Helpers.GetProcessHandle(), address, buffer, length, ref tmp);
        }

        /// <summary>Write T to address.</summary>
        static public bool Write<T>(int address, T obj)
        {
            int size = Unsafe.SizeOf<T>();
            IntPtr buffer = (IntPtr)Unsafe.AsPointer(ref obj);
            Span<byte> bytes = new Span<byte>(buffer.ToPointer(), size);

            return Write(address, bytes.ToArray(), size);
        }

        /// <summary>Read T from address.</summary>
        static public bool Read<T>(int address, ref T obj)
        {
            int size = Unsafe.SizeOf<T>();
            IntPtr buffer = (IntPtr)Unsafe.AsPointer(ref obj);
            byte[] bytes = new byte[size];

            bool ret = Read(address, bytes, size);

            Marshal.Copy(bytes, 0, buffer, size);
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
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, int lpBaseAddress, int dwSize, AllocationType flAllocationType, Protect flProtect);

        enum FreeType
        {
            MEM_RELEASE = 0x00008000
        }
        [DllImport("kernel32.dll")]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpBaseAddress, int dwSize, FreeType flFreeType);


        private static Allocator allocator = new Allocator(
            (size) => VirtualAllocEx(Helpers.GetProcessHandle(), 0, size, AllocationType.MEM_RESERVE | AllocationType.MEM_COMMIT, Protect.PAGE_EXECUTE_READWRITE),
            (address) => VirtualFreeEx(Helpers.GetProcessHandle(), address, 0, FreeType.MEM_RELEASE));

        /// <summary>Allocate fixed size memory on target process.</summary>
        static public int AllocMemory(int size)
        {
            return (int)allocator.Alloc(size);
        }

        /// <summary>Free memory on target process.</summary>
        static public bool FreeMemory(int address)
        {
            allocator.Free((IntPtr)address);
            return true;
        }
    }
}
