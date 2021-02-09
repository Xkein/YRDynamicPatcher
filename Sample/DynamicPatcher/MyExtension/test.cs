
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;

namespace Test
{
    public class HTest
    {
        //[Hook(HookType.AresHook, Address = 0x6FCFA0, Size = 5)]
        static public unsafe UInt32 ShowFirer_Test(REGISTERS* R)
        {
            ref TechnoClass rTechno = ref ((Pointer<TechnoClass>)R->ESI).Ref;
            ref TechnoTypeClass rType = ref rTechno.Type.Ref;
            ref HouseClass rHouse = ref rTechno.Owner.Ref;
            TechnoExt ext = TechnoExt.ExtMap.Find((Pointer<TechnoClass>)R->ESI);

            string ID = rType.Base.GetUIName();
            string HouseID = rHouse.Type.Ref.Base.GetUIName();
            Logger.Log("{0}({1}) {2} Berzerk {3}", ID, HouseID, ext.MyExtensionTest, rTechno.Berzerk);

            return 0;
        }
    }
}
