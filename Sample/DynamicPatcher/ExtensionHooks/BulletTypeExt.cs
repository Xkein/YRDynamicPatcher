
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;

namespace ExtensionHooks
{
    public class BulletTypeExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x46BDD9, Size = 5)]
        static public unsafe UInt32 BulletTypeClass_CTOR(REGISTERS* R)
        {
            return BulletTypeExt.BulletTypeClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x46C8B6, Size = 6)]
        static public unsafe UInt32 BulletTypeClass_SDDTOR(REGISTERS* R)
        {
            return BulletTypeExt.BulletTypeClass_SDDTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x46C429, Size = 0xA)]
        [Hook(HookType.AresHook, Address = 0x46C41C, Size = 0xA)]
        static public unsafe UInt32 BulletTypeClass_LoadFromINI(REGISTERS* R)
        {
            return BulletTypeExt.BulletTypeClass_LoadFromINI(R);
        }

        [Hook(HookType.AresHook, Address = 0x46C730, Size = 8)]
        [Hook(HookType.AresHook, Address = 0x46C6A0, Size = 5)]
        static public unsafe UInt32 BulletTypeClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return BulletTypeExt.BulletTypeClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x46C722, Size = 5)]
        static public unsafe UInt32 BulletTypeClass_Load_Suffix(REGISTERS* R)
        {
            return BulletTypeExt.BulletTypeClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x46C74A, Size = 5)]
        static public unsafe UInt32 BulletTypeClass_Save_Suffix(REGISTERS* R)
        {
            return BulletTypeExt.BulletTypeClass_Save_Suffix(R);
        }
    }
}