
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using PatcherSample;

namespace Test
{
    public class TechnoTypeExtTest
    {
        [Hook(HookType.AresHook, Address = 0x711835, Size = 5)]
        static public unsafe UInt32 TechnoTypeClass_CTOR(REGISTERS* R)
        {
            return TechnoTypeExt.TechnoTypeClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x711AE0, Size = 5)]
        static public unsafe UInt32 TechnoTypeClass_DTOR(REGISTERS* R)
        {
            return TechnoTypeExt.TechnoTypeClass_DTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x716132, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x716123, Size = 5)]
        static public unsafe UInt32 TechnoTypeClass_LoadFromINI(REGISTERS* R)
        {
            return TechnoTypeExt.TechnoTypeClass_LoadFromINI(R);
        }
    }
}