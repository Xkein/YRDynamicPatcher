
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;

namespace ExtensionHooks
{
    public class TechnoTypeExtHooks
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
        
        [Hook(HookType.AresHook, Address = 0x716DC0, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x7162F0, Size = 6)]
        static public unsafe UInt32 TechnoTypeClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return TechnoTypeExt.TechnoTypeClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x716DAC, Size = 0xA)]
        static public unsafe UInt32 TechnoTypeClass_Load_Suffix(REGISTERS* R)
        {
            return TechnoTypeExt.TechnoTypeClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x717094, Size = 5)]
        static public unsafe UInt32 TechnoTypeClass_Save_Suffix(REGISTERS* R)
        {
            return TechnoTypeExt.TechnoTypeClass_Save_Suffix(R);
        }
    }
}