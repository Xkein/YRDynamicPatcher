using DynamicPatcher;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherSample
{
    [Serializable]
    public class TechnoTypeExt : Extension<TechnoTypeClass>
    {
        public static Container<TechnoTypeExt, TechnoTypeClass> ExtMap = new Container<TechnoTypeExt, TechnoTypeClass>("TechnoTypeClass");

        public Script Script;

        public TechnoTypeExt(Pointer<TechnoTypeClass> OwnerObject) : base(OwnerObject)
        {

        }

        protected override void LoadFromINIFile(Pointer<CCINIClass> pINI)
        {
            INI_EX exINI = new INI_EX(pINI);
            string section = OwnerObject.Ref.Base.GetID();

            string scriptName = null;
            exINI.Read(section, "Script", ref scriptName);
            if(scriptName != null)
            {
                Script = ScriptManager.GetScript(scriptName);
            }
        }

        //[Hook(HookType.AresHook, Address = 0x711835, Size = 5)]
        static public unsafe UInt32 TechnoTypeClass_CTOR(REGISTERS* R)
        {
            var pItem = (Pointer<TechnoTypeClass>)R->ESI;

            TechnoTypeExt.ExtMap.FindOrAllocate(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x711AE0, Size = 5)]
        static public unsafe UInt32 TechnoTypeClass_DTOR(REGISTERS* R)
        {
            var pItem = (Pointer<TechnoTypeClass>)R->ECX;

            TechnoTypeExt.ExtMap.Remove(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x716132, Size = 5)]
        //[Hook(HookType.AresHook, Address = 0x716123, Size = 5)]
        static public unsafe UInt32 TechnoTypeClass_LoadFromINI(REGISTERS* R)
        {
            var pItem = (Pointer<TechnoTypeClass>)R->EBP;
            var pINI = R->Stack<Pointer<CCINIClass>>(0x380);

            TechnoTypeExt.ExtMap.LoadFromINI(pItem, pINI);
            return 0;
        }

    }

}
