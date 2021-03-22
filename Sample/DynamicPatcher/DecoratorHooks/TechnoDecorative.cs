
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Decorators;

namespace DecoratorHooks
{
    public class TechnoDecorativeHooks
    {
        [Hook(HookType.AresHook, Address = 0x6F9E50, Size = 5)]
        static public unsafe UInt32 OnUpdate(REGISTERS* R)
        {
            try {
            return TechnoDecorative.OnUpdate(R);
            }
			catch (Exception e)
			{
                Logger.PrintException(e);
				return (uint)0;
			}
        }
        [Hook(HookType.AresHook, Address = 0x701900, Size = 6)]
        static public unsafe UInt32 OnReceiveDamage(REGISTERS* R)
        {
            return TechnoDecorative.OnReceiveDamage(R);
        }
        [Hook(HookType.AresHook, Address = 0x6FDD50, Size = 6)]
        static public unsafe UInt32 OnFire(REGISTERS* R)
        {
            return TechnoDecorative.OnFire(R);
        }
    }
}