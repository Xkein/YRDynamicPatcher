
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;

namespace PatcherSample
{
    public class HookTest
    {
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate IntPtr ThisCall_0(IntPtr pThis);

        [Hook(HookType.AresHook, Address = 0x6FCFA0, Size = 5)]
        static public UInt32 ShowFirer(ref REGISTERS R)
        {
            var GetTechnoType = Marshal.GetDelegateForFunctionPointer<ThisCall_0>(new IntPtr(0x6F3270));
            var pTechno = (IntPtr)R.ESI;
            var pType = GetTechnoType(pTechno);
            IntPtr IDPtr = Marshal.ReadIntPtr(pType, 96);
            string ID = Marshal.PtrToStringUni(IDPtr);
            Console.WriteLine(ID + " fired");
            var rof = new Random().Next(10, 50);
            Console.WriteLine("next ROF: " + rof);
            R.EAX = (uint)rof;
            Console.WriteLine();

            return 0x6FCFBE;
        }

    }
}

