
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;

namespace PatcherSample
{
    public class HookTest
    {
        [Hook(HookType.AresHook, Address = 0x6FCFA0, Size = 5)]
        static public UInt32 ShowFirer(ref REGISTERS R)
        {
            var pTechno = (IntPtr)R.ESI;
            var pType = YRPP.GetTechnoType(pTechno);
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

