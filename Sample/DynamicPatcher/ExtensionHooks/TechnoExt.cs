
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;

namespace Test
{
    public class TechnoExtTest
    {
        [Hook(HookType.AresHook, Address = 0x6F3260, Size = 5)]
        static public unsafe UInt32 TechnoClass_CTOR(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x6F4500, Size = 5)]
        static public unsafe UInt32 TechnoClass_DTOR(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_DTOR(R);
        }
        
        [Hook(HookType.AresHook, Address = 0x6F9E50, Size = 5)]
        static public unsafe UInt32 TechnoClass_Update(REGISTERS* R)
        {
            try{
                return ScriptManager.TechnoClass_Update_Script(R);
            }
			catch (Exception e)
			{
                Helpers.PrintException(e);
				return (uint)0;
			}

        }
    }
}