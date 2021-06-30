
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;

namespace ExtensionHooks
{
    public class TechnoExtHooks
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
        
        [Hook(HookType.AresHook, Address = 0x70C250, Size = 8)]
        [Hook(HookType.AresHook, Address = 0x70BF50, Size = 5)]
        static public unsafe UInt32 TechnoClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x70C249, Size = 5)]
        static public unsafe UInt32 TechnoClass_Load_Suffix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x70C264, Size = 5)]
        static public unsafe UInt32 TechnoClass_Save_Suffix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_Save_Suffix(R);
        }
        
        
        [Hook(HookType.AresHook, Address = 0x6F9E50, Size = 5)]
        static public unsafe UInt32 TechnoClass_Update(REGISTERS* R)
        {
            try{
                return ScriptManager.TechnoClass_Update_Script(R);
            }
			catch (Exception e)
			{
                Logger.PrintException(e);
				return (uint)0;
			}
        }
        [Hook(HookType.AresHook, Address = 0x6F6CA0, Size = 7)]
        static public unsafe UInt32 TechnoClass_Put_Script(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
            var pCoord = R->Stack<Pointer<CoordStruct>>(0x4);
            var faceDir = R->Stack<Direction>(0x8);

            TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
            ext.Scriptable?.OnPut(pCoord.Data, faceDir);

            return 0;
        }
        // avoid hook conflict with phobos feature -- shield
        //[Hook(HookType.AresHook, Address = 0x6F6AC0, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x6F6AC4, Size = 5)]
        static public unsafe UInt32 TechnoClass_Remove_Script(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;

            TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
            ext.Scriptable?.OnRemove();

            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x701900, Size = 6)]
        static public unsafe UInt32 TechnoClass_ReceiveDamage_Script(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
            var pDamage = R->Stack<Pointer<int>>(0x4);
            var distanceFromEpicenter = R->Stack<int>(0x8);
            var pWH = R->Stack<Pointer<WarheadTypeClass>>(0xC);
            var pAttacker = R->Stack<Pointer<ObjectClass>>(0x10);
            var ignoreDefenses = R->Stack<bool>(0x14);
            var preventPassengerEscape = R->Stack<bool>(0x18);
            var pAttackingHouse = R->Stack<Pointer<HouseClass>>(0x1C);

            TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
            ext.Scriptable?.OnReceiveDamage(pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);

            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x6FDD50, Size = 6)]
        static public unsafe UInt32 TechnoClass_Fire(REGISTERS* R)
        {
            try{
                return ScriptManager.TechnoClass_Fire_Script(R);
            }
			catch (Exception e)
			{
                Logger.PrintException(e);
				return (uint)0;
			}
        }
    }
}