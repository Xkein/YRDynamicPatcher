using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
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
