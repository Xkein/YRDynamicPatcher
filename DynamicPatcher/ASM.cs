using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    /// <summary>Represents assembly codes.</summary>
    public static class ASM
    {
        /// <summary>Meaningless byte.</summary>
        public const byte INIT = 0x00;
        /// <summary>INT3</summary>
        public const byte INT3 = 0xCC;
        /// <summary>NOP</summary>
        public const byte NOP = 0x90;

        /// <summary>JMP relative_address</summary>
        public static readonly byte[] Jmp = { 0xE9, INIT, INIT, INIT, INIT };
        /// <summary>CALL relative_address</summary>
        public static readonly byte[] Call = { 0xE8, INIT, INIT, INIT, INIT };

        /// <summary>Get MemoryHandle of code`s buffer.</summary>
        public static MemoryHandle CreateCodeHandle(byte[] code)
        {
            var handle = new MemoryHandle(code.Length);
            MemoryHelper.Write(handle.Memory, code, code.Length);
            return handle;
        }

        static ASM()
        {
            byte[] this2fastcall = {
                        0x8B, 0x54, 0xE4, 0x08, //MOV EDX, [ESP + 8]
                        0x58, // POP EAX
                        0x89, 0x44, 0xE4, 0x04, // MOV [ESP + 4], EAX
                        0x89, 0xC8, // MOV EAX, ECX
                        0x59, // POP ECX
                        0xFF, 0xE0 // JMP EAX
                     };
            fastCallHandle = CreateCodeHandle(this2fastcall);

            byte[] jmp_code = {
                        0x58, // POP EAX
                        0x83, 0xC4, 0x04, // ADD ESP, 4
                        0xFF, 0xE0 // JMP EAX
                     };
            jmpHandle = CreateCodeHandle(jmp_code);
        }

        private static MemoryHandle fastCallHandle;
        /// <summary>ThisCall to FastCall.</summary>
        public static IntPtr FastCallTransferStation => (IntPtr)fastCallHandle.Memory;

        private static MemoryHandle jmpHandle;
        /// <summary>Jump to address.</summary>
        static public unsafe void JMP(int address)
        {
            var jmp = (delegate* unmanaged[Stdcall]<int, void>)jmpHandle.Memory;
            jmp(address);
        }
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
            get => To - From - ASM.Jmp.Length;
            set => To = From + value + ASM.Jmp.Length;
        }
    }

    class ASMWriter
    {
        [DllImport("kernel32.dll")]
        static extern bool FlushInstructionCache(IntPtr hProcess, int lpBaseAddress, int dwSize);
        static public bool FlushInstructionCache(int lpBaseAddress, int dwSize)
        {
            return FlushInstructionCache(Helpers.GetProcessHandle(), lpBaseAddress, dwSize);
        }

        static public void WriteJump(JumpStruct jump)
        {
            byte[] buffer = ASM.Jmp;
            MemoryHelper.Write(jump.From, buffer, buffer.Length);
            MemoryHelper.Write(jump.From + 1, jump.Offset);
        }
        static public void WriteCall(JumpStruct jump)
        {
            byte[] buffer = ASM.Call;
            MemoryHelper.Write(jump.From, buffer, buffer.Length);
            MemoryHelper.Write(jump.From + 1, jump.Offset);
        }
    }
}
