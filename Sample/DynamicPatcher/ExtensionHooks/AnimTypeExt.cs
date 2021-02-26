
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;

namespace ExtensionHooks
{
    public class AnimTypeExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x42784B, Size = 5)]
        static public unsafe UInt32 AnimTypeClass_CTOR(REGISTERS* R)
        {
            return AnimTypeExt.AnimTypeClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x428EA8, Size = 5)]
        static public unsafe UInt32 AnimTypeClass_SDDTOR(REGISTERS* R)
        {
            return AnimTypeExt.AnimTypeClass_SDDTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x4287E9, Size = 0xA)]
        [Hook(HookType.AresHook, Address = 0x4287DC, Size = 0xA)]
        static public unsafe UInt32 AnimTypeClass_LoadFromINI(REGISTERS* R)
        {
            return AnimTypeExt.AnimTypeClass_LoadFromINI(R);
        }

        [Hook(HookType.AresHook, Address = 0x428970, Size = 8)]
        [Hook(HookType.AresHook, Address = 0x428800, Size = 0xA)]
        static public unsafe UInt32 AnimTypeClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return AnimTypeExt.AnimTypeClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x42892C, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x428958, Size = 6)]
        static public unsafe UInt32 AnimTypeClass_Load_Suffix(REGISTERS* R)
        {
            return AnimTypeExt.AnimTypeClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x42898A, Size = 5)]
        static public unsafe UInt32 AnimTypeClass_Save_Suffix(REGISTERS* R)
        {
            return AnimTypeExt.AnimTypeClass_Save_Suffix(R);
        }
    }
}